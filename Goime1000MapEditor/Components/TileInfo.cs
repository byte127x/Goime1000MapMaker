using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BigGustave;
using SkiaSharp;

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

    public static Point[] PortalLocations = new Point[]
    {
        // The portal proper takes up 4 tiles (while the image itself extends 2 tiles outwords for the glow)
        // Each point represents the NORTHWEST tile of the 4 inner tiles

        new Point(230, 20), new Point(130, 317),
        new Point(118, 322), new Point(169, 356),
        new Point(121, 322), new Point(1, 397),
        new Point(52, 388), new Point(45, 168),
        new Point(48, 397), new Point(203, 79),
        new Point(101, 124), new Point(237, 388),
        new Point(233, 389), new Point(168, 366),
        new Point(239, 367), new Point(157, 209),
        new Point(147, 167), new Point(209, 254),
    };

    public static IBrush[] PortalBrushes = new IBrush[]
    {
        LoadImage("avares://Goime1000MapEditor/Assets/portal/portal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/greenportal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/redportal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/maroonportal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/yellowportal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/tennisballportal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/blueportal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/orangeportal.png"),
        LoadImage("avares://Goime1000MapEditor/Assets/portal/blackportal.png"),
    };

    public static IBrush CourseStartBrush = LoadImage("avares://Goime1000MapEditor/Assets/coursestart.png");
    public static IBrush CourseEndBrush = LoadImage("avares://Goime1000MapEditor/Assets/courseend.png");

    public static Point[] CourseLocations = new Point[]
    {
        // Points here represent the NW corner of the tile, alternating between starting course and ending course flags
        // Each start takes up a 3x2 tile area, ends take up a 2x2 tile area
        new Point(28, 384), new Point(52, 395),
        new Point(8, 376), new Point(231, 369),
        new Point(93, 395), new Point(141, 368),
        
        new Point(122, 175), new Point(151, 167),
        new Point(128, 173), new Point(218, 176),
    };

    private static Pixel CreatePixel(int r, int g, int b, int a)
    {
        return new Pixel((byte)r, (byte)g, (byte)b, (byte)a, false);
    }

    public static Pixel[] TilePNGPixels = new Pixel[]
    {
        CreatePixel(0, 0, 70, 255),
        CreatePixel(0, 0, 102, 255),
        CreatePixel(0, 0, 0, 0),
        CreatePixel(40, 24, 24, 255),
        CreatePixel(64, 48, 48, 255),
        CreatePixel(255, 204, 0, 255),
        CreatePixel(255, 255, 120, 255),
        CreatePixel(166, 166, 166, 255),
        CreatePixel(255, 255, 0, 255),
        CreatePixel(214, 214, 214, 255),
        CreatePixel(191, 64, 191, 255),
        CreatePixel(199, 149, 56, 255),
        CreatePixel(240, 240, 240, 255),
        
        CreatePixel(0, 0, 0, 255),
        CreatePixel(102, 102, 102, 255),
        CreatePixel(255, 255, 255, 255),
        CreatePixel(153, 0, 0, 255),
        CreatePixel(255, 238, 32, 255),
        CreatePixel(51, 153, 51, 255),
        CreatePixel(0, 136, 255, 255),
        CreatePixel(224, 224, 255, 255),
    };
    
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
        Console.Write("This tile is not yet supported : ");
        Console.WriteLine(c);
        return 2;
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
    
    public static SKBitmap LoadTileSkia(Uri uri, int size)
    {
        return LoadTileSkia(uri, size, size);
    }
    
    public static SKBitmap LoadTileSkia(Uri uri, int width, int height)
    {
        Stream stream = AssetLoader.Open(uri);
        SKBitmap original = SKBitmap.Decode(stream);

        SKBitmap bitmap = new SKBitmap(new SKImageInfo(width, height));
        original.ScalePixels(bitmap, SKFilterQuality.Low);
        return bitmap;
    }
    
    // TileBrushes have two entries for the spawn, but every other array has start as one block
    public static Color?[] TileSolidColor =
    {
        null,
        null,
        Colors.Transparent,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        
        Colors.Black,
        Color.FromRgb(102, 102, 102),
        Colors.White,
        Color.FromRgb(153, 0, 0),
        Color.FromRgb(255, 238, 32),
        Color.FromRgb(51, 153, 51),
        Color.FromRgb(0, 136, 255),
        null,
    };

    public static string?[] TileImageUris = new string[]
    {
        "avares://Goime1000MapEditor/Assets/spawn_l.png",
        "avares://Goime1000MapEditor/Assets/spawn_r.png",
        null,
        "avares://Goime1000MapEditor/Assets/spikes.png",
        "avares://Goime1000MapEditor/Assets/grayspikes.png",
        "avares://Goime1000MapEditor/Assets/fire.png",
        "avares://Goime1000MapEditor/Assets/coin.png",
        "avares://Goime1000MapEditor/Assets/colorwheel.png",
        "avares://Goime1000MapEditor/Assets/guy.png",
        "avares://Goime1000MapEditor/Assets/paintable.png",
        "avares://Goime1000MapEditor/Assets/trampoline.png",
        "avares://Goime1000MapEditor/Assets/wood.png",
        "avares://Goime1000MapEditor/Assets/platform.png",
        
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        "avares://Goime1000MapEditor/Assets/invisible.png"
    };
    
    public static IBrush[] TileBrushes =
    {
        LoadImage(TileImageUris[0]),
        LoadImage(TileImageUris[1]),
        Brushes.Transparent,
        LoadImage(TileImageUris[3]),
        LoadImage(TileImageUris[4]),
        LoadImage(TileImageUris[5]),
        LoadImage(TileImageUris[6]),
        LoadImage(TileImageUris[7]),
        LoadImage(TileImageUris[8]),
        LoadImage(TileImageUris[9]),
        LoadImage(TileImageUris[10]),
        LoadImage(TileImageUris[11]),
        LoadImage(TileImageUris[12]),
        
        Brushes.Black,
        new SolidColorBrush(TileSolidColor[14].Value),
        Brushes.White,
        new SolidColorBrush(TileSolidColor[16].Value),
        new SolidColorBrush(TileSolidColor[17].Value),
        new SolidColorBrush(TileSolidColor[18].Value),
        new SolidColorBrush(TileSolidColor[19].Value),
        LoadImage(TileImageUris[20]),
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