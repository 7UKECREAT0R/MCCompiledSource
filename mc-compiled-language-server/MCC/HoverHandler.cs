using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace mc_compiled_language_server.MCC;

public class HoverHandler(ILanguageServerFacade router) : IHoverHandler
{
    private readonly ILanguageServerFacade _router = router;

    public HoverRegistrationOptions GetRegistrationOptions(HoverCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                })
        };
    }

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}