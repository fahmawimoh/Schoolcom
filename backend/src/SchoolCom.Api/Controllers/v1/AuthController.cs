using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Auth.Commands.Login;
using SchoolCom.Application.Features.Auth.Commands.Logout;
using SchoolCom.Application.Features.Auth.Commands.RequestOtp;
using SchoolCom.Application.Features.Auth.Commands.VerifyOtp;
using SchoolCom.Application.Features.Auth.Queries.GetCurrentUser;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/auth")]
[Asp.Versioning.ApiVersion("1.0")]
[EnableRateLimiting("api")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(new ApiResponse<LoginResponse>(result));
    }

    [HttpPost("otp/request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequest request, CancellationToken ct)
    {
        var channel = request.Channel?.ToLower() == "email"
            ? OtpChannel.Email
            : OtpChannel.Sms;

        var result = await _mediator.Send(new RequestOtpCommand(request.Identifier, channel), ct);
        return Ok(new ApiResponse<RequestOtpResponse>(result));
    }

    [HttpPost("otp/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyOtpCommand(request.Identifier, request.Code), ct);
        return Ok(new ApiResponse<LoginResponse>(result));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), ct);
        return Ok(new ApiResponse<CurrentUserResponse>(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}

public record LoginRequest(string Email, string Password);
public record OtpRequest(string Identifier, string? Channel);
public record OtpVerifyRequest(string Identifier, string Code);
