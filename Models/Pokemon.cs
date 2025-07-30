// Models/Pokemon.cs - Modelli di dominio per i dati Pokemon
namespace PokemonApi.Models;

/// <summary>
/// Record per rappresentare un riferimento generico alle API di Pokemon.
/// Questo pattern è utilizzato da PokeAPI per referenziare risorse correlate
/// senza includere tutti i dati annidati, riducendo la dimensione delle risposte.
/// </summary>
public record PokeApiReference(string Name, string Url);

/// <summary>
/// Record principale che rappresenta un Pokemon completo con tutte le sue informazioni.
/// Utilizza il pattern Record di C# per immutabilità e performance ottimali.
/// L'immutabilità previene modifiche accidentali e facilita il caching sicuro.
/// </summary>
public record Pokemon(
    /// <summary>ID numerico univoco del Pokemon secondo la classificazione ufficiale</summary>
    int Id,

    /// <summary>Nome del Pokemon in formato lowercase come definito da PokeAPI</summary>
    string Name,

    /// <summary>Numero d'ordine del Pokemon nel Pokedex nazionale</summary>
    int Order,

    /// <summary>Altezza del Pokemon in decimetri (es: 7 = 0.7 metri)</summary>
    int Height,

    /// <summary>Peso del Pokemon in ettogrammi (es: 69 = 6.9 kg)</summary>
    int Weight,

    /// <summary>Collezione degli sprite (immagini) del Pokemon per diverse visualizzazioni</summary>
    PokemonSprites Sprites,

    /// <summary>Lista delle abilità che il Pokemon può possedere, incluse quelle nascoste</summary>
    List<PokemonAbility> Abilities,

    /// <summary>Statistiche base del Pokemon (HP, Attack, Defense, ecc.)</summary>
    List<PokemonStats> Stats,

    /// <summary>Tipi del Pokemon (primario e secondario se presente)</summary>
    List<PokemonType> Types
);

/// <summary>
/// Record che rappresenta una singola abilità posseduta da un Pokemon.
/// Le abilità possono essere visibili normalmente o nascoste (ottenibili solo in condizioni speciali).
/// </summary>
public record PokemonAbility(
    /// <summary>Riferimento all'abilità con nome e URL per dettagli completi</summary>
    PokeApiReference Ability,

    /// <summary>Indica se l'abilità è nascosta e non ottenibile normalmente</summary>
    bool IsHidden,

    /// <summary>Slot dell'abilità (1 = prima abilità, 2 = seconda abilità, 3 = abilità nascosta)</summary>
    int Slot
);

/// <summary>
/// Record che rappresenta una singola statistica base di un Pokemon.
/// Ogni Pokemon ha esattamente 6 statistiche: HP, Attack, Defense, Special Attack, Special Defense, Speed.
/// Queste statistiche determinano le prestazioni del Pokemon in battaglia.
/// </summary>
public record PokemonStats(
    /// <summary>Valore base della statistica, determina la forza naturale del Pokemon in questa area</summary>
    int BaseStat,

    /// <summary>Punti sforzo (Effort Values) guadagnati sconfiggendo questo Pokemon</summary>
    int Effort,

    /// <summary>Riferimento al tipo di statistica (hp, attack, defense, ecc.)</summary>
    PokeApiReference Stat
);

/// <summary>
/// Record che rappresenta un tipo posseduto da un Pokemon.
/// Ogni Pokemon può avere uno o due tipi che determinano le sue debolezze e resistenze.
/// </summary>
public record PokemonType(
    /// <summary>Slot del tipo (1 = tipo primario, 2 = tipo secondario)</summary>
    int Slot,

    /// <summary>Riferimento al tipo con nome (es: "fire", "water") e URL per dettagli</summary>
    PokeApiReference Type
);

/// <summary>
/// Record che contiene gli URL delle immagini sprite del Pokemon.
/// Gli sprite sono le rappresentazioni grafiche del Pokemon utilizzate nei giochi.
/// Tutti i campi sono nullable perché non tutti i Pokemon hanno sprite per tutte le varianti.
/// </summary>
public record PokemonSprites(
    /// <summary>Sprite posteriore standard del Pokemon</summary>
    string? BackDefault,

    /// <summary>Sprite posteriore della variante femmina (se diversa dal maschio)</summary>
    string? BackFemale,

    /// <summary>Sprite posteriore shiny della variante femmina</summary>
    string? BackShinyFemale,

    /// <summary>Sprite frontale standard del Pokemon - il più comunemente utilizzato</summary>
    string? FrontDefault,

    /// <summary>Sprite frontale della variante femmina (se diversa dal maschio)</summary>
    string? FrontFemale,

    /// <summary>Sprite frontale della variante shiny (colori alternativi rari)</summary>
    string? FrontShiny,

    /// <summary>Sprite frontale shiny della variante femmina</summary>
    string? FrontShinyFemale
);

/// <summary>
/// Record ottimizzato per liste di Pokemon dove non servono tutti i dettagli.
/// Riduce significativamente la dimensione delle risposte API e migliora le prestazioni
/// quando si visualizzano molti Pokemon contemporaneamente (es: lista per generazione).
/// </summary>
public record PokemonSummary(
    /// <summary>ID numerico del Pokemon per identificazione univoca</summary>
    int Id,

    /// <summary>Nome del Pokemon per visualizzazione</summary>
    string Name,

    /// <summary>Lista dei tipi per visualizzazione rapida delle caratteristiche</summary>
    List<PokemonType> Types,

    /// <summary>URL dello sprite principale per anteprima visiva</summary>
    string Sprite
);

