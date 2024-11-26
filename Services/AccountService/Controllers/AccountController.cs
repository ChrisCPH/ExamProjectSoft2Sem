using Microsoft.AspNetCore.Mvc;
using AccountService.Models;
using AccountService.Services;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount([FromBody] Account account)
    {
        if (account == null)
        {
            return BadRequest("Account data is required");
        }

        try
        {
            var createdAccount = await _accountService.CreateAccountAsync(account);
            return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.AccountID }, createdAccount);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating account: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(int id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null)
        {
            return NotFound("Account not found");
        }
        return Ok(account);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest("Email and password are required.");
        }

        var token = await _accountService.LoginAsync(loginRequest.Email, loginRequest.Password);

        if (token == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new { Message = "Login successful", Token = token });
    }

    [HttpPost("linkRestaurant")]
    public async Task<IActionResult> LinkRestaurant([FromQuery] string name, [FromQuery] string address, [FromQuery] string category)
    {
        try
        {
            await _accountService.LinkRestaurant(name, address, category);
            return Ok(new { Message = "Restaurant linked successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            bool result = await _accountService.DeleteAccountAsync(id);

            if (result)
            {
                return Ok("Account deleted successfully.");
            }
            else
            {
                return NotFound("Account not found.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}