using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Krompaco.RecordCollector.Content.Tests.Converters
{
    public class StringToStreamConverter
    {
        private string s;

        public StringToStreamConverter(string s)
        {
            this.s = s;
        }

        public Stream GetStreamFromString()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
