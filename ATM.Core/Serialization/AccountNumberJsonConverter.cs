using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATM.Core.Serialization
{

    public class AccountNumberJsonConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                int oldNumber = reader.GetInt32();
                return oldNumber == 0 ? string.Empty : $"GE{oldNumber:D9}";
            }
            return reader.GetString() ?? string.Empty;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
