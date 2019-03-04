using System;
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
