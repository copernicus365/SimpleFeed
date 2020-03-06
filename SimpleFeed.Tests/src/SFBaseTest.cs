using System;
using System.Collections.Generic;
using SimpleFeedNS;
using DotNetXtensions;
using DotNetXtensions.Test;
using Xunit;

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
