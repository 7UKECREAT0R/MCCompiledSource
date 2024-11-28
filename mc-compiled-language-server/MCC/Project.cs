using System.Diagnostics;
using System.Text;

namespace mc_compiled_language_server.MCC;

/// <summary>
/// Represents an open MCCompiled project. Each file is its own project.
/// </summary>
public class Project
{
    private const string MinecraftUWP = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
    
    public Project(string file, string code, string? projectName)
    {
        this.fileLocation = file;
        this.fileDirectory = Path.GetDirectoryName(file) ?? "./";
        this.code = [..code.Split('\n').Select(x => new StringBuilder(x.Trim('\r')))];
        this.projectName = projectName ?? Path.GetFileNameWithoutExtension(file);

        if (OperatingSystem.IsWindows())
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string comMojang = Path.Combine(localAppData, "Packages",
                MinecraftUWP, "LocalState", "games", "com.mojang");
            this.outputResourcePack = Path.Combine(comMojang, "development_resource_packs", $"{this.projectName}_RP");
            this.outputBehaviorPack = Path.Combine(comMojang, "development_behavior_packs", $"{this.projectName}_BP");
        }
        else
        {
            // I don't know where the game is stored on other platforms
            this.outputResourcePack = Path.Combine(AppContext.BaseDirectory, "mccompiled", $"{this.projectName}_RP");
            this.outputBehaviorPack = Path.Combine(AppContext.BaseDirectory, "mccompiled", $"{this.projectName}_BP");
        }
    }
    
    /// <summary>
    /// The location of the loaded file.
    /// </summary>
    internal string fileLocation;
    /// <summary>
    /// The directory the loaded file resides in.
    /// </summary>
    internal string fileDirectory;
    /// <summary>
    /// The name of this project; if unspecified, this usually ends up at the stripped file name.
    /// </summary>
    internal string projectName;

    /// <summary>
    /// The current code this project's working with. Represented as individual lines.
    /// </summary>
    private readonly List<StringBuilder> code;
    /// <summary>
    /// Is the current version of <see cref="code"/> processed?
    /// </summary>
    private bool isCodeProcessed = false;
    
    /// <summary>
    /// Updates the code, completely discarding the old code.
    /// </summary>
    /// <param name="newCode"></param>
    public void UpdateCode(string newCode)
    {
        this.code.Clear();
        string[] lines = newCode.Split('\n');

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim('\r');
            this.code.Add(new StringBuilder(trimmedLine));
        }

        this.isCodeProcessed = false;
    }
    /// <summary>
    /// Updates the code's lines, completely discarding the old code.
    /// </summary>
    /// <param name="newCode"></param>
    public void UpdateCode(List<string> newCode)
    {
        this.code.Clear();
        foreach (string line in newCode)
        {
            string trimmedLine = line.Trim('\r');
            this.code.Add(new StringBuilder(trimmedLine));
        }
        
        this.isCodeProcessed = false;
    }
    private void UpdateCodeSection(int line, int startCharacter, int endCharacter, string newCode)
    {
        if (startCharacter > endCharacter)
            (startCharacter, endCharacter) = (endCharacter, startCharacter);
        
        // expand the list if needed
        if (line >= this.code.Count)
        {
            int diff = line - this.code.Count + 1;
            for (int i = 0; i < diff; i++)
                this.code.Add(new StringBuilder());
        }
        
        StringBuilder lineBuilder = this.code[line];
        if (startCharacter > lineBuilder.Length)
            startCharacter = lineBuilder.Length;
        if(endCharacter > lineBuilder.Length)
            endCharacter = lineBuilder.Length;
        
        // remove from startCharacter to endCharacter
        int areaToRemove = endCharacter - startCharacter;
        if (areaToRemove > 0)
            lineBuilder.Remove(startCharacter, areaToRemove);
        
        // insert newCode at startCharacter
        lineBuilder.Insert(startCharacter, newCode);
    }
    public void UpdateCodeSection(OmniSharp.Extensions.LanguageServer.Protocol.Models.Range? range, string codeToChange)
    {
        if (range == null)
        {
            UpdateCode(codeToChange);
            return;
        }

        int startLine = range.Start.Line;
        int endLine = range.End.Line;

        int startCharacter = range.Start.Character;
        int endCharacter = range.End.Character;

        if (startLine == endLine)
        {
            if (startCharacter == endCharacter)
                return; // noop
            UpdateCodeSection(startLine, startCharacter, endCharacter, codeToChange);
        }
        else
        {
            string[] codeToChangeLines = codeToChange.Split('\n');

            Debug.Assert(codeToChangeLines.Length == (endLine - startLine + 1),
                "codeToChange did not match the region given to change.");

            int a = 0;
            UpdateCodeSection(startLine, startCharacter, this.code[startLine].Length, codeToChangeLines[a++]);
            for (int i = startLine + 1; i < endLine; i++)
                UpdateCodeSection(i, 0, this.code[i].Length, codeToChangeLines[a++]);
            UpdateCodeSection(endLine, 0, endCharacter, codeToChangeLines[a]);
        }
    }
    
    /// <summary>
    /// The location where the resource pack will be emitted.
    /// </summary>
    private readonly string outputResourcePack;
    /// <summary>
    /// The location where the behavior pack will be emitted.
    /// </summary>
    private readonly string outputBehaviorPack;
    
    /// <summary>
    /// Processes the code; i.e., linting
    /// </summary>
    internal void Process()
    {
        if (this.isCodeProcessed)
            return;
        
    }
    /// <summary>
    /// Runs the code.
    /// </summary>
    internal void Run()
    {
        
    }
}