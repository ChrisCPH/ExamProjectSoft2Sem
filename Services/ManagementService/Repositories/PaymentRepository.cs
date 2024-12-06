using ManagementService.Models;
using ManagementService.Data;
using Microsoft.EntityFrameworkCore;

namespace ManagementService.Repositories
{
    public interface IPaymentRepository
    {
        Task<List<Payment>> GetAllPayments();
        Task<Payment?> GetPaymentById(int id);
        Task AddPayment(Payment payment);
        Task UpdatePayment(Payment payment);
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly ManagementDbContext _context;

        public PaymentRepository(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payment>> GetAllPayments()
        {
            return await _context.Payment.ToListAsync();
        }

        public async Task<Payment?> GetPaymentById(int paymentId)
        {
            return await _context.Payment.FirstOrDefaultAsync(f => f.PaymentID == paymentId);
        }

        public async Task AddPayment(Payment payment)
        {
            await _context.Payment.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePayment(Payment payment)
        {
            _context.Payment.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}
