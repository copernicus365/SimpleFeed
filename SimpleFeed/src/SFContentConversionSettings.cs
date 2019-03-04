using System;

namespace SimpleFeedNS
{
	public enum SFContentConversionType
	{
		None = 0,
		HtmlToMarkdown = 1,
		SimpleHtmlTagStrip = 2,
		OldHtmlTagStrip = 3,
	}

	public class SFContentConversionSettings
	{
		public SFContentConversionType ContentTag { get; set; } = SFContentConversionType.HtmlToMarkdown;

		public SFContentConversionType SummaryTag { get; set; } = SFContentConversionType.HtmlToMarkdown;

		public SFContentConversionType TitleTag { get; set; } = SFContentConversionType.HtmlToMarkdown;

		public bool HtmlDecodeTextValues { get; set; } = true;

		#region --- OLD settings, moved to ContentConversionSettings ---

		//public bool ConvertHtmlContentInContentTag { get; set; } = false;

		//public bool ConvertHtmlContentInSummaryTag { get; set; } = true;

		//public bool ConvertHtmlContentInTitleTag { get; set; } = true;

		//public bool HtmlDecodeTextValues { get; set; } = true;

		///// <summary>
		///// TRUE by default, allows the html tag stripping process to convert a minimal 
		///// number of html elements to markdown. In a very simple and basic way, to be clear,
		///// e.g. paragraphs and breaks are given line breaks, bold and italic are responded to,
		///// and list-items are treated as bullets.
		///// </summary>
		//public bool ConvertHtmlContentToMarkdown { get; set; } = true;

		///// <summary>
		///// FALSE by default, set to true to have the older xml tag stripper
		///// used, as opposed to the much superior html tag stripper. This setting
		///// may only be here temporarily, in order to visualize the difference 
		///// between the old and the new.
		///// </summary>
		//public bool UseOldXmlTagStripForHtmlContentConversion { get; set; } = false;

		#endregion

	}
}
