using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        IEnumerable<string>? extraParameters = null)
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

        if (!string.IsNullOrWhiteSpace(compression))
        {
            args.Add($"-sCompression={compression}");
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

        using var p = Process.Start(psi)!;
        string stdout = await p.StandardOutput.ReadToEndAsync();
        string stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();

        if (p.ExitCode != 0)
            throw new Exception($"Ghostscript a échoué (code {p.ExitCode}).\n{stderr}\n{stdout}");
    }

    static string ResolveGhostscriptExe()
    {
        // 1) Variable env explicite
        var fromEnv = Environment.GetEnvironmentVariable("GHOSTSCRIPT_EXE");
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv;

        // 2) Nom “classique” selon l’OS (nécessite que PATH soit OK)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "gswin64c"; // ou gswin32c selon install
        return "gs"; // macOS / Linux
    }

    static string Quote(string s) => s.Contains(' ') ? $"\"{s}\"" : s;
}
