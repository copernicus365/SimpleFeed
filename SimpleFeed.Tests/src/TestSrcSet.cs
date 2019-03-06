using System;
using System.Collections.Generic;
using Xunit;
using DotNetXtensions;
using DotNetXtensions.Net;
using System.Threading.Tasks;
using System.IO;
using SimpleFeed.Tests;

namespace SimpleFeedNS.Tests
{
	public class TestSrcSet : FeedTestsBase
	{

		static string Ex1 = @"<img class=""thumb-image"" data-image=""https://static1.squarespace.com/static/556c7d21e4b0159e7291d601/t/5c756b09ee6eb07a7f4bd9f2/1551199003020/Leadership+Podcast+Title+Graphic+4K.png"" data-image-dimensions=""1920x1080"" data-image-focal-point=""0.5,0.5"" alt=""Lorem Podcast Title Graphic 4K.png"" data-load=""false"" data-image-id=""4c7c6b99eb1fb0739f4fd9f5"" data-type=""image"" src=""https://static1.squarespace.com/static/556c7d21e4b0159e7291d601/t/5c756b09ee6eb07a7f4bd9f2/1551199003020/Leadership+Podcast+Title+Graphic+4K.png?format=1000w"" />
            
 <p>Lorem ipsum...";

		ExtraTextFuncs efuncs = new ExtraTextFuncs();

		[Fact]
		public void Test2()
		{
			string html = Ex1;
			Dictionary<string, string> dict = ExtraTextFuncs.GetHtmlTagAttributes(html);

			SrcSet srcset = efuncs.GetFirstImageTagFromHtmlText(html, 1000, false);

			Assert.True(srcset != null && srcset.Src.Url.NotNulle());
		}

		[Fact]
		public async Task Test1()
		{
			//_testFeedUrl = NoCommitValues.TestSrcSet_FeedUrl;
			_testFeedPath = NoCommitValues.TestSrcSet_FeedPath;

			SimpleFeed feed = await SetFeed(
				alterSettings: stg => {
					stg.ContentSettings = new SFContentConversionSettings() {
						ContentTag = SFContentConversionType.HtmlToMarkdown,
						SummaryTag = SFContentConversionType.HtmlToMarkdown,
						TitleTag = SFContentConversionType.HtmlToMarkdown,
					};
					stg.GetImageUrlsFromContentImgTag = true;
					//stg.ConvertHtmlContentInContentTag = true;
				});

			feed.Parse(_feedContent);

			Assert.True(!feed.Error && feed.Items.NotNulle());
		}
	}
}
