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
The [Netlify build-image](https://github.com/netlify/build-image) is easy to work with and can run .NET 9.0 if you use this guide.

With this setup, Netlify pulls from a Git repository and does the build.

So no need to handle API keys and an external build agent any more.

You just use the built-in Netlify features to give access to your Git repository when adding your site.

## Paths to use in configuration

First go to [app.netlify.com](https://app.netlify.com/) and add or find your site. Find the section for Build & deploy and add an Environment variable. Set the key `ASPNETCORE_ENVIRONMENT` and value to `Netlify`.

This means the config used will come from **appsettings.Netlify.json** and this is how I've configured the sample site in that file.

```js
{
  "AppSettings": {
    "SiteUrl": "https://record-collector.netlify.app/",
    "ContentRootPath": "/opt/build/repo/content/demo-site/",
    "StaticSiteRootPath": "/opt/build/repo/artifacts/static-site/",
    "FrontendSetup": "default",
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
| Build command     | **./netlify.build.default.sh** |
| Publish directory | **artifacts/static-site/** |
| Builds            | *Activate builds*          |

Change **default** to **simplecss** if you are using the Simple CSS version. 

The Shell script should have execute rights, if not, be in your local repository and run `git update-index --chmod=+x netlify.build.default.sh` or `git update-index --chmod=+x netlify.build.simplecss.sh` to set it.

[![Netlify Status](https://api.netlify.com/api/v1/badges/d83429cd-4060-466a-8491-1afbb1c97149/deploy-status)](https://record-collector.net/)

This is such a great feature and really excited that it works this well with Record Collector! Thank you Netlify!
