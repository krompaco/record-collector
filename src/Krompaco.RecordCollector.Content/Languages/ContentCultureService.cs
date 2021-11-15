using System.Globalization;

namespace Krompaco.RecordCollector.Content.Languages
{
    public class ContentCultureService
    {
        #pragma warning disable CA1822 // Mark members as static
        public bool DoesCultureExist(string cultureName)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }
        #pragma warning restore CA1822 // Mark members as static
    }
}
