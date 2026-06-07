using System.Collections.Concurrent;
using SchoolCom.Application.Features.Messaging;

namespace SchoolCom.Api.Services;

public sealed class ChatPresenceService : IChatPresenceService
{
    private readonly ConcurrentDictionary<Guid, int> _connections = new();

    public void UserConnected(Guid userId) =>
        _connections.AddOrUpdate(userId, 1, (_, count) => count + 1);

    public void UserDisconnected(Guid userId)
    {
        if (!_connections.TryGetValue(userId, out var count))
            return;

        if (count <= 1)
            _connections.TryRemove(userId, out _);
        else
            _connections[userId] = count - 1;
    }

    public bool IsOnline(Guid userId) =>
        _connections.ContainsKey(userId);
}
