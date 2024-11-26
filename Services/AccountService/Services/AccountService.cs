using AccountService.Models;
using AccountService.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AccountService.Services
{
    public interface IAccountService
    {
        Task<Account> CreateAccountAsync(Account account);
        Task<Account?> GetAccountByIdAsync(int id);
        Task<string?> LoginAsync(string email, string password);
        Task LinkRestaurant(string name, string address, string category);
        Task<bool> DeleteAccountAsync(int accountId);
    }

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly PasswordHasher<Account> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AccountService(IAccountRepository accountRepository, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _passwordHasher = new PasswordHasher<Account>();
            _configuration = configuration;
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            if (string.IsNullOrEmpty(account.Password))
            {
                throw new ArgumentException("Password is required.");
            }

            var existingAccount = await _accountRepository.GetAccountByEmailAsync(account.Email);
            if (existingAccount != null)
            {
                throw new InvalidOperationException($"An account with the email '{account.Email}' already exists.");
            }

            account.Password = _passwordHasher.HashPassword(account, account.Password);

            account.CreatedAt = DateTime.UtcNow;

            return await _accountRepository.AddAccountAsync(account);
        }

        public async Task<Account?> GetAccountByIdAsync(int id)
        {
            return await _accountRepository.GetAccountByIdAsync(id);
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var account = await _accountRepository.GetAccountByEmailAsync(email);
            if (account == null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(account, account.Password, password);
            if (result != PasswordVerificationResult.Success)
            {
                return null;
            }

            return GenerateJwtToken(account);
        }

        private string GenerateJwtToken(Account account)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is missing."));
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, account.Name),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim("AccountID", account.AccountID.ToString()),
            new Claim(ClaimTypes.Role, account.GetType().Name)
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"] ?? throw new InvalidOperationException("JWT timer is missing."))),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task LinkRestaurant(string name, string address, string category)
        {
            var account = await _accountRepository.GetAccountByNameAsync(name);

            if (account?.AccountType != AccountType.Restaurant)
            {
                throw new InvalidOperationException("Account is not a restaurant.");
            }

            var apiUrl = "http://localhost:5045/api/restaurant/search";
            var queryParameters = $"?name={name}&address={address}&category={category}";

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(apiUrl + queryParameters);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch restaurant data. Status: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var searchResults = JsonSerializer.Deserialize<List<RestaurantSearchRequest>>(responseContent);

            if (searchResults == null || !searchResults.Any())
            {
                throw new Exception("No matching restaurant found in the search.");
            }

            var matchingRestaurant = searchResults.First();

            Console.WriteLine(matchingRestaurant.restaurantID);

            account.RestaurantSearchID = matchingRestaurant.restaurantID;

            await _accountRepository.UpdateAccountAsync(account);
        }

        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            bool isDeleted = await _accountRepository.DeleteAccountAsync(accountId);

            if (!isDeleted)
            {
                throw new InvalidOperationException("Account deletion failed or account does not exist.");
            }

            return true;
        }
    }
}