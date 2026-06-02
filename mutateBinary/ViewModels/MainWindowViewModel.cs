namespace mutateBinary.ViewModels;

using mutateBinary.Models.Functions;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
using System.Threading;
using System.Linq;

public partial class MainWindowViewModel : ViewModelBase
{
    // Getting / Listing files in folder
    [ObservableProperty]
    private string? _selectedFolderPath;
    private CancellationTokenSource? cts;
    public ObservableCollection<string> FoundFiles { get; } = new();

    // Mutate values
    // generator will turn _menuPointValue into MenuPointValue.
    [ObservableProperty] private float _menuPointValue = default;
    [ObservableProperty] private float _menuFrameshiftValue = default;
    [ObservableProperty] private float _menuFrameInsertDeleteValue = default;
    [ObservableProperty] private float _menuDuplicationsValue = default;
    [ObservableProperty] private float _menuDeletionValue = default;
    [ObservableProperty] private float _menuInversionValue = default;
    [ObservableProperty] private float _menuTranslocationValue = default;
    [ObservableProperty] private int _menuCyclesValue = 1;
    [ObservableProperty] private int _menuRepetitionValue = 0;
    // DNA Mapping values
    [ObservableProperty] private string _mapping00 = "A";
    [ObservableProperty] private string _mapping01 = "C";
    [ObservableProperty] private string _mapping10 = "G";
    [ObservableProperty] private string _mapping11 = "T";
    public IReadOnlyList<string> DnaBases { get; } = new[] { "A", "T", "C", "G" };  
    // Multithreading toggle
    [ObservableProperty] private bool _disableMultithreading = false;

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
        sb.Append(BuildDnaMapping().ToSuffix());
        return sb.ToString();
    }
    // Helper method that abuilds a DNA mapping object based on the current menu values.
    private DnaMapping BuildDnaMapping() => new DnaMapping {
        Map00 = Mapping00[0], Map01 = Mapping01[0],
        Map10 = Mapping10[0], Map11 = Mapping11[0]
    };

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
    public async Task OpenSelectedDirectoryAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;
        
        var topLevel = TopLevel.GetTopLevel((Application.Current?.ApplicationLifetime 
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        if (topLevel == null) return;
        // Encapsulating topLevel.Launcher into var launcher to perform a null check because otherwies the compiler keeps whining
        var launcher = topLevel.Launcher;
        if (launcher == null) return;

        await launcher.LaunchDirectoryInfoAsync(
            new DirectoryInfo(SelectedFolderPath));

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

            var mapping = BuildDnaMapping();
            await Task.Run(() => manager.ConvertFileToDNA(file, outputFolderDNA, mapping));
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
        cts = new CancellationTokenSource();
        try
        {
            await MutationProcessChain(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Mutation process was cancelled.");
        }
        finally
        {
            cts.Dispose();
            cts = null;
        }
    }
    
    //Helper method to cancel mutation process via finally block
    [RelayCommand]
    public void CancelMutation() 
    {
        cts?.Cancel(); 
    }

    private async Task MutationProcessChain(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        string outputFolder = Path.Combine(SelectedFolderPath, "mutated", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
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
        var mapping = BuildDnaMapping();
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

        // Phase 1: Ensure all DNA files exist
        foreach (var file in files)
        {
            string dnaPath = Path.Combine(dnaFolder, Path.GetFileName(file) + ".dna");
            if (!File.Exists(dnaPath))
                await Task.Run(() => manager.ConvertFileToDNA(file, dnaFolder, mapping), cancellationToken);
        }

        // Phase 2: Build flat 
        var workItems = files
            .SelectMany(file => Enumerable.Range(0, MenuRepetitionValue + 1).Select(r => (file, r)))
            .ToList();

        // Phase 3: Sequential or parallel
        if (DisableMultithreading)
        {
            foreach (var (file, repIdx) in workItems)
                await ProcessSingleOutput(file, repIdx, dnaFolder, outputFolder, mutator, manager, mapping, cancellationToken);
        }
        else
        {
            await Parallel.ForEachAsync(
                workItems,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = cancellationToken
                },
                async (item, ct) => await ProcessSingleOutput(item.file, item.r, dnaFolder, outputFolder, mutator, manager, mapping, ct)
            );
        }

        Console.WriteLine($"Debug: Mutation complete - {files.Count} files × {totalOutputs} outputs = {files.Count * totalOutputs} total outputs (directory: {outputFolder})");
    }

    [RelayCommand]
    public void RestoreDefaultMapping() {
        Mapping00 = "A"; Mapping01 = "C"; Mapping10 = "G"; Mapping11 = "T";
    }

    private async Task ProcessSingleOutput(
        string file, int repetitionIndex,
        string dnaFolder, string outputFolder,
        MutateFuncs mutator, FileManager manager, DnaMapping mapping,
        CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileName(file);
        string baseName = Path.GetFileNameWithoutExtension(file);
        string extension = Path.GetExtension(file);
        string dnaPath = Path.Combine(dnaFolder, fileName + ".dna");

        string workingDna = dnaPath + $".r{repetitionIndex}.work";
        string mutatedDna = workingDna + ".mutated";

        File.Copy(dnaPath, workingDna, overwrite: true);

        try
        {
            await Task.Run(() => mutator.MutateDNAFile(workingDna, mutatedDna), cancellationToken);
            await Task.Run(() => manager.ConvertDNAToFile(mutatedDna, outputFolder, mapping), cancellationToken);
        }
        finally
        {
            if (File.Exists(workingDna)) File.Delete(workingDna);
            if (File.Exists(mutatedDna)) File.Delete(mutatedDna);
        }

        cancellationToken.ThrowIfCancellationRequested();

        string decodedFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(mutatedDna));
        string finalName = baseName + "_mutated" + BuildMutationSuffix(repetitionIndex) + extension;
        string finalPath = Path.Combine(outputFolder, finalName);

        if (File.Exists(finalPath)) File.Delete(finalPath);
        File.Move(decodedFile, finalPath);

        Console.WriteLine($"  → Generated: {finalName}");
    }
}
