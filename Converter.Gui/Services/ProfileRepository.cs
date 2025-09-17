using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Converter.Core;

namespace Converter.Gui.Services;

internal sealed class ProfileRepository
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ProfileRepository()
    {
        var directory = GetConfigurationDirectory();
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "profiles.json");
    }

    public IReadOnlyList<ConversionProfile> Load()
    {
        if (!File.Exists(_filePath))
        {
            return ConversionProfile.GetDefaultProfiles();
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            var profiles = JsonSerializer.Deserialize<List<ConversionProfile>>(stream, Options) ?? new();
            if (profiles.Count == 0)
            {
                profiles.AddRange(ConversionProfile.GetDefaultProfiles());
            }

            return profiles
                .Select(p => p.Normalize())
                .GroupBy(p => p.Name)
                .Select(g => g.First())
                .ToArray();
        }
        catch
        {
            return ConversionProfile.GetDefaultProfiles();
        }
    }

    public void Save(IEnumerable<ConversionProfile> profiles)
    {
        var normalized = profiles
            .Select(p => p.Normalize())
            .GroupBy(p => p.Name)
            .Select(g => g.First())
            .ToArray();

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var json = JsonSerializer.Serialize(normalized, Options);
        File.WriteAllText(_filePath, json);
    }

    public static string GetConfigurationDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "ColorFileConverter");
    }
}
