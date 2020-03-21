using System;

namespace Krompaco.RecordCollector.Content.Models
{
    public interface IFile
    {
        string FullName { get; set; }

        Uri RelativeUrl { get; set; }
    }
}
