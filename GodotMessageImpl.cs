using GodotTools.IdeMessaging;


class GNPMessageHandler : IMessageHandler
{
    public async Task<MessageContent> HandleRequest(Peer peer, string id, MessageContent content, ILogger logger)
    {
        logger.LogInfo($"Receive Godot ID={id}");
        return await Task.FromResult(new MessageContent(MessageStatus.Ok, string.Empty));
    }
}

class GNPLogger : ILogger
{
    public void LogDebug(string message)
    {
        Console.Error.WriteLine($"[Godot-Node-Path] DEBUG: {message}");
    }

    public void LogError(string message)
    {
        Console.Error.WriteLine($"[Godot-Node-Path] ERROR: {message}");
    }

    public void LogError(string message, Exception e)
    {
        Console.Error.WriteLine($"[Godot-Node-Path] ERROR: {message}, with exception: {e}");
    }

    public void LogInfo(string message)
    {
        Console.Error.WriteLine($"[Godot-Node-Path] INFO: {message}");
    }

    public void LogWarning(string message)
    {
        Console.Error.WriteLine($"[Godot-Node-Path] WARN: {message}");
    }
}

