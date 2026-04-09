namespace deepMutate.ViewModels;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using deepMutate.Models.Data;
using Avalonia.Controls;
using System;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _selectedFolderPath;

    public ObservableCollection<string> FoundFiles { get; } = new();

    [RelayCommand]
    public async Task PickFolderAsync()
    {
        var topLevel = TopLevel.GetTopLevel((Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose directory",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            SelectedFolderPath = folders[0].Path.LocalPath;

            var manager = new FileManager();
            var files = manager.GetFilesInFolder(SelectedFolderPath);

            FoundFiles.Clear();
            foreach (var file in files)
            {
                FoundFiles.Add(Path.GetFileName(file));
            }
        }
    }

    [RelayCommand]
    public async Task MutateFilesAsync()
    {
        // has a folder been selected
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        // set folder path
        string outputFolder = Path.Combine(SelectedFolderPath, "bin");

        // create folder if it doesn't exist yet
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        var manager = new FileManager();
        var files = manager.GetFilesInFolder(SelectedFolderPath);

        foreach (var file in files)
        {
            // fetch filename without old path
            string fileName = Path.GetFileName(file);

            // create new target path in /bin
            string targetPath = Path.Combine(outputFolder, fileName + ".bin");

            // start conversion
            await Task.Run(() => manager.ConvertFileToBinaryText(file, targetPath));
        }

        Console.WriteLine($"Debug All data has been converted to binary (directory: {outputFolder})");
    }


}
