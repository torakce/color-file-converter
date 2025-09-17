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
        CancellationToken cancellationToken = default)
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
            var output = Path.Combine(outputFolder, Path.ChangeExtension(Path.GetFileName(input), ".tif"));

            progress?.Report(new BatchConversionProgress(
                i,
                inputFiles.Count,
                input,
                output,
                BatchConversionStage.Starting,
                null));

            var result = await ConvertSingleAsync(input, output, profile, cancellationToken).ConfigureAwait(false);
            results.Add(result);

            progress?.Report(new BatchConversionProgress(
                i + 1,
                inputFiles.Count,
                input,
                output,
                BatchConversionStage.Completed,
                result));
        }

        return new BatchConversionResult(results);
    }

    private static async Task<FileConversionResult> ConvertSingleAsync(
        string input,
        string output,
        ConversionProfile profile,
        CancellationToken cancellationToken)
    {
        var fileStopwatch = Stopwatch.StartNew();
        var extra = profile.ExtraParameters ?? Array.Empty<string>();
        string? error = null;
        bool success = false;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            await GhostscriptRunner.ConvertPdfToTiffAsync(
                input,
                output,
                profile.Device,
                profile.Dpi,
                profile.Compression,
                extra,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            success = true;
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
        long? outputSize = success ? GetFileSizeSafe(output) : null;

        return new FileConversionResult(
            input,
            output,
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
    string OutputPath,
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
