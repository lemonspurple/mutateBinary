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
using System.Runtime.CompilerServices;

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

        // set folder path + add bin as directory
        string outputFolderBin = Path.Combine(SelectedFolderPath, "bin");
        string outputFolderDNA = Path.Combine(SelectedFolderPath, "dna");

        // create folder if it doesn't exist yet
        if (!Directory.Exists(outputFolderBin))
        {
            Directory.CreateDirectory(outputFolderBin);
        }
        if (!Directory.Exists(outputFolderDNA))
        {
            Directory.CreateDirectory(outputFolderDNA);
        }

        var manager = new FileManager();
        var rawfiles = manager.GetFilesInFolder(SelectedFolderPath);

        foreach (var file in rawfiles)
        {
            // fetch filename without old path
            string fileName = Path.GetFileName(file);

            // create new target path in /bin & /dna
            string targetPathBin = Path.Combine(outputFolderBin, fileName + ".bin");
            string targetPathDNA = Path.Combine(outputFolderDNA, fileName + ".txt");

            // start conversion
            await Task.Run(() => manager.ConvertFileToBinaryText(file, targetPathBin));
            await Task.Run(() => manager.ConvertFileToDNA(targetPathBin, targetPathDNA));
        }

        Console.WriteLine($"Debug All data has been converted (directory: {outputFolderBin})");



    }


}
