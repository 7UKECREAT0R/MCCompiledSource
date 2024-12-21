using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace mc_compiled_language_server.MCC;

public class SignatureHelpHandler(ILanguageServerFacade router) : ISignatureHelpHandler
{
    private readonly ILanguageServerFacade _router = router;
    private static readonly string[] TRIGGER_CHARACTERS = "(".ToCharArray().Select(c => c.ToString()).ToArray();
    
    public SignatureHelpRegistrationOptions GetRegistrationOptions(SignatureHelpCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new SignatureHelpRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter()
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                }),
            TriggerCharacters = new Container<string>("(")
        };
    }
    
    public Task<SignatureHelp?> Handle(SignatureHelpParams request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}