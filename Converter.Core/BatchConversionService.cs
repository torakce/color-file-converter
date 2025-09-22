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
        string? outputFolder,
        ConversionProfile profile,
        IProgress<BatchConversionProgress>? progress = null,
        CancellationToken cancellationToken = default,
        string? fileNameSuffix = null,
        bool splitMultipage = false,
        bool useInputFolder = false,
        int maxConcurrency = 2)
    {
        if (inputFiles.Count == 0)
        {
            return new BatchConversionResult(Array.Empty<FileConversionResult>());
        }

        if (!useInputFolder)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                throw new ArgumentException("An output folder must be provided when useInputFolder is false.", nameof(outputFolder));
            }

            Directory.CreateDirectory(outputFolder);
        }

        var results = new FileConversionResult[inputFiles.Count];
        var completedCount = 0;
        var lockObject = new object();

        // Utilisation de SemaphoreSlim pour limiter la concurrence
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var tasks = inputFiles.Select(async (input, index) =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var baseName = Path.GetFileNameWithoutExtension(input);
                if (!string.IsNullOrEmpty(fileNameSuffix))
                {
                    baseName += fileNameSuffix;
                }

                var targetFolder = useInputFolder
                    ? Path.GetDirectoryName(input) ?? outputFolder ?? Directory.GetCurrentDirectory()
                    : outputFolder!;
                Directory.CreateDirectory(targetFolder);

                var singleOutput = Path.Combine(targetFolder, baseName + ".tif");
                var splitPattern = Path.Combine(targetFolder, baseName + "_%03d.tif");

                // Report start
                progress?.Report(new BatchConversionProgress(
                    index,
                    inputFiles.Count,
                    input,
                    BatchConversionStage.Starting,
                    null,
                    null));

                var result = await ConvertSingleAsync(
                    input,
                    splitMultipage ? splitPattern : singleOutput,
                    profile,
                    cancellationToken,
                    splitMultipage).ConfigureAwait(false);

                results[index] = result;

                // Thread-safe increment and progress report
                int currentCompleted;
                lock (lockObject)
                {
                    currentCompleted = ++completedCount;
                }

                progress?.Report(new BatchConversionProgress(
                    currentCompleted,
                    inputFiles.Count,
                    input,
                    BatchConversionStage.Completed,
                    result.Success ? null : result.ErrorMessage,
                    result));
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks).ConfigureAwait(false);

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
            if (!File.Exists(input))
            {
                error = "Le fichier d'entrée n'existe pas";
                return new FileConversionResult(
                    input,
                    Array.Empty<string>(),
                    false,
                    error,
                    fileStopwatch.Elapsed,
                    0,
                    null);
            }
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
