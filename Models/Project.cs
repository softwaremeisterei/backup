using System.Collections.Generic;

namespace Backup.Models
{
    public class Project
    {
        public string Version { get; set; }

        /// <summary>
        /// List of source folders
        /// </summary>
        public List<string> Sources { get; set; }

        /// <summary>
        /// Destination folder
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// If True, the backup process will comply to gitignore patterns 
        /// (only if a .gitignore file is present in the source directory)
        /// </summary>
        public bool ComplyToGitIgnore { get; set; }

        public Project()
        {
            Version = "1.0";
            Sources = new List<string>();
            ComplyToGitIgnore = true;
        }
    }
}
