using System;
using System.IO;
using Krompaco.RecordCollector.Content.FrontMatterParsers;
using Krompaco.RecordCollector.Content.Models;
using Xunit;

namespace Krompaco.RecordCollector.Content.Tests
{
    public class JsonTests
    {
        [Fact]
        public void ParserTest()
        {
            string input = @"{
       ""categories"": [
      ""Development"",
      ""VIM""
   ],
   ""date"": ""2012-04-06"",
""images"": [""site-feature-image.jpg""],
""weight"": 1,
""testint"": 1,
""testbool"": false,
""teststring"": ""What?"",
""cascade"": {
      ""banner"": ""images/typewriter.jpg"",
 ""tags"": [
      "".vimrc"",
      ""plugins"",
      ""spf13-vim"",
      ""vim""
   ]
   },
""testdate"": ""2012-04-06"",
""testarray"": [""Development"", ""VIM""],
   ""description"": ""spf13-vim is a cross platform distribution of vim plugins and resources for Vim."",
   ""slug"": ""spf13-vim-3-0-release-and-new-website"",
   ""tags"": [
      "".vimrc"",
      ""plugins"",
      ""spf13-vim"",
      ""vim""
   ],
   ""title"": ""About"",
   ""resources"": [
      {
         ""name"": ""header"",
         ""src"": ""images/sunset.jpg""
      },
      {
         ""params"": {
            ""icon"": ""photo""
         },
         ""src"": ""documents/photo_specs.pdf"",
         ""title"": ""Photo Specifications""
      },
      {
         ""src"": ""documents/guide.pdf"",
         ""title"": ""Instruction Guide""
      },
      {
         ""src"": ""documents/checklist.pdf"",
         ""title"": ""Document Checklist""
      },
      {
         ""src"": ""documents/payment.docx"",
         ""title"": ""Proof of Payment""
      },
      {
         ""name"": ""pdf-file-:counter"",
         ""params"": {
            ""icon"": ""pdf""
         },
         ""src"": ""**.pdf""
      },
      {
         ""params"": {
            ""icon"": ""word""
         },
         ""src"": ""**.docx""
      }
   ]
}
Lorem ipsum";

            using TextReader sr = new StringReader(input);
            var parser = new JsonParser<SinglePage>(sr, string.Empty);
            var single = parser.GetAsSinglePage();

            Assert.Equal("About", single.Title);
            Assert.Equal("Development", single.Categories[0]);
            Assert.Equal("VIM", single.Categories[1]);
            Assert.Equal(new DateTime(2012, 4, 6).Date, single.Date.Date);
            Assert.Equal("spf13-vim is a cross platform distribution of vim plugins and resources for Vim.", single.Description);
            Assert.Equal("spf13-vim-3-0-release-and-new-website", single.Slug);
            Assert.Equal("1", single.CustomStringProperties["testint"]);
            Assert.Equal("False", single.CustomStringProperties["testbool"], StringComparer.OrdinalIgnoreCase);
            Assert.Equal("What?", single.CustomStringProperties["teststring"]);
            Assert.Equal("2012-04-06", single.CustomStringProperties["testdate"]);
            Assert.Equal("Development", single.CustomArrayProperties["testarray"][0]);
            Assert.Equal("VIM", single.CustomArrayProperties["testarray"][1]);
            Assert.Equal(".vimrc", single.Tags[0]);
            Assert.Equal("plugins", single.Tags[1]);
            Assert.Equal("spf13-vim", single.Tags[2]);
            Assert.Equal("vim", single.Tags[3]);
            Assert.Equal("images/typewriter.jpg", single.Cascade.CustomStringProperties["banner"]);
            Assert.Equal(".vimrc", single.Cascade.CustomArrayProperties["tags"][0]);
            Assert.Equal("Lorem ipsum", single.Content);
        }
    }
}
