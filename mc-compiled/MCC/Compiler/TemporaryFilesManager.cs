using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Manages the caching and downloading of default pack objects, as well as I/O for other temporary files.
    /// </summary>
    public static class TemporaryFilesManager
    {
        public enum PackType
        {
            BehaviorPack,
            ResourcePack
        }
        
        private static readonly string TEMP_FOLDER_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".mccompiled");
        private const string SOURCE_PATH = "https://raw.githubusercontent.com/Mojang/bedrock-samples/main/";

        /// <summary>
        /// Builds a traversal path, similar to Path.Combine()
        /// </summary>
        /// <param name="packType"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string BuildTraversalPath(PackType packType, params string[] path)
        {
            string packTypeString;

            switch (packType)
            {
                case PackType.ResourcePack:
                    packTypeString = "resource_pack";
                    break;
                case PackType.BehaviorPack:
                default:
                    packTypeString = "behavior_pack";
                    break;
            }

            IEnumerable<string> enumerable = path.Prepend(packTypeString);
            return Path.Combine(enumerable.ToArray());
        }

        /// <summary>
        /// Returns the given traversal path combined with the source path.
        /// </summary>
        /// <param name="path"></param>
        private static string GetSourcePath(string path)
        {
            return Path.Combine(SOURCE_PATH, path);
        }
        /// <summary>
        /// Returns the given traversal path combined with the destination path.
        /// </summary>
        /// <param name="path"></param>
        private static string GetDestPath(string path)
        {
            return Path.Combine(TEMP_FOLDER_PATH, path);
        }
        /// <summary>
        /// Download the newest file from remote source, overwriting the old one if it exists.
        /// Returns boolean indicating if the download was success.
        /// </summary>
        /// <param name="packType"></param>
        /// <param name="pathEntries"></param>
        private static bool DownloadNewest(PackType packType, params string[] pathEntries)
        {
            string path = BuildTraversalPath(packType, pathEntries);
            return DownloadNewest(path).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Download the newest file from the remote source, overwriting the old one if it exists.
        /// Returns boolean indicating if the download was success.
        /// </summary>
        /// <param name="traversalPath">The relative path of the file to be downloaded.</param>
        private static async Task<bool> DownloadNewest(string traversalPath)
        {
            try
            {
                Console.WriteLine("[DefaultPackManager] Downloading remote file {0}...", traversalPath);

                string sourcePath = GetSourcePath(traversalPath);
                string destPath = GetDestPath(traversalPath);

                // ensure the directory exists for the file to be placed in.
                string directory = Path.GetDirectoryName(destPath);
                if (destPath == null || directory == null)
                    throw new ArgumentException("Path was invalid.", nameof(traversalPath));
                Directory.CreateDirectory(directory);

                // delete the old file
                if (File.Exists(destPath))
                    File.Delete(destPath);

                // Download the file from the remote source.
                using (var client = new HttpClient())
                using (var response = await client.GetAsync(sourcePath))
                {
                    response.EnsureSuccessStatusCode();
                    await using (var fs = new FileStream(destPath, FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                Console.WriteLine("[DefaultPackManager] Complete: {0}.", destPath);

                return true; // return true if download is successful
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DefaultPackManager] Exception: {0}.", ex.Message);
                return false; // return false if an exception occurred, we conclude that the download was unsuccessful
            }
        }

        /// <summary>
        /// Gets the given default pack file, downloading it if it's not already cached.
        /// </summary>
        /// <param name="packType">The type of pack to look in.</param>
        /// <param name="callingStatement">The calling statement for exceptions.</param>
        /// <param name="pathEntries">The entries to the path of the file from the root of the given pack type.</param>
        /// <returns>The path to the gotten file.</returns>
        public static string Get(PackType packType, Statement callingStatement, params string[] pathEntries)
        {
            string path = BuildTraversalPath(packType, pathEntries);
            string destPath = GetDestPath(path);

            if (File.Exists(destPath))
                return destPath;

            if (!DownloadNewest(packType, pathEntries))
                throw new StatementException(callingStatement, $"Couldn't download file: {(packType == PackType.BehaviorPack ? "BP" : "RP")}/{string.Join("/", pathEntries)}");
            
            return destPath;
        }
        
        /// <summary>
        /// Retrieves the bytes of a file from the specified path in the temp directory.
        /// </summary>
        /// <param name="file">The name or path of the file.</param>
        /// <returns>An array of bytes containing the data from the file.</returns>
        public static byte[] GetFileBytes(string file)
        {
            string path = Path.Combine(TEMP_FOLDER_PATH, file);
            return File.ReadAllBytes(path);
        }
        /// <summary>
        /// Retrieves the content of a file in the temp directory.
        /// </summary>
        /// <param name="file">The name of the file to read.</param>
        /// <returns>The content of the file.</returns>
        public static string GetFileText(string file)
        {
            string path = Path.Combine(TEMP_FOLDER_PATH, file);
            return File.ReadAllText(path);
        }
        /// <summary>
        /// Retrieves all the lines in the specified file in the temp directory.
        /// </summary>
        /// <param name="file">The name of the file.</param>
        /// <returns>An array of strings representing each line of the file.</returns>
        public static string[] GetFileLines(string file)
        {
            string path = Path.Combine(TEMP_FOLDER_PATH, file);
            return File.ReadAllLines(path);
        }
        /// <summary>
        /// Checks if the specified file exists in the temp folder location.
        /// </summary>
        /// <param name="file">The name of the file including its extension.</param>
        /// <returns>True if the file exists in the temporary folder location; otherwise, false.</returns>
        public static bool HasFile(string file)
        {
            string path = Path.Combine(TEMP_FOLDER_PATH, file);
            return File.Exists(path);
        }
        /// <summary>
        /// Returns where the given file is in the temp folder.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ResolveFile(string file)
        {
            return Path.Combine(TEMP_FOLDER_PATH, file);
        }
        /// <summary>
        /// Writes the given contents to a file in the temp folder with the specified file name.
        /// </summary>
        /// <param name="file">The name of the file to write.</param>
        /// <param name="contents">The contents to write to the file.</param>
        public static void WriteFile(string file, string contents)
        {
            string path = Path.Combine(TEMP_FOLDER_PATH, file);
            
            string directoryName = Path.GetDirectoryName(path);
            if (directoryName != null && !Directory.Exists(directoryName)) 
                Directory.CreateDirectory(directoryName);
            
            File.WriteAllText(path, contents);
        }
        /// <summary>
        /// Writes the given contents to a file in the temp folder with the specified file name.
        /// </summary>
        /// <param name="file">The name of the file to write.</param>
        /// <param name="contents">The contents to write to the file.</param>
        public static void WriteFile(string file, byte[] contents)
        {
            string path = Path.Combine(TEMP_FOLDER_PATH, file);
            
            string directoryName = Path.GetDirectoryName(path);
            if (directoryName != null && !Directory.Exists(directoryName)) 
                Directory.CreateDirectory(directoryName);
            
            File.WriteAllBytes(path, contents);
        }
        
        
        /// <summary>
        /// Clears the MCCompiled file cache.
        /// </summary>
        public static void ClearCache()
        {
            if (!Directory.Exists(TEMP_FOLDER_PATH))
                return;

            Console.WriteLine("Clearing temporary cache...");

            foreach (string item in Directory.EnumerateFiles(TEMP_FOLDER_PATH, "*", SearchOption.AllDirectories))
                File.Delete(item);
        }
    }
}
