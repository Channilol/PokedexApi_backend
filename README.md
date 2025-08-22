# 🐾 Pokemon API

Una moderna API REST costruita con .NET 9 e Minimal APIs per esplorare e ricercare informazioni sui Pokemon. Questo progetto dimostra l'implementazione di pattern architetturali avanzati come lazy loading, dependency injection e gestione robusta degli errori.

## 🚀 Caratteristiche Principali

Questa API offre un accesso completo e performante ai dati di tutti i Pokemon attraverso endpoint intuitivi e ben documentati. Il sistema è progettato per essere scalabile e maintainable, utilizzando le tecnologie più moderne dell'ecosistema .NET.

L'architettura implementa un pattern di lazy loading intelligente che carica i dati Pokemon solo quando necessario, ottimizzando sia i tempi di avvio dell'applicazione che l'utilizzo della memoria. Una volta caricati, i dati rimangono in memoria per garantire risposte rapidissime alle richieste successive.

Il sistema di ricerca supporta filtri flessibili e combinabili per nome e tipo, con ricerca case-insensitive che rende l'API estremamente user-friendly. La gestione degli errori segue le best practice REST, restituendo codici di stato HTTP appropriati e messaggi di errore informativi.

## 🛠️ Tecnologie Utilizzate

**Backend Framework**: .NET 9 con Minimal APIs per massime prestazioni e codice conciso

**Architettura**: Pattern Repository con Dependency Injection e Singleton Service per gestione ottimale delle risorse

**Data Management**: Lazy Loading con System.Text.Json per deserializzazione performante

**API Documentation**: OpenAPI/Swagger integrato per documentazione automatica e testing interattivo

**Cross-Origin Support**: CORS configurato per integrazione seamless con applicazioni frontend

## 📋 Prerequisiti

Per eseguire questo progetto localmente, avrai bisogno di avere installato .NET 9 SDK sulla tua macchina. Puoi verificare l'installazione aprendo un terminale ed eseguendo il comando `dotnet --version`. Se non hai .NET installato, puoi scaricarlo gratuitamente dal sito ufficiale Microsoft.

L'editor consigliato è Visual Studio Code con l'estensione "C# Dev Kit" per un'esperienza di sviluppo ottimale con IntelliSense completo, debugging integrato e supporto per progetti .NET moderni.

## 🚀 Installazione e Avvio

Clona il repository sulla tua macchina locale usando Git, poi naviga nella cartella del progetto. Il processo di setup è progettato per essere il più semplice possibile.

```bash
git clone https://github.com/Channilol/PokedexApi_backend.git
cd pokemon-api
```

Assicurati che il file dei dati Pokemon sia posizionato correttamente nella cartella `Data` all'interno del progetto. Il file dovrebbe chiamarsi `pokemon_data.json` e contenere tutti i dati Pokemon in formato JSON compatibile con PokeAPI.

Installa le dipendenze e avvia l'applicazione con questi comandi:

```bash
dotnet restore
dotnet run
```

L'applicazione si avvierà tipicamente sulla porta 5026 per HTTPS e 5000 per HTTP. Una volta avviata, potrai accedere alla documentazione interattiva dell'API visitando `https://localhost:5026/openapi` nel tuo browser.

## 📚 Documentazione API

### Endpoint per Recupero Completo

**GET** `/api/pokemon`

Restituisce la lista completa di tutti i Pokemon ordinati per ID numerico. Questo endpoint utilizza il lazy loading, quindi la prima richiesta potrebbe richiedere alcuni secondi per caricare tutti i dati, mentre le richieste successive saranno praticamente istantanee.

```bash
curl -X GET "https://localhost:5026/api/pokemon"
```

La risposta include tutti i dettagli di ogni Pokemon: informazioni di base come nome e ID, statistiche complete, tipi, abilità e collegamenti agli sprite delle immagini.

### Endpoint per Pokemon Specifico

**GET** `/api/pokemon/{id}`

Recupera i dettagli completi di un Pokemon specifico utilizzando il suo ID numerico. Se il Pokemon richiesto non esiste, l'endpoint restituisce appropriatamente un errore 404 Not Found.

