using Softwaremeisterei.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup.Core
{
    public class BackupService
    {
        public void Backup(
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
                    zip.Create(CompressionLevel.Optimal, source, destZipfile, isCancellationRequested, true, excludedFilesPredicate);
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
                    .Where(line => !line.StartsWith("#"))
                    .ToArray();
                return (filepath) => IsGitignored(filepath, patterns);
            }
            else
            {
                return (_) => false;
            }
        }

        private bool IsGitignored(string filepath, string[] patterns)
        {
            var isIgnored = false;

            foreach (var pattern in patterns)
            {
                if (pattern.StartsWith("!")) // negative ignore pattern => if matched => do _not_ ignore
                {
                    if (GlobMatches.GitIgnoreGlobMatch(filepath, pattern.Substring(1)))
                    {
                        isIgnored = false;
                    }
                }
                else if (GlobMatches.GitIgnoreGlobMatch(filepath, pattern))
                {
                    isIgnored = true;
                }
            }

            return isIgnored;
        }

    }
}
