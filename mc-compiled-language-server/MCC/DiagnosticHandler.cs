using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace mc_compiled_language_server.MCC;

public class DiagnosticHandler(ILanguageServerFacade router) : IDocumentDiagnosticHandler
{
    private readonly ILanguageServerFacade _router = router;

    public DiagnosticsRegistrationOptions GetRegistrationOptions(DiagnosticClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new DiagnosticsRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter()
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                }),
            WorkspaceDiagnostics = true,
            Identifier = MCCompiledLanguageServer.LANGUAGE_ID
        };
    }
    
    public Task<RelatedDocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}