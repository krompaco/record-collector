+++
linkTitle = "Getting started"
title = "Getting started"
description = "To learn how Record Collector works it's easiest to grab a copy of the repository run it locally. Here are the steps."
weight = 10
+++
To learn how Record Collector works it's easiest to grab a copy of the repository run it locally.

## Grab the repository

Go to the [Github repo](https://github.com/krompaco/record-collector) and choose if you want to clone, _fork and then clone_ or just download as a ZIP-file.

## Add a Development appsettings file to Web project

Pick one of the example `appsettings.*.json` files or the base file and "save as" `appsettings.Development.json` in `src\Krompaco.RecordCollector.Web`. On Windows my file looks like this:

```js
{
	"AppSettings": {
		"SiteUrl": "http://localhost:5000/",
		"ContentRootPath": "C:\\github\\record-collector\\docs\\content-record-collector-net\\",
		"StaticSiteRootPath": "C:\\github\\record-collector\\artifacts\\static-site\\",
		"FrontendSetup": "default",
		"SectionsToExcludeFromLists": [ "pages", "sidor" ],
		"MainNavigationSections": [ "pages", "sidor" ],
		"PaginationPageCount": 2,
		"PaginationPageSize": 2
	}
}
```

Note that `C:\github\record-collector\` is where I put the repository on my machine. Look at the files named Action, Docker or Netlify to see what works on Linux based systems.

### Frontend setup

Two options are in the repository. The `default` setup is based on [Tailwind CSS](https://tailwindcss.com/) and [Hotwire](https://hotwired.dev/) and requires npm steps, you can also use [Simple.css](https://simplecss.org/) by having the line set as `"FrontendSetup": "simplecss",` in your appsettings files.

* [record-collector.netlify.app](https://record-collector.netlify.app/) is published with `default`
* [record-collector-simplecss.netlify.app](https://record-collector-simplecss.netlify.app/) is using `simplecss`

## Run the web app in ASP.NET mode

When running, Kestrel is recommended and the default URL in the ASP.NET mode is `http://localhost:5000/`.

### Alternative A: Run using Visual Studio

Open the SLN file in repository root. Focus the Web project and switch from IIS Express to `Krompaco.RecordCollector.Web` in the Run/Debug menu and then Start Debugging or Start Without Debugging.

### Alternative B: Run using command line

If you have PowerShell you can run `run-web.ps1` in the repository root or paste this in to your terminal from there:

```
dotnet run --project ./src/Krompaco.RecordCollector.Web/Krompaco.RecordCollector.Web.csproj --configuration Release
```

## Generate as a static site

Run `run-static-site-generator.ps1` in the repository root or use command:

```
dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"
```

This will write static HTML pages of your site with the assets you put in Web project's wwwroot folder to the path you specified in the appsettings file as `StaticSiteRootPath`.

### Check static site locally

To verify the static site you can check the file `run-docker-nginx-static-site.ps1` for an example on how to launch the static site in a nginx web server using Docker. But usually it's easier to just setup a real deploy and use for example Netlify's preview function.

## Deploy to Netlify

These two blog posts should help with most deployment scenarios using different Git and build server providers. I think the most elegant way is to just use Netlify for the build which works with GitHub, GitLab, or Bitbucket for your repository copy.

* [Continuous Deployment using just Netlify](/updates/continuous-deployment-using-just-netlify/)
* [Deploy to Netlify using a GitHub Action](/updates/deploy-to-netlify-using-a-github-action/)

## Customize templates using the `ViewPrefix` setting

This feature was removed with the move to .NET 8.0 and Blazor SSR.

Thanks for your interest in Record Collector and reading this far!