// Services/IPokemonService.cs
namespace PokemonApi.Services;

using PokemonApi.Models;

/// <summary>
/// Definisce i servizi per l'accesso e la gestione dei dati Pokemon.
/// L'interfaccia astrae l'implementazione specifica, permettendo
/// facile testing e future modifiche alla strategia di accesso ai dati.
/// </summary>
public interface IPokemonService
{
    /// <summary>
    /// Ottiene tutti i Pokemon ordinati per ID.
    /// Il primo accesso trigghera il lazy loading dei dati.
    /// </summary>
    Task<List<Pokemon>> GetAllPokemonAsync();

    /// <summary>
    /// Cerca un Pokemon specifico tramite il suo ID numerico.
    /// Restituisce null se il Pokemon non viene trovato.
    /// </summary>
    Task<Pokemon?> GetPokemonByIdAsync(int id);

    /// <summary>
    /// Ottiene tutti i Pokemon che appartengono a una specifica generazione.
    /// Utilizza la logica delle generazioni definita nel modello Generation.
    /// </summary>
    Task<List<Pokemon>> GetPokemonByGenerationAsync(int generationNumber);

    Task<List<Pokemon>> GetPokemonByName(string name);
    Task<List<Pokemon>> GetPokemonByType(string type);
}