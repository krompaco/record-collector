using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Krompaco.RecordCollector.Content.IO;
using Krompaco.RecordCollector.Content.Languages;
using Krompaco.RecordCollector.Content.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Sdk;

namespace Krompaco.RecordCollector.Generator
{
    public class Workers : IClassFixture<WebApplicationFactory<Web.Startup>>
    {
        private readonly HttpClient client;

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
            ClearDirectory(contentProperties);

            var fileService = new FileService(
                contentProperties.ContentRootPath,
                contentProperties.SectionsToExcludeFromLists,
                new ContentCultureService(),
                NullLogger.Instance);

            var outputPath = contentProperties.StaticSiteRootPath;
            Directory.CreateDirectory(outputPath);

            var allFileModels = fileService.GetAllFileModels();
            var allRequestTasks = new List<Task<HttpResponseMessage>>();

            // Start sending requests
            foreach (var file in allFileModels.Where(x => x is SinglePage))
            {
                var task = this.client.GetAsync(file.RelativeUrl);
                allRequestTasks.Add(task);
            }

            var f = 0;

            // Copy web project wwwroot static files (CSS etc)
            foreach (var dirPath in Directory.EnumerateDirectories(contentProperties.EnvironmentProjectWebRootPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(
                    dirPath.Replace(
                        contentProperties.EnvironmentProjectWebRootPath,
                        contentProperties.StaticSiteRootPath,
                        StringComparison.OrdinalIgnoreCase));
            }

            foreach (var newPath in Directory.EnumerateFiles(contentProperties.EnvironmentProjectWebRootPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(
                    newPath,
                    newPath.Replace(
                        contentProperties.EnvironmentProjectWebRootPath,
                        contentProperties.StaticSiteRootPath,
                        StringComparison.OrdinalIgnoreCase), true);
                f++;
            }

            // Copy images etc from content root
            foreach (var file in allFileModels.Where(x => x is FileResource))
            {
                f++;
                CopyFileResource(file, contentProperties);
            }

            Console.WriteLine($"Files copied on disk: {f}");

            // Write the requested URL responses to disk
            var responses = await Task.WhenAll(allRequestTasks).ConfigureAwait(true);
            var i = 0;

            foreach (var response in responses)
            {
                i++;
                response.EnsureSuccessStatusCode();

                var file = allFileModels.Single(x => x.RelativeUrl.ToString() == response.RequestMessage.RequestUri.AbsolutePath);
                Console.WriteLine($"Found {file.RelativeUrl}");

#pragma warning disable CA2000 // Dispose objects before losing scope
                await using var output = File.Create(CreateDirectoryAndGetFilePath(file, contentProperties));
#pragma warning restore CA2000 // Dispose objects before losing scope
                await using var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(true);
                await input.CopyToAsync(output).ConfigureAwait(true);
                await input.DisposeAsync().ConfigureAwait(true);
                await output.DisposeAsync().ConfigureAwait(true);
            }

            Console.WriteLine($"HTTP requested files written: {i}");
        }

        private static string CreateDirectoryAndGetFilePath(IRecordCollectorFile page, ContentProperties contentProperties)
        {
            var relativePath = page.RelativeUrl.ToString()
                .Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture),
                    StringComparison.Ordinal);

            if (relativePath.EndsWith(Path.DirectorySeparatorChar))
            {
                relativePath += "index.html";
            }

            relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar);

            var destinationFile = Path.Combine(contentProperties.StaticSiteRootPath, relativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);
            Directory.CreateDirectory(destinationDirectory);
            return destinationFile;
        }

        private static void ClearDirectory(ContentProperties contentProperties)
        {
            var staticSiteRootDirectoryInfo = new DirectoryInfo(contentProperties.StaticSiteRootPath);

            foreach (var file in staticSiteRootDirectoryInfo.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (var dir in staticSiteRootDirectoryInfo.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

        private static void CopyFileResource(IRecordCollectorFile file, ContentProperties contentProperties)
        {
            var fileInfo = new FileInfo(file.FullName);

            if (fileInfo.DirectoryName == null)
            {
                return;
            }

            var directoryFromRoot = fileInfo.DirectoryName
                .Replace(
                    contentProperties.ContentRootPath,
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase);

            var fileFromRoot = file.FullName
                .Replace(
                    contentProperties.ContentRootPath,
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase);

            var destinationDirectory = Path.Combine(contentProperties.StaticSiteRootPath, directoryFromRoot);
            var destinationFile = Path.Combine(contentProperties.StaticSiteRootPath, fileFromRoot);

            Directory.CreateDirectory(destinationDirectory);
            File.Copy(file.FullName, destinationFile, true);
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
