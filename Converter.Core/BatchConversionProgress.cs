namespace Converter.Core;

public enum BatchConversionStage
{
    Starting,
    Completed,
    Failed
}

public class BatchConversionProgress
{
    public int Total { get; }
    public int Completed { get; }
    public string InputPath { get; }
    public BatchConversionStage Stage { get; }
    public string? ErrorMessage { get; }
    public FileConversionResult? Result { get; }

    public BatchConversionProgress(
        int total,
        int completed,
        string inputPath,
        BatchConversionStage stage,
        string? errorMessage = null,
        FileConversionResult? result = null)
    {
        Total = total;
        Completed = completed;
        InputPath = inputPath;
        Stage = stage;
        ErrorMessage = errorMessage;
        Result = result;
    }
}