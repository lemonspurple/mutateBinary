namespace deepMutate.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using deepMutate.Models.Data; 


public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _selectedFolderPath;

    // Eine Liste, die die UI automatisch aktualisiert, wenn Dateien gefunden werden
    public ObservableCollection<string> FoundFiles { get; } = new();

    [RelayCommand]
    public async Task PickFolderAsync()
    {
        // 1. Zugriff auf den StorageProvider von Avalonia
        var topLevel = TopLevel.GetTopLevel((Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
        
        if (topLevel == null) return;

        // 2. Ordner-Dialog öffnen
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Wähle den DeepMutate Quellordner",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            // Pfad aus dem Dialog holen
            SelectedFolderPath = folders[0].Path.LocalPath;

            // 3. Dein Model benutzen
            var manager = new FileManager();
            var files = manager.GetFilesInFolder(SelectedFolderPath);

            // 4. Ergebnisse in der UI anzeigen
            FoundFiles.Clear();
            foreach (var file in files)
            {
                FoundFiles.Add(Path.GetFileName(file)); // Nur Dateiname für die Optik
            }
        }
    }
}
