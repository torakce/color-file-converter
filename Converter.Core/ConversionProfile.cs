using System;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Core;

public sealed record ConversionProfile(
    string Name,
    string Device,
    string? Compression,
    int Dpi,
    IReadOnlyList<string> ExtraParameters)
{
    public static IReadOnlyList<ConversionProfile> GetDefaultProfiles() => new[]
    {
        new ConversionProfile(
            "Fax monochrome",
            "tiffg4",
            null,
            200,
            Array.Empty<string>()),
        new ConversionProfile(
            "TIFF bureau (300 dpi, niveaux de gris)",
            "tiffgray",
            "lzw",
            300,
            Array.Empty<string>()),
        new ConversionProfile(
            "TIFF couleur haute qualité",
            "tiff24nc",
            "lzw",
            300,
            Array.Empty<string>())
    };

    public ConversionProfile WithName(string name) => this with { Name = name };

    public override string ToString() => Name;

    public string Describe()
    {
        var compression = string.IsNullOrWhiteSpace(Compression) ? "Auto" : Compression;
        var extras = ExtraParameters.Count == 0 ? string.Empty : $" + {string.Join(' ', ExtraParameters)}";
        return $"{Device} • {compression} • {Dpi} dpi{extras}";
    }

    public ConversionProfile Normalize()
    {
        var extras = ExtraParameters.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray();
        return this with { ExtraParameters = extras };
    }
}
