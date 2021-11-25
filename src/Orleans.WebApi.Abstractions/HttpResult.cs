using System.Net;

namespace Orleans.WebApi.Abstractions
{
    public record HttpResult<T>
    {
        public Dictionary<string, string>? ResponseHeaders { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public T? Body { get; set; }
    }
}
