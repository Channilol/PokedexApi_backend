// Services/PokemonService.cs
namespace PokemonApi.Services;

using System.Text.Json;
using PokemonApi.Models;

/// <summary>
/// Implementazione del servizio Pokemon con lazy loading e caching in memoria.
/// Utilizza il pattern Singleton attraverso dependency injection per garantire
/// che i dati vengano caricati una sola volta per l'intera vita dell'applicazione.
/// </summary>
public class PokemonService : IPokemonService
{
    // Lazy<T> garantisce thread-safety e inizializzazione singola
    // anche in scenari multi-threaded tipici delle applicazioni web
    private readonly Lazy<Task<Dictionary<string, Pokemon>>> _pokemonData;

    // Logger per tracciare operazioni e potenziali problemi
    // In un'applicazione reale, questo verrebbe iniettato via DI
    private readonly ILogger<PokemonService>? _logger;

    /// <summary>
    /// Costruttore che configura il lazy loading ma non esegue alcun caricamento.
    /// Il caricamento effettivo avverrà solo al primo accesso ai dati.
    /// </summary>
    public PokemonService(ILogger<PokemonService>? logger = null)
    {
        _logger = logger;

        // Lazy<T> con factory method per il caricamento asincrono
        // Il delegate viene eseguito una sola volta, anche se chiamato da thread multipli
        _pokemonData = new Lazy<Task<Dictionary<string, Pokemon>>>(LoadPokemonDataAsync);

        _logger?.LogInformation("PokemonService initialized with lazy loading strategy");
    }

    /// <summary>
    /// Metodo privato che esegue il caricamento effettivo dei dati Pokemon.
    /// Viene chiamato automaticamente dalla proprietà Lazy al primo accesso.
    /// </summary>
    private async Task<Dictionary<string, Pokemon>> LoadPokemonDataAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger?.LogInformation("Starting Pokemon data loading...");

        try
        {
            // Costruisce il percorso del file JSON nella cartella Data
            var filePath = Path.Combine("Data", "pokemon_data.json");

            // Verifica esistenza del file prima di tentare la lettura
            if (!File.Exists(filePath))
            {
                var errorMessage = $"Pokemon data file not found at {filePath}";
                _logger?.LogError(errorMessage);
                throw new FileNotFoundException(errorMessage);
            }

            // Legge il contenuto del file in modo asincrono
            // Questo non blocca il thread durante l'operazione di I/O
            var jsonContent = await File.ReadAllTextAsync(filePath);
            _logger?.LogDebug($"JSON file loaded, size: {jsonContent.Length} characters");

            // Configura le opzioni di deserializzazione per gestire
            // le differenze tra convenzioni di naming JSON e C#
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Gestisce variazioni nel casing
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,  // Converte snake_case a PascalCase
                AllowTrailingCommas = true,  // Permette virgole finali nel JSON
                ReadCommentHandling = JsonCommentHandling.Skip  // Ignora eventuali commenti nel JSON
            };

            // Deserializza il JSON nel formato Dictionary<string, Pokemon>
            // La chiave stringa corrisponde all'ID del Pokemon come stringa
            var pokemonDict = JsonSerializer.Deserialize<Dictionary<string, Pokemon>>(
                jsonContent,
                jsonOptions);

            // Gestisce il caso in cui la deserializzazione restituisca null
            var result = pokemonDict ?? new Dictionary<string, Pokemon>();

            stopwatch.Stop();
            _logger?.LogInformation($"Pokemon data loaded successfully. Count: {result.Count}, Time: {stopwatch.ElapsedMilliseconds}ms");

