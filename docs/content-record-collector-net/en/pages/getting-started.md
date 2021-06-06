+++
linkTitle = "Getting started"
title = "Getting started"
description = "To learn how Record Collector works it's easiest to grab a copy of the repository run it locally. Here are the steps."
weight = 10
+++
To learn how Record Collector works it's easiest to grab a copy of the repository run it locally.

## Grab the repository

Go to the [Github repo](https://github.com/krompaco/record-collector) and choose if you want to clone, fork and then clone or just download as a ZIP-file.

## Add an `appsettings.Development.json` file to src/Krompaco.RecordCollector.Web

Pick one of the example appsettings.*.json files or the base file and "save as" `appsettings.Development.json` - on Windows my file looks like this:

```js
{
	"Logging": {
		"LogLevel": {
			"Default": "Information",
			"Microsoft": "Warning",
			"Microsoft.Hosting.Lifetime": "Information"
		}
	},
	"AppSettings": {
		"SiteUrl": "http://localhost:5000/",
		"ContentRootPath": "C:\\github\\record-collector\\docs\\content-record-collector-net\\",
		"StaticSiteRootPath": "C:\\github\\record-collector\\artifacts\\static-site\\",
		"ViewPrefix": "",
		"SectionsToExcludeFromLists": [ "pages", "sidor" ],
		"MainNavigationSections": [ "pages", "sidor" ],
		"PaginationPageCount": 2,
		"PaginationPageSize": 2
	}
}
```

Note that `C:\github\\record-collector\` is where I put the repository on my machine. Look at the files named Action, Docker or Netlify to see what works on Linux based systems.

## Run the web app in ASP.NET MVC mode

When running, Kestrel should be used and the default URL in the MVC mode should be `http://localhost:5000/`.

### Alternative A: Run using Visual Studio

Open the SLN file in repository root. Focus the Web project and switch from IIS Express to `Krompaco.RecordCollector.Web` in the Run/Debug menu and then Start Debugging or Start Without Debugging.

### Alternative B: Run using command line

If you have PowerShell you can run `run-mvc-web.ps1` in the repository root or paste this in to your terminal from there:

```
dotnet run --project ./src/Krompaco.RecordCollector.Web/Krompaco.RecordCollector.Web.csproj --configuration Release
```

## Generate as a static site

Run `run-static-site-generator.ps1` in the repository root or use command:

```
dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"
```

This will write static HTML pages of your site as well asd assets you put in Web project's wwwroot folder to the path you specified in the appsettings file as `StaticSiteRootPath`.

### Check static site locally

To verify the static site you can check the file `run-docker-nginx-static-site.ps1` for an example on how to launch the static site in a nginx web server using Docker. But usually it's easier to just setup a real deploy and use for example Netlify's preview function.

## Deploy to Netlify

These two blog posts should help with most deployment scenarios using different Git and build server providers. I think the most elegant way is to just use Netlify for the build which works great with GitHub, GitLab, or Bitbucket for your repository copy.

* [Continuous Deployment using just Netlify](/en/updates/continuous-deployment-using-just-netlify/)
* [Deploy to Netlify using a GitHub Action](/en/updates/deploy-to-netlify-using-a-github-action/)

## Customize templates using the `ViewPrefix` setting

You can of course modify anything existing but there is a also a simple built-in feature to be able to add your own templates with own file names so that pulling from the original repository won't overwrite your templates.

This works so that if you change the setting to something like `"ViewPrefix": "MiasTheme"` the default MVC controller will use that setting value in this way:

```
return this.View(viewPrefix + "List", viewModel);
```

As you already have figured out you can now put files named `MiasThemeList.cshtml` and `MiasThemeSingle.cshtml` in `Views/Content` and then from those files refer to any layout file you have or custom partial files. Example start of `MiasThemeList.cshtml` file:

```
@using Krompaco.RecordCollector.Content.Models
@using Markdig

@model ListPageViewModel

@{
	Layout = "MiasThemeExtraWideLayout";
}
..
```

Thanks for your interest in Record Collector and reading this far!