using System;
using System.IO;
using Krompaco.RecordCollector.Content.FrontMatterParsers;
using Krompaco.RecordCollector.Content.Models;
using Krompaco.RecordCollector.Content.Tests.Converters;
using Xunit;

namespace Krompaco.RecordCollector.Content.Tests
{
    public class TypeDetectorTests
    {
        [Fact]
        public async void DetectYamlTest()
        {
            var input = @"
---
title: About
--- 
Lorem ipsum";

            var converter = new StringToStreamConverter(input);
            var typeDetector = new TypeDetector(new StreamReader(converter.GetStreamFromString()));
            using TextReader sr = new StringReader(input);
            var parser = new YamlParser<SinglePage>(sr, string.Empty);
            var single = parser.GetAsSinglePage();
            var content = await single.ContentTextReader.ReadToEndAsync();

            Assert.Equal(FrontMatterType.Yaml, typeDetector.GetFrontMatterType());
            Assert.Equal("About", single.Title);
            Assert.Equal("Lorem ipsum", content);
        }

        [Fact]
        public async void DetectTomlTest()
        {
            var input = @"
+++
title =  ""About"" 
+++ 
Lorem ipsum";

            var converter = new StringToStreamConverter(input);
            var typeDetector = new TypeDetector(new StreamReader(converter.GetStreamFromString()));
            using TextReader sr = new StringReader(input);
            var parser = new TomlParser<SinglePage>(sr, string.Empty);
            var single = parser.GetAsSinglePage();
            var content = await single.ContentTextReader.ReadToEndAsync();

            Assert.Equal(FrontMatterType.Toml, typeDetector.GetFrontMatterType());
            Assert.Equal("About", single.Title);
            Assert.Equal("Lorem ipsum", content);
        }

        [Fact]
        public async void DetectJsonTest()
        {
            var input = @"{
       ""categories"": [
      ""Development"",
      ""VIM""
   ],
   ""date"": ""2012-04-06"",
""images"": [""site-feature-image.jpg""],
""testint"": 1,
""testbool"": false,
""title"": ""About""
}
Lorem ipsum";

            var converter = new StringToStreamConverter(input);
            var typeDetector = new TypeDetector(new StreamReader(converter.GetStreamFromString()));
            using TextReader sr = new StringReader(input);
            var parser = new JsonParser<SinglePage>(sr, string.Empty);
            var single = parser.GetAsSinglePage();
            var content = await single.ContentTextReader.ReadToEndAsync();

            Assert.Equal(FrontMatterType.Json, typeDetector.GetFrontMatterType());
            Assert.Equal("About", single.Title);
            Assert.Equal("Lorem ipsum", content);
        }

        [Fact]
        public void DetectHtmlDocumentTest()
        {
            var input = @"<!DOCTYPE html>
<html>
<head>
<title>Page Title</title>
</head>
<body>
<h1>My First Heading</h1>
<p>My first paragraph.</p>
</body>
</html>";
            var converter = new StringToStreamConverter(input);

            using var sr = new StreamReader(converter.GetStreamFromString());
            var typeDetector = new TypeDetector(sr);
            Assert.Equal(FrontMatterType.HtmlDocument, typeDetector.GetFrontMatterType());
        }

        [Fact]
        public void DetectMarkdownDocumentTest()
        {
            var input = "Lorem ipsum";
            var converter = new StringToStreamConverter(input);
            using var sr = new StreamReader(converter.GetStreamFromString());
            var typeDetector = new TypeDetector(sr);
            Assert.Equal(FrontMatterType.MarkdownDocument, typeDetector.GetFrontMatterType());
        }
    }
}
