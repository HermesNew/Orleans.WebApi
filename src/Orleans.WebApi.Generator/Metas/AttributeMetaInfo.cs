using System.Collections.Generic;

namespace Orleans.WebApi.Generator.Metas
{
    public record AttributeMetaInfo
    {
        public string Namespace { get; set; }

        public string Name { get; set; }

        public bool IsAspNetCoreAttribute { get; set; }

        public List<string> ConstructorArguments { get; set; } = new List<string>();

        public List<string> NamedArguments { get; set; } = new List<string>();
    }
}
