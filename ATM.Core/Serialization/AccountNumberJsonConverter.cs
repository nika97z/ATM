using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATM.Core.Serialization
{
    // Account numbers used to be saved as plain numbers such as 12345. They now look like GE123456789,
    // so an old number is read as GE followed by the number padded to 9 digits (12345 -> GE000012345).
    // The file is rewritten in the new format the next time a user is saved.
    public class AccountNumberJsonConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                int oldNumber = reader.GetInt32();
                // 0 was the default for a loan that was never requested, meaning "no account".
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
