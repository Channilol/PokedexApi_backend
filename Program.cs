// Program.cs - Entry point e configurazione dell'applicazione Pokemon API
using PokemonApi.Models;
using PokemonApi.Services; // Import del nostro namespace per i servizi

// Crea il builder per configurare l'applicazione web
// WebApplication.CreateBuilder configura automaticamente logging, configurazione e hosting
var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAZIONE DEI SERVIZI ===
// Questa sezione configura tutti i servizi prima che l'applicazione venga costruita
// Ogni servizio registrato qui sarà disponibile attraverso dependency injection

// Registra il sistema di documentazione automatica delle API
// OpenAPI (evoluzione di Swagger) genererà automaticamente documentazione interattiva
// La documentazione sarà accessibile via browser per testing e esplorazione dell'API
builder.Services.AddOpenApi();

// Registra il nostro servizio Pokemon come Singleton
// Singleton significa che la stessa istanza verrà riutilizzata per tutta la vita dell'applicazione
// Questo è perfetto per il nostro caso perché vogliamo caricare i dati una sola volta
// e mantenerli in memoria per prestazioni ottimali
builder.Services.AddSingleton<IPokemonService, PokemonService>();

// Registra HttpClient per chiamate HTTP esterne
builder.Services.AddHttpClient();

// Registra il servizio per le abilità come Singleton con caching
builder.Services.AddSingleton<IAbilityService, AbilityService>();

// Configura CORS (Cross-Origin Resource Sharing) per permettere al frontend React di accedere alla nostra API
// Senza questa configurazione, i browser bloccherebbero le richieste dal frontend per motivi di sicurezza
// ATTENZIONE: Questa configurazione è permissiva ed è adatta solo per sviluppo
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
// Da questo punto in poi, l'applicazione è configurata e pronta per definire la pipeline HTTP
var app = builder.Build();

// === CONFIGURAZIONE DELLA PIPELINE HTTP ===
// Questa sezione configura come l'applicazione gestisce le richieste HTTP in arrivo
// L'ordine dei middleware è importante: vengono eseguiti in sequenza per ogni richiesta

// In ambiente di sviluppo, espone la documentazione automatica dell'API
// Potrai accedere a questa documentazione visitando /openapi nell'applicazione in esecuzione
// Questo fornisce un'interfaccia web per testare tutti gli endpoint senza tools esterni
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware per reindirizzare automaticamente le richieste HTTP verso HTTPS
// Importante per la sicurezza in produzione, garantisce che tutti i dati siano crittografati
app.UseHttpsRedirection();

// Abilita il supporto CORS che abbiamo configurato sopra
// Questo middleware deve essere posizionato prima degli endpoint per funzionare correttamente
app.UseCors();

// === DEFINIZIONE DEGLI ENDPOINT ===
// Qui mapperemo tutti i nostri endpoint API utilizzando le Minimal APIs di .NET
// Ogni endpoint definisce un path HTTP e la logica per gestire le richieste