/// <summary>
/// Record che gestisce la logica delle generazioni Pokemon con pattern di accesso avanzati.
/// Le generazioni rappresentano i diversi giochi Pokemon rilasciati nel tempo,
/// ciascuna con un set specifico di Pokemon numerati in sequenza.
/// Questo record implementa sia i dati che la logica business relativa alle generazioni.
/// </summary>
public record Generation(
    /// <summary>Numero della generazione (1-9 attualmente)</summary>
    int Number,

    /// <summary>Numero totale di Pokemon introdotti in questa generazione</summary>
    int Count,

    /// <summary>Offset dall'ID 1, rappresenta quanti Pokemon esistevano prima di questa generazione</summary>
    int Offset
)
{
    /// <summary>
    /// Proprietà calcolata che restituisce l'ID del primo Pokemon della generazione.
    /// Utile per determinare i range di appartenenza senza calcoli manuali.
    /// </summary>
    public int FirstPokemonId => Offset + 1;

    /// <summary>
    /// Proprietà calcolata che restituisce l'ID dell'ultimo Pokemon della generazione.
    /// Combinata con FirstPokemonId, definisce il range completo della generazione.
    /// </summary>
    public int LastPokemonId => Offset + Count;

    /// <summary>
    /// Dictionary statico che contiene la definizione di tutte le generazioni Pokemon.
    /// Questo approccio centralizza i dati delle generazioni e garantisce consistenza.
    /// I valori sono basati sulla classificazione ufficiale dei giochi Pokemon.
    /// </summary>
    public static readonly Dictionary<int, Generation> All = new()
    {
        // Generazione 1: Pokemon Rosso/Blu/Giallo (1996-1998) - I Pokemon originali iconici
        {1, new Generation(1, 151, 0)},
        
        // Generazione 2: Pokemon Oro/Argento/Cristallo (1999-2000) - Introduce Pokemon tipo Buio e Acciaio
        { 2, new Generation(2, 100, 151) },
        
        // Generazione 3: Pokemon Rubino/Zaffiro/Smeraldo (2002-2004) - Introduce le abilità Pokemon
        { 3, new Generation(3, 135, 251) },
        
        // Generazione 4: Pokemon Diamante/Perla/Platino (2006-2008) - Separazione fisica/speciale
        { 4, new Generation(4, 107, 386) },
        
        // Generazione 5: Pokemon Nero/Bianco (2010-2012) - Solo Pokemon nuovi nella storia principale
        { 5, new Generation(5, 156, 493) },
        
        // Generazione 6: Pokemon X/Y (2013) - Introduce i Mega Pokemon e il tipo Folletto
        { 6, new Generation(6, 72, 649) },
        
        // Generazione 7: Pokemon Sole/Luna (2016-2017) - Introduce le forme regionali
        { 7, new Generation(7, 81, 721) },
        
        // Generazione 8: Pokemon Spada/Scudo (2019) - Introduce il fenomeno Dynamax
        { 8, new Generation(8, 89, 802) },
        
        // Generazione 9: Pokemon Scarlatto/Violetto (2022) - Mondo aperto e forme Terastal
        { 9, new Generation(9, 120, 905) }
    };

    /// <summary>
    /// Metodo statico che implementa la logica di filtraggio per generazione.
    /// Centralizza la business logic evitando duplicazione di codice negli endpoint.
    /// Utilizza pattern di early return per gestione efficiente dei casi limite.
    /// </summary>
    /// <param name="data">Lista completa dei Pokemon da filtrare</param>
    /// <param name="genNumber">Numero della generazione richiesta</param>
    /// <returns>Lista dei Pokemon appartenenti alla generazione, vuota se la generazione non esiste</returns>
    public static List<Pokemon> GetPokemonByGeneration(List<Pokemon> data, int genNumber)
    {
        // Ottiene la generazione richiesta dal dictionary statico
        Generation? gen = GetByNumber(genNumber);

        // Early return: se la generazione non esiste, restituisce lista vuota
        if (gen == null) return [];

        // Filtra i Pokemon utilizzando la logica di appartenenza incapsulata
        return data.FindAll(p => gen.ContainsPokemon(p.Id));
    }

    /// <summary>
    /// Metodo di utilità per ottenere una generazione specifica per numero.
    /// Utilizza TryGetValue per accesso sicuro senza eccezioni.
    /// Pattern comune per dictionary lookup con gestione null appropriata.
    /// </summary>
    public static Generation? GetByNumber(int number) =>
        All.TryGetValue(number, out var gen) ? gen : null;

    /// <summary>
    /// Metodo di utilità per trovare a quale generazione appartiene un Pokemon specifico.
    /// Utile per operazioni inverse quando hai un ID Pokemon e vuoi sapere la generazione.
    /// Utilizza LINQ FirstOrDefault per ricerca efficiente con condizione personalizzata.
    /// </summary>
    public static Generation? GetByPokemonId(int pokemonId) =>
        All.Values.FirstOrDefault(g => g.ContainsPokemon(pokemonId));

    /// <summary>
    /// Metodo che determina se un Pokemon appartiene a questa generazione specifica.
    /// Implementa la logica di range checking utilizzando le proprietà calcolate.
    /// Questo approccio è più leggibile e manutenibile di calcoli manuali inline.
    /// </summary>
    public bool ContainsPokemon(int pokemonId) =>
        pokemonId >= FirstPokemonId && pokemonId <= LastPokemonId;
}