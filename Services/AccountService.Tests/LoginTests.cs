using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using AccountService.Services;
using AccountService.Repositories;
using Microsoft.AspNetCore.Identity;
using AccountService.Models;

namespace AccountService.Tests
{
    public class LoginTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly IAccountService _accountService;

        public LoginTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _configurationMock = new Mock<IConfiguration>();

            var jwtSectionMock = new Mock<IConfigurationSection>();
            jwtSectionMock.Setup(x => x["Key"]).Returns("MySuperSecretKey12345ForTestsNeedsToBe256bits");
            jwtSectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSectionMock.Setup(x => x["Audience"]).Returns("TestAudience");
            jwtSectionMock.Setup(x => x["ExpiresInMinutes"]).Returns("60");

            _configurationMock.Setup(x => x.GetSection("Jwt")).Returns(jwtSectionMock.Object);

            _accountService = new AccountService.Services.AccountService(
                _accountRepositoryMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ReturnsJwtToken_WhenCredentialsAreValid()
        {
            var account = new Account
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = new PasswordHasher<Account>().HashPassword(null!, "validpassword"),
                AccountType = 0
            };

            _accountRepositoryMock
                .Setup(x => x.GetAccountByEmailAsync(account.Email))
                .ReturnsAsync(account);

            var result = await _accountService.LoginAsync(account.Email, "validpassword");

            Assert.NotNull(result);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(result) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == account.Name);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenAccountDoesNotExist()
        {
            _accountRepositoryMock
                .Setup(x => x.GetAccountByEmailAsync("nonexistent@example.com"))
                .ReturnsAsync((Account?)null);

            var result = await _accountService.LoginAsync("nonexistent@example.com", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenPasswordIsInvalid()
        {
            var account = new Account
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = new PasswordHasher<Account>().HashPassword(null!, "validpassword"),
                AccountType = 0
            };

            _accountRepositoryMock
                .Setup(x => x.GetAccountByEmailAsync(account.Email))
                .ReturnsAsync(account);

            var result = await _accountService.LoginAsync(account.Email, "invalidpassword");

            Assert.Null(result);
        }
    }
}