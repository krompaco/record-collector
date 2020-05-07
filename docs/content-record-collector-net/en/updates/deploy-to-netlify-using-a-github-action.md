---
title: "Deploy to Netflify using a GitHub Action"
date: 2020-05-06
description: "How to set up deployment to Netlify using a GitHub Action."
authorname: "Johan Kronberg"
authorimage: "/files/jk.jpg"
---
This definition will deploy to the production URL on **push to master** and deploy a draft that will get a preview URL on push to any other branch.
<!--more-->
You first need to add secrets to you GitHub repo for `NETLIFY_AUTH_TOKEN` and `NETLIFY_SITE_ID`, these are easily created and copied from your Netlify site settings.

## YAML file to use in your action

Then add an action that has the following definition. It sets the environment name to `Action` so look in that appsettings file on what gets pulled.

```yml
name: .NET Core
on: [push]
env:
  ASPNETCORE_ENVIRONMENT: 'Action'
jobs:
  deployCommitDraft:
    name: Deploy draft to Netlify
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref != 'refs/heads/master'
    steps:
      - name: Checkout repository
        uses: actions/checkout@v2

      - name: .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 3.1.101
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
    if: github.event_name == 'push' && github.ref == 'refs/heads/master'
    steps:
      - name: Checkout repository
        uses: actions/checkout@v2

      - name: .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 3.1.101
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