            return result;
        }
        catch (JsonException jsonEx)
        {
            // Gestisce errori specifici di deserializzazione JSON
            _logger?.LogError(jsonEx, "Error deserializing Pokemon JSON data");
            throw new InvalidOperationException("Failed to parse Pokemon data file", jsonEx);
        }
        catch (Exception ex)
        {
            // Gestisce tutti gli altri tipi di errori
            _logger?.LogError(ex, "Unexpected error loading Pokemon data");
            throw;  // Rilancia l'eccezione originale per debugging
        }
    }

    /// <summary>
    /// Implementa l'accesso ai dati Pokemon con lazy loading.
    /// Questo metodo helper centralizza l'accesso ai dati e gestisce
    /// automaticamente il caricamento al primo utilizzo.
    /// </summary>
    private async Task<Dictionary<string, Pokemon>> GetPokemonDataAsync()
    {
        // .Value trigghera il caricamento se non è già stato fatto
        // Operazioni successive utilizzeranno i dati già caricati in memoria
        return await _pokemonData.Value;
    }

    /// <summary>
    /// Ottiene tutti i Pokemon ordinati per ID numerico.
    /// Utilizza LINQ per ordinamento efficiente dopo il caricamento.
    /// </summary>
    public async Task<List<Pokemon>> GetAllPokemonAsync()
    {
        try
        {
            var data = await GetPokemonDataAsync();

            // Converte i valori del dictionary in lista ordinata
            // OrderBy con selector numerico per ordinamento corretto
            var result = data.Values
                .OrderBy(pokemon => pokemon.Id)
                .ToList();

            _logger?.LogDebug($"Retrieved all Pokemon, count: {result.Count}");
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrieving all Pokemon");
            throw;  // Propaga l'errore per gestione a livello superiore
        }
    }

    /// <summary>
    /// Cerca un Pokemon specifico tramite ID numerico.
    /// Utilizza TryGetValue per accesso efficiente senza eccezioni.
    /// </summary>
    public async Task<Pokemon?> GetPokemonByIdAsync(int id)
    {
        try
        {
            var data = await GetPokemonDataAsync();

            // Converte l'ID numerico in stringa per lookup nel dictionary
            // TryGetValue è più performante di ContainsKey + indicizzazione
            var found = data.TryGetValue(id.ToString(), out var pokemon);

            _logger?.LogDebug($"Pokemon lookup for ID {id}: {(found ? "found" : "not found")}");
            return pokemon;  // Restituisce il Pokemon trovato o null
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrieving Pokemon by ID {PokemonId}", id);
            throw;
        }
    }

    /// <summary>
    /// Ottiene tutti i Pokemon che appartengono a una generazione specifica.
    /// Utilizza il metodo statico della classe Generation per la logica di filtro.
    /// </summary>
    public async Task<List<Pokemon>> GetPokemonByGenerationAsync(int generationNumber)
    {
        try
        {
            // Prima ottiene tutti i Pokemon (utilizzando il caching)
            var allPokemon = await GetAllPokemonAsync();

            // Delega alla logica statica della classe Generation
            // Questo mantiene la business logic centralizzata nel modello appropriato
            var result = Generation.GetPokemonByGeneration(allPokemon, generationNumber);

            _logger?.LogDebug($"Retrieved Pokemon for generation {generationNumber}, count: {result.Count}");
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error retrieving Pokemon for generation {generationNumber}", generationNumber);
            throw;
        }
    }

    public async Task<List<Pokemon>> GetPokemonByName(string name)
    {
        try
        {
            var allPokemon = await GetAllPokemonAsync();
            var result = allPokemon.FindAll(p => p.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
            _logger?.LogDebug($"Retrieved {result.Count} Pokemons with the name {name}");
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error retrieving Pokemon with the name {name}", name);
            throw;
        }
    }

    public async Task<List<Pokemon>> GetPokemonByType(string type)
    {
        try
        {
            var allPokemon = await GetAllPokemonAsync();
            var result = allPokemon.FindAll(p => p.Types.Any(t => t.Type.Name.Contains(type, StringComparison.CurrentCultureIgnoreCase)));
            _logger?.LogDebug($"Retrieved {result.Count} Pokemons with the type {type}");
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error retrieving Pokemon with the type {type}", type);
            throw;
        }
    }
}