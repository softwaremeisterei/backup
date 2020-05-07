using Backup.Models;
using Poetica.BL.Storage;
using Softwaremeisterei.Lib;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace Backup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Repository repo;
        readonly SortableBindingList<string> sources;

        public Project Project { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            repo = new Repository();
            Project = repo.Load();
            sources = new SortableBindingList<string>(Project.Sources);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = string.Concat(Title, " V", Project.Version);
            SourcesView.ItemsSource = sources;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            repo.Save(Project);
        }

        private void AddDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sources.Add(dialog.SelectedPath);
            }
        }

        private void RemoveSourceDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (SourcesView.SelectedIndex >= 0)
            {
                sources.RemoveAt(SourcesView.SelectedIndex);
            }
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                var commandTokens = Project.Command.Split(' ');
                var command = commandTokens.FirstOrDefault();

                if (command != null)
                {
                    foreach (var source in sources)
                    {
                        var arguments = string.Join(" ", commandTokens.Skip(1));
                        arguments = arguments.Replace("{SOURCE}", source);
                        arguments = arguments.Replace("{DESTINATION}", Path.Combine(Project.Destination, Path.GetFileName(source)));
                        Process.Start(command, arguments);
                    }
                }
            }
        }

        private void ChooseDestionationFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Project.Destination = dialog.SelectedPath;
            }
        }
    }
}
