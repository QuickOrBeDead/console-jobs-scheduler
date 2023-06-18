namespace ConsoleJobScheduler.WindowsService.Hubs;

using Microsoft.AspNetCore.SignalR;

public class JobRunConsoleHub : Hub
{
    public async Task AddToGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
}