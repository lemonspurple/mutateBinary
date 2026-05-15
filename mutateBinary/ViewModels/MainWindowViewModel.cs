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
using System.Linq;

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
    [ObservableProperty]
    private int _menuRepetitionValue = 0;

    // Helper method that adds parameter values to filename as methodology. 
    private string BuildMutationSuffix(int repetitionIndex)
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
        sb.Append($"_r{repetitionIndex}");
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
            MenuTranslocationValue, MenuCyclesValue, MenuRepetitionValue
        );
        mutateDebug.printMutateValuesToDebug();
    }

    [RelayCommand]
    public async Task MutateDataAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;
        
        // Guard against negative repetition values
        if (MenuRepetitionValue < 0)
        {
            Console.WriteLine("Error: Repetition value cannot be negative");
            return;
        }

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
            MenuTranslocationValue, MenuCyclesValue, MenuRepetitionValue
        );

        var manager = new FileManager();
        var allFiles = manager.GetFilesInFolder(SelectedFolderPath);

        Console.WriteLine($"Debug: Selected folder: {SelectedFolderPath}");
        Console.WriteLine($"Debug: Total files found in root: {allFiles.Count}");
        foreach (var f in allFiles)
            Console.WriteLine($"  Found: {Path.GetFileName(f)}");

        // Filter out generated files and artifacts
        var files = allFiles.Where(f => 
        {
            string fileName = Path.GetFileName(f).ToLowerInvariant();
            
            if (fileName.EndsWith(".dna") || fileName.EndsWith(".work") || 
                fileName.EndsWith(".mutated") || fileName.EndsWith(".tmp") ||
                fileName.EndsWith(".s1") || fileName.EndsWith(".s2") || fileName.EndsWith(".tl"))
            {
                Console.WriteLine($"  Excluded (bad extension): {fileName}");
                return false;
            }
            
            return true;
        }).ToList();

        Console.WriteLine($"Debug: Files after filter: {files.Count}");

        int totalOutputs = MenuRepetitionValue + 1;
        Console.WriteLine($"Debug: Processing {files.Count} files with {totalOutputs} outputs each (Repetition={MenuRepetitionValue})");

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            string baseName = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);
            string dnaPath = Path.Combine(dnaFolder, fileName + ".dna");

            // Ensure DNA file exists
            if (!File.Exists(dnaPath))
                await Task.Run(() => manager.ConvertFileToDNA(file, dnaFolder));

            // Repetition loop: create multiple output files
            for (int repetitionIndex = 0; repetitionIndex <= MenuRepetitionValue; repetitionIndex++)
            {
                // Create unique temp files for this repetition
                string workingDna = dnaPath + $".r{repetitionIndex}.work";
                string mutatedDna = workingDna + ".mutated";

                // Copy original DNA to working file
                File.Copy(dnaPath, workingDna, overwrite: true);

                // Mutate
                await Task.Run(() => mutator.MutateDNAFile(workingDna, mutatedDna));

                // Decode to binary
                await Task.Run(() => manager.ConvertDNAToFile(mutatedDna, outputFolder));

                // Cleanup temp files
                File.Delete(workingDna);
                File.Delete(mutatedDna);

                // Rename decoded file to final name
                string decodedFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(mutatedDna));
                string finalName = baseName + "_mutated" + BuildMutationSuffix(repetitionIndex) + extension;
                string finalPath = Path.Combine(outputFolder, finalName);
                
                if (File.Exists(finalPath))
                    File.Delete(finalPath);
                
                File.Move(decodedFile, finalPath);

                Console.WriteLine($"  → Generated: {finalName}");
            }
        }

        Console.WriteLine($"Debug: Mutation complete - {files.Count} files × {totalOutputs} outputs = {files.Count * totalOutputs} total outputs (directory: {outputFolder})");
    }
}
