# record-collector.net

The [Record Collector example and documentation web site](https://record-collector.net) is being filled with content and will have all the information on what the project is and does. Easiest starting point is to set up the repository locally and run the web project, see the [getting started](https://record-collector.net/en/pages/getting-started/) page for instructions.

## Inspiration

This project is heavily inspired by [Hugo](https://gohugo.io/) and I try to be somewhat compatible with the same content structure and support parsing of TOML, YAML and JSON front matter.

## Bright ideas

The content file layer is converted to C# lists and objects and then used in a regular ASP.NET Core MVC project which is also where you can work _live_ on both content preview and templating. This means you now can use your existing ASP.NET MVC and C# skills creating static site templates!

In this setup the generation process work using the WebApplicationFactory from the Microsoft.AspNetCore.Mvc.Testing package so I don't have to fall back to a console application or custom web servers for crunching the files and templates which is the norm for static site generation. Feels a lot better to me and while keeping really fast generation times.

## Installed packages

Built using .NET 6.0 (through 3.1 and 5.0) and with these packages and projects. Thank you!

| Project                                                   | License                   |
|-----------------------------------------------------------|---------------------------|
| [YamlDotNet](https://github.com/aaubry/YamlDotNet/wiki)   | MIT                       |
| [Tomlyn](https://github.com/xoofx/Tomlyn)                 | BSD-Clause 2              |
| [Markdig](https://github.com/lunet-io/markdig)            | BSD-Clause 2              |
| [JetBrains Mono font](https://www.jetbrains.com/lp/mono/) | SIL Open Font License 1.1 |
| [Tailwind CSS](https://tailwindcss.com/)                  | MIT                       |
| [Hotwire](https://hotwired.dev/)                          | MIT                       |
| [Simple.css {}](https://simplecss.org/)                   | MIT                       |

Other things such as Webpack are involved too but these are the ones I firstly referenced and then they in turn depend on other packages.

### Quality control tools

* [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
* [Microsoft.CodeAnalysis.NetAnalyzers](https://github.com/dotnet/roslyn-analyzers)

### Demo sites

The [demo web site](https://record-collector.net) is generated and [deployed with Netlify using their build-image](https://github.com/krompaco/record-collector/blob/main/docs/content-record-collector-net/en/updates/continuous-deployment-using-just-netlify.md).

[![Netlify Status](https://api.netlify.com/api/v1/badges/d83429cd-4060-466a-8491-1afbb1c97149/deploy-status)](https://app.netlify.com/sites/record-collector-ui/deploys)

This [other instance of the same web site](https://record-collector.netlify.app) is generated on push and [deployed to Netlify using a GitHub Action](https://github.com/krompaco/record-collector/blob/main/docs/content-record-collector-net/en/updates/deploy-to-netlify-using-a-github-action.md).

[![Netlify Status](https://api.netlify.com/api/v1/badges/97fc0268-36e9-408f-995c-13ed2605a11e/deploy-status)](https://app.netlify.com/sites/record-collector/deploys)

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.

## The name

The name is a tribute to the brightest and most sophisticated humans on the planet; the record collectors.
