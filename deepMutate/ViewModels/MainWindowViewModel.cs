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

            var manager = new Mutate();
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

        var manager = new Mutate();
        var rawfiles = manager.GetFilesInFolder(SelectedFolderPath);

        foreach (var file in rawfiles)
        {
            // fetch filename without old path
            string fileName = Path.GetFileName(file);  // "photo.jpg"

            // create new target path in /bin & /dna
            string targetPath = Path.Combine(outputFolderBin, fileName + ".bin");  // "/bin/photo.jpg.bin"
            string fileNameWithBin = Path.GetFileName(targetPath);  // "photo.jpg.bin"
            string targetPathDNA = Path.Combine(outputFolderDNA, fileNameWithBin.Replace(".bin", ".txt"));  // "/dna/photo.jpg.txt"

            // start conversion
            await Task.Run(() => manager.ConvertFileToBinaryText(file, targetPath));
            await Task.Run(() => manager.ConvertFileToDNA(targetPath, targetPathDNA));
        }

        Console.WriteLine($"Debug All data has been converted \n (directory: {outputFolderBin} \n (directory: {outputFolderDNA})");
    }


}
