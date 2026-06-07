using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.Auth.Commands.Login;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Application.Features.Auth.Commands.RequestOtp;

public record RequestOtpCommand(string Identifier, OtpChannel Channel) : IRequest<RequestOtpResponse>;

public record RequestOtpResponse(string Message, int ExpiresInSeconds);

public class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty();
    }
}

public class RequestOtpCommandHandler : IRequestHandler<RequestOtpCommand, RequestOtpResponse>
{
    private readonly IOtpService _otpService;
    private readonly IUserRepository _users;

    public RequestOtpCommandHandler(IOtpService otpService, IUserRepository users)
    {
        _otpService = otpService;
        _users = users;
    }

    public async Task<RequestOtpResponse> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var user = request.Channel == OtpChannel.Sms
            ? await _users.GetByPhoneAsync(request.Identifier, cancellationToken)
            : await _users.GetByEmailAsync(request.Identifier, cancellationToken);

        if (user is null)
            throw new AuthException("AUTH_USER_NOT_FOUND", "auth.userNotFound");

        await _otpService.GenerateAndStoreAsync(request.Identifier, request.Channel, cancellationToken);

        return new RequestOtpResponse("Verification code sent", 300);
    }
}
