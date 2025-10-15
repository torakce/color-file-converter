# Bonnes Pratiques C# - Color File Converter

> 📚 Guide de référence des patterns et bonnes pratiques appliqués dans le projet

## Table des Matières
1. [Async/Await Best Practices](#asyncawait-best-practices)
2. [Validation et Defensive Programming](#validation-et-defensive-programming)
3. [Performance et Caching](#performance-et-caching)
4. [Resource Management](#resource-management)
5. [Error Handling](#error-handling)
6. [Logging](#logging)
7. [CLI Design](#cli-design)

---

## Async/Await Best Practices

### ✅ Utiliser ConfigureAwait(false) dans les bibliothèques

```csharp
// ❌ MAUVAIS - Peut causer des deadlocks
public async Task ProcessAsync()
{
    await Task.Delay(1000);
    await File.ReadAllTextAsync("file.txt");
}

// ✅ BON - Évite le retour au contexte de synchronisation
public async Task ProcessAsync()
{
    await Task.Delay(1000).ConfigureAwait(false);
    await File.ReadAllTextAsync("file.txt").ConfigureAwait(false);
}
```

**Pourquoi?**
- Évite les deadlocks dans les applications UI (WinForms, WPF)
- Meilleure performance (pas de context switching)
- Standard pour les bibliothèques réutilisables

**Quand NE PAS l'utiliser:**
- Dans le code UI qui doit revenir au thread UI
- Dans les contrôleurs ASP.NET Core (pas nécessaire)

### ✅ Passer CancellationToken aux méthodes async

```csharp
// ❌ MAUVAIS - Pas d'annulation possible
public async Task<string> ReadFileAsync(string path)
{
    return await File.ReadAllTextAsync(path);
}

// ✅ BON - Support d'annulation
public async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken = default)
{
    cancellationToken.ThrowIfCancellationRequested();
    return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
}
```

### ✅ Gérer l'annulation de processus

```csharp
// ✅ BON - Pattern complet pour annuler un Process
using var registration = cancellationToken.Register(() =>
{
    try
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
    }
    catch
    {
        // Ignorer - le processus peut déjà être terminé
    }
});

await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
```

---

## Validation et Defensive Programming

### ✅ Fail-Fast avec ArgumentNullException.ThrowIfNull (.NET 8+)

```csharp
// ❌ MAUVAIS - Validation manuelle verbeuse
public void Process(string input, object data)
{
    if (input == null) throw new ArgumentNullException(nameof(input));
    if (data == null) throw new ArgumentNullException(nameof(data));
    // ...
}

// ✅ BON - Concis et clair (.NET 8+)
public void Process(string input, object data)
{
    ArgumentNullException.ThrowIfNull(input);
    ArgumentNullException.ThrowIfNull(data);
    // ...
}

// ✅ ACCEPTABLE - Pour .NET 6/7
public void Process(string input, object data)
{
    if (input is null) throw new ArgumentNullException(nameof(input));
    if (data is null) throw new ArgumentNullException(nameof(data));
    // ...
}
```

### ✅ Validation de plages avec messages clairs

```csharp
// ❌ MAUVAIS - Message générique
public void SetDpi(int dpi)
{
    if (dpi < 72 || dpi > 2400)
        throw new ArgumentException("Invalid DPI");
}

// ✅ BON - Message explicite avec contexte
public void SetDpi(int dpi)
{
    if (dpi < 72 || dpi > 2400)
    {
        throw new ArgumentOutOfRangeException(
            nameof(dpi),
            dpi,
            "Le DPI doit être entre 72 et 2400"
        );
    }
}
```

### ✅ Vérification de fichiers avant utilisation

```csharp
// ❌ MAUVAIS - Laisse FileNotFoundException se produire plus tard
public async Task ProcessFile(string path)
{
    var content = await File.ReadAllTextAsync(path);
    // ...
}

// ✅ BON - Échec rapide avec message clair
public async Task ProcessFile(string path)
{
    if (!File.Exists(path))
    {
        throw new FileNotFoundException(
            $"Le fichier n'existe pas: {path}",
            path
        );
    }
    
    var content = await File.ReadAllTextAsync(path).ConfigureAwait(false);
    // ...
}
```

---

## Performance et Caching

### ✅ Cache thread-safe pour opérations coûteuses

```csharp
// ❌ MAUVAIS - Recherche répétée à chaque appel
public string FindExecutable()
{
    // Recherche dans tout le système de fichiers...
    return SearchFileSystem();
}

// ✅ BON - Cache avec thread safety
private static string? _cachedPath;
private static readonly object _lock = new object();

public string FindExecutable()
{
    lock (_lock)
    {
        if (_cachedPath != null)
            return _cachedPath;
            
        _cachedPath = SearchFileSystem();
        return _cachedPath;
    }
}

// ✅ BONUS - Méthode pour réinitialiser (utile pour tests)
public static void ResetCache()
{
    lock (_lock)
    {
        _cachedPath = null;
    }
}
```

### ✅ Lazy<T> pour initialisation différée

```csharp
// ✅ Alternative avec Lazy<T> (thread-safe par défaut)
private static readonly Lazy<string> _lazyPath = new(() => SearchFileSystem());

public string FindExecutable() => _lazyPath.Value;
```

---

## Resource Management

### ✅ Rotation de fichiers de logs

```csharp
// ✅ BON - Pattern de rotation avec limite
private const long MaxLogFileSize = 10 * 1024 * 1024; // 10 MB
private const int MaxRotatedFiles = 5;

private void RotateLogFileIfNeeded()
{
    if (!File.Exists(_logFilePath)) return;
    
    var fileInfo = new FileInfo(_logFilePath);
    if (fileInfo.Length > MaxLogFileSize)
    {
        // Créer fichier de rotation avec timestamp
        var rotatedPath = $"{_logFilePath}.{DateTime.Now:yyyyMMdd-HHmmss}.old";
        File.Move(_logFilePath, rotatedPath);
        
        // Garder seulement les N derniers fichiers
        var directory = Path.GetDirectoryName(_logFilePath)!;
        var logName = Path.GetFileName(_logFilePath);
        
        var oldLogs = Directory
            .GetFiles(directory, $"{logName}.*.old")
            .OrderByDescending(f => f)
            .Skip(MaxRotatedFiles)
            .ToArray();
        
        foreach (var oldLog in oldLogs)
        {
            File.Delete(oldLog);
        }
    }
}
```

### ✅ Using statements et Dispose

```csharp
// ❌ MAUVAIS - Risque de fuite de ressources
public void WriteLog(string message)
{
    var writer = new StreamWriter(_logPath, append: true);
    writer.WriteLine(message);
    writer.Dispose(); // Peut ne jamais s'exécuter si exception
}

// ✅ BON - Using garantit le Dispose
public void WriteLog(string message)
{
    using var writer = new StreamWriter(_logPath, append: true);
    writer.WriteLine(message);
    // Dispose automatique en fin de scope
}

// ✅ AUSSI BON - Using statement classique
public void WriteLog(string message)
{
    using (var writer = new StreamWriter(_logPath, append: true))
    {
        writer.WriteLine(message);
    }
}
```

---

## Error Handling

### ✅ Messages d'erreur contextualisés

```csharp
// ❌ MAUVAIS - Message générique sans contexte
catch (Exception ex)
{
    throw new Exception(ex.Message);
}

// ✅ BON - Message avec contexte et préservation de l'exception
catch (Exception ex)
{
    throw new Exception(
        $"Échec de la conversion du fichier '{inputFile}' vers '{outputFile}'",
        ex // Préserve la stack trace originale
    );
}
```

### ✅ Codes de sortie standardisés pour CLI

```csharp
// ✅ BON - Codes de sortie standards Unix
const int EXIT_SUCCESS = 0;      // Succès
const int EXIT_FAILURE = 1;      // Erreur générale
const int EXIT_USAGE_ERROR = 2;  // Mauvaise utilisation

try
{
    await ProcessAsync();
    Environment.Exit(EXIT_SUCCESS);
}
catch (ArgumentException)
{
    Console.Error.WriteLine("Erreur d'argument. Utilisez --help");
    Environment.Exit(EXIT_USAGE_ERROR);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erreur: {ex.Message}");
    Environment.Exit(EXIT_FAILURE);
}
```

### ✅ Try-Catch sélectif

```csharp
// ❌ MAUVAIS - Attrape tout, masque les bugs
try
{
    ProcessData();
}
catch (Exception)
{
    // Silence...
}

// ✅ BON - Attrape seulement les erreurs attendues
try
{
    ProcessData();
}
catch (FileNotFoundException ex)
{
    Logger.LogWarning($"Fichier non trouvé: {ex.FileName}");
    // Gestion appropriée
}
catch (UnauthorizedAccessException ex)
{
    Logger.LogError($"Accès refusé: {ex.Message}");
    throw; // Re-throw si pas gérable
}
// Laisse les autres exceptions se propager
```

---

## Logging

### ✅ Niveaux de logs appropriés

```csharp
// ✅ BON - Utilisation appropriée des niveaux
public class MyService
{
    private readonly Logger _logger;
    
    public void ProcessFile(string file)
    {
        _logger.LogDebug("Processing", $"Starting file: {file}");
        
        try
        {
            var data = LoadFile(file);
            _logger.LogInfo("Processing", $"Loaded {data.Length} bytes");
            
            if (data.Length == 0)
            {
                _logger.LogWarning("Processing", $"File is empty: {file}");
                return;
            }
            
            ProcessData(data);
            _logger.LogInfo("Processing", $"Successfully processed {file}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Processing", $"Failed to process {file}", ex);
            throw;
        }
    }
}
```

**Guide des niveaux:**
- **Debug**: Détails pour le développement (verbose)
- **Info**: Événements importants normaux
- **Warning**: Situations anormales mais récupérables
- **Error**: Erreurs nécessitant attention

### ✅ Flush automatique et manuel

```csharp
// ✅ BON - Pattern de logger avec flush
public class Logger : IDisposable
{
    private readonly Timer _flushTimer;
    
    public Logger()
    {
        // Flush automatique toutes les 5 secondes
        _flushTimer = new Timer(
            _ => Flush(),
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5)
        );
    }
    
    public void LogError(string category, string message, Exception? ex = null)
    {
        Log(LogLevel.Error, category, message, ex?.ToString());
        
        // Flush immédiat pour les erreurs critiques
        Flush();
    }
    
    public void Dispose()
    {
        _flushTimer?.Dispose();
        Flush(); // Flush final
    }
}
```

---

## CLI Design

### ✅ Help complet avec exemples

```csharp
// ✅ BON - Aide structurée et complète
static void ShowHelp()
{
    Console.WriteLine(@"MyApp - Description courte

Usage:
    myapp <command> [options]

Commands:
    convert        Convertit un fichier
    validate       Valide un fichier

Options globales:
    --verbose, -v  Mode verbose
    --help, -h     Affiche cette aide
    --version      Affiche la version

Exemples:
    myapp convert --input file.pdf --output file.tif
    myapp convert --input file.pdf --output file.tif --dpi 600
    myapp validate --input file.pdf

Pour plus d'aide sur une commande:
    myapp <command> --help
");
}
```

### ✅ Validation progressive avec messages clairs

```csharp
// ✅ BON - Validation avec messages explicites
static Options ParseArguments(string[] args)
{
    if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
    {
        ShowHelp();
        Environment.Exit(0);
    }
    
    var options = new Options();
    
    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--input":
                if (i + 1 >= args.Length)
                    throw new ArgumentException("--input requiert un chemin de fichier");
                options.Input = args[++i];
                break;
                
            case "--dpi":
                if (i + 1 >= args.Length)
                    throw new ArgumentException("--dpi requiert un nombre");
                    
                if (!int.TryParse(args[++i], out var dpi) || dpi < 72 || dpi > 2400)
                    throw new ArgumentException("--dpi doit être entre 72 et 2400");
                    
                options.Dpi = dpi;
                break;
                
            default:
                throw new ArgumentException($"Option inconnue: {args[i]}. Utilisez --help");
        }
    }
    
    // Validation finale
    if (string.IsNullOrEmpty(options.Input))
        throw new ArgumentException("--input est obligatoire");
        
    if (!File.Exists(options.Input))
        throw new FileNotFoundException($"Le fichier n'existe pas: {options.Input}");
    
    return options;
}
```

### ✅ Sortie structurée pour humans et scripts

```csharp
// ✅ BON - Sortie lisible par humains et scripts
public async Task RunAsync(Options options)
{
    if (options.Verbose)
    {
        Console.WriteLine($"Conversion en cours...");
        Console.WriteLine($"  Entrée: {options.Input}");
        Console.WriteLine($"  Sortie: {options.Output}");
        Console.WriteLine($"  DPI: {options.Dpi}");
    }
    
    try
    {
        await ConvertAsync(options);
        
        if (options.Verbose)
            Console.WriteLine("✓ Conversion réussie");
        else
            Console.WriteLine("OK"); // Simple pour parsing
            
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        // Toujours sur stderr pour erreurs
        Console.Error.WriteLine($"Erreur: {ex.Message}");
        Environment.Exit(1);
    }
}
```

---

## Récapitulatif des Gains

| Bonne Pratique | Impact Performance | Impact Qualité | Impact Maintenance |
|----------------|-------------------|----------------|-------------------|
| ConfigureAwait(false) | ⭐⭐⭐ Moyen | ⭐⭐⭐⭐ Élevé | ⭐⭐⭐ Moyen |
| CancellationToken | ⭐⭐ Faible | ⭐⭐⭐⭐⭐ Très élevé | ⭐⭐⭐ Moyen |
| Fail-Fast Validation | ⭐⭐⭐⭐ Élevé | ⭐⭐⭐⭐⭐ Très élevé | ⭐⭐⭐⭐ Élevé |
| Caching | ⭐⭐⭐⭐⭐ Très élevé | ⭐⭐⭐ Moyen | ⭐⭐⭐ Moyen |
| Rotation Logs | ⭐⭐⭐ Moyen | ⭐⭐⭐ Moyen | ⭐⭐⭐⭐⭐ Très élevé |
| Messages Clairs | ⭐ Négligeable | ⭐⭐⭐⭐⭐ Très élevé | ⭐⭐⭐⭐⭐ Très élevé |
| CLI Help Complet | ⭐ Négligeable | ⭐⭐⭐⭐⭐ Très élevé | ⭐⭐⭐⭐ Élevé |

---

## Ressources Complémentaires

### Documentation Microsoft
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/)
- [Exception Handling Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

### Livres Recommandés
- **C# in Depth** - Jon Skeet
- **CLR via C#** - Jeffrey Richter
- **Clean Code** - Robert C. Martin
- **The Pragmatic Programmer** - Hunt & Thomas

### Outils d'Analyse
- **SonarLint** - Analyse statique en temps réel
- **Roslyn Analyzers** - Analyseurs intégrés .NET
- **BenchmarkDotNet** - Mesure de performance précise
- **dotMemory** - Profilage mémoire

---

**Dernière mise à jour**: Octobre 2025  
**Version**: 1.0
