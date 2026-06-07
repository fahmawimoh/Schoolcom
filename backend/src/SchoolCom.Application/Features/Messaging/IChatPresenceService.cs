namespace SchoolCom.Application.Features.Messaging;

public interface IChatPresenceService
{
    void UserConnected(Guid userId);
    void UserDisconnected(Guid userId);
    bool IsOnline(Guid userId);
}
