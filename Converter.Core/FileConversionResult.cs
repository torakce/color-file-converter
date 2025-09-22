namespace Converter.Core;

public sealed record FileConversionResult(
    string InputPath,
    IReadOnlyList<string> OutputPaths,
    bool Success,
    string? ErrorMessage,
    TimeSpan Duration,
    long InputSize,
    long? OutputSize);