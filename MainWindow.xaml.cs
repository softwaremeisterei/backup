using Backup.Models;
using Poetica.BL.Storage;
using Softwaremeisterei.Lib;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
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
                foreach (var source in sources)
                {
                    var destFile = Path.Combine(Project.Destination, Path.GetFileName(source));

                    for (var iter = 0; ; iter++)
                    {
                        var destZipfile = string.Concat(destFile, DateTime.Now.ToString("-yyyyMMdd"), iter == 0 ? "" : $"-({iter})", ".zip");

                        if (!File.Exists(destZipfile))
                        {
                            Predicate<string> exclude = Project.ComplyToGitIgnore ? CreateExcludePredicateByGitignore(source) : (_) => false;
                            ZipFiles.Create(source, destZipfile, CompressionLevel.Optimal, true, exclude);
                            break;
                        }
                    }
                }
            }
        }

        private Predicate<string> CreateExcludePredicateByGitignore(string srcDirectory)
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
            bool isIgnored = false;

            foreach (var pattern in patterns)
            {
                if (pattern.StartsWith("!")) // negative pattern => if matched => dismiss previous ignores
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

        private void ChooseDestionationFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Project.Destination = dialog.SelectedPath;
            }
        }

        private void SourcesView_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.None;

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
        }

        private void SourcesView_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;

                var files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

                foreach (var file in files)
                {
                    if (Directory.Exists(file))
                    {
                        sources.Add(file);
                    }
                }
            }
        }
    }
}
