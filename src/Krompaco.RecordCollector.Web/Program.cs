using System.Globalization;
using HtmlAgilityPack;
using Krompaco.RecordCollector.Web;
using Krompaco.RecordCollector.Web.Extensions;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container
builder.Services.AddHostedService<FileSystemWatcherService>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

    // TODO: This is the place to set the default culture
    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new SiteRequestCultureProvider(builder.Configuration));
});

builder.Services.AddLocalization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
app.Logger.LogInformation("\r\nRecord Collector Version 2.0\r\n");

app.UseRequestLocalization();

app.UseDeveloperExceptionPage();

app.UseStaticFiles();

app.UseRouting();

var frontendSetup = app.Configuration.GetAppSettingsFrontendSetup();

if (frontendSetup == "simplecss")
{
    // Trying out a way to strip class attributes from HTML if Simple.css is used
    app.Use(async (context, next) =>
    {
        // Way that should work to only process HTML output
        if (context.Request.Path.HasValue && context.Request.Path.Value.EndsWith("/"))
        {
            var responseBody = context.Response.Body;

            await using var newResponseBody = new MemoryStream();
            context.Response.Body = newResponseBody;
            await next();

            context.Response.Body = new MemoryStream();

            newResponseBody.Seek(0, SeekOrigin.Begin);
            context.Response.Body = responseBody;

            var html = new StreamReader(newResponseBody).ReadToEnd();

            // Using HtmlAgilityPack to modify whole document
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.Descendants())
            {
                if (node.NodeType != HtmlNodeType.Element)
                {
                    continue;
                }

                if (node.Attributes.Contains("class"))
                {
                    node.Attributes["class"].Remove();
                }
            }

            await context.Response.WriteAsync(doc.DocumentNode.OuterHtml);
        }
        else
        {
            await next();
        }
    });
}

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "rc-content-report",
        pattern: "rc-content-report",
        defaults: new { controller = "Content", action = "Report" });

    endpoints.MapControllerRoute(
        name: "rc-content-properties",
        pattern: "rc-content-properties",
        defaults: new { controller = "Content", action = "Properties" });

    endpoints.MapControllerRoute(
        name: "files",
        pattern: "{**path}",
        defaults: new { controller = "Content", action = "Files" });
});

app.Logger.LogInformation($"In {app.Environment.EnvironmentName} using {builder.Configuration.GetAppSettingsContentRootPath()}");

app.Run();

public partial class Program
{
    public const string ToBeVisibleInTestProjects = "To be visible in test projects.";
}
