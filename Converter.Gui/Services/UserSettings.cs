using System;
using System.IO;
using System.Text.Json;

namespace Converter.Gui.Services;

public sealed class UserSettings
{
    public string? LastOutputFolder { get; set; }
    public string? LastProfileName { get; set; }
    public bool OpenExplorerAfterConversion { get; set; } = true;
    public bool OpenLogAfterConversion { get; set; }
    public string? FileNameSuffix { get; set; }
    public bool SeparateTiffPages { get; set; }
    public bool WatchFolderEnabled { get; set; }
    public string? WatchFolderPath { get; set; }
    public double PreviewZoom { get; set; } = 1.0;
    public bool UseInputFolder { get; set; }
    public string? LastDevice { get; set; }
    public string? LastCompression { get; set; }
    public int? LastDpi { get; set; }
    public string? LastExtraParameters { get; set; }

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static UserSettings Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                var settings = JsonSerializer.Deserialize<UserSettings>(stream, Options);
                if (settings is not null)
                {
                    return settings;
                }
            }
        }
        catch
        {
            // Ignore corrupted settings.
        }

        return new UserSettings();
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(this, Options);
        File.WriteAllText(path, json);
    }
}
