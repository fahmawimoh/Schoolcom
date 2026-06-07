using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.WhatsApp.Commands.SendWhatsAppMessage;

public record SendWhatsAppMessageCommand(string Phone, string Message) : IRequest<SendWhatsAppMessageResponse>;

public record SendWhatsAppMessageResponse(string MessageId);

public class SendWhatsAppMessageCommandValidator : AbstractValidator<SendWhatsAppMessageCommand>
{
    public SendWhatsAppMessageCommandValidator()
    {
        RuleFor(x => x.Phone).NotEmpty().MinimumLength(10);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4096);
    }
}

public class SendWhatsAppMessageCommandHandler
    : IRequestHandler<SendWhatsAppMessageCommand, SendWhatsAppMessageResponse>
{
    private readonly IWhatsAppService _whatsApp;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public SendWhatsAppMessageCommandHandler(
        IWhatsAppService whatsApp,
        ICurrentUserService currentUser,
        IAuditService audit)
    {
        _whatsApp = whatsApp;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<SendWhatsAppMessageResponse> Handle(
        SendWhatsAppMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is null || _currentUser.UserId is null)
            throw new UnauthorizedAccessException();

        if (!_currentUser.Roles.Any(r => r is "SchoolAdmin" or "SuperAdmin"))
            throw new UnauthorizedAccessException("Admin access required for WhatsApp");

        var messageId = await _whatsApp.SendMessageAsync(
            _currentUser.TenantId.Value,
            _currentUser.UserId.Value,
            request.Phone,
            request.Message,
            cancellationToken);

        await _audit.LogAsync(
            "WhatsAppSend",
            "WhatsAppMessage",
            null,
            new { request.Phone },
            cancellationToken);

        return new SendWhatsAppMessageResponse(messageId);
    }
}
