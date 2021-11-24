using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Orleans.WebApi.Generator
{
    public class XmlReaderProvider
    {
        private readonly string _filePath;
        private static readonly XmlReaderSettings _xmlSettings = new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
        };
        private Dictionary<string, string> _docComments;

        public XmlReaderProvider(string filePath)

        {
            _filePath = filePath;
        }

        private Stream GetSourceStream() =>
            new FileStream(_filePath, FileMode.Open, FileAccess.Read);

        private XDocument GetXDocument()
        {
            using var stream = GetSourceStream();
            using var xmlReader = XmlReader.Create(stream, _xmlSettings);
            return XDocument.Load(xmlReader);
        }

        public string? GetDocumentationForSymbol(string? documentationMemberId)
        {
            if (documentationMemberId == default)
            {
                return default;
            }

            if (_docComments == null)
            {
                try
                {
                    var comments = new Dictionary<string, string>();

                    var doc = GetXDocument();
                    foreach (var e in doc.Descendants("member"))
                    {
                        if (e.Attribute("name") != null)
                        {
                            using var reader = e.CreateReader();
                            reader.MoveToContent();
                            comments[e.Attribute("name").Value] = reader.ReadInnerXml();
                        }
                    }

                    _docComments = comments;
                }
                catch (Exception)
                {
                    _docComments = new Dictionary<string, string>();
                }
            }

            return _docComments.TryGetValue(documentationMemberId, out var docComment) ? docComment : "";
        }
    }
}
