using System.Collections.Generic;

namespace Orleans.WebApi.Generator.Metas
{
    public record ParameterMetaInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string DefaultValue { get; set; }

        public List<AttributeMetaInfo> Attributes { get; set; } = new List<AttributeMetaInfo>();
    }
}
