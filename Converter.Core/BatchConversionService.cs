using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Core;

public sealed class BatchConversionService
{
    public async Task<BatchConversionResult> ConvertAsync(
        IReadOnlyList<string> inputFiles,
        string outputFolder,
        ConversionProfile profile,
        IProgress<BatchConversionProgress>? progress = null,
        CancellationToken cancellationToken = default,
        string? fileNameSuffix = null,
        bool splitMultipage = false)
    {
        if (inputFiles.Count == 0)
        {
            return new BatchConversionResult(Array.Empty<FileConversionResult>());
        }

        Directory.CreateDirectory(outputFolder);

        var results = new List<FileConversionResult>(inputFiles.Count);

        for (int i = 0; i < inputFiles.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var input = inputFiles[i];
            var baseName = Path.GetFileNameWithoutExtension(input);
            if (!string.IsNullOrEmpty(fileNameSuffix))
            {
                baseName += fileNameSuffix;
            }

            var singleOutput = Path.Combine(outputFolder, baseName + ".tif");
            var splitPattern = Path.Combine(outputFolder, baseName + "_%03d.tif");
            var progressOutput = splitMultipage ? splitPattern : singleOutput;

            progress?.Report(new BatchConversionProgress(
                i,
                inputFiles.Count,
                input,
                progressOutput,
                BatchConversionStage.Starting,
                null));

            var result = await ConvertSingleAsync(
                input,
                splitMultipage ? splitPattern : singleOutput,
                profile,
                cancellationToken,
                splitMultipage).ConfigureAwait(false);
            results.Add(result);

            progress?.Report(new BatchConversionProgress(
                i + 1,
                inputFiles.Count,
                input,
                progressOutput,
                BatchConversionStage.Completed,
                result));
        }

        return new BatchConversionResult(results);
    }

    private static async Task<FileConversionResult> ConvertSingleAsync(
        string input,
        string outputPattern,
        ConversionProfile profile,
        CancellationToken cancellationToken,
        bool splitMultipage)
    {
        var fileStopwatch = Stopwatch.StartNew();
        var extra = profile.ExtraParameters ?? Array.Empty<string>();
        string? error = null;
        bool success = false;
        var producedFiles = Array.Empty<string>();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPattern)!);
            var conversionStartUtc = DateTime.UtcNow;
            await GhostscriptRunner.ConvertPdfToTiffAsync(
                input,
                outputPattern,
                profile.Device,
                profile.Dpi,
                profile.Compression,
                extra,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            success = true;

            if (splitMultipage)
            {
                var directory = Path.GetDirectoryName(outputPattern)!;
                var baseName = Path.GetFileNameWithoutExtension(outputPattern);
                var searchPattern = baseName!.Replace("%03d", "*") + ".tif";
                producedFiles = Directory
                    .EnumerateFiles(directory, searchPattern)
                    .Where(f => File.GetLastWriteTimeUtc(f) >= conversionStartUtc - TimeSpan.FromSeconds(1))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (producedFiles.Length == 0)
                {
                    success = false;
                    error = "Aucun fichier genere";
                }
            }
            else
            {
                producedFiles = new[] { outputPattern };
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        fileStopwatch.Stop();

        long inputSize = GetFileSizeSafe(input);
        long? outputSize = success ? producedFiles.Sum(GetFileSizeSafe) : null;

        return new FileConversionResult(
            input,
            producedFiles,
            success,
            error,
            fileStopwatch.Elapsed,
            inputSize,
            outputSize);
    }

    private static long GetFileSizeSafe(string path)
    {
        try
        {
            var info = new FileInfo(path);
            return info.Exists ? info.Length : 0;
        }
        catch
        {
            return 0;
        }
    }
}

public sealed record BatchConversionResult(IReadOnlyList<FileConversionResult> Files)
{
    public TimeSpan TotalDuration => TimeSpan.FromMilliseconds(Files.Sum(f => f.Duration.TotalMilliseconds));
    public int SuccessCount => Files.Count(f => f.Success);
    public int FailureCount => Files.Count - SuccessCount;
}

public sealed record FileConversionResult(
    string InputPath,
    IReadOnlyList<string> OutputPaths,
    bool Success,
    string? ErrorMessage,
    TimeSpan Duration,
    long InputSize,
    long? OutputSize);

public sealed record BatchConversionProgress(
    int Completed,
    int Total,
    string InputPath,
    string OutputPath,
    BatchConversionStage Stage,
    FileConversionResult? Result)
{
    public double Percentage => Total == 0 ? 0 : Completed * 100.0 / Total;
}

public enum BatchConversionStage
{
    Starting,
    Completed
}
