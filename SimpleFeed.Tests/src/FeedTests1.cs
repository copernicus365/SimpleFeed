namespace SFTests;

public class FeedTests1 : SFBaseTest
{
	static FeedInstance _feed = new("feed1.rss");

	static IList<SFFeedEntry> GetItems() => _feed.Items();

	[Fact]
	public void Test1()
	{
		var items = GetItems();

		Assert.Equal(5, items.Count);

		Assert.True(items.All(itm => itm.Links.CountN() == 1));
	}

}
