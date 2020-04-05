namespace Krompaco.RecordCollector.Content.Models
{
    public enum FrontMatterType
    {
        /// <summary>
        /// JSON.
        /// </summary>
        Json,

        /// <summary>
        /// TOML.
        /// </summary>
        Toml,

        /// <summary>
        /// YAML.
        /// </summary>
        Yaml,

        /// <summary>
        /// No front matter found, complete HTML document.
        /// </summary>
        HtmlDocument,

        /// <summary>
        /// No front matter found, complete Markdown document.
        /// </summary>
        MarkdownDocument,
    }
}
