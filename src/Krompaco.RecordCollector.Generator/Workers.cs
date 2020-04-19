using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private readonly WebApplicationFactory<Web.Startup> factory;

        private readonly FileService fileService;

        public Workers(WebApplicationFactory<Web.Startup> factory)
        {
            this.factory = factory;
            this.fileService = new FileService("C:\\DevStuff\\github\\docsy-example\\content", new ContentCultureService(), NullLogger.Instance);
        }

        [Fact]
        public async Task GenerateStaticSite()
        {
            var outputPath = $"c:\\DevStuff\\temp\\rc-content\\{DateTime.Now:yyyyMMddHHmmss}";
            Directory.CreateDirectory(outputPath);

            var client = this.factory.CreateClient();
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
    }
}
