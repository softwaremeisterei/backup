using Backup.Models;
using Softwaremeisterei.Lib;
using System.IO;
using System.Reflection;
using System.Text;

namespace Poetica.BL.Storage
{
    public class Repository
    {
        private const string ProjectFileName = "user.bkproj";

        private readonly string BrojectFilePath;

        public Repository()
        {
            this.BrojectFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ProjectFileName);

            if (!File.Exists(this.BrojectFilePath))
            {
                Save(new Project());
            }
        }

        public void Save(Project project)
        {
            var xml = Serialization.ToXml(project);
            File.WriteAllText(this.BrojectFilePath, xml, Encoding.UTF8);
        }


        public Project Load()
        {
            var xml = File.ReadAllText(this.BrojectFilePath);
            var project = Serialization.ParseXml<Project>(xml);
            return project;
        }
    }
}
