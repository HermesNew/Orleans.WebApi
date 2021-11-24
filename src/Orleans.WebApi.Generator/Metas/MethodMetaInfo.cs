using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Orleans.WebApi.Generator.Metas
{
    public record MethodMetaInfo
    {
        public IMethodSymbol Symbol { get; set; }

        public string Name { get; set; }

        public string Return { get; set; }

        public List<ParameterMetaInfo> Parameters { get; set; } = new List<ParameterMetaInfo>();

        public List<AttributeMetaInfo> Attributes { get; set; } = new List<AttributeMetaInfo>();
    }
}
