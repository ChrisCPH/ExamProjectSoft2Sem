using ManagementService.Models;
using ManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeeController : ControllerBase
    {
        private readonly IFeeService _feeService;

        public FeeController(IFeeService feeService)
        {
            _feeService = feeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFees()
        {
            var fees = await _feeService.GetAllFees();
            if (fees == null)
                return NotFound(new { Message = "No fees found" });
            return Ok(fees);
        }

        [HttpGet("{feeId}")]
        public async Task<IActionResult> GetFeeById(int feeId)
        {
            var fee = await _feeService.GetFeeById(feeId);
            if (fee == null)
                return NotFound(new { Message = "No fee with id: " + feeId });

            return Ok(fee);
        }

        [HttpPost("{restaurantId}")]
        public async Task<IActionResult> CalculateFee(int restaurantId, [FromBody] DateRequest request)
        {
            var fee = await _feeService.CalculateAndSaveFeeAsync(restaurantId, request.StartDate, request.EndDate);
            return CreatedAtAction(nameof(CalculateFee), new { feeId = fee.FeeID }, fee);
        }

        [HttpPut("{feeId}")]
        public async Task<IActionResult> UpdateFeeStatus(int feeId)
        {
            var result = await _feeService.UpdateFeeStatus(feeId);

            if (result)
            {
                return Ok(new { message = "Fee updated successfully." });
            }
            return NotFound(new { message = "Fee not found." });
        }
    }
}