using GodotTools.IdeMessaging;


class GodotClient(string identifier, string projectPath, IMessageHandler messageHandler, ILogger logger)
{
    private readonly string _Client_IDENTIFIER = identifier;
    private string _projectPath = projectPath;
    private IMessageHandler _messageHandler = messageHandler;
    private ILogger _logger = logger;

    private Client _client;

    async public Task StartAsync() {
        _client = new Client(_Client_IDENTIFIER, _projectPath, _messageHandler, _logger);
        _client.Start();
        await _client.AwaitConnected();
        return;
    }
}

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

