using DotNet.Globbing;
using Softwaremeisterei.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Backup.Core
{
    public class BackupService
    {
        public void Backup(
            CompressionLevel compressionLevel,
            string[] sources,
            string destination,
            bool complyToGitIgnore,
            Func<bool> isCancellationRequested,
            Action onCompleted,
            Action<string> onStatusChanged)
        {
            try
            {
                foreach (var source in sources)
                {
                    var destFile = Path.Combine(destination, Path.GetFileName(source));
                    string destZipfile;
                    var n = 0;

                    do
                    {
                        destZipfile = string.Concat(destFile, DateTime.Now.ToString("-yyyyMMdd"), n == 0 ? "" : $"-({n})", ".zip");
                        n++;
                    } while (File.Exists(destZipfile));

                    var excludedFilesPredicate = complyToGitIgnore ? CreateExcludePredicateFromGitignore(source) : (_) => false;
                    var zip = new ZipFiles();
                    zip.StatusEvent += (_, ev) => onStatusChanged(ev.StatusMessage);
                    zip.Create(compressionLevel,
                        source,
                        destZipfile,
                        isCancellationRequested,
                        true,
                        excludedFilesPredicate);
                }
            }
            finally
            {
                onCompleted();
            }
        }

        private Predicate<string> CreateExcludePredicateFromGitignore(string srcDirectory)
        {
            var gitIgnoreFile = Path.Combine(srcDirectory, ".gitignore");
            if (File.Exists(gitIgnoreFile))
            {
                var patterns = File.ReadAllLines(gitIgnoreFile)
                    .Select(line => line.Trim().ToLower())
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    .ToArray();
                return (filepath) => IsGitignored(filepath, patterns);
            }
            else
            {
                return (_) => false;
            }
        }

        static Dictionary<string, Glob> globDictionary = new Dictionary<string, Glob>();

        private bool IsGitignored(string filepath, string[] patterns)
        {
            var isIgnored = false;

            foreach (var pattern0 in patterns)
            {
                Glob glob;
                var pattern = pattern0.Replace("/", "\\");
                pattern = string.Concat("**\\", pattern.TrimStart('\\'), "**");

                bool isNegation = pattern.StartsWith("!");

                if (isNegation)
                {
                    pattern = pattern.Substring(1);
                }

                if (!globDictionary.TryGetValue(pattern, out glob))
                {
                    glob = Glob.Parse(pattern);
                    globDictionary.Add(pattern, glob);
                }

                var isMatch = glob.IsMatch(filepath);

                if (isMatch)
                {
                    isIgnored = !isNegation;
                }
            }

            return isIgnored;
        }

    }
}
