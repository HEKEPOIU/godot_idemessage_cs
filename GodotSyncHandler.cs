using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using GodotTools.IdeMessaging.Requests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using GodotTools.IdeMessaging;

public class DocumentStore(ILogger logger)
{
    private readonly Dictionary<string, string> _docs = [];
    private readonly ILogger _logger = logger;

    public void Update(string uri, string text) => _docs[uri] = text;
    public void Remove(string uri) => _docs.Remove(uri);
    public string? Get(string uri) => _docs.GetValueOrDefault(uri);


    public CodeCompletionRequest.CompletionKind? GetCompletionKind(string uri, int line, int offset)
    {
        var source = Get(uri);
        if (source == null) return null;

        var tree = CSharpSyntaxTree.ParseText(source);
        var sourceText = tree.GetText();
        int position = sourceText.Lines[line].Start + offset;
        var token = tree.GetRoot().FindToken(position);

        if (token.Parent is LiteralExpressionSyntax stringLiteral && stringLiteral.IsKind(SyntaxKind.StringLiteralExpression))
        {
            if (stringLiteral.Parent is ArgumentSyntax argument &&
                argument.Parent is ArgumentListSyntax argumentList &&
                argumentList.Parent is InvocationExpressionSyntax invocation)
            {
                string methodName = GetMethodName(invocation.Expression);

                _logger.LogInfo($"Detected Method Name: {methodName}");

                return MapCompletionKind(methodName);
            }
        }

        return null;
    }

    private static CodeCompletionRequest.CompletionKind? MapCompletionKind(string methodName)
    {
        return methodName switch
        {
            "GetNode" or "GetNodeOrNull" or "GetNodeGeneric" or "HasNode" or
            "GetPathTo" or "FindChild" or "FindChildren" or "GetChild" or
            "RemoveChild" or "MoveChild" => CodeCompletionRequest.CompletionKind.NodePaths,

            "ChangeSceneToFile" or "Instantiate" or "Instance" => CodeCompletionRequest.CompletionKind.ScenePaths,

            "Load" or "Preload" or "Exists" or
            "LoadThreadedRequest" or "SetScript" => CodeCompletionRequest.CompletionKind.ResourcePaths,

            "IsActionPressed" or "IsActionJustPressed" or "IsActionJustReleased" or
            "IsActionReleased" or "GetActionStrength" or "GetActionRawStrength" or
            "ActionPress" or "ActionRelease" or "IsActionPressed" or
            "HasAction" or "GetVector" or "GetAxis" => CodeCompletionRequest.CompletionKind.InputActions,

            "EmitSignal" or "Connect" or "Disconnect" or "IsConnected" or
            "HasSignal" => CodeCompletionRequest.CompletionKind.Signals,

            "SetShaderParameter" or "GetShaderParameter" => CodeCompletionRequest.CompletionKind.ShaderParams,

            "AddThemeFontOverride" or "GetThemeFont" or "HasThemeFont" or
            "HasThemeFontOverride" => CodeCompletionRequest.CompletionKind.ThemeFonts,

            "AddThemeColorOverride" or "GetThemeColor" or "HasThemeColor" or
            "HasThemeColorOverride" => CodeCompletionRequest.CompletionKind.ThemeColors,

            "AddThemeConstantOverride" or "GetThemeConstant" or "HasThemeConstant" or
            "HasThemeConstantOverride" => CodeCompletionRequest.CompletionKind.ThemeConstants,

            "AddThemeStyleboxOverride" or "GetThemeStylebox" or "HasThemeStylebox" or
            "HasThemeStyleboxOverride" => CodeCompletionRequest.CompletionKind.ThemeStyles,
            _ => null
        };
    }
    private static string GetMethodName(ExpressionSyntax expression)
    {
        if (expression is IdentifierNameSyntax id)
        {
            return id.Identifier.Text;
        }
        else if (expression is GenericNameSyntax generic)
        {
            return generic.Identifier.Text;
        }
        else if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            return GetMethodName(memberAccess.Name);
        }

        return string.Empty;
    }
}

public class GodotSyncHandler(DocumentStore store) : ITextDocumentSyncHandler
{
    public static TextDocumentSyncKind Change => TextDocumentSyncKind.Full;

    public TextDocumentChangeRegistrationOptions GetRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities) => new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("csharp"),
            SyncKind = Change
        };

    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken ct)
    {
        store.Update(request.TextDocument.Uri.ToString(), request.TextDocument.Text);
        return Unit.Task;
    }

    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken ct)
    {
        var text = request.ContentChanges.LastOrDefault()?.Text;
        if (text is not null)
            store.Update(request.TextDocument.Uri.ToString(), text);
        return Unit.Task;
    }

    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken ct)
    {
        store.Remove(request.TextDocument.Uri.ToString());
        return Unit.Task;
    }

    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken ct)
        => Unit.Task;

    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        => new(uri, "csharp");

    TextDocumentOpenRegistrationOptions
    IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new() { DocumentSelector = TextDocumentSelector.ForLanguage("csharp") };

    }

    TextDocumentCloseRegistrationOptions
    IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new() { DocumentSelector = TextDocumentSelector.ForLanguage("csharp") };
    }

    TextDocumentSaveRegistrationOptions
    IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new() { DocumentSelector = TextDocumentSelector.ForLanguage("csharp") };
    }
}

