using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DotNetXtensions; //using DotNetXtensionsPrivate;

namespace SimpleFeedNS
{
	public class SFFeedSettings
	{
		/// <summary>
		/// If non-null, sets a limit to the number of entries that should be parsed (0 for none).
		/// This is useful if you don't need to parse any of the entries (just need top-level
		/// feed information), or if you just need the first number of items, etc. All of this 
		/// can help performance wise, though note that we still have to parse the whole data
		/// into an XElement.
		/// </summary>
		public int? NumberOfEntriesToParseLimit { get; set; }

		public bool ClearXmlContent_ContentTag { get; set; } = false;

		public bool ClearXmlContent_SummaryTag { get; set; } = true;

		public bool ClearXmlContent_TitleTag { get; set; } = true;

		public bool KeepXmlDocument { get; set; } = true;

		public bool HtmlDecodeTextValues { get; set; } = true;

		/// <summary>
		/// FALSE by default, set to true to have the older xml tag stripper
		/// used, as opposed to the much superior html tag stripper. This setting
		/// may only be here temporarily, in order to visualize the difference 
		/// between the old and the new.
		/// </summary>
		public bool ClearXmlContent_BasicXmlTagStrip { get; set; } = false;

		/// <summary>
		/// TRUE by default: Allows the html tag stripping process to convert a minimal 
		/// number of html elements to markdown. In a very simple and basic way, to be clear,
		/// e.g. paragraphs and breaks are given line breaks, bold and italic are responded to,
		/// and list-items are treated as bullets.
		/// </summary>
		public bool ClearXmlContent_ConvertHtmlTagsToMinimalMarkdown { get; set; } = true;

		/// <summary>
		/// When adding categories, detect if the value is a link and if so,
		/// add that url to the Links for that entry.
		/// </summary>
		public bool ConvertCategoryUrlsToLinks { get; set; }

		/// <summary>
		/// True in order to extract any urls within the main content fields 
		/// and convert them to links.
		/// </summary>
		public bool ConvertContentUrlsToLinks { get; set; }

		/// <summary>
		/// True in order to extract image urls from a content `img`
		/// tag (often which uses `srcset`).
		/// </summary>
		public bool GetImageUrlsFromContentImgTag { get; set; }

		// Removed because not deleting would add duplicate information, should do only one way or the other...
		//public bool DeleteCategoryIfConvertedToLink { get; set; }

		public SimpleFeed ParentFeed { get; set; }

		public SFAlterFeedLinks AlterFeedLinks { get; set; } = new SFAlterFeedLinks();

		/// <summary>
		/// If not null, the final items can be altered by setting this func. If
		/// an item returns null, that will be filtered out.
		/// </summary>
		public Func<SFFeedEntry, SFFeedEntry> AlterEntryOnComplete { get; set; }

		/// <summary>
		/// Set this func in order to generate an id for entries that didn't have
		/// an id or guid (null by default). Sends the FeedEntry and SimpleFeed.IdBase
		/// (will be null if none is set).
		/// </summary>
		public Func<SFFeedEntry, string, string> GenerateIdForEntriesWithNoId { get; set; }

		public TimeZoneInfo AlterUTCDatesToThisTimezone { get; set; }

	}
}
