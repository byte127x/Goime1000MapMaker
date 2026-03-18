using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Goime1000MapEditor;

public partial class NewProjectWindow : Window
{
    public bool Successful = false;
    
    public NewProjectWindow()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Successful = false;
        this.Close();
    }

    private void OK_Click(object? sender, RoutedEventArgs e)
    {
        Successful = true;
        this.Close();
    }
}