using System.Text.Json;

namespace Converter.Core;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public sealed class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Exception { get; set; }
    
    public override string ToString()
    {
        var levelStr = Level.ToString().ToUpperInvariant();
        var timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var result = $"[{timestamp}] [{levelStr}] [{Category}] {Message}";
        
        if (!string.IsNullOrEmpty(Details))
        {
            result += $" - {Details}";
        }
        
        if (!string.IsNullOrEmpty(Exception))
        {
            result += $"\n  Exception: {Exception}";
        }
        
        return result;
    }
}

public sealed class Logger : IDisposable
{
    private readonly string _logFilePath;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new object();
    private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();
    private readonly Timer _flushTimer;
    private bool _disposed = false;

    public Logger(string logFilePath, LogLevel minLevel = LogLevel.Info)
    {
        _logFilePath = logFilePath;
        _minLevel = minLevel;
        
        // Créer le répertoire du fichier de log s'il n'existe pas
        Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)!);
        
        // Timer pour flush automatique toutes les 5 secondes
        _flushTimer = new Timer(FlushLogs, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        
        // Log de démarrage
        LogInfo("Logger", "Système de logging initialisé", $"Fichier: {_logFilePath}, Niveau minimum: {_minLevel}");
    }

    public void LogDebug(string category, string message, string? details = null)
    {
        Log(LogLevel.Debug, category, message, details);
    }

    public void LogInfo(string category, string message, string? details = null)
    {
        Log(LogLevel.Info, category, message, details);
    }

    public void LogWarning(string category, string message, string? details = null)
    {
        Log(LogLevel.Warning, category, message, details);
    }

    public void LogError(string category, string message, Exception? exception = null, string? details = null)
    {
        var exceptionStr = exception?.ToString();
        Log(LogLevel.Error, category, message, details, exceptionStr);
    }

    public void Log(LogLevel level, string category, string message, string? details = null, string? exception = null)
    {
        if (level < _minLevel || _disposed) return;

        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Category = category,
            Message = message,
            Details = details,
            Exception = exception
        };

        lock (_lock)
        {
            _logQueue.Enqueue(entry);
            
            // Si c'est une erreur, flush immédiatement
            if (level == LogLevel.Error)
            {
                FlushLogsInternal();
            }
        }
    }

    private void FlushLogs(object? state)
    {
        lock (_lock)
        {
            FlushLogsInternal();
        }
    }

    private void FlushLogsInternal()
    {
        if (_logQueue.Count == 0) return;

        try
        {
            using var writer = new StreamWriter(_logFilePath, append: true);
            
            while (_logQueue.Count > 0)
            {
                var entry = _logQueue.Dequeue();
                writer.WriteLine(entry.ToString());
            }
        }
        catch (Exception ex)
        {
            // En cas d'erreur de logging, on essaie d'écrire dans la console
            Console.WriteLine($"Erreur de logging: {ex.Message}");
        }
    }

    public void Flush()
    {
        lock (_lock)
        {
            FlushLogsInternal();
        }
    }

    public List<LogEntry> GetRecentLogs(int maxEntries = 100)
    {
        var logs = new List<LogEntry>();
        
        try
        {
            if (!File.Exists(_logFilePath)) return logs;
            
            var lines = File.ReadAllLines(_logFilePath);
            var recentLines = lines.TakeLast(maxEntries * 2).ToArray(); // Plus de lignes au cas où il y a des exceptions multi-lignes
            
            foreach (var line in recentLines)
            {
                if (TryParseLogLine(line, out var entry))
                {
                    logs.Add(entry);
                }
            }
            
            return logs.TakeLast(maxEntries).ToList();
        }
        catch (Exception ex)
        {
            LogError("Logger", "Erreur lors de la lecture des logs récents", ex);
            return logs;
        }
    }

    private static bool TryParseLogLine(string line, out LogEntry entry)
    {
        entry = new LogEntry();
        
        try
        {
            // Format: [2024-09-19 15:30:45.123] [INFO] [Category] Message - Details
            var parts = line.Split(new[] { "] [" }, StringSplitOptions.None);
            if (parts.Length < 3) return false;
            
            // Timestamp
            var timestampStr = parts[0].TrimStart('[');
            if (!DateTime.TryParse(timestampStr, out var timestamp)) return false;
            entry.Timestamp = timestamp;
            
            // Level
            var levelStr = parts[1];
            if (!Enum.TryParse<LogLevel>(levelStr, ignoreCase: true, out var level)) return false;
            entry.Level = level;
            
            // Category et message
            var remaining = string.Join("] [", parts.Skip(2));
            var categoryEnd = remaining.IndexOf(']');
            if (categoryEnd == -1) return false;
            
            entry.Category = remaining.Substring(0, categoryEnd);
            entry.Message = remaining.Substring(categoryEnd + 2); // +2 pour "] "
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ClearOldLogs(TimeSpan maxAge)
    {
        try
        {
            if (!File.Exists(_logFilePath)) return;
            
            var lines = File.ReadAllLines(_logFilePath);
            var cutoffDate = DateTime.Now - maxAge;
            var validLines = new List<string>();
            
            foreach (var line in lines)
            {
                if (TryParseLogLine(line, out var entry) && entry.Timestamp > cutoffDate)
                {
                    validLines.Add(line);
                }
            }
            
            File.WriteAllLines(_logFilePath, validLines);
            LogInfo("Logger", $"Nettoyage des logs terminé", $"{lines.Length - validLines.Count} entrées supprimées");
        }
        catch (Exception ex)
        {
            LogError("Logger", "Erreur lors du nettoyage des logs", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _flushTimer?.Dispose();
        
        // Flush final
        Flush();
        
        LogInfo("Logger", "Système de logging fermé");
        Flush(); // Double flush pour s'assurer que le message de fermeture est écrit
    }
}