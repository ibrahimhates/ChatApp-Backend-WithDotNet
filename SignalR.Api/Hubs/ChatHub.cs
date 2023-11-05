using Microsoft.AspNetCore.SignalR;

namespace SignalR.Api.Hubs;
public class ChatHub : Hub
{
    private readonly string _botUser;
    private readonly IDictionary<string,UserConnection> _connections;
    
    public ChatHub(IDictionary<string,UserConnection> connections)
    {
        _botUser = "Chat Bot";
        _connections = connections;
    }
    
    public async Task SendMessage(string message)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            await Clients.Group(userConnection.Room)
                .SendAsync("ReceiveMessage", userConnection.User, message);
        }
    }

    public async Task SendConnectedUsers(string room)
    {
            var users = _connections
                .Values
                .Where(x => x.Room == room)
                .Select(x => x.User);
            
            await Clients.Group(room)
                .SendAsync("UsersInRoom", users);
    }
    
    public async Task JoinRoom(UserConnection userConnection)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

        _connections[Context.ConnectionId] = userConnection;
        await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser,
            $"{userConnection.User} has joined {userConnection.Room}");

        await SendConnectedUsers(userConnection.Room);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            _connections.Remove(Context.ConnectionId);
            Clients.Group(userConnection.Room)
                .SendAsync("ReceiveMessage", _botUser, 
                    $"{userConnection.User} has left");

            SendConnectedUsers(userConnection.Room);
        }

        return base.OnDisconnectedAsync(exception);
    }
}