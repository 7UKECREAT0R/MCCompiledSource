using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.Commands;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     The <see cref="WorkspaceManager" /> is an all-in-one class for compiling, analyzing, and overall running MCCompiled
///     files.
///     Its goal is to juggle the regular compilation via command line,
///     linting/code analysis for the language server, and file/dependency management.
/// </summary>
public class WorkspaceManager
{
    /// <summary>
    ///     A mapping of absolute file paths and the number of milliseconds the tokenization/assembly of the file took.
    /// </summary>
    private readonly Dictionary<string, long> assemblyTimesMs = new();

    /// <summary>
    ///     A collection of absolute file paths that are currently open.
    /// </summary>
    private readonly HashSet<string> openFiles = [];
    /// <summary>
    ///     A mapping of absolute file paths and parsed file contents.
    /// </summary>
    private readonly Dictionary<string, Statement[]> openFilesParsed = new();
    /// <summary>
    ///     A mapping of absolute file paths and file contents.
    /// </summary>
    private readonly Dictionary<string, string> openFilesRaw = new();

    private static string EnsureAbsolute(string path)
    {
        return Path.IsPathFullyQualified(path) ? path : Path.GetFullPath(path);
    }
    [PublicAPI]
    public bool IsFileOpen(string path)
    {
        path = EnsureAbsolute(path);
        return this.openFiles.Contains(path);
    }

