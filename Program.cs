using OmniSharp.Extensions.LanguageServer.Server;
using GodotTools.IdeMessaging;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using GodotTools.IdeMessaging.Requests;
using Microsoft.Extensions.DependencyInjection;


var logger = new GNPLogger();

string projectPath = Directory.GetCurrentDirectory();

if (!File.Exists(Path.Combine(projectPath, "project.godot")))
{
    logger.LogError("No project.godot found in current directory");
    return;
}
logger.LogInfo("Project.godot found");

var messageHandler = new GNPMessageHandler();
var documentStore = new DocumentStore(logger);

var client = new Client("godot-node-path", projectPath, messageHandler, logger);
client.Start();
await client.AwaitConnected();

var server = await LanguageServer.From(options =>
    options
        .WithOutput(Console.OpenStandardOutput())
        .WithInput(Console.OpenStandardInput())
        .WithServices(services =>
        {
            services.AddSingleton(client);
            services.AddSingleton(documentStore);
            services.AddSingleton<ILogger>(logger);
        })
        .WithHandler<GodotNodePathHandler>()
        .WithHandler<GodotSyncHandler>()
);
await server.WaitForExit;


class GodotNodePathHandler(Client godotClient, DocumentStore store, ILogger logger) : ICompletionHandler
{
    private readonly Client _godotClient = godotClient;
    private readonly ILogger _logger = logger;
    private readonly DocumentStore _store = store;

    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("csharp"),
            TriggerCharacters = new Container<string>("\"", "$")
        };
    }
    public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        // Check Current Position to determine if we should trigger completion and CompletionKind
        var kind = _store.GetCompletionKind(request.TextDocument.Uri.ToString(), request.Position.Line, request.Position.Character);
        if (kind == null)
        {
            _logger.LogInfo("No completion kind found");
            return new CompletionList();
        }
        var godotRequest = new CodeCompletionRequest
        {
            Kind = (CodeCompletionRequest.CompletionKind)kind,
            ScriptFile = request.TextDocument.Uri.GetFileSystemPath(),
        };

        var response = await _godotClient.SendRequest<CodeCompletionResponse>(godotRequest);

        if (response == null || response.Status != MessageStatus.Ok)
        {
            _logger.LogInfo($"Completion request failed : {response}, with Status: {response?.Status}");
            return new CompletionList();
        }

        var items = response.Suggestions.Select(s => new CompletionItem
        {
            Label = s,
            InsertText = s.TrimStart('\"'),
            Kind = CompletionItemKind.Text,
            Detail = "Godot Node Path",
        });

        return new CompletionList(items);
    }
}
