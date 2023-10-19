---
title: "Deploy to your Netlify site from GitHub using a GitHub Action"
date: 2020-05-06
description: "How to set up deployment of a Record Collector site to Netlify using a GitHub Action."
categories:
  - Deployment
  - Automation
authorname: "Johan Kronberg"
authorimage: "/files/jk.jpg"
---
This definition will deploy your site to your Netlify by building on GitHub.
<!--more-->
It will deploy to production (publish) on **push to main** and deploy a draft that will get a preview URL on push to any other branch.

## Secret variables

You first need to add secrets to you GitHub repo for `NETLIFY_AUTH_TOKEN` and `NETLIFY_SITE_ID`, these are easily created/found in and copied from your Netlify site settings.

## YAML file to use in your action

Then add an action that has the following definition. I'm not sure if it's necessary but I started from the default .NET Core Action and kept that name and some other things.

I thought this [marketplace GitHub Action for deploying to Netlify](https://github.com/marketplace/actions/netlify-actions) looked the best. It's in the YAML below and doesn't need anything else to work.

```yml
name: Build and deploy to Netlify
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

      - name: Setup npm
        uses: actions/setup-node@v2
        with:
          node-version: 18.18.2

      - run: npm ci
      - run: npm run prodbuild

      - name: Setup .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.100-rc.2.23502.2'

      - name: Add robots.txt disallow
        shell: pwsh
        run: |
          Set-Content "./src/Krompaco.RecordCollector.Web/wwwroot/robots.txt" "User-agent: *`r`nDisallow: /"

      - name: Generate static site
        run: dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"

      - name: Publish draft to Netlify
        uses: nwtgck/actions-netlify@v1.2
        with:
          publish-dir: './artifacts/static-site'
          enable-commit-comment: true
          production-deploy: false
          github-token: ${{ secrets.GITHUB_TOKEN }}
        env:
          NETLIFY_AUTH_TOKEN: ${{ secrets.NETLIFY_AUTH_TOKEN }}
          NETLIFY_SITE_ID: ${{ secrets.NETLIFY_SITE_ID }}
  publishMasterCommit:
    name: Publish to Netlify
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    steps:
      - name: Checkout repository
        uses: actions/checkout@v2

      - name: Setup npm
        uses: actions/setup-node@v2
        with:
          node-version: 18.18.2

      - run: npm ci
      - run: npm run prodbuild

      - name: Setup .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.100-rc.2.23502.2'

      - name: Generate static site
        run: dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"

      - name: Publish to Netlify production
        uses: nwtgck/actions-netlify@v1.2
        with:
          publish-dir: './artifacts/static-site'
          enable-commit-comment: true
          production-deploy: true
          github-token: ${{ secrets.GITHUB_TOKEN }}
        env:
          NETLIFY_AUTH_TOKEN: ${{ secrets.NETLIFY_AUTH_TOKEN }}
          NETLIFY_SITE_ID: ${{ secrets.NETLIFY_SITE_ID }}
```

## Paths to use in configuration

As you can see the environment name gets set to `Action` so in **appsettings.Action.json** this is how I've configured the sample site.

```js
{
  "AppSettings": {
    "SiteUrl": "https://record-collector.netlify.app/",
    "ContentRootPath": "/home/runner/work/record-collector/record-collector/content/demo-site/",
    "StaticSiteRootPath": "/home/runner/work/record-collector/record-collector/artifacts/static-site/",
    "FrontendSetup": "default",
    "SectionsToExcludeFromLists": [ "pages", "sidor" ],
    "MainNavigationSections": [ "pages", "sidor" ],
    "PaginationPageCount": 2,
    "PaginationPageSize": 3
  }
}
```

Notice the full paths to use on the GitHub Action work runner. I have no idea why the GitHub repository name`record-collector` needs to be used twice in the structure but that is the case.
