using System;
using System.IO;
using Krompaco.RecordCollector.Content.FrontMatterParsers;
using Xunit;

namespace Krompaco.RecordCollector.Content.Tests
{
    public class TomlTests
    {
        [Fact]
        public void ParserTest()
        {
            string input = @"
 +++
title =  ""About"" 
 categories = [""Development"", ""VIM""]
date = ""2012-04-06""
description = ""spf13-vim is a cross platform distribution of vim plugins and resources for Vim.""
slug = ""spf13-vim-3-0-release-and-new-website""
testint = 1
testbool = false
teststring = ""What?""
testdate = ""2012-04-06""
testarray = [""Development"", ""VIM""]
tags = ["".vimrc"", ""plugins"", ""spf13-vim"", ""vim""]
+++ 

Lorem ipsum";

            using TextReader sr = new StringReader(input);
            var parser = new TomlParser(sr);
            var single = parser.GetAsSinglePage();

            Assert.Equal("About", single.Title);
            Assert.Equal("Development", single.Categories[0]);
            Assert.Equal("VIM", single.Categories[1]);
            Assert.Equal(new DateTime(2012, 4, 6).Date, single.Date.Date);
            Assert.Equal("spf13-vim is a cross platform distribution of vim plugins and resources for Vim.", single.Description);
            Assert.Equal("spf13-vim-3-0-release-and-new-website", single.Slug);
            Assert.Equal("1", single.CustomStringProperties["testint"]);
            Assert.Equal("False", single.CustomStringProperties["testbool"]);
            Assert.Equal("What?", single.CustomStringProperties["teststring"]);
            Assert.Equal("2012-04-06", single.CustomStringProperties["testdate"]);
            Assert.Equal("Development", single.CustomArrayProperties["testarray"][0]);
            Assert.Equal("VIM", single.CustomArrayProperties["testarray"][1]);
            Assert.Equal(".vimrc", single.Tags[0]);
            Assert.Equal("plugins", single.Tags[1]);
            Assert.Equal("spf13-vim", single.Tags[2]);
            Assert.Equal("vim", single.Tags[3]);
        }
    }
}
