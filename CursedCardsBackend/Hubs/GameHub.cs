using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    public async Task JoinRoom(string roomId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

    public async Task LeaveRoom(string roomId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
}
