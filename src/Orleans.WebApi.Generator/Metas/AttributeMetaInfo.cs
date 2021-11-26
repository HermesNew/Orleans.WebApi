namespace Orleans.WebApi.Generator.Metas
{
    public record AttributeMetaInfo
    {
        public AttributeMetaInfo(string nspace, string name)
        {
            this.Namespace = nspace;
            this.Name = name;
        }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public bool IsAspNetCoreAttribute { get; set; }

        public List<string> ConstructorArguments { get; set; } = new List<string>();

        public List<string> NamedArguments { get; set; } = new List<string>();
    }
}
