namespace mutateBinary.ViewModels;

using mutateBinary.Models.Functions;
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

public partial class MainWindowViewModel : ViewModelBase
{
    // Getting / Listing files in folder
    [ObservableProperty]
    private string? _selectedFolderPath;
    public ObservableCollection<string> FoundFiles { get; } = new();

    // Mutate values
    // generator will turn _menuPointValue into MenuPointValue.
    [ObservableProperty]
    private float _menuPointValue = default;
    [ObservableProperty]
    private float _menuFrameshiftValue = default;
    [ObservableProperty]
    private float _menuFrameInsertDeleteValue = default;
    [ObservableProperty]
    private float _menuDuplicationsValue = default;
    [ObservableProperty]
    private float _menuDeletionValue = default;
    [ObservableProperty]
    private float _menuInversionValue = default;
    [ObservableProperty]
    private float _menuTranslocationValue = default;
    [ObservableProperty]
    private int _menuCyclesValue = 1;

    // Helper method that adds parameter values to filename as methodology. 
    private string BuildMutationSuffix()
    {
        var sb = new System.Text.StringBuilder();
        if (MenuPointValue != 0) sb.Append($"_pt{MenuPointValue}");
        if (MenuFrameshiftValue != 0) sb.Append($"_fs{MenuFrameshiftValue}");
        if (MenuFrameInsertDeleteValue != 0) sb.Append($"_fi{MenuFrameInsertDeleteValue}");
        if (MenuDuplicationsValue != 0) sb.Append($"_du{MenuDuplicationsValue}");
        if (MenuDeletionValue != 0) sb.Append($"_de{MenuDeletionValue}");
        if (MenuInversionValue != 0) sb.Append($"_in{MenuInversionValue}");
        if (MenuTranslocationValue != 0) sb.Append($"_tr{MenuTranslocationValue}");
        sb.Append($"_c{MenuCyclesValue}");
        return sb.ToString();
    }


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
    public async Task EncodeFilesToDNAAsync()
    {
        // has a folder been selected
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        // set folder path + add dna as directory
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
            //TODO FIX NAMING. THE SUFFIX HAS TO BE CACHED SOMEWHERE
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
        var mutateDebug = new MutateFuncs(
            MenuPointValue, MenuFrameshiftValue, MenuFrameInsertDeleteValue,
            MenuDuplicationsValue, MenuDeletionValue, MenuInversionValue,
            MenuTranslocationValue, MenuCyclesValue
        );
        mutateDebug.printMutateValuesToDebug();
    }

    [RelayCommand]
    public async Task MutateDataAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;
        // self healing segment that creates DNA folders, in case it hasn't yet.
        string outputFolder = Path.Combine(SelectedFolderPath, "mutated");
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);
        string dnaFolder = Path.Combine(SelectedFolderPath, "dna");
        if (!Directory.Exists(dnaFolder))
            Directory.CreateDirectory(dnaFolder);

        var mutator = new MutateFuncs(
            MenuPointValue, MenuFrameshiftValue, MenuFrameInsertDeleteValue,
            MenuDuplicationsValue, MenuDeletionValue, MenuInversionValue,
            MenuTranslocationValue, MenuCyclesValue
        );

        var manager = new FileManager();
        var files = manager.GetFilesInFolder(SelectedFolderPath);

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            string dnaPath = Path.Combine(dnaFolder, Path.GetFileName(file) + ".dna");
            string outputPath = Path.Combine(outputFolder, fileName);

            if (!File.Exists(dnaPath))
                await Task.Run(() => manager.ConvertFileToDNA(file, dnaFolder));

            // Copy .dna to temp working file, then mutate to output
            string workingDna = dnaPath + ".work";
            File.Copy(dnaPath, workingDna, overwrite: true);

            await Task.Run(() => mutator.MutateDNAFile(workingDna, workingDna + ".mutated"));

            string mutatedDna = workingDna + ".mutated";
            await Task.Run(() => manager.ConvertDNAToFile(mutatedDna, outputFolder));

            File.Delete(mutatedDna);
            string decodedFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(mutatedDna));
            string finalName = Path.GetFileNameWithoutExtension(file) + "_mutated" + BuildMutationSuffix() + Path.GetExtension(file);
            File.Move(decodedFile, Path.Combine(outputFolder, finalName), overwrite: true);
        }

        Console.WriteLine($"Debug: Mutation complete (directory: {outputFolder})");
    }
}
