using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Orleans.WebApi.Generator.Metas;

namespace Orleans.WebApi.Generator
{
    public static class Extensions
    {
        public static (string? controllerName, string? grainClassNamePrefix) GetProps(this AttributeData attribute)
        {
            var list = attribute.ConstructorArguments.ToArray();
            return (list[2].Value as string, list[1].Value as string);
        }

        public static AttributeMetaInfo GetMetaInfo(this AttributeData attribute)
        {
            if (attribute.AttributeClass == default)
            {
                throw new ArgumentNullException(nameof(attribute.AttributeClass));
            }

            var attributeMeta = new AttributeMetaInfo(GetNamespace(attribute.AttributeClass.ContainingNamespace), attribute.AttributeClass.Name)
            {
                IsAspNetCoreAttribute = attribute.AttributeClass.GetAttributes().Any(o => o.AttributeClass != default && o.AttributeClass.Name == "AspNetCoreAttribute")
            };

            foreach (var cArgument in attribute.ConstructorArguments)
            {
                if (cArgument.Type?.Name == nameof(Type))
                {
                    attributeMeta.ConstructorArguments.Add($"typeof({cArgument.Value})");
                    continue;
                }

                if (cArgument.Value is string)
                {
                    attributeMeta.ConstructorArguments.Add($"\"{cArgument.Value}\"");
                }
                else if (cArgument.Value is null)
                {
                    attributeMeta.ConstructorArguments.Add("null");
                }
                else
                {
                    attributeMeta.ConstructorArguments.Add(cArgument.Value.ToString());
                }
            }

            foreach (var nArgument in attribute.NamedArguments)
            {
                if (nArgument.Value.Type?.Name == nameof(Type))
                {
                    attributeMeta.NamedArguments.Add($"{nArgument.Key} = typeof({nArgument.Value.Value})");
                    continue;
                }

                if (nArgument.Value.Value is string)
                {
                    attributeMeta.NamedArguments.Add($"{nArgument.Key} = \"{nArgument.Value.Value}\"");
                }
                else if (nArgument.Value.Value is null)
                {
                    attributeMeta.NamedArguments.Add($"{nArgument.Key} = null");
                }
                else
                {
                    attributeMeta.NamedArguments.Add($"{nArgument.Key} = {nArgument.Value.Value}");
                }
            }

            return attributeMeta;
        }

        public static string GetNamespace(this INamespaceSymbol symbol)
        {
            if (symbol.ContainingNamespace == default)
            {
                return symbol.Name;
            }
            else
            {
                var parent = GetNamespace(symbol.ContainingNamespace);
                if (!string.IsNullOrEmpty(parent))
                {
                    return parent + "." + symbol.Name;
                }
                else
                {
                    return symbol.Name;
                }
            }
        }

        public static bool IsControllerAttribute(this AttributeMetaInfo attribute) =>
            attribute.Namespace.StartsWith("Microsoft.AspNetCore")
            || attribute.IsAspNetCoreAttribute
            || attribute.Name == "RouteAttribute"
            || attribute.Name == "ObsoleteAttribute";

        public static bool IsActionAttribute(this AttributeMetaInfo attribute) =>
            attribute.Namespace.StartsWith("Microsoft.AspNetCore")
            || attribute.IsAspNetCoreAttribute
            || attribute.Namespace.Contains("AspNetCore")
            || attribute.Name == "RouteAttribute"
            || attribute.Name == "ObsoleteAttribute"
            || attribute.Name.Contains("Swagger");

        public static bool IsParameterAttribute(this AttributeMetaInfo attribute) =>
             attribute.Namespace.StartsWith("Microsoft.AspNetCore")
            || attribute.IsAspNetCoreAttribute
            || attribute.Namespace.Contains("AspNetCore");

        public static string? GetNamespace(this TypeDeclarationSyntax source)
        {
            var parent = source.Parent;
            var nameSpace = parent as FileScopedNamespaceDeclarationSyntax;
            var result = nameSpace?.Name.ToFullString();
            return result;
        }

        public static bool TryParseHttpResult(this ITypeSymbol typeSymbol, out ITypeSymbol? bodyType)
        {
            if (typeSymbol is not INamedTypeSymbol namedType)
            {
                namedType = typeSymbol.ContainingType;
            }

            if (namedType.IsGenericType && namedType.BaseType?.Name == "Task")
            {
                var objType = namedType.TypeArguments[0];
                if (objType is not INamedTypeSymbol objNamedType)
                {
                    objNamedType = objType.ContainingType;
                }

                if (!objNamedType.IsGenericType && objNamedType.Name == "HttpResult")
                {
                    bodyType = objNamedType.BaseType?.TypeArguments.First();
                    return true;
                }

                if (objNamedType.IsGenericType && objNamedType.Name == "HttpResult")
                {
                    bodyType = objNamedType.TypeArguments[0];
                    return true;
                }
            }

            bodyType = default;

            return false;
        }

        public static string BuildType(this ITypeSymbol typeSymbol, ref List<string> namespaces)
        {
            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return $"{BuildType(arrayType.ElementType, ref namespaces)}[]";
            }

            if (typeSymbol is not INamedTypeSymbol namedType)
            {
                namedType = typeSymbol.ContainingType;
            }
            namespaces.Add(typeSymbol.ContainingNamespace.GetNamespace());
            if (namedType.IsGenericType)
            {
                var type = namedType.Name + "<";
                for (int index = 0; index < namedType.TypeArguments.Length; index++)
                {
                    var childType = namedType.TypeArguments[index];
                    var childValue = BuildType(childType, ref namespaces);
                    if (index > 0)
                    {
                        type += ",";
                    }
                    type += childValue;
                }
                type += ">";

                return type;
            }
            else
            {
                if (namedType.Name == "String")
                {
                    return "string";
                }

                return namedType.Name;
            }
        }

        public static string? GetDefaultValue(this IParameterSymbol parameter)
        {
            if (parameter.HasExplicitDefaultValue && parameter.ExplicitDefaultValue != default)
            {
                if (parameter.ExplicitDefaultValue is string value)
                {
                    return $"\"{value}\"";
                }
                else if (parameter.ExplicitDefaultValue.GetType().Name != parameter.Type.Name)
                {
                    return $"({parameter.Type.Name}){parameter.ExplicitDefaultValue}";
                }
                else
                {
                    {
                        return parameter.ExplicitDefaultValue.ToString();
                    }
                }
            }

            return default;
        }
    }
}
