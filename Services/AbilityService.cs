// Services/AbilityService.cs
namespace PokemonApi.Services;

using System.Text.Json;
using PokemonApi.Models;

/// <summary>
/// Implementazione del servizio per le abilità Pokemon con caching in memoria
/// e gestione delle chiamate HTTP all'API PokeAPI.
/// </summary>
public class AbilityService : IAbilityService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AbilityService> _logger;
    
    // Cache in memoria per evitare chiamate ripetute alla stessa abilità
    private readonly Dictionary<string, AbilityDescription> _abilityCache = new();
    private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);

    public AbilityService(HttpClient httpClient, ILogger<AbilityService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configura timeout per le chiamate HTTP
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Ottiene la descrizione di un'abilità dall'URL PokeAPI con caching.
    /// </summary>
    public async Task<AbilityDescription?> GetAbilityDescriptionAsync(string abilityUrl)
    {
        if (string.IsNullOrWhiteSpace(abilityUrl))
        {
            _logger.LogWarning("Ability URL is null or empty");
            return null;
        }

        try
        {
            // Controlla prima la cache
            await _cacheSemaphore.WaitAsync();
            try
            {
                if (_abilityCache.TryGetValue(abilityUrl, out var cachedAbility))
                {
                    _logger.LogDebug("Ability description found in cache for URL: {AbilityUrl}", abilityUrl);
                    return cachedAbility;
                }
            }
            finally
            {
                _cacheSemaphore.Release();
            }

            _logger.LogDebug("Fetching ability description from PokeAPI: {AbilityUrl}", abilityUrl);

            // Effettua la chiamata HTTP all'API PokeAPI
            var response = await _httpClient.GetAsync(abilityUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch ability data. Status: {StatusCode}, URL: {AbilityUrl}", 
                    response.StatusCode, abilityUrl);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            
            // Configura le opzioni di deserializzazione per PokeAPI
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var abilityDetail = JsonSerializer.Deserialize<AbilityDetail>(jsonContent, jsonOptions);
            
            if (abilityDetail == null)
            {
                _logger.LogWarning("Failed to deserialize ability data from URL: {AbilityUrl}", abilityUrl);
                return null;
            }

            // Cerca la descrizione in inglese
            var englishEffect = abilityDetail.EffectEntries
                .FirstOrDefault(entry => entry.Language.Name.Equals("en", StringComparison.OrdinalIgnoreCase));

            if (englishEffect == null)
            {
                _logger.LogWarning("No English description found for ability: {AbilityName}", abilityDetail.Name);
                return null;
            }

            var result = new AbilityDescription(abilityDetail.Name, englishEffect.Effect);

            // Salva nella cache
            await _cacheSemaphore.WaitAsync();
            try
            {
                _abilityCache[abilityUrl] = result;
                _logger.LogDebug("Cached ability description for: {AbilityName}", result.Name);
            }
            finally
            {
                _cacheSemaphore.Release();
            }

            return result;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error while fetching ability from URL: {AbilityUrl}", abilityUrl);
            return null;
        }
        catch (TaskCanceledException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Timeout while fetching ability from URL: {AbilityUrl}", abilityUrl);
            return null;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error for ability URL: {AbilityUrl}", abilityUrl);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching ability from URL: {AbilityUrl}", abilityUrl);
            return null;
        }
    }
}