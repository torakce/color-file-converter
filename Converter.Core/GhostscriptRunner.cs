using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Core;

public static class GhostscriptRunner
{
    public static async Task ConvertPdfToTiffAsync(
        string inputPdf,
        string outputPattern,
        string device = "tiffg4",   // "tiffg4" (NB, CCITT G4), "tiff24nc" (couleur 24b)
        int dpi = 300,
        string? compression = null,
        IEnumerable<string>? extraParameters = null,
        int? firstPage = null,
        int? lastPage = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPdf)) throw new FileNotFoundException(inputPdf);

        string exe = ResolveGhostscriptExe();
        // Arguments Ghostscript
        var args = new List<string>
        {
            "-dBATCH",
            "-dNOPAUSE",
            "-dSAFER",
            $"-sDEVICE={device}",
            $"-r{dpi}"
        };

        if (firstPage is not null)
        {
            args.Add($"-dFirstPage={firstPage}");
        }

        if (lastPage is not null)
        {
            args.Add($"-dLastPage={lastPage}");
        }

        if (!string.IsNullOrWhiteSpace(compression))
        {
            // Handle different compression types properly for TIFF
            var compressionParam = GetCompressionParameter(compression, device);
            if (!string.IsNullOrWhiteSpace(compressionParam))
            {
                args.Add(compressionParam);
            }
        }

        if (extraParameters is not null)
        {
            foreach (var parameter in extraParameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter))
                {
                    args.Add(parameter.Trim());
                }
            }
        }

        args.Add($"-sOutputFile={Quote(outputPattern)}");
        args.Add(Quote(inputPdf));

        string arguments = string.Join(' ', args);

        var psi = new ProcessStartInfo(exe, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = new Process { StartInfo = psi };
        if (!p.Start())
        {
            throw new InvalidOperationException("Impossible de démarrer Ghostscript.");
        }

        var stdoutTask = p.StandardOutput.ReadToEndAsync();
        var stderrTask = p.StandardError.ReadToEndAsync();

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                if (!p.HasExited)
                {
                    p.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignoré : le processus peut déjà être terminé.
            }
        });

        try
        {
            await p.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        string stdout = await stdoutTask;
        string stderr = await stderrTask;

        cancellationToken.ThrowIfCancellationRequested();

        if (p.ExitCode != 0)
        {
            throw new Exception($"Ghostscript a échoué (code {p.ExitCode}).\n{stderr}\n{stdout}");
        }
    }

    public static Task RenderPdfAsync(
        string inputPdf,
        string outputPattern,
        string device,
        int dpi,
        string? compression = null,
        IEnumerable<string>? extraParameters = null,
        int? firstPage = null,
        int? lastPage = null,
        CancellationToken cancellationToken = default)
        => ConvertPdfToTiffAsync(
            inputPdf,
            outputPattern,
            device,
            dpi,
            compression,
            extraParameters,
            firstPage,
            lastPage,
            cancellationToken);

    static string ResolveGhostscriptExe()
    {
        // 1) Variable env explicite
        var fromEnv = Environment.GetEnvironmentVariable("GHOSTSCRIPT_EXE");
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv;

        // 2) Dossier local "Ghostscript" à côté de l'exécutable
        string baseDirectory = AppContext.BaseDirectory;
        string localFolder = Path.Combine(baseDirectory, "Ghostscript");
        if (Directory.Exists(localFolder))
        {
            var candidates = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[] { "gswin64c.exe", "gswin32c.exe", "gswin64c.cmd", "gswin32c.cmd" }
                : new[] { "gs", "gsx" };

            foreach (var candidate in candidates)
            {
                var path = Path.Combine(localFolder, candidate);
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        // 3) Nom “classique” selon l’OS (nécessite que PATH soit OK)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "gswin64c"; // ou gswin32c selon install
        return "gs"; // macOS / Linux
    }

    static string Quote(string s) => s.Contains(' ') ? $"\"{s}\"" : s;

    static string? GetCompressionParameter(string compression, string device)
    {
        // For TIFF devices, some compressions need special handling
        if (device.StartsWith("tiff", StringComparison.OrdinalIgnoreCase))
        {
            return compression.ToLower() switch
            {
                "lzw" => "-sCompression=lzw",
                "zip" => "-sCompression=zip", // Also known as deflate
                "packbits" => "-sCompression=packbits",
                "g3" => "-sCompression=g3",
                "g4" => "-sCompression=g4",
                "jpeg" => "-sCompression=jpeg",
                "none" or "null" => null, // No compression parameter
                _ => $"-sCompression={compression}"
            };
        }
        
        // For other devices, use the compression as-is
        return string.IsNullOrWhiteSpace(compression) ? null : $"-sCompression={compression}";
    }
}
