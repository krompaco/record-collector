---
title: "Continuous Deployment using just Netlify"
date: 2021-03-09
description: "How to set up deployment of a Record Collector site using only Netlify."
categories:
  - Deployment
  - Automation
images:
  - /files/photo-8.jpg
authorname: "Johan Kronberg"
authorimage: "/files/jk.jpg"
---
How to set up deployment of a Record Collector site using only Netlify.
<!--more-->
I was very happy to notice that the [Netlify build-image](https://github.com/netlify/build-image) now has the .NET 5.0.200 SDK installed.

For Record Collector this means that you can get deploys done where Netlify pulls from a Git repository and does the build.

So no need to handle API keys and an external build agent any more.

You just use the built-in Netlify features to give access to your Git repository when adding your site.

## Paths to use in configuration

First go to Build & deploy and add an Environment variable. Set the key `ASPNETCORE_ENVIRONMENT` and value to `Netlify`.

This means the config used will come from **appsettings.Netlify.json** and this is how I've configured the sample site in that file.

```js
{
  "AppSettings": {
    "SiteUrl": "https://record-collector.netlify.app/",
    "ContentRootPath": "/opt/build/repo/docs/content-record-collector-net/",
    "StaticSiteRootPath": "/opt/build/repo/artifacts/static-site/",
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
| Build command     | **dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"** |
| Publish directory | **artifacts/static-site/** |
| Builds            | *Activate builds*          |

 You should also be able to push a Shell script and have more work done, for example building front-end stuff before generating the static site using `npm run` or similar. The build-image has support for a wide variety of languages and runtimes.

[![Netlify Status](https://api.netlify.com/api/v1/badges/97fc0268-36e9-408f-995c-13ed2605a11e/deploy-status)](https://record-collector.netlify.app/)

This is such a great feature and really excited that it works this well with Record Collector! Thank you Netlify!
