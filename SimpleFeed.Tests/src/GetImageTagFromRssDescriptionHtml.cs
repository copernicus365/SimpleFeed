using System;
using Xunit;
using DotNetXtensions;
using DotNetXtensions.Net;
using System.Threading.Tasks;
using System.IO;

namespace SimpleFeedNS.Tests
{
	public class GetImageTagFromRssDescriptionHtml : FeedTestsBase
	{
		[Fact]
		public async Task Test1()
		{
			//_testFeedUrl = "https://www.sdcoc.org/stories/feed/";
			_testFeedPath = @"C:\Dropbox (Personal)\Desktop2\TL\test-feed1-sdcoc.xml";

			SimpleFeed feed = await SetFeed(
				alterSettings: stg => stg.GetImageUrlsFromContentImgTag = true);

			feed.Parse(_feedContent);


		}
	}
}
