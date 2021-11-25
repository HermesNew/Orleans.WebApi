using Orleans.WebApi.Generator.Metas;
using System.CodeDom.Compiler;

namespace Orleans.WebApi.Generator
{
    internal sealed class SourceBuilder : IDisposable
    {
        private readonly StringWriter _writer;
        private readonly IndentedTextWriter _indentedWriter;

        public SourceBuilder()
        {
            _writer = new StringWriter();
            _indentedWriter = new IndentedTextWriter(_writer, new string(' ', 4));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _writer.Dispose();
            _indentedWriter.Dispose();
        }

        public SourceBuilder WriteLine(string? value = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _indentedWriter.WriteLine();
            }
            else
            {
                _indentedWriter.WriteLine(value);
            }

            return this;
        }

        public SourceBuilder Write(string? value = null)
        {
            _indentedWriter.Write(value);
            return this;
        }

        public SourceBuilder WriteLineIf(bool condition, string? value)
        {
            if (condition)
            {
                WriteLine(value);
            }

            return this;
        }

        public SourceBuilder WriteNullableContextOptionIf(bool enabled) => WriteLineIf(enabled, "#nullable enable");

        public SourceBuilder WriteOpeningBracket()
        {
            _indentedWriter.WriteLine("{");
            _indentedWriter.Indent++;

            return this;
        }

        public SourceBuilder WriteClosingBracket()
        {
            _indentedWriter.Indent--;
            _indentedWriter.WriteLine("}");

            return this;
        }

        public SourceBuilder WriteUsings(IEnumerable<string> usings)
        {
            foreach (var u in usings)
            {
                WriteUsing(u);
            }

            return this;
        }

        public SourceBuilder WriteUsing(string u)
        {
            WriteLine($"using {u};");

            return this;
        }

        public void WriteComment(string[] commentLines)
        {
            foreach (var line in commentLines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    WriteLine($"/// {line.Trim()}");
                }
            }
        }

        public void WriteAttributes(List<AttributeMetaInfo> attributeMetaInfos, bool lineFeed)
        {
            foreach (var methodAttribute in attributeMetaInfos)
            {
                Write($"[{methodAttribute.Name?.Substring(0, methodAttribute.Name.Length - 9)}");
                if (methodAttribute.ConstructorArguments.Count > 0 || methodAttribute.NamedArguments.Count > 0)
                {
                    Write("(");
                    var paraIndex = 0;
                    foreach (var arg in methodAttribute.ConstructorArguments)
                    {
                        if (paraIndex > 0)
                        {
                            Write(" , ");
                        }
                        Write(arg);
                        paraIndex++;
                    }

                    foreach (var arg in methodAttribute.NamedArguments)
                    {
                        if (paraIndex > 0)
                        {
                            Write(" , ");
                        }
                        Write(arg);
                        paraIndex++;
                    }
                    Write(")");
                }

                Write("]");
                if (lineFeed)
                {
                    WriteLine();
                }
            }
        }

        public SourceBuilder Indent()
        {
            _indentedWriter.Indent++;
            return this;
        }

        public SourceBuilder Unindent()
        {
            _indentedWriter.Indent--;
            return this;
        }

        /// <inheritdoc />
        public override string ToString() => _writer.ToString();
    }
}
