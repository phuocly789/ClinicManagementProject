using System.Text.Json;
using System.Text.Json.Serialization;

public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // ✅ Thử parse trực tiếp kiểu ISO (có giờ)
            if (DateTime.TryParse(dateString, out var dateTime))
                return DateOnly.FromDateTime(dateTime);

            // ✅ Thử parse đúng định dạng yyyy-MM-dd
            if (
                DateOnly.TryParseExact(
                    dateString,
                    Format,
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var date
                )
            )
                return date;
        }

        throw new JsonException($"Cannot convert to DateOnly: {reader.GetString()}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateOnly? value,
        JsonSerializerOptions options
    )
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString(Format));
    }
}
