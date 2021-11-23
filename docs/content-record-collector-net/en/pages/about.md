+++
linkTitle = "About Record Collector"
title = "What does Record Collector do?"
description = "The main features and advantages when using Record Collector."
weight = 15
+++
Record Collector reads a typical static site content folder into C# lists and objects and then uses them in a regular ASP.NET Core MVC project.

![Sketch of Record Collector flow](https://record-collector.net/files/flow.png)

This means when you are working with customization of a static site you are in a regular and very familiar ASP.NET MVC application.

You can also just use the file reading layer and integrate in some other .NET initiative.

The typical usage is to get help generating a static site that you host on for example Netlify.

A blog like site such as this one or some type of documentation site are good candidates for Record Collector.

You could also find a great fit if your site is just a few pages but you still want help with a little "front matter" and get a shared place for head, header and footer. In these scenarios you will probably also find it useful that Record Collector supports HTML content with "front matter" and makes it easy to for example have a Webpack build as part of the Netlify deploy setup.

Thanks for your interest in Record Collector and reading this far!

See the getting started page for [instructions on how to get going](/en/pages/getting-started/) with Record Collector.