    /// <summary>
    ///     Creates an <see cref="Executor" /> instance for the specified file using the tokenized and parsed content of the
    ///     file.
    /// </summary>
    /// <param name="file">
    ///     The path to the file for which the <see cref="Executor" /> should be created. This file must already be tokenized
    ///     and
    ///     parsed in the workspace, or it will be opened if <paramref name="openFileIfNotOpen" /> is set to <c>true</c>.
    /// </param>
    /// <param name="openFileIfNotOpen">
    ///     A flag indicating whether the method should attempt to open and parse the specified file if it is not already open
    ///     in
    ///     the workspace. If <c>true</c>, the file will be automatically opened using
    ///     <see cref="WorkspaceManager.OpenFile(string)" />.
    ///     If <c>false</c>, an exception will be thrown if the file is not already open.
    /// </param>
    /// <returns>
    ///     An <see cref="Executor" /> instance initialized with the parsed statements of the specified file
    ///     and configured with input PPVs from the current global context.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown if the specified file is not found in the collection of open and parsed files, and
    ///     <paramref name="openFileIfNotOpen" /> is <c>false</c>.
    /// </exception>
    [PublicAPI]
    public Executor CreateExecutorForFile(string file, bool openFileIfNotOpen)
    {
        if (openFileIfNotOpen)
            OpenFileIfNotOpen(file);
        if (!this.openFilesParsed.TryGetValue(file, out Statement[] code))
            throw new KeyNotFoundException(
                $"File \"{file}\" was not open but an executor was requested for it. Consider passing `true` to `openFileIfNotOpen`, or opening the file manually ahead of time.");
        return new Executor(code, this)
            .SetPPVsFromInput(GlobalContext.Current.inputPPVs);
    }
    /// <summary>
    ///     Retrieves the parsed <see cref="Statement" /> array for a specified file.
    ///     If the file is not already open, it will be optionally opened and parsed based on the value of
    ///     <paramref name="openFileIfNotOpen" />.
    /// </summary>
    /// <param name="file">
    ///     The path to the file for which the parsed <see cref="Statement" /> array is requested.
    ///     This file must be opened and parsed in the workspace. If it is not open, and
    ///     <paramref name="openFileIfNotOpen" /> is <c>true</c>, the file will be automatically opened
    ///     using <see cref="WorkspaceManager.OpenFile(string)" />.
    /// </param>
    /// <param name="openFileIfNotOpen">
    ///     A flag indicating whether the method should attempt to open and parse the specified file if it is not
    ///     already open in the workspace. If <c>true</c>, the file is opened and its statements are parsed.
    ///     If <c>false</c>, an exception will be thrown if the file is not open.
    /// </param>
    /// <returns>
    ///     An array of <see cref="Statement" /> instances representing the parsed content of the specified file.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown if the specified file is not open and <paramref name="openFileIfNotOpen" /> is <c>false</c>.
    /// </exception>
    [PublicAPI]
    public Statement[] GetParsedStatements(string file, bool openFileIfNotOpen)
    {
        if (openFileIfNotOpen)
            OpenFileIfNotOpen(file);
        if (this.openFilesParsed.TryGetValue(file, out Statement[] statements))
            return statements;
        throw new KeyNotFoundException(
            $"File \"{file}\" was not open but the parsed statements were requested. Consider passing `true` to `openFileIfNotOpen`, or opening the file manually ahead of time.");
    }
    /// <summary>
    ///     Opens a string of code as though it were a file, tokenizing, assembling, and storing it for further compilation or
    ///     execution.
    ///     The content is parsed into tokens and statements, and performance diagnostic information is gathered if debugging
    ///     is enabled
    ///     in <see cref="GlobalContext" />.
    /// </summary>
    /// <param name="fileName">
    ///     The name of the "virtual file" representing this code. This name serves to identify the code within the workspace
    ///     and is used for error reporting during tokenization and assembly.
    /// </param>
    /// <param name="code">
    ///     The raw string content of the code to be processed. This content is tokenized, parsed into statements, and stored
    ///     within
    ///     the workspace to simulate opening a file.
    /// </param>
    [PublicAPI]
    public void OpenCodeAsFile(string fileName, string code)
    {
        // tokenize/assemble content
        var stopwatch = Stopwatch.StartNew();
        Token[] tokens = new Tokenizer(code).Tokenize( /* only included for errors: */ fileName);
        Statement[] statements = Assembler.AssembleTokens(tokens, fileName);
        stopwatch.Stop();

        // debug stuff
        if (GlobalContext.Debug)
        {
            Console.WriteLine("Loaded and parsed file \"{0}\"", fileName);
            Console.WriteLine("\tDetailed overview of tokenization results:");
            Console.WriteLine(string.Join("", from t in tokens select t.DebugString()));
            Console.WriteLine();
            Console.WriteLine("\tReconstruction of the original code using parsed tokens:");
            Console.WriteLine(string.Join(" ", from t in tokens select t.AsString()));
            Console.WriteLine();
            Console.WriteLine("\tDetailed overview of assembly results:");
            Console.WriteLine(string.Join("\n", from s in statements select s.ToString()));
            Console.WriteLine();
        }

        // got through without exception
        this.openFilesRaw[fileName] = code;
        this.openFilesParsed[fileName] = statements;
        this.assemblyTimesMs[fileName] = stopwatch.ElapsedMilliseconds;
        this.openFiles.Add(fileName);
    }

    /// <summary>
    ///     Compiles a specified file by executing its tokenized contents, optionally linting during execution.
    ///     Collects performance diagnostics and completes the emission process after execution.
    /// </summary>
    /// <param name="file">
    ///     The path to the file to be compiled.
    /// </param>
    /// <param name="lint">
    ///     A boolean flag indicating whether linting should be enabled during the compilation process. If set to true,
    ///     linting-specific optimizations will be enabled and no files will be written.
    /// </param>
    /// <param name="openFileIfNotOpen">
    ///     Should the given file be opened if it's not yet open?
    ///     If <c>false</c>, the compiler will just trust your word that it's open.
    /// </param>
    /// <param name="resultEmission">
    ///     An <see cref="Emission" /> object that will hold the results of the compilation.
    ///     The parameter is provided as an <c>out</c>, meaning the method will initialize and populate
    ///     this parameter during its execution in case of an error occurring.
    /// </param>
    [PublicAPI]
    public void CompileFile(string file,
        bool lint,
        bool openFileIfNotOpen,
        out Emission resultEmission)
    {
        if (openFileIfNotOpen)
            OpenFileIfNotOpen(file);

        long tokenizationMilliseconds = this.assemblyTimesMs[file];
        var stopwatch = Stopwatch.StartNew();

        // build an executor containing the file's tokenized contents
        Executor executor = CreateExecutorForFile(file, openFileIfNotOpen);
        executor.Execute(lint, out resultEmission);

        stopwatch.Stop();
        long executionMilliseconds = stopwatch.ElapsedMilliseconds;
        long totalMilliseconds = tokenizationMilliseconds + executionMilliseconds;
        float totalSeconds = totalMilliseconds / 1000F;
        float tokenizationSeconds = tokenizationMilliseconds / 1000F;
        float executionSeconds = executionMilliseconds / 1000F;

        Console.WriteLine(GlobalContext.Debug ? "Compiled in {0} seconds. Breakdown:" : "Compiled in {0} seconds.",
            totalSeconds);

        if (GlobalContext.Debug)
        {
            Console.WriteLine("\t\tTokenization took {0} seconds.", tokenizationSeconds);
            Console.WriteLine("\t\tExecution took {0} seconds.", executionSeconds);
            Console.WriteLine();
        }

        resultEmission.Complete();
    }

