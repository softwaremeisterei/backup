using Backup.Core;
using Backup.Models;
using Poetica.BL.Storage;
using Softwaremeisterei.Lib;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Backup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Repository repo;
        readonly BackupService backupService;
        readonly SortableBindingList<string> sources;

        public Project Project { get; set; }

        public bool IsClosing { get; private set; }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            repo = new Repository();
            backupService = new BackupService();
            Project = repo.Load();
            sources = new SortableBindingList<string>(Project.Sources);
            StatusLabel1.Content = "Ready.";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = string.Concat(Title, " V", Project.Version);
            SourcesView.ItemsSource = sources;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.IsClosing = true;

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
            this.IsEnabled = false;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            bool isCancellationRequested() => this.IsClosing;
            void onCompleted() => Dispatcher.Invoke(() =>
            {
                this.IsEnabled = true;
                Mouse.OverrideCursor = null;
            });

            Task.Run(() => backupService.Backup(
                sources.ToArray(),
                Project.Destination,
                Project.ComplyToGitIgnore,
                isCancellationRequested,
                onCompleted,
                (statusMessage) => SetStatusText(statusMessage)));
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

        private void SetStatusText(string message)
        {
            Dispatcher.Invoke(() => StatusLabel1.Content = message);
        }
    }
}
