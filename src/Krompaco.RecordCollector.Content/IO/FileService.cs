using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Krompaco.RecordCollector.Content.Models;

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

        public string[] GetAllFileFullNames()
        {
            var di = new DirectoryInfo(this.contentRoot);
            var files = di.EnumerateFiles("*.*", SearchOption.AllDirectories);
            return files.Select(x => x.FullName).ToArray();
        }

        public IFile GetFile(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new NullReferenceException("Don't send null or empty fullName");
            }

            if (fullName.EndsWith("_index.md", StringComparison.OrdinalIgnoreCase)
                || fullName.EndsWith("_index.html", StringComparison.OrdinalIgnoreCase))
            {
                return new ListPage();
            }

            if (fullName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                || fullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                return new SinglePage();
            }

            return null;
        }
    }
}