```bash
curl -X GET "https://localhost:5026/api/pokemon/25"
```

Questo endpoint è ottimizzato per prestazioni massime utilizzando lookup diretti nel dictionary interno, garantendo tempi di risposta costanti indipendentemente dall'ID richiesto.

### Endpoint per Generazioni Pokemon

**GET** `/api/pokemon/generation/{generationNumber}`

Ottiene tutti i Pokemon appartenenti a una generazione specifica. Le generazioni sono numerate da 1 a 9, seguendo la classificazione ufficiale dei giochi Pokemon.

```bash
curl -X GET "https://localhost:5026/api/pokemon/generation/1"
```

La logica di appartenenza alla generazione è basata sui range di ID Pokemon ufficiali, garantendo accuratezza completa nella classificazione. Se viene richiesta una generazione inesistente, l'endpoint restituisce 404 Not Found.

### Endpoint di Ricerca Avanzata

**GET** `/api/pokemon/search`

Permette ricerche flessibili utilizzando parametri di query opzionali per nome e tipo. Questo endpoint supporta tre modalità di ricerca distinte per massima flessibilità.

**Ricerca per Nome Solo**
```bash
curl -X GET "https://localhost:5026/api/pokemon/search?name=pika"
```

Trova tutti i Pokemon il cui nome contiene la stringa specificata, utilizzando ricerca case-insensitive per massima usabilità.

**Ricerca per Tipo Solo**
```bash
curl -X GET "https://localhost:5026/api/pokemon/search?type=fire"
```

Restituisce tutti i Pokemon che possiedono il tipo specificato come tipo primario o secondario.

**Ricerca Combinata (Logica AND)**
```bash
curl -X GET "https://localhost:5026/api/pokemon/search?name=char&type=fire"
```

Quando entrambi i parametri sono forniti, l'API utilizza logica AND per trovare Pokemon che soddisfano entrambi i criteri simultaneamente, offrendo risultati più specifici e utili.

## 🏗️ Architettura del Sistema

### Pattern Architetturali Implementati

L'applicazione è costruita seguendo principi di clean architecture con separazione chiara delle responsabilità. Il layer dei modelli definisce la struttura dei dati Pokemon utilizzando record C# moderni per immutabilità e prestazioni ottimali.

Il layer dei servizi implementa la logica business e l'accesso ai dati attraverso un'interfaccia pulita che astrae i dettagli implementativi. Questo design facilita enormemente il testing e future modifiche alla strategia di accesso ai dati.

Il layer degli endpoint utilizza le Minimal APIs di .NET per definire le route HTTP in modo conciso e performante, con dependency injection automatica per i servizi necessari.

### Gestione Avanzata dei Dati

Il sistema utilizza un pattern di lazy loading sophisticato che combina la classe `Lazy<T>` di .NET con operazioni asincrone per garantire che i dati vengano caricati una sola volta, anche in scenari multi-thread tipici delle applicazioni web.

I dati vengono mantenuti in memoria utilizzando strutture `Dictionary<string, Pokemon>` per garantire lookup O(1) per ricerche per ID, mentre le operazioni di filtro utilizzano LINQ per prestazioni ottimali su operazioni complesse.

La deserializzazione JSON è ottimizzata utilizzando `System.Text.Json` con configurazioni specifiche per gestire le differenze tra le convenzioni di naming JSON (snake_case) e C# (PascalCase).

### Gestione Robusta degli Errori

Ogni endpoint implementa gestione completa degli errori con codici di stato HTTP semanticamente corretti. Gli errori 400 Bad Request vengono utilizzati per richieste malformate, 404 Not Found per risorse inesistenti, e 500 Internal Server Error per errori imprevisti del server.

I messaggi di errore sono informativi ma sicuri, fornendo informazioni utili per il debugging senza esporre dettagli sensibili dell'implementazione interna.

## 🎯 Esempi di Utilizzo Avanzato

### Integrazione con Applicazioni Frontend

