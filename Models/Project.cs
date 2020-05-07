using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup.Models
{
    public class Project
    {
        public string Version { get; set; }
        public List<string> Sources { get; set; }
        public string Destination { get; set; }
        public string Command { get; set; }

        public Project()
        {
            Version = "1.0";
            Sources = new List<string>();
            Command = "ROBOCOPY.EXE /E {SOURCE} {DESTINATION}";
        }
    }
}
