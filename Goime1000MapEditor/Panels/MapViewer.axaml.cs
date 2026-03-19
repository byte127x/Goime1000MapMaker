using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using BigGustave;
using Goime1000MapEditor.Components;

namespace Goime1000MapEditor.Panels;

public partial class MapViewer : UserControl
{
    public double ZoomFactor = 0.8;
    public double TileSize = 32;
    
    // Pan measures in units of block size
    public double PanX = 0;
    public double PanY = 0;
    
    private static int Width = 250;  // 251
    private static int Height = 400; // 400
    
    public int? SelectedTileX = null;
    public int? SelectedTileY = null;
    private int[] Tiles = new int[Width * Height];

    public IBrush[] TileBrushes = TileInfo.TileBrushes;
    
    private static uint UndoLength = 512;
    private MapAction?[] UndoStack = new MapAction[UndoLength];
    private uint UndoStackIndex = 0;
    
    public MapViewer()
    {
        InitializeComponent();
        ClearMap();

    }

    public void ClearMap()
    {
        // Fills all tiles with air
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tiles[(y * Width) + x] = 2;
            }
        }
    }
    
    // Undo / Redo
    public void Undo()
    {
        if (UndoStackIndex <= 0)
        {
            return;
        }
        MapAction LastAction = UndoStack[UndoStackIndex - 1];

        Tiles[(LastAction.Y * Width) + LastAction.X] = LastAction.Before;
        UndoStackIndex--;
    }

    public void Redo()
    {
        if (UndoStackIndex >= UndoStack.Length)
        {
            return;
        } 
        if (UndoStack[UndoStackIndex] == null)
        {
            return;
        }
        
        MapAction LastAction = UndoStack[UndoStackIndex];
        Tiles[(LastAction.Y * Width) + LastAction.X] = LastAction.After;
        UndoStackIndex++;
    }

    // Map data
    public int CurrentTile()
    {
        return Tiles[((int)SelectedTileY * Width) + (int)SelectedTileX];
    }
    
    public void SetTile(int type, int? x, int? y)
    {
        if ((x != null) && (y != null))
        {
            int before = Tiles[((int)y * Width) + (int)x];
            Tiles[((int)y * Width) + (int)x] = type;

            // IF we just came from a redo, clear the old future so we can live without the ghosts of parallel timelines
            if (UndoStack[UndoStackIndex] != null)
            {
                for (uint i = UndoStackIndex; i < UndoStack.Length; i++)
                {
                    UndoStack[i] = null;
                }
            }
            
            // Add new tile to undo stack
            MapAction CurrentAction = new MapAction()
            {
                X = x.Value, Y = y.Value, Before = before, After = type
            };
            UndoStack[UndoStackIndex] = CurrentAction;
            UndoStackIndex++;
            
            // If this happens to fill the last stack element, just cut off the first action and roll back 1 (to a empty action space)
            if (UndoStackIndex >= UndoStack.Length)
            {
                UndoStackIndex--;
                
                MapAction?[] NewStack = new MapAction?[UndoLength];
                Array.Copy(UndoStack, 1, NewStack, 0, UndoLength - 1);
                UndoStack = NewStack;
            }
        }
    }

    public void NewMap(int NewWidth, int NewHeight, bool ClearTiles)
    {
        Width = NewWidth;
        Height = NewHeight;
        UndoStack = new MapAction[UndoLength];
        UndoStackIndex = 0;

        if (ClearTiles)
        {
            Tiles = new int[Width * Height];
            ClearMap();
        }
    }

    public bool ImportText(string text)
    {
        // gets rid of those pesky \r's that windows likes, and any empty new lines that may otherwise lead to an empty row
        text = text.Replace("\r", "");
        if (text[text.Length - 1] == '\n')
        {
            text = text.Substring(0, text.Length - 1);
        }
        string[] rows = text.Split("\n");

        // Check for loadedMap= header
        if (rows[0] == "loadedMap=")
        {
            // Get the height and width based off the text (and first "real" row)
            int NewHeight = rows.Length - 1;
            int NewWidth = rows[1].Length;
            int[] NewTiles = new int[NewWidth * NewHeight];

            // Goes through text, replaces characters with tile numbers 
            for (int rowIdx = 1; rowIdx < rows.Length; rowIdx++)
            {
                string row = rows[rowIdx];
                for (int colIdx = 0; colIdx < row.Length; colIdx++)
                {
                    NewTiles[((rowIdx - 1) * NewWidth) + colIdx] = TileInfo.GetTileFromChar(row[colIdx]);
                }
            }
            
            // Switches the map's tile info to the new information, also clears undo/redo stack
            NewMap(NewWidth, NewHeight, false);
            Tiles = NewTiles;
            
            return true;
        } 
        
        return false;
    }

    public bool ImportImage(Stream stream)
    {
        Png image = Png.Open(stream);
        int NewWidth = image.Width;
        int NewHeight = image.Height;
        int[] NewTiles = new int[NewWidth * NewHeight];

        // Compare each pixel to the "color value" of each tile. If there's no match, assume air (index 2) 
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                Pixel pixel = image.GetPixel(x, y);
        
                int PixelTile = -1;
                for (int i = 0; i < TileInfo.TilePNGPixels.Length; i++)
                {
                    Pixel ComparativePixel = TileInfo.TilePNGPixels[i];
                    if (ComparativePixel.Equals(pixel))
                    {
                        PixelTile = i;
                        break;
                    }
                }

                if (PixelTile == -1)
                {
                    PixelTile = 2;
                }

                NewTiles[(y * NewWidth) + x] = PixelTile;
            }
        }

        // Switches the map's tile info to the new information, also clears undo/redo stack
        NewMap(NewWidth, NewHeight, false);
        Tiles = NewTiles;
        
        return true;
    }

    public async void ExportText(StreamWriter stream)
    {
        // Write header
        var file = new StringBuilder();
        file.Append("loadedMap=");
        
        // Go through each row in the map, convert the row to characters, then write collection of rows to the file
        for (int y = 0; y < Height; y++)
        {
            var row = new StringBuilder();
            for (int x = 0; x < Width; x++)
            {
                int TileData = Tiles[(y * Width) + x];
                char c = TileInfo.TileChars[TileData];
                row.Append(c);
            }

            string line = row.ToString();
            file.Append("\n");
            file.Append(line);
        }
        await stream.WriteLineAsync(file);
    }

    // Rendering map
    private Pen OutlineBrush = new Pen(Brushes.White, 1);
    private IBrush BackgroundBrush = new SolidColorBrush(Color.FromArgb(16, 255, 255, 255));
    public override void Render(DrawingContext context)
    {
        using (context.PushRenderOptions(new RenderOptions() { BitmapInterpolationMode = BitmapInterpolationMode.None }))
        {
            // Used for scaling properly from the center
            double CenterX = Bounds.Width / 2;
            double CenterY = Bounds.Height / 2;
        
            // Current X and Y are measured in units of tile size
            double CurrentX = 0;
            double CurrentY = PanY - 1;

            Point ScreenSizeTiles = ScreenSizeInTiles();
            double ScreenWidthTiles = ScreenSizeTiles.X;
            double ScreenHeightTiles = ScreenSizeTiles.Y;  

            // Background of drawing area
            Rect MapBounds = new Rect((PanX * TileSize) + CenterX, (PanY * TileSize) + CenterY, Width * TileSize, Height * TileSize);
            context.DrawRectangle(
                BackgroundBrush,
                null,
                MapBounds
            );
            
            // Actual tiles
            for (int y = 0; y < Height; y++)
            {
                CurrentY += 1;
            
                // Don't draw if not in the screen bounds
                if ((CurrentY < (-ScreenHeightTiles - 2) / 2) || (CurrentY > ScreenHeightTiles / 2))
                {
                    continue;
                }
            
                CurrentX = PanX - 1;
                for (int x = 0; x < Width; x++)
                {
                    CurrentX += 1;
                
                    // Don't draw if not in the screen bounds ...... but for width
                    if ((CurrentX < (-ScreenWidthTiles - 2) / 2) || (CurrentX > ScreenWidthTiles / 2))
                    {
                        continue;
                    }
                
                    // Draw tile
                    Rect bounds = new Rect(CurrentX * TileSize + CenterX, CurrentY * TileSize + CenterY, TileSize, TileSize);

                    Pen? outline = null;
                    if ((SelectedTileX == x) && (SelectedTileY == y))
                    {
                        outline = OutlineBrush;
                    }
                
                    int TileData = Tiles[(y * Width) + x];
                    IBrush TileBrush = TileBrushes[TileData];

                    context.DrawRectangle(
                        TileBrush,
                        outline,
                        bounds
                    );
                }
            }
            
            // Portal markers for reference
            for (int portalID = 0; portalID < TileInfo.PortalBrushes.Length; portalID++)
            {
                IBrush PortalBrush = TileInfo.PortalBrushes[portalID];
                
                // Renders portal at TWO locations
                for (int i = 0; i < 2; i++)
                {
                    Point NorthwestInner = TileInfo.PortalLocations[(portalID * 2) + i];
                    Rect bounds = new Rect(
                        (NorthwestInner.X + PanX - 1) * TileSize + CenterX, (NorthwestInner.Y + PanY - 1) * TileSize + CenterY,
                        4 * TileSize, 4 * TileSize
                    );
                    
                    context.DrawRectangle(
                        PortalBrush,
                        null,
                        bounds
                    );
                }
            }
        }
    }

    public Point ScreenSizeInTiles()
    {
        return new Point(
            Bounds.Width / TileSize,
            Bounds.Height / TileSize  
        );
    }
}