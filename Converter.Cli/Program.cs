using System;
using System.Collections.Generic;
using Converter.Core;

static (string?, string?, string, int, string?, List<string>) Parse(string[] args)
{
    string? input = null, output = null;
    string device = "tiffg4";
    int dpi = 300;
    string? compression = null;
    var extraParameters = new List<string>();

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--input":  input  = args[++i]; break;
            case "--output": output = args[++i]; break;           // ex: out-%03d.tif
            case "--device": device = args[++i]; break;           // tiffg4|tiff24nc|tiffgray…
            case "--dpi":    dpi    = int.Parse(args[++i]); break;
            case "--compression": compression = args[++i]; break;
            case "--gs-arg":
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--gs-arg requiert une valeur");
                }
                extraParameters.Add(args[++i]);
                break;
            default: break;
        }
    }
    return (input, output, device, dpi, compression, extraParameters);
}

var (input, output, device, dpi, compression, extraParameters) = Parse(args);
if (input is null || output is null)
{
    Console.Error.WriteLine("Usage: Converter.Cli --input in.pdf --output out.tif [--device tiffg4|tiff24nc] [--dpi 300] [--compression lzw] [--gs-arg ...]");
    Environment.Exit(2);
}

try
{
    await GhostscriptRunner.ConvertPdfToTiffAsync(input!, output!, device, dpi, compression, extraParameters);
    Console.WriteLine("OK");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
}
