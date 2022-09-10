---
title: "Continuous Deployment using just Netlify"
date: 2021-03-09
description: "How to set up deployment of a Record Collector site using only Netlify."
categories:
  - Deployment
  - Automation
authorname: "Johan Kronberg"
authorimage: "/files/jk.jpg"
---
How to set up deployment of a Record Collector site using only Netlify.
<!--more-->
I was very happy to notice that the [Netlify build-image](https://github.com/netlify/build-image) now has the .NET 6.0.100 SDK installed.

For Record Collector this means that you can get deploys done where Netlify pulls from a Git repository and does the build.

So no need to handle API keys and an external build agent any more.

You just use the built-in Netlify features to give access to your Git repository when adding your site.

## Paths to use in configuration

First go to [app.netlify.com](https://app.netlify.com/) and add or find your site. Find the section for Build & deploy and add an Environment variable. Set the key `ASPNETCORE_ENVIRONMENT` and value to `Netlify`.

This means the config used will come from **appsettings.Netlify.json** and this is how I've configured the sample site in that file.

```js
{
  "AppSettings": {
    "SiteUrl": "https://record-collector.netlify.app/",
    "ContentRootPath": "/opt/build/repo/docs/content-record-collector-net/",
    "StaticSiteRootPath": "/opt/build/repo/artifacts/static-site/",
    "FrontendSetup": "default",
    "ViewPrefix": "",
    "SectionsToExcludeFromLists": [ "pages", "sidor" ],
    "MainNavigationSections": [ "pages", "sidor" ],
    "PaginationPageCount": 2,
    "PaginationPageSize": 3
  }
}
```

## Build settings in Netlify

These are settings that work well with the sample site.

| Setting           | Value                      |
|-------------------|----------------------------|
| Base directory    | *Leave empty*              |
| Build command     | **npm ci && npm run prodbuild && dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"** |
| Publish directory | **artifacts/static-site/** |
| Builds            | *Activate builds*          |

You should also be able to push a Shell script and have more work done, for example setting up a specific dotnet SDK version or do more frontend related things before generating the static site. The build-image has support for a wide variety of languages and runtimes.

If you use `"FrontendSetup": "simplecss",` you can remove the the two `npm` commands and just have `dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"` as Build command.

[![Netlify Status](https://api.netlify.com/api/v1/badges/d83429cd-4060-466a-8491-1afbb1c97149/deploy-status)](https://record-collector.net/)

This is such a great feature and really excited that it works this well with Record Collector! Thank you Netlify!
