using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Krompaco.RecordCollector.Generator
{
    public class Workers : IClassFixture<WebApplicationFactory<Web.Startup>>
    {
        private readonly WebApplicationFactory<Web.Startup> factory;

        public Workers(WebApplicationFactory<Web.Startup> factory)
        {
            this.factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/?whaddup=1")]
        public async Task Request(string url)
        {
            var client = this.factory.CreateClient();

            var response = await client.GetAsync(url).ConfigureAwait(true);

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }
    }
}
