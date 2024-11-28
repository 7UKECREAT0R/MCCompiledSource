using System.Collections.Concurrent;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace mc_compiled_language_server.MCC;

public class SyncHandler(ILanguageServerFacade router) : ITextDocumentSyncHandler
{
    private readonly ILanguageServerFacade _router = router;
    private const TextDocumentSyncKind DOCUMENT_SYNC_KIND = TextDocumentSyncKind.Incremental;
    
    TextDocumentChangeRegistrationOptions
        IRegistration<TextDocumentChangeRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(
            TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentChangeRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter()
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                }),
            SyncKind = DOCUMENT_SYNC_KIND
        };
    }
    TextDocumentOpenRegistrationOptions
        IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>
        .GetRegistrationOptions(TextSynchronizationCapability capability,
            ClientCapabilities clientCapabilities)
    {
        return new TextDocumentOpenRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter()
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                })
        };
    }
    TextDocumentCloseRegistrationOptions
        IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>
        .GetRegistrationOptions(TextSynchronizationCapability capability,
            ClientCapabilities clientCapabilities)
    {
        return new TextDocumentCloseRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter()
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                })
        };
    }
    TextDocumentSaveRegistrationOptions
        IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>
        .GetRegistrationOptions(TextSynchronizationCapability capability,
            ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSaveRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter()
                {
                    Pattern = MCCompiledLanguageServer.EXTENSION_PATTERN,
                    Language = MCCompiledLanguageServer.LANGUAGE_ID
                }),
            IncludeText = true
        };
    }

    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        DocumentUri document = request.TextDocument.Uri;
        string documentURL = document.GetFileSystemPath();

        if (!MCCompiledLanguageServer.PROJECTS.TryGetValue(documentURL, out Project? project))
            return Unit.Task;

        foreach (TextDocumentContentChangeEvent change in request.ContentChanges)
        {
            Range? range = change.Range;
            string text = change.Text;
            
            project.UpdateCodeSection(range, text);
        }
        return Unit.Task;
    }
    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        TextDocumentItem document = request.TextDocument;
        
        if(document.LanguageId != MCCompiledLanguageServer.LANGUAGE_ID)
            return Unit.Task;

        try
        {
            string code = document.Text;
            string uri = document.Uri.GetFileSystemPath();
            if (MCCompiledLanguageServer.PROJECTS.ContainsKey(uri))
            {
                Console.Error.WriteLine($"Already has project {uri} open.");
                return Unit.Task;
            }

            MCCompiledLanguageServer.PROJECTS[uri] = new Project(uri, code, Path.GetFileNameWithoutExtension(uri));
            return Unit.Task;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error opening document {document.Uri.GetFileSystemPath()}: {e.Message}");
            return Unit.Task;
        }
    }
    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        TextDocumentIdentifier document = request.TextDocument;
        MCCompiledLanguageServer.PROJECTS.Remove(document.Uri.GetFileSystemPath(), out Project? _);
        return Unit.Task;
    }
    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        // handle document saving (do we even do this?)
        return Unit.Task;
    }

    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, MCCompiledLanguageServer.LANGUAGE_ID);
    }
}