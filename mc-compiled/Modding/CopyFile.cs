using System;
using System.IO;

namespace mc_compiled.Modding
{
    /// <summary>
    /// Copies a source file to the destination on write.
    /// </summary>
    internal class CopyFile : IAddonFile
    {
        internal readonly string sourceFile;
        
        private readonly OutputLocation outputLocation;
        private readonly string outputDirectoryExtra;
        private readonly string outputFile;
        
        public CopyFile(string sourceFile, OutputLocation outputLocation, string outputFile)
        {
            outputDirectoryExtra = Path.GetDirectoryName(sourceFile);
            if (string.IsNullOrEmpty(outputDirectoryExtra))
                outputDirectoryExtra = null;
            else
                this.outputFile = Path.GetFileName(sourceFile);
            
            this.sourceFile = sourceFile;
            this.outputLocation = outputLocation;
            this.outputFile = outputFile;
        }

        private bool Equals(CopyFile other)
        {
            return sourceFile == other.sourceFile && outputLocation == other.outputLocation && outputDirectoryExtra == other.outputDirectoryExtra && outputFile == other.outputFile;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CopyFile) obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (sourceFile != null ? sourceFile.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) outputLocation;
                hashCode = (hashCode * 397) ^ (outputDirectoryExtra != null ? outputDirectoryExtra.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (outputFile != null ? outputFile.GetHashCode() : 0);
                return hashCode;
            }
        }

        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => outputDirectoryExtra;
        public string GetOutputFile() => outputFile;
        public byte[] GetOutputData() => throw new NotImplementedException(); // handled by ProjectManager.Instance.WriteSingleFile(..)
        public OutputLocation GetOutputLocation() => outputLocation;
    }
}