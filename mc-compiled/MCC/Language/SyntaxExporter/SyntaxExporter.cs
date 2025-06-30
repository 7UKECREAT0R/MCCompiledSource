using System.IO;

namespace mc_compiled.MCC.Language.SyntaxExporter;

/// <summary>
///     A class which can export language information out to a file.
/// </summary>
public abstract class SyntaxExporter(string fileName, string identifier, string description)
{
    /// <summary>
    ///     The file name and extension to output to.
    /// </summary>
    public string FileName { get; } = fileName;
    /// <summary>
    ///     The unique identifier of this exporter.
    /// </summary>
    public string Identifier { get; } = identifier;
    /// <summary>
    ///     The description of this exporter.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    ///     Generate the string which will be written to the file indicated by <see cref="FileName" />.
    /// </summary>
    /// <returns></returns>
    public abstract string Export();

    /// <summary>
    ///     Helper method which writes the output of the <see cref="Export" /> method to the file specified by
    ///     <see cref="FileName" />.
    /// </summary>
    /// <remarks>
    ///     The method generates a string, using <see cref="Export" />, which contains the exported language information.
    ///     This string is then written to the file specified by the <see cref="FileName" /> property.
    ///     This process overwrites any existing contents of the specified file with the new data.
    /// </remarks>
    public void ExportToFile()
    {
        string output = Export();
        File.WriteAllText(this.FileName, output);
    }
}