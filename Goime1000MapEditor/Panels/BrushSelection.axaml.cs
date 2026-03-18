using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Goime1000MapEditor.Components;

namespace Goime1000MapEditor.Panels;

public partial class BrushSelection : UserControl
{
    public int SelectedBrush = 0;
    
    public BrushSelection()
    {
        InitializeComponent();
        
        // Spawn brush
        Components.TileBrush SpawnBrush = new Components.TileBrush()
        {
            IsSelected = true,
            TileDescription = TileInfo.TileDescriptions[0],
            Background = TileInfo.SpawnFullBrush,
            OnClick = OnBrushSelected
        };
        SpawnBrush.Width = SpawnBrush.Height * 2;
        InnerPanel.Children.Add(SpawnBrush);
        
        // Other tile brushes
        for (int i = 1; i < TileInfo.TileDescriptions.Length; i++)
        {
            string Description = TileInfo.TileDescriptions[i];
            IBrush TileBrush = TileInfo.TileBrushes[i + 1];
            
            Components.TileBrush NewBrush = new Components.TileBrush()
            {
                TileID = i,
                TileDescription = Description,
                Background = TileBrush,
                OnClick = OnBrushSelected
            };
            InnerPanel.Children.Add(NewBrush);
        }
    }

    public int OnBrushSelected(int tileBrush)
    {
        // Deselect all brushes and then select the newly selected brush
        SelectedBrush = tileBrush;
    
        foreach (var child in InnerPanel.Children)
        {
            Components.TileBrush? brush = child as Components.TileBrush;

            if (brush != null)
            {
                if (brush.TileID == SelectedBrush)
                {
                    brush.IsSelected = true;
                }
                else
                {
                    brush.IsSelected = false;
                }
            }
        }
        
        return 0;
    }
}