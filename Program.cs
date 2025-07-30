using PokemonApi.Models;
using PokemonApi.Services; // Import del nostro namespace per i servizi

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAZIONE DEI SERVIZI ===
// Questa sezione configura tutti i servizi prima che l'applicazione venga costruita

// Registra il sistema di documentazione automatica delle API
// OpenAPI (evoluzione di Swagger) genererà automaticamente documentazione interattiva
builder.Services.AddOpenApi();

// Registra il nostro servizio Pokemon come Singleton
// Singleton significa che la stessa istanza verrà riutilizzata per tutta la vita dell'applicazione
// Questo è perfetto per il nostro caso perché vogliamo caricare i dati una sola volta
builder.Services.AddSingleton<IPokemonService, PokemonService>();

// Configura CORS (Cross-Origin Resource Sharing) per permettere al frontend React di accedere alla nostra API
// Senza questa configurazione, i browser bloccherebbero le richieste dal frontend per motivi di sicurezza
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()      // Permette richieste da qualsiasi dominio (solo per sviluppo)
              .AllowAnyMethod()      // Permette tutti i metodi HTTP (GET, POST, PUT, DELETE, etc.)
              .AllowAnyHeader();     // Permette tutti gli header HTTP
    });
});

// Costruisce l'applicazione con tutti i servizi configurati
var app = builder.Build();

// === CONFIGURAZIONE DELLA PIPELINE HTTP ===
// Questa sezione configura come l'applicazione gestisce le richieste HTTP in arrivo

// In ambiente di sviluppo, espone la documentazione automatica dell'API
// Potrai accedere a questa documentazione visitando /openapi nell'applicazione in esecuzione
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware per reindirizzare automaticamente le richieste HTTP verso HTTPS
// Importante per la sicurezza in produzione
app.UseHttpsRedirection();

// Abilita il supporto CORS che abbiamo configurato sopra
app.UseCors();

// === DEFINIZIONE DEGLI ENDPOINT ===
// Qui mapperemo tutti i nostri endpoint API

// Endpoint per ottenere tutti i Pokemon
// Risponde alle richieste GET su /api/pokemon
app.MapGet("/api/pokemon", async (IPokemonService pokemonService) =>
{
    try
    {
        // Utilizza il servizio iniettato automaticamente per ottenere i dati
        // Il lazy loading si attiva automaticamente al primo accesso
        var pokemon = await pokemonService.GetAllPokemonAsync();

        // Results.Ok() crea una risposta HTTP 200 con i dati in formato JSON
        // La serializzazione da List<Pokemon> a JSON avviene automaticamente
        return Results.Ok(pokemon);
    }
    catch (Exception ex)
    {
        // In caso di errore, restituisce una risposta HTTP 500 con dettagli dell'errore
        // Results.Problem() è il modo standard per gestire errori nelle Minimal APIs
        return Results.Problem(
            title: "Error retrieving Pokemon data",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("GetAllPokemon")           // Nome interno per l'endpoint, utile per routing avanzato
.WithOpenApi();                     // Include questo endpoint nella documentazione automatica

app.MapGet("/api/pokemon/{id}", async (IPokemonService pokemonService, int id) =>
{
    try
    {
        var pokemon = await pokemonService.GetPokemonByIdAsync(id);
        if (pokemon == null) return Results.NotFound($"Pokemon with ID {id} not found");
        return Results.Ok(pokemon);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: $"Error retrieving pokemon with id:{id}", detail: ex.Message, statusCode: 500);
    }
}).WithName("GetPokemonById").WithOpenApi();

app.MapGet("/api/pokemon/generation/{genNum}", async (IPokemonService pokemonService, int genNum) =>
{
    try
    {
        List<Pokemon> pokemonList = await pokemonService.GetPokemonByGenerationAsync(genNum);
        if (pokemonList.Count == 0) return Results.NotFound($"Pokemon from generation {genNum} not found");
        return Results.Ok(pokemonList);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: $"Error retrieving pokemon from generation {genNum}", detail: ex.Message, statusCode: 500);
    }
}).WithName("GetPokemonByGeneration").WithOpenApi();

app.MapGet("/api/pokemon/search", async (IPokemonService pokemonService, string? name, string? type) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(type)) return Results.BadRequest("Please provide a name or type parameter to search for Pokemon");
        List<Pokemon> pokemonList = [];
        if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(type))
        {
            List<Pokemon> listByName = await pokemonService.GetPokemonByName(name);
            pokemonList.AddRange(listByName);
        }
        if (!string.IsNullOrWhiteSpace(type) && string.IsNullOrWhiteSpace(name))
        {
            List<Pokemon> listByType = await pokemonService.GetPokemonByType(type);
            pokemonList.AddRange(listByType);
        }
        if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(name))
        {
            List<Pokemon> listByType = await pokemonService.GetPokemonByType(type);
            pokemonList.AddRange(listByType.Where(p => p.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase)));
        }

        if (pokemonList.Count == 0) return Results.NotFound($"Pokemon not found");
        return Results.Ok(pokemonList);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: $"Error retrieving pokemon with the {name}", detail: ex.Message, statusCode: 500);
    }
}).WithName("SearchPokemonByName").WithOpenApi();

// Avvia l'applicazione e inizia ad ascoltare le richieste HTTP
app.Run();