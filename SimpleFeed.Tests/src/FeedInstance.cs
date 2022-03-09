namespace SFTests;

/// <summary>
/// Wraps a feed to be parsed as a singleton in effect,
/// useful for when doing a battery tests in a test class
/// on a single feed. Use this to cache and thus only having
/// to read and parse once the feed.
/// </summary>
public class FeedInstance
{
	SimpleFeed _feed;
	SFFeedSettings _settings;
	string _contentPath;
	object _feedLock = new();
	public int TimesParsed;

	public FeedInstance(
		string contentPath,
		Action<SFFeedSettings> alterSettings = null)
	{
		_contentPath = contentPath;
		_settings = new SFFeedSettings() {
			KeepXmlDocument = true,
			ConvertCategoryUrlsToLinks = true,
			ConvertContentUrlsToLinks = true,
			AlterUTCDatesToThisTimezone = TimeZoneInfo.Local,
			//GetImageUrlsFromContentImgTag = true,
		};

		alterSettings?.Invoke(_settings);
	}

	public IList<SFFeedEntry> Items() => (_feed ?? GetFeed()).Items;

	public SimpleFeed GetFeed()
	{
		lock(_feedLock) {
			if(_feed == null) {
				if(++TimesParsed != 1)
					throw new Exception();

				_feed = new SimpleFeed {
					Settings = _settings
				};

				var content = SFBaseTest.DataBytes(_contentPath);

				_feed.Parse(content);
			}
			return _feed;
		}
	}
}
