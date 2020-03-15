using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Krompaco.RecordCollector.Content.IO
{
    public class FileService
    {
        private readonly string contentRoot;

        public FileService(string contentRoot)
        {
            if (string.IsNullOrWhiteSpace(contentRoot))
            {
                throw new Exception("FileService expects a contentRoot when initialized.");
            }

            if (!Directory.Exists(contentRoot))
            {
                throw new Exception($"Path for contentRoot does not exist: {contentRoot}");
            }

            this.contentRoot = contentRoot;
        }

        public IEnumerable<string> GetAllFiles()
        {
            var di = new DirectoryInfo(this.contentRoot);
            var files = di.EnumerateFiles("*.*", SearchOption.AllDirectories);



            return files.Select(x => x.FullName);
        }

        public string GetPhysicalPath(string path)
        {
            var partlyLocalPath = path?.Replace("/", Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)) ?? string.Empty;
            var physicalPath = Path.Combine(this.contentRoot, partlyLocalPath);
            return physicalPath;
        }
    }
}
