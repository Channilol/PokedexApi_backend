// Models/CommonTypes.cs - Tipi base condivisi
namespace PokemonApi.Models;

// Record per rappresentare un riferimento PokeAPI generico
public record PokeApiReference(string Name, string Url);

// Record per rappresentare le statistiche base
public record Pokemon(
    int Id,
    string Name,
    int Order,
    int Height,
    int Weight,
    PokemonSprites Sprites,
    List<PokemonAbility> Abilities,
    List<PokemonStats> Stats,
    List<PokemonType> Types
);

// Record per le abilitá
public record PokemonAbility(
    PokeApiReference Ability,
    bool IsHidden,
    int Slot
);

// Record per le statistiche
public record PokemonStats(
    int BaseStat,
    int Effort,
    PokeApiReference Stat
);

// Record per i tipi
public record PokemonType(
    int Slot,
    PokeApiReference Type
);

// Record per gli sprites dei pokemon
public record PokemonSprites(
    string? BackDefault,
    string? BackFemale,
    string? BackShinyFemale,
    string? FrontDefault,
    string? FrontFemale,
    string? FrontShiny,
    string? FrontShinyFemale
);

// Record per le lista di pokemon semplificati
public record PokemonSummary(
    int Id,
    string Name,
    List<PokemonType> Types,
    string Sprite
);

// Record per gestire le generazioni
public record Generation(
    int Number,
    int Count,
    int Offset
)
{
    public int FirstPokemonId => Offset + 1;
    public int LastPokemonId => Offset + Count;

    public static readonly Dictionary<int, Generation> All = new()
    {
        {1, new Generation(1, 151, 0)},
        { 2, new Generation(2, 100, 151) },
        { 3, new Generation(3, 135, 251) },
        { 4, new Generation(4, 107, 386) },
        { 5, new Generation(5, 156, 493) },
        { 6, new Generation(6, 72, 649) },
        { 7, new Generation(7, 81, 721) },
        { 8, new Generation(8, 89, 802) },
        { 9, new Generation(9, 120, 905) }
    };

    public static List<Pokemon> GetPokemonByGeneration(List<Pokemon> data, int genNumber)
    {
        Generation? gen = GetByNumber(genNumber);
        if (gen == null) return [];
        return data.FindAll(p => gen.ContainsPokemon(p.Id));
    }
    public static Generation? GetByNumber(int number) => All.TryGetValue(number, out var gen) ? gen : null;
    public static Generation? GetByPokemonId(int pokemonId) => All.Values.FirstOrDefault(g => g.ContainsPokemon(pokemonId));
    public bool ContainsPokemon(int pokemonId) => pokemonId >= FirstPokemonId && pokemonId <= LastPokemonId;
}