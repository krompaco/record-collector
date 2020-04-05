using System;
using System.IO;
using Krompaco.RecordCollector.Content.FrontMatterParsers;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Content.Tests.Converters;
using Xunit;

namespace Krompaco.RecordCollector.Content.Tests
{
    public class SummaryExtractorTests
    {
        [Fact]
        public void SummaryFromFileWithYamlFrontMatterTest()
        {
            string input = @"
---
title: About
categories:
  - Development
  - VIM
date: '2012-04-06'
description: spf13-vim is a cross platform distribution of vim plugins and resources for Vim.
slug: spf13-vim-3-0-release-and-new-website
testint: 1
images:
  - site-feature-image.jpg
---
Summary.

That can be on another line.
<!--more-->
Lorem ipsum";

            using TextReader sr = new StringReader(input);
            var parser = new YamlParser<SinglePage>(sr, string.Empty);
            var single = parser.GetAsSinglePage();

            var converter = new StringToStreamConverter(input);
            var extractor = new SummaryExtractor(new StreamReader(converter.GetStreamFromString()));

            var summary = extractor.GetSummaryFromContent();

            Assert.Equal(@"Summary.

That can be on another line.
", summary);

            Assert.Equal(@"Summary.

That can be on another line.
<!--more-->
Lorem ipsum", single.Content);
        }

        [Fact]
        public void SummaryFromFileWithTomlFrontMatterTest()
        {
            string input = @"
 +++
title =  ""About"" 
 categories = [""Development"", ""VIM""]
date = ""2012-04-06""
description = ""spf13-vim is a cross platform distribution of vim plugins and resources for Vim.""
slug = ""spf13-vim-3-0-release-and-new-website""
testint = 1
+++
Summary.

That can be on another line.
<!--more-->
Lorem ipsum";

            using TextReader sr = new StringReader(input);
            var parser = new TomlParser<SinglePage>(sr, string.Empty);
            var single = parser.GetAsSinglePage();

            var converter = new StringToStreamConverter(input);
            var extractor = new SummaryExtractor(new StreamReader(converter.GetStreamFromString()));

            var summary = extractor.GetSummaryFromContent();

            Assert.Equal(@"Summary.

That can be on another line.
", summary);

            Assert.Equal(@"Summary.

That can be on another line.
<!--more-->
Lorem ipsum", single.Content);
        }

        [Fact]
        public void SummaryFromFileWithJsonFrontMatterTest()
        {
            string input = @"{
       ""categories"": [
      ""Development"",
      ""VIM""
   ],
   ""date"": ""2012-04-06"",
""images"": [""site-feature-image.jpg""],
""testint"": 1,
""testbool"": false,
""teststring"": ""What?""
}
Summary.

That can be on another line.
<!--more-->
Lorem ipsum";

            using TextReader sr = new StringReader(input);
            var parser = new JsonParser<SinglePage>(sr, string.Empty);
            var single = parser.GetAsSinglePage();

            var converter = new StringToStreamConverter(input);
            var extractor = new SummaryExtractor(new StreamReader(converter.GetStreamFromString()));

            var summary = extractor.GetSummaryFromContent();

            Assert.Equal(@"Summary.

That can be on another line.
", summary);

            Assert.Equal(@"Summary.

That can be on another line.
<!--more-->
Lorem ipsum", single.Content);
        }
    }
}
