using ManagementService.Models;
using ManagementService.Data;
using Microsoft.EntityFrameworkCore;

namespace ManagementService.Repositories
{
    public interface IFeeRepository
    {
        Task<List<Fee>> GetAllFees();
        Task<Fee?> GetFeeById(int id);
        Task AddFee(Fee fee);
        Task UpdateFee(Fee fee);
    }

    public class FeeRepository : IFeeRepository
    {
        private readonly ManagementDbContext _context;

        public FeeRepository(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<Fee>> GetAllFees()
        {
            return await _context.Fee.ToListAsync();
        }

        public async Task<Fee?> GetFeeById(int feeId)
        {
            return await _context.Fee.FirstOrDefaultAsync(f => f.FeeID == feeId);
        }

        public async Task AddFee(Fee fee)
        {
            await _context.Fee.AddAsync(fee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFee(Fee fee)
        {
            _context.Fee.Update(fee);
            await _context.SaveChangesAsync();
        }
    }
}