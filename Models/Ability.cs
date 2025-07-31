// Models/Ability.cs - Modelli per gestire i dati delle abilità Pokemon
namespace PokemonApi.Models;

/// <summary>
/// Record che rappresenta una voce di effetto di un'abilità in una lingua specifica.
/// Le abilità Pokemon possono avere descrizioni in diverse lingue.
/// </summary>
public record EffectEntry(
    /// <summary>Descrizione dell'effetto dell'abilità</summary>
    string Effect,
    
    /// <summary>Versione breve della descrizione dell'effetto</summary>
    string? ShortEffect,
    
    /// <summary>Informazioni sulla lingua della descrizione</summary>
    LanguageReference Language
);

/// <summary>
/// Record che rappresenta un riferimento a una lingua.
/// </summary>
public record LanguageReference(
    /// <summary>Nome della lingua (es: "en", "it", "fr")</summary>
    string Name,
    
    /// <summary>URL per ottenere dettagli completi sulla lingua</summary>
    string Url
);

/// <summary>
/// Record che rappresenta i dettagli completi di un'abilità Pokemon.
/// Questo modello viene utilizzato per deserializzare la risposta dall'API PokeAPI.
/// </summary>
public record AbilityDetail(
    /// <summary>ID numerico dell'abilità</summary>
    int Id,
    
    /// <summary>Nome dell'abilità</summary>
    string Name,
    
    /// <summary>Lista delle descrizioni dell'effetto in diverse lingue</summary>
    List<EffectEntry> EffectEntries,
    
    /// <summary>Indica se l'abilità è una abilità principale dei Pokemon</summary>
    bool IsMainSeries
);

/// <summary>
/// Record semplificato per la risposta dell'API che contiene solo la descrizione dell'abilità.
/// Utilizzato per ottimizzare le risposte dell'API riducendo i dati non necessari al frontend.
/// </summary>
public record AbilityDescription(
    /// <summary>Nome dell'abilità</summary>
    string Name,
    
    /// <summary>Descrizione dell'effetto dell'abilità in inglese</summary>
    string Effect
);