L'API è progettata per integrazione seamless con applicazioni frontend moderne. Il supporto CORS è configurato per permettere richieste da qualsiasi origine durante lo sviluppo, facilitando l'integrazione con framework come React, Vue, o Angular.

```javascript
// Esempio di utilizzo con fetch API in JavaScript
const getPokemonByGeneration = async (generation) => {
  try {
    const response = await fetch(`https://localhost:5026/api/pokemon/generation/${generation}`);
    if (!response.ok) {
      throw new Error(`Generation ${generation} not found`);
    }
    return await response.json();
  } catch (error) {
    console.error('Error fetching Pokemon:', error);
  }
};
```

### Utilizzo per Applicazioni Mobile

L'API restituisce JSON ottimizzato per applicazioni mobile, includendo solo i dati necessari senza payload eccessivi. Gli sprite Pokemon sono referenziati tramite URL diretti per implementazione facile di image loading asincrono.

## 🔧 Configurazione e Personalizzazione

### Personalizzazione dei Dati

Per utilizzare dataset Pokemon personalizzati, sostituisci il file `Data/pokemon_data.json` con i tuoi dati mantenendo la struttura JSON compatibile con PokeAPI. Il sistema si adatterà automaticamente a diversi dataset purché rispettino lo schema dei modelli definiti.

### Configurazione dell'Ambiente

L'applicazione utilizza il sistema di configurazione standard di .NET, permettendo personalizzazione attraverso file `appsettings.json` per diversi ambienti (Development, Staging, Production).

Per deployment in produzione, considera la configurazione di reverse proxy con nginx o l'utilizzo di servizi cloud come Azure App Service o AWS Elastic Beanstalk.

## 📈 Performance e Scalabilità

### Ottimizzazioni Implementate

Il sistema implementa diverse ottimizzazioni per massimizzare le prestazioni. Il lazy loading riduce il tempo di startup dell'applicazione, mentre il caching in memoria garantisce tempi di risposta sub-millisecondo dopo il caricamento iniziale.

Le operazioni di ricerca utilizzano algoritmi ottimizzati con complessità temporale minima, e la serializzazione JSON è configurata per prestazioni massime utilizzando il serializer più veloce disponibile in .NET.

### Considerazioni per la Scalabilità

Per deployment ad alto traffico, considera l'implementazione di caching distribuito con Redis per condividere i dati tra multiple istanze dell'applicazione. Il design stateless degli endpoint facilita l'implementazione di load balancing orizzontale.

## 🤝 Contribuzioni

Questo progetto è aperto a contribuzioni! Se hai idee per miglioramenti o nuove funzionalità, sentiti libero di aprire un issue o submitare una pull request. Alcune aree che potrebbero beneficiare di contribuzioni includono implementazione di rate limiting, autenticazione JWT, o estensione degli endpoint con funzionalità di filtraggio più avanzate.

## 📄 Licenza e Riconoscimenti

I dati Pokemon utilizzati in questo progetto provengono da [PokeAPI](https://pokeapi.co/), un'incredibile risorsa RESTful per informazioni Pokemon. Tutti i dati Pokemon sono proprietà di The Pokémon Company e Nintendo.

Questo progetto è sviluppato per scopi educativi e di dimostrazione tecnica, utilizzando le API pubbliche e liberamente disponibili di PokeAPI sotto fair use.

## 🐛 Troubleshooting

### Problemi Comuni di Setup

Se incontri errori durante l'avvio, verifica che il file `pokemon_data.json` sia presente nella cartella `Data` e sia in formato JSON valido. Puoi utilizzare strumenti online per validare la sintassi JSON se necessario.

Per problemi di porte già in uso, modifica la configurazione in `appsettings.json` o utilizza l'argomento `--urls` quando avvii l'applicazione con `dotnet run`.

### Debugging e Logging

L'applicazione include logging strutturato che puoi abilitare modificando il livello di log in `appsettings.json`. Per debugging dettagliato, imposta il livello su `Debug` per vedere informazioni complete sulle operazioni di caricamento dati e ricerca.

---

Sviluppato con ❤️ utilizzando .NET 9 e le tecnologie più moderne per API REST performanti e scalabili.
