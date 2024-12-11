# Record Collector

Record Collector's offical web site is being filled with content and will have all the information on what the project is and does. Easiest starting point is to set up the repository locally and run the web project, see the [getting started](https://record-collector.net/pages/getting-started/) page for instructions.

## Inspiration

This project is inspired by [Hugo](https://gohugo.io/) and I try to be somewhat compatible with the same content structure and support parsing of TOML, YAML and JSON front matter.

## Bright ideas

The content file layer is converted to C# lists and objects and then used in a regular ASP.NET project which is also where you can work _live_ on both content preview and templating. This means you now can use your existing ASP.NET Blazor SSR and C# skills creating static site templates!

In this setup the generation process work using the WebApplicationFactory from the Microsoft.AspNetCore.Mvc.Testing package so I don't have to fall back to a console application or custom web servers for crunching the files and templates which seems to be the norm for static site generation. Feels good to me and generation times are still fast.

## Open source references

Built using .NET 9.0 (through 3.1, 5.0, 6.0 and 8.0) and with these packages and projects. Thank you!

| Project                                                   | License                   |
|-----------------------------------------------------------|---------------------------|
| [YamlDotNet](https://github.com/aaubry/YamlDotNet/wiki)   | MIT                       |
| [Tomlyn](https://github.com/xoofx/Tomlyn)                 | BSD-Clause 2              |
| [Markdig](https://github.com/lunet-io/markdig)            | BSD-Clause 2              |
| [Tailwind CSS](https://tailwindcss.com/)                  | MIT                       |
| [htmx.org](https://htmx.org/)                             | Zero-Clause BSD           |
| [hyperscript.org](https://hyperscript.org/)               | BSD 2-Clause              |
| [Simple.css](https://simplecss.org/)                      | MIT                       |
| [Html Agility Pack](https://html-agility-pack.net/)       | MIT                       |
| [Webpack](https://webpack.js.org/)                        | MIT                       |
| [cross-env](https://github.com/kentcdodds/cross-env)      | MIT                       |
| [npm-run-all](https://github.com/mysticatea/npm-run-all)  | MIT                       |

Other things can be involved too but these are the ones I take a driect dependency on.

### Quality control tools

* [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
* [Microsoft.CodeAnalysis.NetAnalyzers](https://github.com/dotnet/roslyn-analyzers)

## [Official web site  ↗](https://record-collector.net)

[![Netlify Status](https://api.netlify.com/api/v1/badges/d83429cd-4060-466a-8491-1afbb1c97149/deploy-status)](https://app.netlify.com/sites/record-collector-ui/deploys)

The official web site is generated and [deployed with Netlify using their build-image](https://github.com/krompaco/record-collector/blob/main/content/record-collector-dotnet/updates/continuous-deployment-using-just-netlify.md).

It has the `default` frontend.

## [Demo web site with Simple.css frontend  ↗](https://record-collector-simplecss.netlify.app)

[![Netlify Status](https://api.netlify.com/api/v1/badges/471c792b-b666-4bb4-aa60-270f22c98180/deploy-status)](https://app.netlify.com/sites/record-collector-simplecss/deploys)

The Simple.css demo web site is generated and deployed with Netlify using their build-image.

It has the `simplecss` frontend configuration.

## [Demo web site with default frontend  ↗](https://record-collector.netlify.app)

[![Build and deploy to Netlify](https://github.com/krompaco/record-collector/actions/workflows/build-and-deploy-to-netlify.yml/badge.svg)](https://github.com/krompaco/record-collector/actions/workflows/build-and-deploy-to-netlify.yml)

This site is generated on push and [deployed to Netlify using a GitHub Action](https://github.com/krompaco/record-collector/blob/main/content/record-collector-dotnet/updates/deploy-to-netlify-using-a-github-action.md).

It has the `default` frontend.

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.

## The name

The name is a tribute to the brightest and most sophisticated humans on the planet; the record collectors.
