using DotNetXtensions.Test;

namespace SimpleFeed.Tests
{
	public class SFBaseTest : XUnitTestBase
	{
		public SFBaseTest(string resourceBasePath = "data") : base(
			typeof(SFBaseTest),
			resourceBasePath,
			cacheResourceGets: true)
		{

		}
	}
}
