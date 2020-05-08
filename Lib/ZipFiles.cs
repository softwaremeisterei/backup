using System;
using System.IO;
using System.IO.Compression;

namespace Softwaremeisterei.Lib
{
    public static class ZipFiles
    {
        public static void Create(
            string srcDirectory,
            string destArchiveFile,
            CompressionLevel compressionLevel,
            bool includeBaseDirectory = true,
            Predicate<string> exclude = null,
            Predicate<string> include = null)
        {
            if (string.IsNullOrEmpty(srcDirectory)) throw new ArgumentNullException("sourceDirectoryName");
            if (string.IsNullOrEmpty(destArchiveFile)) throw new ArgumentNullException("destinationArchiveFileName");

            var filesToAdd = Directory.GetFiles(srcDirectory, "*", SearchOption.AllDirectories);
            var entryNames = CreateEntryNames(filesToAdd, srcDirectory, includeBaseDirectory);

            using (var zipFileStream = new FileStream(destArchiveFile, FileMode.Create))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    for (int i = 0; i < filesToAdd.Length; i++)
                    {
                        var fileToAdd = filesToAdd[i];
                        if (exclude != null && exclude(fileToAdd)) continue;
                        if (include != null && !include(fileToAdd)) continue;
                        archive.CreateEntryFromFile(fileToAdd, entryNames[i], compressionLevel);
                    }
                }
            }
        }

        private static string[] CreateEntryNames(string[] names, string srcDirectory, bool includeBaseName)
        {
            if (names == null || names.Length == 0) return new string[0];

            if (includeBaseName)
            {
                srcDirectory = Path.GetDirectoryName(srcDirectory);
            }

            var length = string.IsNullOrEmpty(srcDirectory) ? 0 : srcDirectory.Length;

            if (length > 0 && srcDirectory != null &&
                srcDirectory[length - 1] != Path.DirectorySeparatorChar &&
                srcDirectory[length - 1] != Path.AltDirectorySeparatorChar)
            {
                length++;
            }

            var result = new string[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                result[i] = names[i].Substring(length);
            }

            return result;
        }

    }
}
