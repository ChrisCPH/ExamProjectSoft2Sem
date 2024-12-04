using AccountService.Models;
using AccountService.Repositories;
using AccountService.Services;
using AccountService.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AccountService.Tests
{
    public class AccountServiceTests
    {
        private IAccountService _accountService;
        private IAccountRepository _accountRepository;
        private AccountDbContext _dbContext;

        public AccountServiceTests()
        {
            _dbContext = GetInMemoryDbContext();

            _accountRepository = new AccountRepository(_dbContext);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("ThisIsA32CharacterSecretTestKey!");
            mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("TestIssuer");
            mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("TestAudience");
            mockConfiguration.Setup(config => config["Jwt:ExpiresInMinutes"]).Returns("60");

            _accountService = new Services.AccountService(_accountRepository, mockConfiguration.Object);
        }

        private AccountDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AccountDbContext(options);
        }

        // Fix login tests
        /*
        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var email = "restaurantA@test.com";
            var password = "validPassword";
            var hashedPassword = new PasswordHasher<Account>().HashPassword(null, password);

            var account = new Account
            {
                AccountID = 1,
                Email = email,
                Password = hashedPassword,
                AccountType = AccountType.Restaurant,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Account.AddAsync(account);
            await _dbContext.SaveChangesAsync();

            var token = await _accountService.LoginAsync(email, password);
            Assert.NotNull(token);
        }


        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            var email = "restaurantA@test.com";
            var password = "wrongPassword";
            var correctPassword = "validPassword";
            var hashedPassword = new PasswordHasher<Account>().HashPassword(null, correctPassword);

            var account = new Account
            {
                AccountID = 1,
                Email = email,
                Password = hashedPassword,
                AccountType = AccountType.Restaurant,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Account.Add(account);
            await _dbContext.SaveChangesAsync();

            var token = await _accountService.LoginAsync(email, password);

            Assert.Null(token);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenAccountDoesNotExist()
        {
            var email = "nonexistent@test.com";
            var password = "validPassword";

            var token = await _accountService.LoginAsync(email, password);

            Assert.Null(token);
        }*/

        [Fact]
        public async Task CreateAccountAsync_ShouldCreateAccount_WhenDataIsValid()
        {
            var account = new Account
            {
                Email = "newuser@test.com",
                Password = "validPassword",
                AccountType = AccountType.Restaurant,
                Name = "New Restaurant",
                CreatedAt = DateTime.UtcNow
            };

            var createdAccount = await _accountService.CreateAccountAsync(account);

            Assert.NotNull(createdAccount);
            Assert.Equal(account.Email, createdAccount.Email);
            Assert.Equal(account.Name, createdAccount.Name);
            Assert.Equal(AccountType.Restaurant, createdAccount.AccountType);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            var existingAccount = new Account
            {
                Email = "existinguser@test.com",
                Password = "validPassword",
                AccountType = AccountType.Customer,
                Name = "Existing Customer",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Account.Add(existingAccount);
            await _dbContext.SaveChangesAsync();

            var newAccount = new Account
            {
                Email = "existinguser@test.com",
                Password = "newPassword",
                AccountType = AccountType.Restaurant,
                Name = "New Restaurant",
                CreatedAt = DateTime.UtcNow
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.CreateAccountAsync(newAccount));

            Assert.Equal("An account with the email 'existinguser@test.com' already exists.", exception.Message);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldThrowException_WhenPasswordIsEmpty()
        {
            var account = new Account
            {
                Email = "noPassword@test.com",
                Password = "",
                AccountType = AccountType.DeliveryDriver,
                Name = "Driver",
                CreatedAt = DateTime.UtcNow
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _accountService.CreateAccountAsync(account));

            Assert.Equal("Password is required.", exception.Message);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenAccountExists()
        {
            var existingAccount = new Account
            {
                Email = "existinguser@test.com",
                Password = "validPassword",
                AccountType = AccountType.Customer,
                Name = "Existing Customer",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Account.Add(existingAccount);
            await _dbContext.SaveChangesAsync();

            var account = await _accountService.GetAccountByIdAsync(existingAccount.AccountID);

            Assert.NotNull(account);
            Assert.Equal(existingAccount.AccountID, account.AccountID);
            Assert.Equal(existingAccount.Email, account.Email);
            Assert.Equal(existingAccount.Name, account.Name);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
        {
            var account = await _accountService.GetAccountByIdAsync(999);

            Assert.Null(account);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldReturnTrue_WhenAccountExists()
        {
            var existingAccount = new Account
            {
                Email = "deleteuser@test.com",
                Password = "validPassword",
                AccountType = AccountType.Customer,
                Name = "Customer to Delete",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Account.Add(existingAccount);
            await _dbContext.SaveChangesAsync();

            var result = await _accountService.DeleteAccountAsync(existingAccount.AccountID);

            Assert.True(result);

            var deletedAccount = await _dbContext.Account.FindAsync(existingAccount.AccountID);
            Assert.Null(deletedAccount);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldThrowException_WhenAccountDoesNotExist()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.DeleteAccountAsync(999));
            Assert.Equal("Account deletion failed or account does not exist.", exception.Message);
        }

        [Fact]
        public async Task SetUnavailable_ValidDriver_UpdatesStatus()
        {
            var account = new Account
            {
                AccountID = 1,
                AccountType = AccountType.DeliveryDriver,
                Status = "Available"
            };
            _dbContext.Account.Add(account);
            await _dbContext.SaveChangesAsync();

            var updatedAccount = await _accountService.SetUnavailable(account.AccountID);

            Assert.Equal("Unavailable", updatedAccount.Status);
            var dbAccount = await _dbContext.Account.FindAsync(account.AccountID);
            Assert.Equal("Unavailable", dbAccount?.Status);
        }

        [Fact]
        public async Task SetUnavailable_NonDriver_ThrowsException()
        {
            var account = new Account
            {
                AccountID = 2,
                AccountType = AccountType.Customer,
                Status = "Available"
            };
            _dbContext.Account.Add(account);
            await _dbContext.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _accountService.SetUnavailable(account.AccountID));
        }

        [Fact]
        public async Task SetAvailable_ValidDriver_UpdatesStatus()
        {
            var account = new Account
            {
                AccountID = 3,
                AccountType = AccountType.DeliveryDriver,
                Status = "Unavailable"
            };
            _dbContext.Account.Add(account);
            await _dbContext.SaveChangesAsync();

            var updatedAccount = await _accountService.SetAvailable(account.AccountID);

            Assert.Equal("Available", updatedAccount.Status);
            var dbAccount = await _dbContext.Account.FindAsync(account.AccountID);
            Assert.Equal("Available", dbAccount?.Status);
        }

        [Fact]
        public async Task GetAvailableDriverWithLongestWaitTime_ReturnsCorrectDriver()
        {
            _dbContext.Account.AddRange(
                new Account { AccountID = 1, AccountType = AccountType.DeliveryDriver, Status = "Available", StatusChanged = DateTime.UtcNow.AddHours(-2) },
                new Account { AccountID = 2, AccountType = AccountType.DeliveryDriver, Status = "Available", StatusChanged = DateTime.UtcNow.AddHours(-1) }
            );
            await _dbContext.SaveChangesAsync();

            var driverId = await _accountService.GetAvailableDriverWithLongestWaitTime();

            Assert.Equal(1, driverId);
        }

        [Fact]
        public async Task GetAvailableDriverWithLongestWaitTime_NoDrivers_ThrowsException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _accountService.GetAvailableDriverWithLongestWaitTime());
        }

    }
}
