using System.Text;

namespace Krompaco.RecordCollector.Content.FrontMatterParsers
{
    public class SummaryExtractor
    {
        private readonly StreamReader tr;

        public SummaryExtractor(StreamReader tr)
        {
            this.tr = tr;
        }

        public string? GetSummaryFromContent()
        {
            var rowCount = 0;
            var frontMatterOpened = false;
            var frontMatterClosed = false;
            var sb = new StringBuilder();

            while (this.tr.Peek() >= 0)
            {
                rowCount++;
                var line = this.tr.ReadLine() ?? string.Empty;

                if (line.Trim() == "<!--more-->" || line.Trim() == "# more")
                {
                    return this.ResetStreamAndReturn(sb.ToString());
                }

                if (frontMatterClosed)
                {
                    sb.AppendLine(line);
                }

                if (frontMatterOpened && line.StartsWith("}", StringComparison.Ordinal))
                {
                    frontMatterClosed = true;
                }

                if (line.StartsWith("{", StringComparison.Ordinal))
                {
                    frontMatterOpened = true;
                }

                if (line.Trim() == "+++")
                {
                    if (frontMatterOpened)
                    {
                        frontMatterClosed = true;
                    }

                    frontMatterOpened = true;
                }

                if (line.Trim() == "---")
                {
                    if (frontMatterOpened)
                    {
                        frontMatterClosed = true;
                    }

                    frontMatterOpened = true;
                }

                if (line.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                {
                    return this.ResetStreamAndReturn(null);
                }

                if (rowCount == 100)
                {
                    break;
                }
            }

            return this.ResetStreamAndReturn(null);
        }

        private string? ResetStreamAndReturn(string? summary)
        {
            this.tr.BaseStream.Position = 0;
            this.tr.DiscardBufferedData();
            return summary;
        }
    }
}
