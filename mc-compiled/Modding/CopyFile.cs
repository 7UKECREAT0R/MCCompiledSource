using System;
using System.IO;

namespace mc_compiled.Modding;

/// <summary>
///     Copies a source file to the destination on write.
/// </summary>
internal class CopyFile : IAddonFile
{
    private readonly string outputDirectoryExtra;
    private readonly string outputFile;

    private readonly OutputLocation outputLocation;
    internal readonly string sourceFile;

    public CopyFile(string sourceFile, OutputLocation outputLocation, string outputFile)
    {
        this.outputDirectoryExtra = Path.GetDirectoryName(sourceFile);
        this.sourceFile = sourceFile;
        this.outputLocation = outputLocation;
        this.outputFile = outputFile;

        if (string.IsNullOrEmpty(this.outputDirectoryExtra))
            this.outputDirectoryExtra = null;
        else
            this.outputFile = Path.GetFileName(sourceFile);
    }

    public string CommandReference => throw new NotImplementedException();
    public string GetExtendedDirectory()
    {
        return this.outputDirectoryExtra;
    }
    public string GetOutputFile()
    {
        return this.outputFile;
    }
    public byte[] GetOutputData()
    {
        throw new NotImplementedException();
        // handled by ProjectManager.Instance.WriteSingleFile(..)
    }
    public OutputLocation GetOutputLocation()
    {
        return this.outputLocation;
    }

    private bool Equals(CopyFile other)
    {
        return this.sourceFile == other.sourceFile && this.outputLocation == other.outputLocation &&
               this.outputDirectoryExtra == other.outputDirectoryExtra && this.outputFile == other.outputFile;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((CopyFile) obj);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = this.sourceFile != null ? this.sourceFile.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (int) this.outputLocation;
            hashCode = (hashCode * 397) ^
                       (this.outputDirectoryExtra != null ? this.outputDirectoryExtra.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.outputFile != null ? this.outputFile.GetHashCode() : 0);
            return hashCode;
        }
    }
}