using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Core;

/// <summary>
/// Version améliorée de GhostscriptRunner avec cache et meilleure gestion d'erreurs.
/// Renommez ce fichier en GhostscriptRunner.cs pour l'utiliser.
/// </summary>
public static class GhostscriptRunnerImproved
{
    private static string? _cachedGhostscriptPath;
    private static readonly object _cacheLock = new object();

    /// <summary>
    /// Convertit un PDF en fichiers TIFF en utilisant Ghostscript.
    /// </summary>
    /// <param name="inputPdf">Chemin du fichier PDF d'entrée</param>
    /// <param name="outputPattern">Motif de sortie (ex: output.tif ou output_%03d.tif pour multi-pages)</param>
    /// <param name="device">Device Ghostscript (tiffg4, tiff24nc, tiffgray, etc.)</param>
    /// <param name="dpi">Résolution en DPI</param>
    /// <param name="compression">Type de compression (lzw, zip, g4, etc.)</param>
    /// <param name="extraParameters">Paramètres Ghostscript supplémentaires</param>
    /// <param name="firstPage">Première page à convertir (optionnel)</param>
    /// <param name="lastPage">Dernière page à convertir (optionnel)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <exception cref="FileNotFoundException">Le fichier PDF n'existe pas</exception>
    /// <exception cref="InvalidOperationException">Ghostscript ne peut pas être démarré</exception>
    /// <exception cref="Exception">La conversion a échoué</exception>
    public static async Task ConvertPdfToTiffAsync(
        string inputPdf,
        string outputPattern,
        string device = "tiffg4",
        int dpi = 300,
        string? compression = null,
        IEnumerable<string>? extraParameters = null,
        int? firstPage = null,
        int? lastPage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputPdf);
        ArgumentNullException.ThrowIfNull(outputPattern);

        if (!File.Exists(inputPdf))
        {
            throw new FileNotFoundException($"Le fichier PDF n'existe pas: {inputPdf}", inputPdf);
        }

        if (dpi < 72 || dpi > 2400)
        {
            throw new ArgumentOutOfRangeException(nameof(dpi), "Le DPI doit être entre 72 et 2400");
        }

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
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = new Process { StartInfo = psi };
        if (!p.Start())
        {
            throw new InvalidOperationException($"Impossible de démarrer Ghostscript à partir de: {exe}");
        }

        var stdoutTask = p.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = p.StandardError.ReadToEndAsync(cancellationToken);

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
                // Ignoré : le processus peut déjà être terminé
            }
        });

        try
        {
            await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Le processus a déjà été tué par le callback d'annulation
            throw;
        }

        string stdout = await stdoutTask.ConfigureAwait(false);
        string stderr = await stderrTask.ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (p.ExitCode != 0)
        {
            var errorMessage = $"Ghostscript a échoué (code {p.ExitCode})";
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                errorMessage += $"\nErreur: {stderr}";
            }
            if (!string.IsNullOrWhiteSpace(stdout))
            {
                errorMessage += $"\nSortie: {stdout}";
            }
            throw new Exception(errorMessage);
        }
    }

    /// <summary>
    /// Alias pour ConvertPdfToTiffAsync pour compatibilité.
    /// </summary>
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

    /// <summary>
    /// Résout le chemin de l'exécutable Ghostscript avec cache.
    /// </summary>
    /// <returns>Chemin de l'exécutable Ghostscript</returns>
    static string ResolveGhostscriptExe()
    {
        // Vérifier le cache d'abord
        lock (_cacheLock)
        {
            if (_cachedGhostscriptPath != null)
            {
                return _cachedGhostscriptPath;
            }
        }

        string? resolvedPath = null;

        // 1) Variable d'environnement explicite
        var fromEnv = Environment.GetEnvironmentVariable("GHOSTSCRIPT_EXE");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            resolvedPath = fromEnv;
            lock (_cacheLock)
            {
                _cachedGhostscriptPath = resolvedPath;
            }
            return resolvedPath;
        }

        // 2) Dossier portable "Resources/Ghostscript" à côté de l'exécutable
        string baseDirectory = AppContext.BaseDirectory;
        string resourcesFolder = Path.Combine(baseDirectory, "Resources", "Ghostscript");
        
        // Essayer aussi le dossier "Ghostscript" direct (pour compatibilité)
        var ghostscriptPaths = new[]
        {
            resourcesFolder,
            Path.Combine(baseDirectory, "Ghostscript")
        };
        
        foreach (var ghostscriptFolder in ghostscriptPaths)
        {
            if (Directory.Exists(ghostscriptFolder))
            {
                var candidates = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new[] { "gswin64c.exe", "gswin32c.exe", "gswin64.exe", "gswin32.exe" }
                    : new[] { "gs", "gsx" };

                foreach (var candidate in candidates)
                {
                    var path = Path.Combine(ghostscriptFolder, candidate);
                    if (File.Exists(path))
                    {
                        resolvedPath = path;
                        lock (_cacheLock)
                        {
                            _cachedGhostscriptPath = resolvedPath;
                        }
                        return resolvedPath;
                    }
                }
            }
        }

        // 3) Nom "classique" selon l'OS (nécessite que Ghostscript soit dans le PATH)
        resolvedPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "gswin64c" : "gs";
        
        lock (_cacheLock)
        {
            _cachedGhostscriptPath = resolvedPath;
        }
        
        return resolvedPath;
    }

    /// <summary>
    /// Réinitialise le cache du chemin Ghostscript (utile pour les tests).
    /// </summary>
    public static void ResetCache()
    {
        lock (_cacheLock)
        {
            _cachedGhostscriptPath = null;
        }
    }

    /// <summary>
    /// Quote un chemin s'il contient des espaces.
    /// </summary>
    static string Quote(string s) => s.Contains(' ') ? $"\"{s}\"" : s;

    /// <summary>
    /// Obtient le paramètre de compression approprié selon le device.
    /// </summary>
    static string? GetCompressionParameter(string compression, string device)
    {
        // Pour les devices TIFF, certaines compressions nécessitent un traitement spécial
        if (device.StartsWith("tiff", StringComparison.OrdinalIgnoreCase))
        {
            return compression.ToLower() switch
            {
                "lzw" => "-sCompression=lzw",
                "zip" => "-sCompression=zip", // Aussi connu comme deflate
                "packbits" => "-sCompression=packbits",
                "g3" => "-sCompression=g3",
                "g4" => "-sCompression=g4",
                "jpeg" => "-sCompression=jpeg",
                "none" or "null" => null, // Pas de paramètre de compression
                _ => $"-sCompression={compression}"
            };
        }
        
        // Pour les autres devices, utiliser la compression telle quelle
        return string.IsNullOrWhiteSpace(compression) ? null : $"-sCompression={compression}";
    }
}
