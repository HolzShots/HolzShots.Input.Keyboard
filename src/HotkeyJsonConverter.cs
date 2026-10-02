using System.Text.Json;
using System.Text.Json.Serialization;

namespace HolzShots.Input.Keyboard;

/// <summary>Converts hotkeys to and from their string representation in JSON.</summary>
public sealed class HotkeyJsonConverter : JsonConverter<Hotkey>
{
    /// <inheritdoc />
    public override Hotkey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("A hotkey must be a string.");

        var value = reader.GetString();
        if (!Hotkey.TryParse(value, out var hotkey))
            throw new JsonException($"Invalid hotkey: {value}");

        return hotkey;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Hotkey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}