using Avalonia.Controls;
using System.Diagnostics;

namespace mutateBinary.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void AboutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => AboutPopup.IsOpen = !AboutPopup.IsOpen;

    private void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => AboutPopup.IsOpen = false;

    private void OpenGitHub_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = "https://github.com/lemonspurple/mutateBinary", UseShellExecute = true });
        AboutPopup.IsOpen = false;
    }
}