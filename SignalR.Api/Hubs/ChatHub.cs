using Microsoft.AspNetCore.SignalR;

namespace SignalR.Api.Hubs;

public class ChatHub : Hub
{
    private readonly string _botUser;
    private readonly IDictionary<string, UserConnection> _connections;
    private readonly IDictionary<Guid, string> _rooms;

    public ChatHub(IDictionary<string, UserConnection> connections,IDictionary<Guid, string> rooms)
    {
        _botUser = "Chat Bot";
        _connections = connections;
        _rooms = rooms;
    }

    public async Task SendMessage(string message)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            await Clients.Group(userConnection.RoomId.Value.ToString())
                .SendAsync("ReceiveMessage", userConnection.User, message);
        }
    }

    public async Task SendAudioToRoom(string audio)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
        {
            var users = _connections
                .Where(x => x.Key != Context.ConnectionId && x.Value.RoomId == userConnection.RoomId)
                .Select(x => x.Key);
            
            await Clients.Clients(users)
                .SendAsync("ReceiveAudio", audio);
        }
        else
        {
            Context.Abort();
        }
    }
    
    private async Task SendConnectedUsers(Guid room)
    {
        var users = _connections
            .Values
            .Where(x => x.RoomId == room)
            .Select(x => x.User);

        await Clients.Group(room.ToString())
            .SendAsync("UsersInRoom", users);
    }

    public async Task JoinRoom(UserConnection userConnection)
    {
        if (userConnection.RoomId.HasValue)
        {
            if(_rooms.TryGetValue(userConnection.RoomId.Value, out var roomName))
                userConnection.RoomName = roomName;
            else
            {
                Context.Abort();
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.RoomId.Value.ToString());
        }
        else if(userConnection.RoomName != null)
        {
            var roomID = await AddRoom(userConnection.RoomName);
            userConnection.RoomId = roomID;
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.RoomId.Value.ToString());
        }
        else
        {
            Context.Abort();
            return;
        }

        _connections[Context.ConnectionId] = userConnection;
        await Clients.Group(userConnection.RoomId.Value.ToString()).SendAsync("ReceiveMessage", _botUser,
            $"{userConnection.User} has joined {_rooms[userConnection.RoomId.Value]}");
        
        await Clients.Caller.SendAsync("UserInfo", userConnection);

        await SendConnectedUsers(userConnection.RoomId.Value);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.Remove(Context.ConnectionId, out var userConnection))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userConnection.RoomId.ToString());
            await Clients.Group(userConnection.RoomId.ToString())
                .SendAsync("ReceiveMessage", _botUser,
                    $"{userConnection.User} has left");

            await SendConnectedUsers(userConnection.RoomId.Value);
        }
    }

    private async Task<Guid> AddRoom(string roomName)
    {
        if(_rooms.Select(x => x.Value).Contains(roomName))
            return _rooms.First(x => x.Value == roomName).Key;
        var newRoom = Guid.NewGuid();
        _rooms.Add(newRoom, roomName);
        await Clients.All.SendAsync("AllRooms", _rooms.ToArray(), Context.ConnectionAborted);

        return newRoom;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).SendAsync("AllRooms", _rooms.ToArray(), Context.ConnectionAborted);
    }
}