using Orleans.CodeGeneration;
using Orleans.Serialization;
using System.Text.Json;

namespace Sample.IGrains.Models
{
    [Serializer(typeof(User))]
    public class User
    {
        static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        public string? Name { get; set; }

        public int? Sex { get; set; }

        public Dictionary<string, object> ExtensionValues { get; set; }

        [CopierMethod]
        public static object DeepCopier(object original, ICopyContext context)
        {
            return original;
        }

        [SerializerMethod]
        public static void Serializer(object untypedInput, ISerializationContext context, Type expected)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(untypedInput, options);
            context.StreamWriter.Write(bytes.Length);
            context.StreamWriter.Write(bytes);
        }

        [DeserializerMethod]
        public static object Deserializer(Type expected, IDeserializationContext context)
        {
            var length = context.StreamReader.ReadInt();
            var bytes = context.StreamReader.ReadBytes(length);
            return JsonSerializer.Deserialize<User>(bytes, options);
        }
    }
}
