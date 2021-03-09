---
title: "Deploy to your Netlify site from GitHub using a GitHub Action"
date: 2020-05-06
description: "How to set up deployment of a Record Collector site to Netlify using a GitHub Action."
categories:
  - Deployment
  - Automation
images:
  - /files/photo-1.jpg
authorname: "Johan Kronberg"
authorimage: "/files/jk.jpg"
---
This definition will deploy your site to your Netlify site's production URL on **push to main** and deploy a draft that will get a preview URL on push to any other branch.
<!--more-->
[![Netlify Status](https://api.netlify.com/api/v1/badges/97fc0268-36e9-408f-995c-13ed2605a11e/deploy-status)](https://record-collector.netlify.app/)

You first need to add secrets to you GitHub repo for `NETLIFY_AUTH_TOKEN` and `NETLIFY_SITE_ID`, these are easily created/found in and copied from your Netlify site settings.

## YAML file to use in your action

Then add an action that has the following definition. I'm not sure if it's necessary but I started from the default .NET Core Action and kept that name and some other things.

I thought this [marketplace GitHub Action for deploying to Netlify](https://github.com/marketplace/actions/netlify-actions) looked the best. It's in the YAML below and doesn't need anything else to work.

```yml
name: .NET Core
on: [push]
env:
  ASPNETCORE_ENVIRONMENT: 'Action'
jobs:
  deployCommitDraft:
    name: Deploy draft to Netlify
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref != 'refs/heads/main'
    steps:
      - name: Checkout repository
        uses: actions/checkout@v2

      - name: Setup .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 5.0.102

      - name: Generate static site
        run: dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"

      - name: Deploy draft to Netlify
        uses: South-Paw/action-netlify-deploy@v1.0.3
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
          netlify-auth-token: ${{ secrets.NETLIFY_AUTH_TOKEN }}
          netlify-site-id: ${{ secrets.NETLIFY_SITE_ID }}
          build-dir: './artifacts/static-site'
          draft: true
          comment-on-commit: true
  publishMasterCommit:
    name: Publish to Netlify
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    steps:
      - name: Checkout repository
        uses: actions/checkout@v2

      - name: Setup .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 5.0.102

      - name: Generate static site
        run: dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"

      - name: Deploy production to Netlify
        uses: South-Paw/action-netlify-deploy@v1.0.3
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
          netlify-auth-token: ${{ secrets.NETLIFY_AUTH_TOKEN }}
          netlify-site-id: ${{ secrets.NETLIFY_SITE_ID }}
          build-dir: './artifacts/static-site'
          comment-on-commit: true
```

## Paths to use in configuration

As you can see the environment name gets set to `Action` so in **appsettings.Action.json** this is how I've configured the sample site.

```js
{
  "AppSettings": {
    "SiteUrl": "https://record-collector.netlify.app/",
    "ContentRootPath": "/home/runner/work/record-collector/record-collector/docs/content-record-collector-net/",
    "StaticSiteRootPath": "/home/runner/work/record-collector/record-collector/artifacts/static-site/",
    "SectionsToExcludeFromLists": [ "pages", "sidor" ],
    "MainNavigationSections": [ "pages", "sidor" ],
    "PaginationPageCount": 2,
    "PaginationPageSize": 3
  }
}
```

Notice the full paths to use on the GitHub Action work runner. I have no idea why the `record-collector` GitHub repository name needs to be used twice in the structure but that is the case.