    public Exception OpenCodeAsFileAndCaptureExceptions(string fileName, string code)
    {
        try
        {
            OpenCodeAsFile(fileName, code);
            return null; // no exceptions
        }
        catch (TokenizerException exc)
        {
            return exc;
        }
    }
    /// <summary>
    ///     Compiles a specified file by executing its tokenized contents, optionally linting during execution.
    ///     Collects performance diagnostics and completes the emission process after execution. If an exception occurs,
    ///     it will not be thrown, but rather be returned by this method as an object. It can then be processed as needed.
    /// </summary>
    /// <param name="file">
    ///     The path to the file to be compiled.
    /// </param>
    /// <param name="lint">
    ///     A boolean flag indicating whether linting should be enabled during the compilation process. If set to true,
    ///     linting-specific optimizations will be enabled and no files will be written.
    /// </param>
    /// <param name="catchUnmanagedExceptions">Should this method catch unmanaged exceptions as well?</param>
    /// <param name="openFileIfNotOpen">
    ///     Should the given file be opened if it's not yet open? If <c>false</c>, the compiler
    ///     will just trust your word that it's open.
    /// </param>
    /// <param name="resultEmission">
    ///     An <see cref="Emission" /> object that will hold the results of the compilation.
    ///     The parameter is provided as an <c>out</c>, meaning the method will initialize and populate
    ///     this parameter during its execution in case of an error occurring. If the method returns a non-null value,
    ///     the <see cref="Emission" /> will be partially populated, but may be incomplete.
    /// </param>
    [PublicAPI]
    public Exception CompileFileAndCaptureExceptions(string file,
        bool lint,
        bool catchUnmanagedExceptions,
        bool openFileIfNotOpen,
        out Emission resultEmission)
    {
        resultEmission = null;

        try
        {
            CompileFile(file, lint, openFileIfNotOpen, out resultEmission);
            return null; // no exceptions
        }
        catch (TokenizerException exc)
        {
            return exc;
        }
        catch (StatementException exc)
        {
            return exc;
        }
        catch (FeederException exc)
        {
            return exc;
        }
        catch (Exception exc) when (catchUnmanagedExceptions)
        {
            return exc;
        }
        finally
        {
            resultEmission?.Complete();
        }
    }

