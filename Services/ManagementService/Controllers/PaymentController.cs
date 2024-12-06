using ManagementService.Models;
using ManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPayments();
            if (payments == null)
                return NotFound(new { Message = "No payments found" });
            return Ok(payments);
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentById(int paymentId)
        {
            var payment = await _paymentService.GetPaymentById(paymentId);
            if (payment == null)
                return NotFound(new { Message = "No payment with id: " + paymentId });

            return Ok(payment);
        }

        [HttpPost("{driverId}")]
        public async Task<IActionResult> CalculatePayment(int driverId, [FromBody] DateRequest request)
        {
            var payment = await _paymentService.CalculateAndSavePaymentAsync(driverId, request.StartDate, request.EndDate);
            return CreatedAtAction(nameof(CalculatePayment), new { paymentId = payment.PaymentID }, payment);
        }

        [HttpPut("{paymentId}")]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId)
        {
            var result = await _paymentService.UpdatePaymentStatus(paymentId);

            if (result)
            {
                return Ok(new { message = "Payment updated successfully." });
            }
            return NotFound(new { message = "Payment not found." });
        }
    }
}