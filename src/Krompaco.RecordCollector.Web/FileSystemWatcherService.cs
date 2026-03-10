using Krompaco.RecordCollector.Web.Controllers;
using Krompaco.RecordCollector.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Krompaco.RecordCollector.Web;

public class FileSystemWatcherService : BackgroundService
{
    private readonly IOptionsMonitor<AppSettings> monitor;

    private readonly ILogger<FileSystemWatcherService> logger;

    private readonly IMemoryCache memoryCache;

    private FileSystemWatcher fsw;

#pragma warning disable CS8618
    public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger, IOptionsMonitor<AppSettings> monitor, IMemoryCache memoryCache)
#pragma warning restore CS8618
    {
        this.logger = logger;
        this.monitor = monitor;
        this.memoryCache = memoryCache;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var contentRootPath = this.monitor.CurrentValue.ContentRootPath;

        this.fsw = new FileSystemWatcher(contentRootPath, "*.*")
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };

        this.fsw.Created += this.ClearCache;
        this.fsw.Changed += this.ClearCache;
        this.fsw.Deleted += this.ClearCache;
        this.fsw.Renamed += this.ClearCache;

        return Task.CompletedTask;
    }

    private void ClearCache(object sender, FileSystemEventArgs e)
    {
        // Breaking a lot of principles here
        lock (ContentController.AllFilesLock)
        {
            this.memoryCache.Remove(ContentController.AllFilesCacheKey);
        }

        this.logger.LogInformation($"Cleared cache, triggered by: {e.FullPath}");
    }
}