    public bool OpenCodeAsFileWithSimpleErrorHandler(string fileName, string code, bool suppressConsoleOutput)
    {
        try
        {
            OpenCodeAsFile(fileName, code);
            return true;
        }
        catch (TokenizerException e)
        {
            if (GlobalContext.Debug && Debugger.IsAttached)
                throw;
            if (suppressConsoleOutput)
                return false;

            int[] _lines = e.lines;
            string lines = string.Join(", ", _lines.Select(line => line < 0 ? "??" : line.ToString()));
            string fileNameShort = Path.GetFileName(e.file);

            string message = e.Message;
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Problem encountered during tokenization of file:\n" +
                                    $"\t{fileNameShort}:{lines} -- {message}\n\nTokenization cannot be continued.");
            Console.ForegroundColor = oldColor;
            return false;
        }
    }
    /// <summary>
    ///     Compiles a specified file by executing its tokenized contents, optionally linting during execution.
    ///     Collects performance diagnostics and completes the emission process after execution. Includes simple behavior
    ///     to capture compiler-generated exceptions and output them to the console.
    /// </summary>
    /// <param name="file">
    ///     The path to the file to be compiled.
    /// </param>
    /// <param name="lint">
    ///     A boolean flag indicating whether linting should be enabled during the compilation process. If set to true,
    ///     linting-specific optimizations will be enabled and no files will be written.
    /// </param>
    /// <param name="suppressConsoleOutput">
    ///     A boolean flag indicating whether error messages should be suppressed from being output to the console. If set to
    ///     true,
    ///     diagnostic errors will not be displayed in the console upon failures.
    /// </param>
    /// <param name="openFileIfNotOpen">
    ///     Should the given file be opened if it's not yet open? If <c>false</c>, the compiler
    ///     will just trust your word that it's open.
    /// </param>
    /// <param name="resultEmission">
    ///     An <see cref="Emission" /> object that will hold the results of the compilation.
    ///     The parameter is provided as an <c>out</c>, meaning the method will initialize and populate
    ///     this parameter during its execution in case of an error occurring.
    /// </param>
    /// <returns>If the compilation was successful.</returns>
    [PublicAPI]
    public bool CompileFileWithSimpleErrorHandler(string file,
        bool lint,
        bool suppressConsoleOutput,
        bool openFileIfNotOpen,
        out Emission resultEmission)
    {
        resultEmission = null;

        try
        {
            CompileFile(file, lint, openFileIfNotOpen, out resultEmission);
            return true;
        }
        catch (TokenizerException exc)
        {
            if (GlobalContext.Debug && Debugger.IsAttached)
                throw;
            if (suppressConsoleOutput)
                return false;

            int[] _lines = exc.lines;
            string lines = string.Join(", ", _lines.Select(line => line < 0 ? "??" : line.ToString()));
            string fileNameShort = Path.GetFileName(exc.file);

            string message = exc.Message;
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Problem encountered during tokenization of file:\n" +
                                    $"\t{fileNameShort}:{lines} -- {message}\n\nTokenization cannot be continued.");
            Console.ForegroundColor = oldColor;
            return false;
        }
        catch (StatementException exc)
        {
            if (GlobalContext.Debug && Debugger.IsAttached)
                throw;
            if (suppressConsoleOutput)
                return false;

            Statement thrower = exc.statement;
            string message = exc.Message;
            int[] _lines = thrower.Lines;
            string lines = string.Join(", ", _lines.Select(line => line < 0 ? "??" : line.ToString()));
            string fileNameShort = thrower.SourceFile == null ? "unknown" : Path.GetFileName(thrower.SourceFile);

            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("An error has occurred during compilation:\n" +
                                    $"\t{fileNameShort}:{lines} -- {thrower}:\n\t\t{message}\n\nCompilation cannot be continued.");
            Console.ForegroundColor = oldColor;
            return false;
        }
        catch (FeederException exc)
        {
            if (GlobalContext.Debug && Debugger.IsAttached)
                throw;
            if (suppressConsoleOutput)
                return false;

            TokenFeeder thrower = exc.feeder;
            string message = exc.Message;
            int[] _lines = thrower.Lines;
            string lines = string.Join(", ", _lines.Select(line => line < 0 ? "??" : line.ToString()));
            string fileNameShort = thrower.SourceFile == null ? "unknown" : Path.GetFileName(thrower.SourceFile);

            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("An error has occurred during compilation:\n" +
                                    $"\t{fileNameShort}:{lines} -- {thrower}:\n\t\t{message}\n\nCompilation cannot be continued.");
            Console.ForegroundColor = oldColor;
            return false;
        }
        finally
        {
            // complete it just in-case
            resultEmission?.Complete();
        }
    }
    /// <summary>
    ///     Reset static states. This should be called before a compilation but only at the <b>highest</b> level.
    ///     If a child file is being compiled with <c>$include [file]</c>, for example, then this shouldn’t be called to
    ///     prevent overlap.
    /// </summary>
    [PublicAPI]
    public static void ResetStaticStates()
    {
        Executor.ResetGeneratedNames();
        Command.ResetState();
        Tokenizer.CURRENT_LINE = 0;
        DirectiveImplementations.ResetState();
    }
    /// <summary>
    ///     Ensures the specified file is open in the workspace. If the file is not already open, it is opened using
    ///     <see cref="OpenFile" />.
    /// </summary>
    /// <param name="file">
    ///     The absolute or relative path of the file to be opened. If the path is relative, it will be converted to an
    ///     absolute path
    ///     using <see cref="EnsureAbsolute(string)" />. If the file is already open, no further action is taken.
    /// </param>
    [PublicAPI]
    public void OpenFileIfNotOpen(string file)
    {
        file = EnsureAbsolute(file);
        if (this.openFiles.Contains(file))
            return;
        OpenFile(file);
    }
    /// <summary>
    ///     Opens a file, processes its content, and registers it within the workspace for further compilation or execution.
    ///     This method ensures the file path is absolute, reads the file content, tokenizes and assembles the content,
    ///     collects debugging information if enabled, and updates the internal workspace state to track the file.
    /// </summary>
    /// <param name="file">
    ///     The absolute or relative file path of the file to be opened. If the file doesn’t exist, a
    ///     <see cref="System.IO.FileNotFoundException" /> is thrown. The file's content is tokenized, parsed into
    ///     statements, and stored within the workspace to simulate opening and preparing the file for compilation.
    /// </param>
    [PublicAPI]
    public void OpenFile(string file)
    {
        file = EnsureAbsolute(file);

        // even if the file is already registered as "open",
        // we can't rule out that the contents have changed.

        // get content
        if (!File.Exists(file))
            throw new FileNotFoundException($"Couldn't find file: \"{file}\"");

        string content = File.ReadAllText(file);

        // tokenize/assemble content
        var stopwatch = Stopwatch.StartNew();
        Token[] tokens = new Tokenizer(content).Tokenize( /* only included for errors: */ file);
        Statement[] statements = Assembler.AssembleTokens(tokens, file);
        stopwatch.Stop();

        // debug stuff
        if (GlobalContext.Debug)
        {
            Console.WriteLine("Loaded and parsed file \"{0}\"", file);
            Console.WriteLine("\tDetailed overview of tokenization results:");
            Console.WriteLine(string.Join("", from t in tokens select t.DebugString()));
            Console.WriteLine();
            Console.WriteLine("\tReconstruction of the original code using parsed tokens:");
            Console.WriteLine(string.Join(" ", from t in tokens select t.AsString()));
            Console.WriteLine();
            Console.WriteLine("\tDetailed overview of assembly results:");
            Console.WriteLine(string.Join("\n", from s in statements select s.ToString()));
            Console.WriteLine();
        }

        // got through without exception
        this.openFilesRaw[file] = content;
        this.openFilesParsed[file] = statements;
        this.assemblyTimesMs[file] = stopwatch.ElapsedMilliseconds;
        this.openFiles.Add(file);
    }
    /// <summary>
    ///     Closes an open file and removes its associated data from the workspace.
    ///     This operation ensures the file is no longer tracked as open, and all associated
    ///     parsed statements and raw content are removed.
    /// </summary>
    /// <param name="file">
    ///     The absolute or relative file path of the file to close. If a relative path is provided, it will be resolved into
    ///     an absolute path. If the file is not currently open, no action is taken.
    /// </param>
    [PublicAPI]
    public void CloseFile(string file)
    {
        file = EnsureAbsolute(file);

        if (!this.openFiles.Contains(file))
            return;

        this.openFiles.Remove(file);
        this.openFilesParsed.Remove(file);
        this.openFilesRaw.Remove(file);
    }
}