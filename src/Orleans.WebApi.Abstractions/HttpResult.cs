using Orleans.CodeGeneration;
using Orleans.Serialization;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Orleans.WebApi.Abstractions
{
    public record HttpResult<T>
    {
        public Dictionary<string, string>? ResponseHeaders { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public T? Body { get; set; }
    }

    [Serializer(typeof(HttpResult))]
    public record HttpResult : HttpResult<object>
    {
        static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

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
        public static object? Deserializer(Type expected, IDeserializationContext context)
        {
            var length = context.StreamReader.ReadInt();
            var bytes = context.StreamReader.ReadBytes(length);
            return JsonSerializer.Deserialize<HttpResult>(bytes, options);
        }
    }
}
