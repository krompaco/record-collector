﻿using Krompaco.RecordCollector.Web.Controllers;
using Krompaco.RecordCollector.Web.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace Krompaco.RecordCollector.Web;

public class FileSystemWatcherService : BackgroundService
{
    private readonly IConfiguration config;

    private readonly ILogger<FileSystemWatcherService> logger;

    private readonly IMemoryCache memoryCache;

    private FileSystemWatcher fsw;

#pragma warning disable CS8618
    public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger, IConfiguration config, IMemoryCache memoryCache)
#pragma warning restore CS8618
    {
        this.logger = logger;
        this.config = config;
        this.memoryCache = memoryCache;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var contentRootPath = this.config.GetAppSettingsContentRootPath();

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
