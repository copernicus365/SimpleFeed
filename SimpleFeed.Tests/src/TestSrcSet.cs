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
					stg.GetImageUrlsFromContentImgTag = true;
					stg.ClearXmlContent_ContentTag = true;
				});

			feed.Parse(_feedContent);


		}
	}
}
