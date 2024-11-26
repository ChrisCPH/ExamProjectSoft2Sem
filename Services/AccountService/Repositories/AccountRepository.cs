using Microsoft.EntityFrameworkCore;
using AccountService.Data;
using AccountService.Models;

namespace AccountService.Repositories
{
    public interface IAccountRepository
    {
        Task<Account> AddAccountAsync(Account account);
        Task<Account?> GetAccountByIdAsync(int id);
        Task<Account?> GetAccountByEmailAsync(string email);
        Task UpdateAccountAsync(Account account);
        Task<Account?> GetAccountByNameAsync(string name);
        Task<bool> DeleteAccountAsync(int accountId);

    }
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDbContext _context;

        public AccountRepository(AccountDbContext context)
        {
            _context = context;
        }

        public async Task<Account> AddAccountAsync(Account account)
        {
            await _context.Account.AddAsync(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account?> GetAccountByIdAsync(int id)
        {
            return await _context.Account.FindAsync(id);
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {

            return await _context.Account.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task UpdateAccountAsync(Account account)
        {
            _context.Account.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task<Account?> GetAccountByNameAsync(string name)
        {
            return await _context.Account.FirstOrDefaultAsync(a => a.Name == name);
        }

        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            var account = await _context.Account.FindAsync(accountId);

            if (account == null)
            {
                return false;
            }

            _context.Account.Remove(account);

            await _context.SaveChangesAsync();

            return true;
        }


    }
}