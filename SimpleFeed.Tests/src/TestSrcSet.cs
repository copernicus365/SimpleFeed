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
		static string imgUrl = "https://static1.example.com/static/abc/t/abcedfg123/ge/Some+Cool+Idea+1K.png";
		static string imgUrlFull = $"{imgUrl}?format=1200w";

		static string Html1 = $@" <p>howdy</p>
<img  class='thumb-image' data-image=""{imgUrl}"" data-image-dimensions=1920x1080 data-image-focal-point=""0.5,0.5"" alt='Some Cool Pic 1K.png'  data-image-id=""abcedfg123"" data-type=""image"" src=""{imgUrl}?format=1200w"" data-load=false/>
		<p>Hello world</p>";

		ExtraTextFuncs efuncs = new ExtraTextFuncs();

		[Fact]
		public void Test1()
		{
			SrcSet srcset = efuncs.GetFirstImageTagFromHtmlText(Html1, 1000);

			bool success = srcset != null && srcset.Src.Url == imgUrlFull;

			Assert.True(srcset != null && srcset.Src.Url.NotNulle());
		}

		[Fact]
		public void OldHtmlExtract()
		{
			//string html = Ex1;
			Dictionary<string, string> dict = ExtraTextFuncs.GetHtmlTagAttributes(Html1);

			bool success = dict != null && dict.V("src") == imgUrl;
			//Assert.True(success);
		}


		[Fact]
		public async Task Test2()
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
