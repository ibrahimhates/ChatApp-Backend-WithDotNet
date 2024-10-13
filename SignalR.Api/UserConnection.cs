namespace SignalR.Api;

public class UserConnection
{
    public string User { get; set; }
    public Guid? RoomId { get; set; }
    public string? RoomName { get; set; }
}