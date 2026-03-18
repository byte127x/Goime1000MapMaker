using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Goime1000MapEditor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        // Keyboard Shortcuts (undo, redo, etc)
        if ((e.Key == Key.Z) && (e.KeyModifiers == KeyModifiers.Control))
        {
            Menu_Undo(null, null);
        }
        
        // Converting the enums to ints is a hacky way of checking for both key modifiers (Shift = 4 or 0x0100, Control = 2 or 0x0010, Shift+Control = 6 or 0x0110)
        bool ControlAndShift = ((int)e.KeyModifiers) == 6;
        
        if (((e.Key == Key.Z) && ControlAndShift) || ((e.Key == Key.Y) && (e.KeyModifiers == KeyModifiers.Control)))
        {
            Menu_Redo(null, null);
        }
        
        // Keyboard panning
        if (e.Key == Key.W)
        {
            MapViewerPanel.PanY += 1;
        }
        else if (e.Key == Key.A)
        {
            MapViewerPanel.PanX += 1;
        }
        else if (e.Key == Key.S)
        {
            MapViewerPanel.PanY -= 1;
        }
        else if (e.Key == Key.D)
        {
            MapViewerPanel.PanX -= 1;
        }
        
        // Force map viewer to update to show panning
        MapViewerPanel.InvalidateVisual();
    }

    // Clicking and dragging for mouse panning
    private void SetCurrentTile(PointerEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed)
        {
            int TileType = BrushSelection.SelectedBrush;

            if (TileType + 1 == MapViewerPanel.CurrentTile())
            {
                return;
            }
            
            MapViewerPanel.SetTile(TileType + 1, MapViewerPanel.SelectedTileX, MapViewerPanel.SelectedTileY);
            
            // If placing a spawn (type 0), includes left part of spawn as well
            if (TileType == 0)
            {
                if (MapViewerPanel.SelectedTileX > 0)
                {
                    MapViewerPanel.SetTile(0, MapViewerPanel.SelectedTileX - 1, MapViewerPanel.SelectedTileY);                    
                }
            }            
        } else if (e.Properties.IsRightButtonPressed)
        {
            if (MapViewerPanel.CurrentTile() == 2)
            {
                return;
            }
            
            // Use air tiles (erasing) on right click
            MapViewerPanel.SetTile(2, MapViewerPanel.SelectedTileX, MapViewerPanel.SelectedTileY);
        }
            
        // Force map viewer to update to show new tiles
        MapViewerPanel.InvalidateVisual();
    }
    
    private Point StartingPan = new Point(0, 0);
    private Point StartingMousePos = new Point(0, 0);

    private Point? LastTileInCurrentStroke = null;
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsMiddleButtonPressed) {
            StartingPan = new Point(MapViewerPanel.PanX, MapViewerPanel.PanY);
            StartingMousePos = e.GetCurrentPoint(MapViewerPanel).Position;
        }

        if (MapViewerPanel.IsPointerOver)
        {
            SetCurrentTile(e);
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LastTileInCurrentStroke = null;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (MapViewerPanel.IsPointerOver)
        {
            SetCurrentTile(e);
        }
        
        Point CurrentMousePos = e.GetCurrentPoint(MapViewerPanel).Position;

        if (!(MapViewerBackground.IsPointerOver || MapViewerPanel.IsPointerOver)) {return;}
        
        if (e.Properties.IsMiddleButtonPressed)
        {
            // Calculate distance mouse has moved, convert it into tile units, and change the panning
            Point Delta = CurrentMousePos - StartingMousePos;
            Point DeltaTiles = StartingPan + (Delta / MapViewerPanel.TileSize);

            MapViewerPanel.PanX = DeltaTiles.X;
            MapViewerPanel.PanY = DeltaTiles.Y;
            
            // Force map viewer to update to show panning
            MapViewerPanel.InvalidateVisual();
        }
        else
        {
            // Convert mouse position to tiles, and pan it by the screen's center AND the existing map pan
            Point Pan = new Point(MapViewerPanel.PanX, MapViewerPanel.PanY);
            Point ScreenSize = MapViewerPanel.ScreenSizeInTiles();
            Point TilePoint = (CurrentMousePos / MapViewerPanel.TileSize) - (ScreenSize / 2) - Pan;

            int PointX = (int)Math.Floor(TilePoint.X);
            int PointY = (int)Math.Floor(TilePoint.Y);
            
            // Only set the point if its within the map range
            if (((PointX >= 0) || (PointY >= 0)) || (PointX < MapViewerPanel.Width) || (PointY < MapViewerPanel.Height))
            {
                MapViewerPanel.SelectedTileX = PointX;
                MapViewerPanel.SelectedTileY = PointY;
            }
            else
            {
                MapViewerPanel.SelectedTileX = null;
                MapViewerPanel.SelectedTileY = null;
            }
            
            // Force map viewer to update to show selection
            MapViewerPanel.InvalidateVisual();
        }
    }

    private void OnPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        double ZoomFactor = MapViewerPanel.ZoomFactor;
        if (e.Delta.Y < 0)
        {
            ZoomFactor = 1 / MapViewerPanel.ZoomFactor;
        }
        
        // Zoom in or out of map
        
        // Scale tile size
        double OldTileSize = MapViewerPanel.TileSize;
        MapViewerPanel.TileSize /= ZoomFactor;

        // Converts old panning to pixels, zooms in/out, then converts to new tile sizing
        MapViewerPanel.PanX = ((((MapViewerPanel.PanX * OldTileSize) )/ ZoomFactor) )/ MapViewerPanel.TileSize;
        MapViewerPanel.PanY = ((((MapViewerPanel.PanY * OldTileSize) )/ ZoomFactor) )/ MapViewerPanel.TileSize;
        
        // Force map viewer to update to show zooming
        MapViewerPanel.InvalidateVisual();
    }

    // Menu Items
    private async void Menu_About(object? sender, RoutedEventArgs e)
    {
        var about = new AboutWindow();
        await about.ShowDialog(this);
    }


    private async void Menu_Open(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);

        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Map ...",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain, FilePickerFileTypes.ImagePng },
            SuggestedFileType = FilePickerFileTypes.TextPlain
        });

        if (files.Count >= 1)
        {
            IStorageFile file = files[0];
            string extension = Path.GetExtension(file.Name);
            
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);

            bool importSuccess = false;
            if (extension == ".txt")
            {
                var fileContent = await streamReader.ReadToEndAsync();
                importSuccess = MapViewerPanel.ImportText(fileContent);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            MapViewerPanel.InvalidateVisual();

            if (!importSuccess)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Map has failed to load.", ButtonEnum.Ok);
                await box.ShowAsPopupAsync(this);
            }
        }
    }

    private async void Menu_Save(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);

        var file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Map ...",
            FileTypeChoices = new[] { FilePickerFileTypes.TextPlain },
            SuggestedFileName = "map.txt"
        });

        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            await using var streamWriter = new StreamWriter(stream);
            
            MapViewerPanel.ExportText(streamWriter);
        }
    }

    private void Menu_Undo(object? sender, RoutedEventArgs e)
    {
        MapViewerPanel.Undo();
    }
    
    private void Menu_Redo(object? sender, RoutedEventArgs e)
    {
        MapViewerPanel.Redo();
    }
}