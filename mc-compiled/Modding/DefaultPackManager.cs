using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    /// <summary>
    /// Manages the caching and downloading of default pack objects.
    /// </summary>
    public static class DefaultPackManager
    {
        public enum PackType
        {
            BehaviorPack,
            ResourcePack
        }

        public static readonly string DEST_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".mccompiled");
        public static readonly string SOURCE_PATH = "https://raw.githubusercontent.com/Mojang/bedrock-samples/main/";

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

            var enumerable = path.Prepend(packTypeString);
            return Path.Combine(enumerable.ToArray());
        }

        /// <summary>
        /// Returns the given traversal path combined with the source path.
        /// </summary>
        /// <param name="path"></param>
        public static string GetSourcePath(string path)
        {
            return Path.Combine(SOURCE_PATH, path);
        }
        /// <summary>
        /// Returns the given traversal path combined with the destination path.
        /// </summary>
        /// <param name="path"></param>
        public static string GetDestPath(string path)
        {
            return Path.Combine(DEST_PATH, path);
        }

        /// <summary>
        /// Download the newest file from remote source, overwriting the old one if it exists.
        /// </summary>
        /// <param name="packType"></param>
        /// <param name="pathEntries"></param>
        public static void DownloadNewest(PackType packType, params string[] pathEntries)
        {
            string path = BuildTraversalPath(packType, pathEntries);

        }
        /// <summary>
        /// Download the newest file from remote source, overwriting the old one if it exists.
        /// </summary>
        /// <param name="packType"></param>
        /// <param name="pathEntries"></param>
        public static void DownloadNewest(string traversalPath)
        {
            Console.WriteLine("[DefaultPackManager] Downloading remote file {0}...", traversalPath);

            string sourcePath = GetSourcePath(traversalPath);
            string destPath = GetDestPath(traversalPath);

            // ensure the directory exists for the file to be placed in.
            string directory = Path.GetDirectoryName(destPath);
            Directory.CreateDirectory(directory);

            // delete the old file
            if (File.Exists(destPath))
                File.Delete(destPath);

            // Download the file from the remote source.
            using (WebClient client = new WebClient())
                client.DownloadFile(sourcePath, destPath);

            Console.WriteLine("[DefaultPackManager] Complete: {0}.", destPath);
        }

        /// <summary>
        /// Gets the given default pack file, downloading it if it's not already cached.
        /// </summary>
        /// <param name="packType"></param>
        /// <param name="pathEntries"></param>
        /// <returns>The path to the file in question.</returns>
        public static string Get(PackType packType, params string[] pathEntries)
        {
            string path = BuildTraversalPath(packType, pathEntries);
            string destPath = GetDestPath(path);

            if (File.Exists(destPath))
                return destPath;

            DownloadNewest(packType, pathEntries);
            return destPath;
        }

        /// <summary>
        /// Clears the default file cache.
        /// </summary>
        public static void ClearCache()
        {
            if (!Directory.Exists(DEST_PATH))
                return;

            Console.WriteLine("[DefaultPackManager] Clearing cache...");

            foreach (string item in Directory.EnumerateFiles(DEST_PATH, "*", SearchOption.AllDirectories))
                File.Delete(item);
        }
    }
}
