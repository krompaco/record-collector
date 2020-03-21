using System;
using System.IO;
using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Content.FrontMatterParsers
{
    public class TypeDetector
    {
        private readonly StreamReader tr;

        public TypeDetector(StreamReader tr)
        {
            this.tr = tr;
        }

        public FrontMatterType GetFrontMatterType()
        {
            var rowCount = 0;

            while (this.tr.Peek() >= 0)
            {
                rowCount++;
                var line = this.tr.ReadLine() ?? string.Empty;

                if (line.StartsWith("{", StringComparison.Ordinal))
                {
                    return this.ResetStreamAndReturn(FrontMatterType.Json);
                }

                if (line.Trim() == "+++")
                {
                    return this.ResetStreamAndReturn(FrontMatterType.Toml);
                }

                if (line.Trim() == "---")
                {
                    return this.ResetStreamAndReturn(FrontMatterType.Yaml);
                }

                if (line.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                {
                    return this.ResetStreamAndReturn(FrontMatterType.HtmlDocument);
                }

                if (rowCount == 5)
                {
                    break;
                }
            }

            return this.ResetStreamAndReturn(FrontMatterType.MarkdownDocument);
        }

        private FrontMatterType ResetStreamAndReturn(FrontMatterType fmt)
        {
            this.tr.BaseStream.Position = 0;
            this.tr.DiscardBufferedData();
            return fmt;
        }
    }
}
