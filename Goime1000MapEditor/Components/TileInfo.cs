using System;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Goime1000MapEditor.Components;

public class TileInfo
{
    // Map Properties
    
    /*
       TYPES (text from athena):
           #   NAME        TEXT
           00  SPAWN_L     <
           01  SPAWN_R     =
           02  air         .

           INTERACTABLE:
           03  spike       :
           04  grayspike   2
           05  fire        6

           06  coin        ;
           07  colorwheel  >
           08  guy         C
           09  paintable   A

           BLOCKS:
           10  trampoline  5
           11  wood        0
           12  temporary   @

           13  black       5
           14  gray        4
           15  white       7
           16  red         /
           17  yellow      3
           18  green       1
           19  blue        8
           20  invisible   ?

     */

    public static char[] TileChars = new char[]
    {
        '<',
        '=',
        '.',
        ':',
        '2',
        '6',
        ';',
        '>',
        'C',
        'A',
        '5',
        '0',
        '@',
        '9',
        '4',
        '7',
        '/',
        '3',
        '1',
        '8',
        '?',
    };
    
    public static int GetTileFromChar(char c)
    {
        for (int i = 0; i < TileChars.Length; i++)
        {
            if (TileChars[i] == c)
            {
                return i;
            }
        }
        Console.Write("funny number happened at : ");
        Console.WriteLine(c);
        return -1;
    }

    public static IBrush LoadImage(string uri)
    {
        // Loads image from uri into a brush
        return new ImageBrush(new Bitmap(AssetLoader.Open(new Uri(uri))))
        {
            // Without these parameters the images are invisible on the map on MacOS, but with them they are at least visible if partially offset (might be an avalonia bug?)
            Stretch = Stretch.Fill,
            TileMode = TileMode.Tile,
        }
        ;
    }
    
    // TileBrushes have two entries for the spawn, but every other array has start as one block
    public static IBrush[] TileBrushes =
    {
        LoadImage("avares://Goime1000MapEditor/Assets/spawn_l.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/spawn_r.png"),
        //new SolidColorBrush(Color.FromArgb(12, 255, 255, 255)),
        Brushes.Transparent,
        LoadImage("avares://Goime1000MapEditor/Assets/spikes.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/grayspikes.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/fire.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/coin.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/colorwheel.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/guy.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/paintable.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/trampoline.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/wood.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/platform.png"),
        
        Brushes.Black,
        new SolidColorBrush(Color.FromRgb(102, 102, 102)),
        Brushes.White,
        new SolidColorBrush(Color.FromRgb(153, 0, 0)),
        new SolidColorBrush(Color.FromRgb(255, 238, 32)),
        new SolidColorBrush(Color.FromRgb(51, 153, 51)),
        new SolidColorBrush(Color.FromRgb(0, 136, 255)),
        LoadImage("avares://Goime1000MapEditor/Assets/invisible.png"),
    };

    public static IBrush SpawnFullBrush =
        LoadImage("avares://Goime1000MapEditor/Assets/spawn.png");

    public static string[] TileDescriptions =
    {
        "Spawn Tile",
        "Air",
        "Spike",
        "Gray Spike",
        "Fire",
        "Coin",
        "Color Wheel",
        "Guy",
        "Paintable Block",
        "Trampoline",
        "Wood",
        "Temporary Platform",
        
        "Black Block",
        "Gray Block",
        "White Block",
        "Red Block",
        "Yellow Block",
        "Green Block",
        "Blue Block",
        "Invisible Block",
    };
}