using System.ComponentModel.DataAnnotations;

namespace Krompaco.RecordCollector.Web.Models
{
    public class AppSettings : IValidatableObject
    {
        [Required]
        [Url]
        public string SiteUrl { get; set; } = string.Empty;

        [Required]
        public string ContentRootPath { get; set; } = string.Empty;

        [Required]
        public string StaticSiteRootPath { get; set; } = string.Empty;

        [Required]
        public string FrontendSetup { get; set; } = string.Empty;

        [Required]
        public string[] SectionsToExcludeFromLists { get; set; } = [];

        [Required]
        public string[] MainNavigationSections { get; set; } = [];

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PaginationPageCount should be larger than 0.")]
        public int PaginationPageCount { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PaginationPageSize should be larger than 0.")]
        public int PaginationPageSize { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (!Directory.Exists(this.ContentRootPath))
            {
                results.Add(new ValidationResult($"ContentRootPath '{this.ContentRootPath}' does not exist."));
            }
            else if (new DirectoryInfo(this.ContentRootPath).GetFiles().Length == 0)
            {
                results.Add(new ValidationResult($"ContentRootPath '{this.ContentRootPath}' has no files."));
            }

            return results;
        }
    }
}
