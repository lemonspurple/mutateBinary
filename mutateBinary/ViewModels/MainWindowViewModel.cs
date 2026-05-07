namespace mutateBinary.ViewModels;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mutateBinary.Models.Data;
using Avalonia.Controls;
using System;
using System.Runtime.CompilerServices;

public partial class MainWindowViewModel : ViewModelBase
{
    // Getting / Listing files in folder
    [ObservableProperty]
    private string? _selectedFolderPath;
    public ObservableCollection<string> FoundFiles { get; } = new();
    // Mutate values
    [ObservableProperty]
    private float menuPointValue = default;
    [ObservableProperty]
    private float menuFrameshiftValue = default;
    [ObservableProperty]
    private float menuFrameInsertDeleteValue = default;
    [ObservableProperty]
    private float menuDuplicationsValue = default;
    [ObservableProperty]
    private float menuDeletionValue = default;
    [ObservableProperty]
    private float menuInversionValue = default;
    [ObservableProperty]
    private float menuTranslocationValue = default;
    [ObservableProperty]
    private double menuCyclesValue = default;

    

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
        string outputFolderDNA = Path.Combine(SelectedFolderPath, "dna");


        if (!Directory.Exists(outputFolderDNA))
        {
            Directory.CreateDirectory(outputFolderDNA);
        }

        var manager = new FileManager();
        var rawfiles = manager.GetFilesInFolder(SelectedFolderPath);

        foreach (var file in rawfiles)
        {
            // fetch filename without old path
            string fileName = Path.GetFileName(file);  // "photo.jpg"

            // start conversion
            await Task.Run(() => manager.ConvertFileToDNA(file, outputFolderDNA));
        }

        Console.WriteLine($"Debug: All data has been encoded to DNA \n (directory: {outputFolderDNA})");
    }

    [RelayCommand]
    public async Task DecodeFilesAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        string inputFolderDNA = Path.Combine(SelectedFolderPath, "dna");
        string outputFolderReversed = Path.Combine(SelectedFolderPath, "reversed");

        if (!Directory.Exists(inputFolderDNA)) return;

        if (!Directory.Exists(outputFolderReversed))
        {
            Directory.CreateDirectory(outputFolderReversed);
        }

        var manager = new FileManager();
        var DNAFiles = manager.GetFilesInFolder(inputFolderDNA);

        foreach (var file in DNAFiles)
        {
            await Task.Run(() => manager.ConvertDNAToFile(file, outputFolderReversed));
        }

        Console.WriteLine($"Debug: All DNA has been decoded to files \n (directory: {outputFolderReversed})");
    }

    [RelayCommand]
    public async Task PrintDebugLogAsync()
    {
        //MutateFuncs mutateFuncs = new MutateFuncs();
        //mutateFuncs.printMutateValuesToDebug();
        Console.WriteLine($"Debug+{MenuPointValue}+{MenuFrameshiftValue}+{MenuFrameInsertDeleteValue}+{MenuDuplicationsValue}+{MenuDeletionValue}+{MenuInversionValue}+{MenuTranslocationValue}+{MenuCyclesValue}");
    }


}
