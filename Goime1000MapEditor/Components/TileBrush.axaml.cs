using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Goime1000MapEditor.Components;

public partial class TileBrush : UserControl
{
    public int TileID = 0;
    //public string TileDescription = "";

    public static readonly StyledProperty<string> TileDescriptionProperty =
        AvaloniaProperty.Register<TileBrush, string>(nameof(TileDescriptionProperty), defaultValue: "Brush");
    public string TileDescription
    {
        get { return GetValue(TileDescriptionProperty); }
        set { SetValue(TileDescriptionProperty, value); }
    }
    
    public static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<TileBrush, IBrush>(nameof(BackgroundProperty), defaultValue: Brushes.Aquamarine);
    public IBrush Background
    {
        get { return GetValue(BackgroundProperty); }
        set { SetValue(BackgroundProperty, value); }
    }
    
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<TileBrush, bool>(nameof(IsSelectedProperty), defaultValue: false);
    public bool IsSelected
    {
        get { return GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }

    public Func<int, int>? OnClick = null;
    
    public TileBrush()
    {
        InitializeComponent();
    }

    private void OnPointerRelease(object? sender, PointerReleasedEventArgs e)
    {
        if (IsPointerOver && (OnClick != null))
        {
            OnClick(TileID);
        }
    }
}