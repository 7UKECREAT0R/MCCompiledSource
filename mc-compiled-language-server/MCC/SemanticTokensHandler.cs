using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace mc_compiled_language_server.MCC;

public class SemanticTokensHandler(ILanguageServerFacade router) : ISemanticTokensFullHandler
{
    private readonly ILanguageServerFacade _router = router;

    public SemanticTokensRegistrationOptions GetRegistrationOptions(SemanticTokensCapability capability,
        ClientCapabilities clientCapabilities)
    {
        throw new NotImplementedException();
    }
    
    public Task<SemanticTokens?> Handle(SemanticTokensParams request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}