using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PrintingTools.Infrastructure.Services;

public static class PrintStatusWebSocketHandler
{
    private static readonly Dictionary<string, WebSocket> _connections = new();
    
    public static async Task HandleAsync(HttpContext context, WebSocket webSocket)
    {
        var userId = context.User.Claims
            .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.PolicyViolation,
                "Unauthorized",
                CancellationToken.None);
            return;
        }
        
        _connections[userId] = webSocket;
        
        var buffer = new byte[1024 * 4];
        
        try
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), 
                CancellationToken.None);
            
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message == "ping")
                    {
                        var pong = Encoding.UTF8.GetBytes("pong");
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(pong),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
                
                result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    CancellationToken.None);
            }
            
            await webSocket.CloseAsync(
                result.CloseStatus.Value,
                result.CloseStatusDescription,
                CancellationToken.None);
        }
        finally
        {
            _connections.Remove(userId);
        }
    }
    
    public static async Task NotifyPrintStatusAsync(Guid userId, object status)
    {
        if (_connections.TryGetValue(userId.ToString(), out var socket))
        {
            if (socket.State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(status, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var bytes = Encoding.UTF8.GetBytes(json);
                
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
    }
    
    public static async Task BroadcastAsync(object message)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var bytes = Encoding.UTF8.GetBytes(json);
        
        var tasks = new List<Task>();
        
        foreach (var connection in _connections.Values)
        {
            if (connection.State == WebSocketState.Open)
            {
                tasks.Add(connection.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None));
            }
        }
        
        await Task.WhenAll(tasks);
    }
}