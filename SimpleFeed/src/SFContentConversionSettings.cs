using DotNetXtensions;
using System;
using System.Collections.Generic;

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
		public virtual SFContentConversionType ContentTag { get; set; } = SFContentConversionType.HtmlToMarkdown;

		public virtual SFContentConversionType SummaryTag { get; set; } = SFContentConversionType.HtmlToMarkdown;

		public virtual SFContentConversionType TitleTag { get; set; } = SFContentConversionType.HtmlToMarkdown;

		public virtual bool HtmlDecodeTextValues { get; set; } = true;

		/// <summary>
		/// True in order to extract image urls from a content `img`
		/// tag (often which uses `srcset`).
		/// </summary>
		public virtual bool GetFirstImageTagFromHtml { get; set; } = false;

		public virtual string ConvertHtmlContent(
			string input,
			SFContentConversionType conversionType,
			bool? htmlDecode = null)
		{
			if (input == null)
				return null;

			input = input.NullIfEmptyTrimmed();
			if (input == null)
				return null;

			bool _htmlDecode = htmlDecode ?? HtmlDecodeTextValues;
			if (_htmlDecode)
				input = System.Net.WebUtility.HtmlDecode(input);

			return input.NullIfEmptyTrimmed() ?? "";
		}

		public virtual Dictionary<string, string> GetFirstImageTagAttributesFromHtml(
			string html,
			int startIndex)
			=> null;

	}
}
