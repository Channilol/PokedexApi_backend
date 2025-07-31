// Services/IAbilityService.cs
namespace PokemonApi.Services;

using PokemonApi.Models;

/// <summary>
/// Definisce i servizi per l'accesso e la gestione dei dati delle abilità Pokemon.
/// L'interfaccia astrae l'implementazione specifica per permettere facile testing
/// e future modifiche alla strategia di accesso ai dati delle abilità.
/// </summary>
public interface IAbilityService
{
    /// <summary>
    /// Ottiene la descrizione di un'abilità specifica dall'URL fornito.
    /// Questo metodo effettua una chiamata HTTP all'API PokeAPI per ottenere
    /// i dettagli completi dell'abilità e restituisce solo la descrizione in inglese.
    /// </summary>
    /// <param name="abilityUrl">URL completo dell'abilità da PokeAPI</param>
    /// <returns>Descrizione dell'abilità con nome ed effetto</returns>
    Task<AbilityDescription?> GetAbilityDescriptionAsync(string abilityUrl);
}