// Endpoint per ottenere tutti i Pokemon
// Risponde alle richieste GET su /api/pokemon
// Questo endpoint può restituire migliaia di Pokemon, quindi il lazy loading è cruciale
app.MapGet("/api/pokemon", async (IPokemonService pokemonService) =>
{
    try
    {
        // Utilizza il servizio iniettato automaticamente per ottenere i dati
        // Il lazy loading si attiva automaticamente al primo accesso
        // Le richieste successive utilizzeranno i dati già caricati in memoria
        var pokemon = await pokemonService.GetAllPokemonAsync();

        // Results.Ok() crea una risposta HTTP 200 con i dati in formato JSON
        // La serializzazione da List<Pokemon> a JSON avviene automaticamente
        return Results.Ok(pokemon);
    }
    catch (Exception ex)
    {
        // In caso di errore, restituisce una risposta HTTP 500 con dettagli dell'errore
        // Results.Problem() è il modo standard per gestire errori nelle Minimal APIs
        // Include automaticamente timestamp e trace ID per debugging
        return Results.Problem(
            title: "Error retrieving Pokemon data",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("GetAllPokemon")           // Nome interno per l'endpoint, utile per routing avanzato e URL generation
.WithOpenApi();                     // Include questo endpoint nella documentazione automatica

// Endpoint per ottenere un Pokemon specifico tramite ID
// Utilizza route parameter {id} che viene automaticamente convertito in int
// Esempio di utilizzo: GET /api/pokemon/25 per ottenere Pikachu
app.MapGet("/api/pokemon/{id}", async (IPokemonService pokemonService, int id) =>
{
    try
    {
        // Cerca il Pokemon specificato utilizzando lookup ottimizzato
        var pokemon = await pokemonService.GetPokemonByIdAsync(id);

        // Gestione esplicita del caso "non trovato" con HTTP 404
        // Questo è semanticamente corretto: la risorsa richiesta non esiste
        if (pokemon == null)
            return Results.NotFound($"Pokemon with ID {id} not found");

        // Restituisce il Pokemon trovato con HTTP 200
        return Results.Ok(pokemon);
    }
    catch (Exception ex)
    {
        // Gestisce errori imprevisti come problemi di accesso ai dati
        return Results.Problem(
            title: $"Error retrieving Pokemon with ID: {id}",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("GetPokemonById")          // Nome descrittivo per questo endpoint specifico
.WithOpenApi();                     // Documentazione automatica con schema del parametro ID

// Endpoint per ottenere Pokemon di una generazione specifica
// Route parameter {genNum} accetta numeri da 1 a 9 (generazioni attualmente esistenti)
// Esempio: GET /api/pokemon/generation/1 per tutti i Pokemon della prima generazione
app.MapGet("/api/pokemon/generation/{genNum}", async (IPokemonService pokemonService, int genNum) =>
{
    try
    {
        // Utilizza la logica business centralizzata per il filtraggio per generazione
        List<Pokemon> pokemonList = await pokemonService.GetPokemonByGenerationAsync(genNum);

        // Se la lista è vuota, significa che la generazione non esiste o non ha Pokemon
        // HTTP 404 comunica chiaramente che la risorsa (generazione) non è disponibile
        if (pokemonList.Count == 0)
            return Results.NotFound($"Pokemon from generation {genNum} not found");

        return Results.Ok(pokemonList);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: $"Error retrieving Pokemon from generation {genNum}",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("GetPokemonByGeneration")  // Nome che riflette la funzionalità specifica
.WithOpenApi();

// Endpoint di ricerca avanzata con query parameters opzionali
// Supporta ricerca per nome, tipo, o combinazione di entrambi
// Esempi: /api/pokemon/search?name=pika, /api/pokemon/search?type=fire, /api/pokemon/search?name=char&type=fire
app.MapGet("/api/pokemon/search", async (IPokemonService pokemonService, string? name, string? type) =>
{
    try
    {
        // Validazione: almeno uno dei parametri deve essere fornito
        // HTTP 400 Bad Request è semanticamente corretto per richieste malformate
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(type))
            return Results.BadRequest("Please provide a name or type parameter to search for Pokemon");

        // Lista per accumulare i risultati della ricerca
        List<Pokemon> pokemonList = [];

        // Caso 1: Solo ricerca per nome
        // Utilizza Contains case-insensitive per ricerca user-friendly
        if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(type))
        {
            List<Pokemon> listByName = await pokemonService.GetPokemonByName(name);
            pokemonList.AddRange(listByName);
        }

        // Caso 2: Solo ricerca per tipo
        // Cerca Pokemon che hanno il tipo specificato come primario o secondario
        if (!string.IsNullOrWhiteSpace(type) && string.IsNullOrWhiteSpace(name))
        {
            List<Pokemon> listByType = await pokemonService.GetPokemonByType(type);
            pokemonList.AddRange(listByType);
        }

        // Caso 3: Ricerca combinata con logica AND
        // Prima filtra per tipo, poi applica il filtro per nome sui risultati
        // Questo approccio evita duplicati e implementa logica AND intuitiva
        if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(name))
        {
            List<Pokemon> listByType = await pokemonService.GetPokemonByType(type);
            // Filtra ulteriormente per nome utilizzando StringComparison per performance ottimali
            pokemonList.AddRange(listByType.Where(p =>
                p.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase)));
        }

        // Se nessun Pokemon corrisponde ai criteri, restituisce 404
        // Indica che la ricerca è stata eseguita correttamente ma non ha prodotto risultati
        if (pokemonList.Count == 0)
            return Results.NotFound("Pokemon not found");

        return Results.Ok(pokemonList);
    }
    catch (Exception ex)
    {
        // Gestisce errori imprevisti durante la ricerca
        return Results.Problem(
            title: "Error searching Pokemon",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("SearchPokemon")           // Nome generico che riflette la flessibilità dell'endpoint
.WithOpenApi();                     // Documentazione con schema per query parameters opzionali

// Endpoint per ottenere la descrizione di un'abilità Pokemon
// Accetta l'URL dell'abilità come parametro di query e restituisce la descrizione in inglese
// Esempio: GET /api/ability/description?url=https://pokeapi.co/api/v2/ability/65/
app.MapGet("/api/ability/description", async (IAbilityService abilityService, string url) =>
{
    try
    {
        // Validazione dell'URL
        if (string.IsNullOrWhiteSpace(url))
            return Results.BadRequest("Ability URL is required");

        // Ottiene la descrizione dell'abilità
        var abilityDescription = await abilityService.GetAbilityDescriptionAsync(url);

        // Se l'abilità non viene trovata o non ha descrizione in inglese
        if (abilityDescription == null)
            return Results.NotFound("Ability description not found or not available in English");

        return Results.Ok(abilityDescription);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error retrieving ability description",
            detail: ex.Message,
            statusCode: 500
        );
    }
})
.WithName("GetAbilityDescription")
.WithOpenApi();

// Avvia l'applicazione e inizia ad ascoltare le richieste HTTP
// Questo è un metodo bloccante che mantiene l'applicazione in esecuzione
// L'applicazione rimarrà attiva fino a ricevere un segnale di terminazione (Ctrl+C)
app.Run();