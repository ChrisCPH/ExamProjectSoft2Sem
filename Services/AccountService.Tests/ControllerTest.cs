using Microsoft.AspNetCore.Mvc;
using Moq;
using AccountService.Controllers;
using AccountService.Models;
using AccountService.Services;
using System.Threading.Tasks;
using Xunit;

public class AccountControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _accountServiceMock = new Mock<IAccountService>();
        _controller = new AccountController(_accountServiceMock.Object);
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnCreatedAtAction_WhenAccountIsValid()
    {
        var account = new Account { AccountID = 1, Email = "test@example.com" };
        _accountServiceMock.Setup(service => service.CreateAccountAsync(account))
            .ReturnsAsync(account);

        var result = await _controller.CreateAccount(account);

        var actionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, actionResult.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnBadRequest_WhenAccountIsNull()
    {
        var result = await _controller.CreateAccount(null!);

        
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, actionResult.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_ShouldReturnOk_WhenAccountExists()
    {
        var account = new Account { AccountID = 1, Email = "test@example.com" };
        _accountServiceMock.Setup(service => service.GetAccountByIdAsync(1))
            .ReturnsAsync(account);

        var result = await _controller.GetAccountById(1);

        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        _accountServiceMock.Setup(service => service.GetAccountByIdAsync(1))
            .ReturnsAsync((Account)null!);

        var result = await _controller.GetAccountById(1);
        
        var actionResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, actionResult.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        var loginRequest = new LoginRequest { Email = "test@example.com", Password = "password" };
        var token = "mockToken";
        _accountServiceMock.Setup(service => service.LoginAsync("test@example.com", "password"))
            .ReturnsAsync(token);

        var result = await _controller.Login(loginRequest);

        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var loginRequest = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };
        _accountServiceMock.Setup(service => service.LoginAsync("test@example.com", "wrongpassword"))
            .ReturnsAsync((string)null!);

        var result = await _controller.Login(loginRequest);
        
        var actionResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, actionResult.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnOk_WhenAccountIsDeleted()
    {
        _accountServiceMock.Setup(service => service.DeleteAccountAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteAccount(1);

        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        _accountServiceMock.Setup(service => service.DeleteAccountAsync(1))
            .ReturnsAsync(false);

        var result = await _controller.DeleteAccount(1);
        
        var actionResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, actionResult.StatusCode);
    }

    [Fact]
    public async Task SetUnavailable_ShouldReturnOk_WhenAccountIsUpdated()
    {
        var account = new Account { AccountID = 1, Email = "test@example.com" };
        _accountServiceMock.Setup(service => service.SetUnavailable(1))
            .ReturnsAsync(account);

        var result = await _controller.SetUnavailable(1);
        
        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public async Task SetAvailable_ShouldReturnOk_WhenAccountIsUpdated()
    {
        var account = new Account { AccountID = 1, Email = "test@example.com" };
        _accountServiceMock.Setup(service => service.SetAvailable(1))
            .ReturnsAsync(account);

        var result = await _controller.SetAvailable(1);

        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public async Task GetDriverForDelivery_ShouldReturnOk_WhenDriverFound()
    {
        _accountServiceMock.Setup(service => service.GetAvailableDriverWithLongestWaitTime())
            .ReturnsAsync(1);

        var result = await _controller.GetDriverForDelivery();
        
        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_ShouldReturnOk_WhenAccountIsUpdated()
    {
        var updatedAccount = new Account { AccountID = 1, Email = "updated@example.com" };
        _accountServiceMock.Setup(service => service.UpdateAccountAsync(1, updatedAccount))
            .ReturnsAsync(updatedAccount);
        
        var result = await _controller.UpdateAccount(1, updatedAccount);
        
        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, actionResult.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        var updatedAccount = new Account { AccountID = 1, Email = "updated@example.com" };
        _accountServiceMock.Setup(service => service.UpdateAccountAsync(1, updatedAccount))
            .ThrowsAsync(new KeyNotFoundException("Account not found"));
        
        var result = await _controller.UpdateAccount(1, updatedAccount);
        
        var actionResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, actionResult.StatusCode);
    }
}
