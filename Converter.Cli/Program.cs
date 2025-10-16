using System;
using System.Collections.Generic;
using Converter.Core;

static void ShowHelp()
{
    Console.WriteLine(@"Color File Converter CLI - Convertisseur PDF vers TIFF

Usage:
    Converter.Cli --input <fichier.pdf> --output <sortie.tif> [options]

Arguments requis:
    --input <path>        Fichier PDF d'entrée
    --output <path>       Fichier TIFF de sortie (utiliser %03d pour multi-pages)

Options:
    --device <device>     Device Ghostscript (défaut: tiffg4)
                         Valeurs: tiffg4, tiff24nc, tiffgray, etc.
    --dpi <number>        Résolution en DPI (défaut: 300)
    --compression <type>  Type de compression (lzw, zip, g4, etc.)
    --gs-arg <arg>        Arguments Ghostscript supplémentaires (répétable)
    --help, -h, /?        Affiche cette aide

Exemples:
    Converter.Cli --input doc.pdf --output doc.tif
    Converter.Cli --input doc.pdf --output page_%03d.tif --device tiff24nc --dpi 600
    Converter.Cli --input doc.pdf --output doc.tif --compression lzw --gs-arg ""-dFirstPage=1""
");
}

static (string?, string?, string, int, string?, List<string>) Parse(string[] args)
{
    if (args.Length == 0 || args[0] == "--help" || args[0] == "-h" || args[0] == "/?")
    {
        ShowHelp();
        Environment.Exit(0);
    }

    string? input = null, output = null;
    string device = "tiffg4";
    int dpi = 300;
    string? compression = null;
    var extraParameters = new List<string>();

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--input":
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--input requiert un chemin de fichier");
                }
                input = args[++i];
                break;
            case "--output":
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--output requiert un chemin de fichier");
                }
                output = args[++i];
                break;
            case "--device":
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--device requiert une valeur");
                }
                device = args[++i];
                break;
            case "--dpi":
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--dpi requiert un nombre");
                }
                if (!int.TryParse(args[++i], out dpi) || dpi < 72 || dpi > 2400)
                {
                    throw new ArgumentException("--dpi doit être entre 72 et 2400");
                }
                break;
            case "--compression":
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--compression requiert une valeur");
                }
                compression = args[++i];
                break;
            case "--gs-arg":
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--gs-arg requiert une valeur");
                }
                extraParameters.Add(args[++i]);
                break;
            case "--help":
            case "-h":
            case "/?":
                ShowHelp();
                Environment.Exit(0);
                break;
            default:
                Console.Error.WriteLine($"Option inconnue: {args[i]}");
                Console.Error.WriteLine("Utilisez --help pour voir les options disponibles");
                Environment.Exit(2);
                break;
        }
    }
    return (input, output, device, dpi, compression, extraParameters);
}

try
{
    var (input, output, device, dpi, compression, extraParameters) = Parse(args);
    
    if (input is null || output is null)
    {
        Console.Error.WriteLine("Erreur: --input et --output sont obligatoires");
        Console.Error.WriteLine("Utilisez --help pour voir les options disponibles");
        Environment.Exit(2);
    }

    if (!System.IO.File.Exists(input))
    {
        Console.Error.WriteLine($"Erreur: Le fichier d'entrée n'existe pas: {input}");
        Environment.Exit(1);
    }

    Console.WriteLine($"Conversion en cours...");
    Console.WriteLine($"  Entrée: {input}");
    Console.WriteLine($"  Sortie: {output}");
    Console.WriteLine($"  Device: {device}");
    Console.WriteLine($"  DPI: {dpi}");
    if (!string.IsNullOrEmpty(compression))
    {
        Console.WriteLine($"  Compression: {compression}");
    }

    await GhostscriptRunner.ConvertPdfToTiffAsync(input!, output!, device, dpi, compression, extraParameters);
    
    Console.WriteLine("✓ Conversion réussie");
    Environment.Exit(0);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Erreur d'argument: {ex.Message}");
    Console.Error.WriteLine("Utilisez --help pour voir les options disponibles");
    Environment.Exit(2);
}
catch (System.IO.FileNotFoundException ex)
{
    Console.Error.WriteLine($"Erreur: Fichier non trouvé - {ex.Message}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erreur: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.Error.WriteLine($"Détails: {ex.InnerException.Message}");
    }
    Environment.Exit(1);
}
