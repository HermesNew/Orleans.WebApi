
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Orleans.WebApi.Generator.Metas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Orleans.WebApi.Generator;
[Generator]
public class MapToWebApiGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            //Debugger.Launch();
            if (context.SyntaxReceiver is SyntaxReceiver receiver && receiver.CandidateTypes.Any())
            {
                Dictionary<string, XmlReaderProvider> xmlProviderDict = new();
                foreach (var classSyntax in receiver.CandidateTypes)
                {
                    var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);
                    var type = semanticModel.GetDeclaredSymbol(classSyntax);

                    if (type == default)
                    {
                        break;
                    }
                    var attributes = type.GetAttributes();
                    var mapToWebApiAttributes = attributes.Where(o => o.AttributeClass != default && o.AttributeClass.Name == "MapToWebApiAttribute");

                    var classControllerAttributes = attributes
                        .Where(o => o.AttributeClass != default && o.AttributeClass.Name != "MapToWebApiAttribute")
                        .Select(a => a.GetMetaInfo()).Where(o => o.IsControllerAttribute()).ToList();

                    foreach (var attribute in mapToWebApiAttributes)
                    {
                        if (attribute.ConstructorArguments.First().Value is INamedTypeSymbol attributeInterfaceType)
                        {
                            XmlReaderProvider xmlProvider = default;
                            var metadataReference = context.Compilation.GetMetadataReference(attributeInterfaceType.ContainingAssembly);
                            if (metadataReference is not null)
                            {
                                var propInfo = metadataReference.GetType().GetProperty("FilePath");
                                var path = propInfo?.GetValue(metadataReference)?.ToString()?.Replace("\\ref\\", "/").Replace(".dll", ".xml");

                                if (!string.IsNullOrEmpty(path))
                                {
                                    if (!xmlProviderDict.TryGetValue(path, out xmlProvider))
                                    {
                                        xmlProvider = new XmlReaderProvider(path);
                                        xmlProviderDict.Add(path, xmlProvider);
                                    }
                                }
                            }
                            var primaryKeyType = "string";
                            var isCompoundPrimaryKey = false;

                            foreach (var classInterface in attributeInterfaceType.Interfaces)
                            {
                                if (classInterface.Name == "IGrainWithGuidKey")
                                {
                                    primaryKeyType = nameof(Guid);
                                }
                                else if (classInterface.Name == "IGrainWithIntegerKey")
                                {
                                    primaryKeyType = "long";
                                }
                                else if (classInterface.Name == "IGrainWithGuidCompoundKey")
                                {
                                    primaryKeyType = nameof(Guid);
                                    isCompoundPrimaryKey = true;
                                }
                                else if (classInterface.Name == "IGrainWithIntegerCompoundKey")
                                {
                                    primaryKeyType = "long";
                                    isCompoundPrimaryKey = true;
                                }
                            }
                            var members = attributeInterfaceType.GetMembers();
                            var methodMetaList = new List<MethodMetaInfo>();
                            var namespaces = new List<string>
                            {
                                "System",
                                "Microsoft.AspNetCore.Mvc",
                                "Orleans.WebApi.Abstractions",
                                "Orleans",
                                "RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute",
                                attributeInterfaceType.ContainingNamespace.GetNamespace()
                            };

                            foreach (var classControllerAttribute in classControllerAttributes)
                            {
                                namespaces.Add(classControllerAttribute.Namespace);
                            }

                            var (controllerName, grainClassNamePrefix) = attribute.GetProps();

                            foreach (var method in members.OfType<IMethodSymbol>())
                            {
                                var returnValue = method.ReturnType.BuildType(ref namespaces);
                                var methodMeta = new MethodMetaInfo
                                {
                                    Symbol = method,
                                    Name = method.Name,
                                    Return = returnValue
                                };

                                foreach (var parameter in method.Parameters)
                                {
                                    var parameterMetaInfo = new ParameterMetaInfo
                                    {
                                        Name = parameter.Name,
                                        Type = parameter.Type.BuildType(ref namespaces),
                                        DefaultValue = parameter.GetDefaultValue()
                                    };

                                    foreach (var paraAttribute in parameter.GetAttributes())
                                    {
                                        var metaData = paraAttribute.GetMetaInfo();
                                        if (metaData.IsParameterAttribute())
                                        {
                                            parameterMetaInfo.Attributes.Add(metaData);
                                            namespaces.Add(metaData.Namespace);
                                        }
                                    }

                                    methodMeta.Parameters.Add(parameterMetaInfo);
                                }

                                foreach (var methodAttribute in method.GetAttributes())
                                {
                                    var metaData = methodAttribute.GetMetaInfo();
                                    if (metaData.IsActionAttribute())
                                    {
                                        methodMeta.Attributes.Add(metaData);
                                        namespaces.Add(metaData.Namespace);
                                    }
                                }

                                methodMetaList.Add(methodMeta);
                            }

                            var classAttributes = attributeInterfaceType.GetAttributes()
                                .Select(a => a.GetMetaInfo())
                                .Where(o => o.IsControllerAttribute())
                                .ToList();

                            namespaces.AddRange(classAttributes.Select(o => o.Namespace));

                            using var sourcebuilder = new SourceBuilder();

                            sourcebuilder.WriteUsings(namespaces.OrderBy(o => o.Length).Distinct());

                            sourcebuilder.WriteLine();
                            sourcebuilder.WriteLine($"namespace {classSyntax.GetNamespace() ?? "Orleans.WebApi"}");
                            sourcebuilder.WriteOpeningBracket();

                            if (xmlProvider != default)
                            {
                                var classComment = xmlProvider.GetDocumentationForSymbol(attributeInterfaceType.GetDocumentationCommentId())?.Trim();
                                if (classComment != default && !string.IsNullOrEmpty(classComment))
                                {
                                    sourcebuilder.WriteComment(classComment.Split(new char[2] { '\r', '\n' }));
                                }
                            }

                            if (string.IsNullOrEmpty(controllerName))
                            {
                                controllerName = attributeInterfaceType.Name;
                            }

                            if (classControllerAttributes.Count > 0)
                            {
                                sourcebuilder.WriteAttributes(classControllerAttributes, true);
                            }

                            sourcebuilder.WriteAttributes(classAttributes, true);
                            sourcebuilder.WriteLine($"public class {controllerName}Controller : ControllerBase");
                            sourcebuilder.WriteOpeningBracket();

                            sourcebuilder.WriteLine("private readonly IClusterFactory clusterFactory;");
                            sourcebuilder.WriteLine();
                            sourcebuilder.WriteLine($"public {controllerName}Controller(IClusterFactory clusterFactory)");

                            sourcebuilder.WriteOpeningBracket();
                            sourcebuilder.WriteLine("this.clusterFactory = clusterFactory;");
                            sourcebuilder.WriteClosingBracket();

                            foreach (var method in methodMetaList)
                            {
                                if (string.IsNullOrEmpty(method.Return))
                                {
                                    throw new TypeUnloadedException(method.Return);
                                }
                                sourcebuilder.WriteLine();
                                if (xmlProvider != default)
                                {
                                    var grainComment = xmlProvider.GetDocumentationForSymbol(method.Symbol?.GetDocumentationCommentId())?.Trim();
                                    if (grainComment != default && !string.IsNullOrEmpty(grainComment))
                                    {
                                        var lines = BuildActionComment(grainComment, isCompoundPrimaryKey);
                                        sourcebuilder.WriteComment(lines);
                                    }
                                }
                                sourcebuilder.WriteAttributes(method.Attributes, true);
                                sourcebuilder.Write($"public {method.Return} {method.Name}([FromRoute]{primaryKeyType} grainId");

                                if (isCompoundPrimaryKey)
                                {
                                    sourcebuilder.Write(", [FromRoute]string keyExt");
                                }

                                foreach (var para in method.Parameters)
                                {
                                    sourcebuilder.Write(", ");
                                    if (para.Attributes.Count > 0)
                                    {
                                        sourcebuilder.WriteAttributes(para.Attributes, false);
                                    }

                                    if (para.Type == "String")
                                    {
                                        para.Type = "string";
                                    }

                                    sourcebuilder.Write($"{para.Type} {para.Name}");
                                    if (!string.IsNullOrEmpty(para.DefaultValue))
                                    {
                                        sourcebuilder.Write(" = " + para.DefaultValue);
                                    }
                                }
                                sourcebuilder.Write(")");
                                sourcebuilder.WriteLine();

                                sourcebuilder.WriteOpeningBracket();

                                if (!string.IsNullOrEmpty(grainClassNamePrefix))
                                {
                                    if (isCompoundPrimaryKey)
                                    {
                                        sourcebuilder.Write($"return this.clusterFactory.GetCluster<{attributeInterfaceType.Name}, {primaryKeyType}>(grainId).GetGrain<{attributeInterfaceType.Name}>(grainId, keyExt, \"{grainClassNamePrefix}\").{ method.Name} (");
                                    }
                                    else
                                    {
                                        sourcebuilder.Write($"return this.clusterFactory.GetCluster<{attributeInterfaceType.Name}, {primaryKeyType}>(grainId).GetGrain<{attributeInterfaceType.Name}>(grainId, \"{grainClassNamePrefix}\").{ method.Name} (");
                                    }
                                }
                                else
                                {
                                    if (isCompoundPrimaryKey)
                                    {
                                        sourcebuilder.Write($"return this.clusterFactory.GetCluster<{attributeInterfaceType.Name}, {primaryKeyType}>(grainId).GetGrain<{attributeInterfaceType.Name}>(grainId, keyExt, null).{method.Name}(");
                                    }
                                    else
                                    {
                                        sourcebuilder.Write($"return this.clusterFactory.GetCluster<{attributeInterfaceType.Name}, {primaryKeyType}>(grainId).GetGrain<{attributeInterfaceType.Name}>(grainId).{method.Name}(");
                                    }
                                }

                                for (int index = 0; index < method.Parameters.Count; index++)
                                {
                                    var para = method.Parameters[index];
                                    if (index > 0)
                                    {
                                        sourcebuilder.Write(", ");
                                    }
                                    sourcebuilder.Write($"{para.Name}");
                                }
                                sourcebuilder.Write(");");
                                sourcebuilder.WriteLine();
                                sourcebuilder.WriteClosingBracket();
                            }

                            sourcebuilder.WriteClosingBracket();

                            sourcebuilder.WriteClosingBracket();

                            //Debugger.Launch();
                            context.AddSource($"{controllerName}Controller.g.cs", sourcebuilder.ToString());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    private string[] BuildActionComment(string grainComment, bool isCompoundPrimaryKey)
    {
        var lines = grainComment.Split(new char[2] { '\r', '\n' });
        var summaryEndLine = lines.Where(line => !string.IsNullOrEmpty(line) && line.Contains("</summary>")).FirstOrDefault();
        if (summaryEndLine != default)
        {
            var newLines = new List<string>();
            foreach (var line in lines)
            {
                newLines.Add(line);
                if (line == summaryEndLine)
                {
                    newLines.Add("<param name=\"grainId\">primary key of Grain</param>");
                    if (isCompoundPrimaryKey)
                    {
                        newLines.Add("<param name=\"keyExt\">compound primary key of Grain</param>");
                    }
                }
            }
            return newLines.ToArray();
        }

        return lines;
    }
}
