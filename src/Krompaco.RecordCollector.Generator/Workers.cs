using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Content.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Krompaco.RecordCollector.Generator
{
    public class Workers : IClassFixture<WebApplicationFactory<Web.Startup>>
    {
        private readonly HttpClient client;

        private FileService fileService;

        public Workers(WebApplicationFactory<Web.Startup> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            this.client = factory.CreateClient();
        }

        [Fact]
        public async Task GenerateStaticSite()
        {
            var contentProperties = await this.GetContentProperties().ConfigureAwait(true);

            this.fileService = new FileService(
                contentProperties.ContentRootPath,
                new ContentCultureService(),
                NullLogger.Instance);

            var outputPath = contentProperties.StaticSiteRootPath;
            Directory.CreateDirectory(outputPath);

            var allFileModels = this.fileService.GetAllFileModels();
            var allRequestTasks = new List<Task<HttpResponseMessage>>();

            foreach (var file in allFileModels.Where(x => x is SinglePage))
            {
                var task = client.GetAsync(file.RelativeUrl);
                allRequestTasks.Add(task);
            }

            var responses = await Task.WhenAll(allRequestTasks).ConfigureAwait(true);
            var i = 0;

            foreach (var response in responses)
            {
                i++;
                response.EnsureSuccessStatusCode();

                var file = allFileModels.Single(x => x.RelativeUrl.ToString() == response.RequestMessage.RequestUri.AbsolutePath);
                Console.WriteLine($"Found {file.RelativeUrl}");

                await using var output = File.Create(Path.Combine(outputPath, i + ".html"));
                await using var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(true);
                input.CopyTo(output);
                input.Dispose();
                output.Dispose();
            }
        }

        private async Task<ContentProperties> GetContentProperties()
        {
            var contentPropertiesResponse = await this.client
                .GetAsync("/rc-content-properties/")
                .ConfigureAwait(true);

            var contentPropertiesJson = await contentPropertiesResponse
                .Content
                .ReadAsStringAsync()
                .ConfigureAwait(true);

            var contentProperties = JsonSerializer.Deserialize<ContentProperties>(
                contentPropertiesJson,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

            return contentProperties;
        }
    }
}
