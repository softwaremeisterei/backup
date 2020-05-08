using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Softwaremeisterei.Lib
{
    public class ZipFiles
    {
        public delegate void StatusEventHandler(object sender, StatusEventArgs e);
        public event StatusEventHandler StatusEvent;

        public void Create(
            CompressionLevel compressionLevel,
            string srcDirectory,
            string destArchiveFile,
            Func<bool> isCancellationRequested,
            bool includeBaseDirectory = true,
            Predicate<string> excludedFilesPredicate = null)
        {
            if (string.IsNullOrEmpty(srcDirectory)) throw new ArgumentNullException("sourceDirectoryName");
            if (string.IsNullOrEmpty(destArchiveFile)) throw new ArgumentNullException("destinationArchiveFileName");

            var filesToAdd = Directory.GetFiles(srcDirectory, "*", SearchOption.AllDirectories);
            var entryNames = CreateEntryNames(filesToAdd, srcDirectory, includeBaseDirectory);

            using (var zipFileStream = new FileStream(destArchiveFile, FileMode.Create))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    for (var i = 0; i < filesToAdd.Length; i++)
                    {
                        if (isCancellationRequested())
                        {
                            break;
                        }
                        
                        var progressPercent = i * 100 / filesToAdd.Length;
                        var fileToAdd = filesToAdd[i];

                        if (excludedFilesPredicate != null && excludedFilesPredicate(fileToAdd))
                        {
                            continue;
                        }

                        NotifyStatus($"[{progressPercent}%] adding {fileToAdd}");
                        archive.CreateEntryFromFile(fileToAdd, entryNames[i], compressionLevel);
                    }
                }
            }
        }

        private void NotifyStatus(string message)
        {
            StatusEvent?.Invoke(this, new StatusEventArgs(message));
            Thread.Sleep(0);
        }

        private string[] CreateEntryNames(string[] names, string srcDirectory, bool includeBaseName)
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

    public class StatusEventArgs
    {
        public StatusEventArgs(string message) { StatusMessage = message; }
        public String StatusMessage { get; } // readonly
    }
}
