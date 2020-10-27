using System;
using System.IO;
using System.Threading.Tasks;

using DotNetXtensions;
using DotNetXtensions.Net;

using Xunit;

namespace SimpleFeedNS.Tests
{
	public class FeedTestsBase
	{
		protected string _testFeedUrl;
		protected string _testFeedPath;
		protected string _feedContent;
		protected SimpleFeed _feed;

		public async Task<SimpleFeed> SetFeed(
			bool saveToFileIfPossible = true,
			Action<SFFeedSettings> alterSettings = null)
		{
			if(_testFeedPath.NotNulle()) {
				_feedContent = await File.ReadAllTextAsync(_testFeedPath);
			}
			else if(_testFeedUrl.NotNulle()) {
				_feedContent = await XHttp.GetStringAsync(_testFeedUrl);

				if(saveToFileIfPossible && _feedContent.NotNulle() && _testFeedPath.NotNulle()) {
					await File.WriteAllTextAsync(_testFeedPath, _feedContent);
				}
			}

			Assert.True(_feedContent.NotNulle());

			var settings = new SFFeedSettings() {
				KeepXmlDocument = true,
				ConvertCategoryUrlsToLinks = true,
				ConvertContentUrlsToLinks = true,
				AlterUTCDatesToThisTimezone = TimeZoneInfo.Local,
				//GetImageUrlsFromContentImgTag = true,
			};

			alterSettings?.Invoke(settings);

			_feed = new SimpleFeed {
				Settings = settings
			};

			return _feed;
		}

	}
}
