// Services/ShowdownSpritesConverter.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using PokemonApi.Models;

/// <summary>
/// JsonConverter personalizzato che mappa automaticamente gli sprite Pokemon 
/// dalla sezione "other.showdown" invece che dagli sprite standard.
/// Questo permette di utilizzare sempre le GIF animate di Showdown
/// mantenendo invariato il modello PokemonSprites.
/// </summary>
public class ShowdownSpritesConverter : JsonConverter<PokemonSprites>
{
    /// <summary>
    /// Deserializza gli sprite Pokemon mappando automaticamente dai path di Showdown.
    /// Se gli sprite Showdown non sono disponibili, utilizza i valori di fallback standard.
    /// </summary>
    public override PokemonSprites Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Legge l'intero oggetto sprites come JsonElement per navigazione flessibile
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        // Tenta di accedere alla sezione showdown negli sprite
        JsonElement? showdownSprites = null;
        if (root.TryGetProperty("other", out JsonElement other) && 
            other.TryGetProperty("showdown", out JsonElement showdown))
        {
            showdownSprites = showdown;
        }

        // Se gli sprite Showdown sono disponibili, li usa; altrimenti fallback agli sprite standard
        JsonElement spritesToUse = showdownSprites ?? root;

        // Mappa ogni campo del modello PokemonSprites ai valori appropriati
        // Utilizza helper method per gestire valori null e fornire fallback
        return new PokemonSprites(
            BackDefault: GetSpriteUrl(spritesToUse, "back_default") ?? GetSpriteUrl(root, "back_default"),
            BackFemale: GetSpriteUrl(spritesToUse, "back_female") ?? GetSpriteUrl(root, "back_female"),
            BackShinyFemale: GetSpriteUrl(spritesToUse, "back_shiny_female") ?? GetSpriteUrl(root, "back_shiny_female"),
            FrontDefault: GetSpriteUrl(spritesToUse, "front_default") ?? GetSpriteUrl(root, "front_default"),
            FrontFemale: GetSpriteUrl(spritesToUse, "front_female") ?? GetSpriteUrl(root, "front_female"),
            FrontShiny: GetSpriteUrl(spritesToUse, "front_shiny") ?? GetSpriteUrl(root, "front_shiny"),
            FrontShinyFemale: GetSpriteUrl(spritesToUse, "front_shiny_female") ?? GetSpriteUrl(root, "front_shiny_female")
        );
    }

    /// <summary>
    /// Metodo helper per estrarre URL di sprite da un JsonElement in modo sicuro.
    /// Gestisce appropriatamente valori null e stringhe vuote che sono comuni nei dati Pokemon.
    /// </summary>
    /// <param name="element">JsonElement da cui estrarre l'URL</param>
    /// <param name="propertyName">Nome della proprietà contenente l'URL</param>
    /// <returns>URL dello sprite se presente e valido, null altrimenti</returns>
    private static string? GetSpriteUrl(JsonElement element, string propertyName)
    {
        // Verifica che la proprietà esista nel JSON
        if (!element.TryGetProperty(propertyName, out JsonElement property))
            return null;

        // Gestisce il caso in cui il valore JSON sia null esplicito
        if (property.ValueKind == JsonValueKind.Null)
            return null;

        // Estrae la stringa e verifica che non sia vuota
        string? url = property.GetString();
        return string.IsNullOrWhiteSpace(url) ? null : url;
    }

    /// <summary>
    /// Metodo di scrittura per serializzazione (non utilizzato nella nostra API read-only).
    /// Implementato per completezza del contratto JsonConverter.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, PokemonSprites value, JsonSerializerOptions options)
    {
        // Serializza usando il comportamento standard dato che il nostro modello è già corretto
        JsonSerializer.Serialize(writer, value, options);
    }
}