using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace mc_compiled_language_server.MCC;

public class CompletionHandler(ILanguageServerFacade router) : ICompletionHandler
{
    private readonly ILanguageServerFacade _router = router;
    private static readonly string[] TRIGGER_CHARACTERS =
        "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890$"
            .ToCharArray().Select(c => c.ToString()).ToArray();
    
    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new CompletionRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter()
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                }),
            TriggerCharacters = new Container<string>(TRIGGER_CHARACTERS)
        };
    }
    
    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}