using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Krompaco.RecordCollector.Content.Models;

namespace Krompaco.RecordCollector.Content.FrontMatterParsers
{
    public class YamlParser
    {
        private TextReader tr;

        public YamlParser(TextReader tr)
        {
            this.tr = tr;
        }

        public SinglePage GetAsSinglePage()
        {
            var fm = string.Empty;
            var frontMatterOpened = false;

            while (this.tr.Peek() >= 0)
            {
                var line = this.tr.ReadLine()?.Trim();

                if (line == "---")
                {
                    if (frontMatterOpened)
                    {
                        break;
                    }

                    frontMatterOpened = true;
                    line = this.tr.ReadLine()?.Trim();
                }

                if (frontMatterOpened)
                {
                    fm += line + "\r\n";
                }
            }

            var json = fm;
            using TextReader sr = new StringReader(json);
            var jsonParser = new JsonParser(sr);
            var single = jsonParser.GetAsSinglePage();
            return single;
        }
    }
}
