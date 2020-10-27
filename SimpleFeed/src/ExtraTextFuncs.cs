using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetXtensions;

namespace SimpleFeedNS
{
	public class ExtraTextFuncs
	{
		public static Regex Rx_HttpLink = new Regex(
			@"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public string[] GetLinks(string s)
		{
			if (s != null && s.Length > 7) {
				int firstHttpIdx = s.IndexOf("http");
				if (firstHttpIdx >= 0) {

					var matchColl = Rx_HttpLink.Matches(s, firstHttpIdx);
					if (matchColl.Count > 0) {

						string[] urls = matchColl.Cast<Match>()
							.Where(m => m.Success && m.Value.InRange(8, 256))
							.Select(m => m.Value)
							.ToArray();

						if (urls.NotNulle())
							return urls;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Searches for the *first* `img` tag within the input string (ideally a content / 
		/// description field within an RSS item). 
		/// </summary>
		/// <param name="html"></param>
		/// <param name="contentSettings"></param>
		/// <param name="maxStartIndexOfImgTag"></param>
		public SrcSet GetFirstImageTagFromHtmlContent(
			string html,
			SFContentConversionSettings contentSettings,
			int maxStartIndexOfImgTag = -1)
		{
			if (html.IsNulle() || maxStartIndexOfImgTag < 0 || !contentSettings.GetFirstImageTagFromHtml)
				return null;

			int maxSearchLen = html.Length;
			if (maxStartIndexOfImgTag >= 0)
				maxSearchLen = (maxStartIndexOfImgTag + 15).Min(html.Length); // 15, just a rough min after start of img tag, still need an href + the url itself...

			int imgIdx = html.IndexOf("<img ", 0, maxSearchLen);
			if (imgIdx < 0)
				return null;

			Dictionary<string, string> imgAttributes = contentSettings
				.GetFirstImageTagAttributesFromHtml(html, imgIdx);

			if (imgAttributes.IsNulle() || !imgAttributes.ContainsKey("src"))
				return null;

			SrcSet srcSet = _AttributeKeyValuesToSrcSet(imgAttributes);
			return srcSet;
		}

		static SrcSet _AttributeKeyValuesToSrcSet(Dictionary<string, string> kvs)
		{
			if (kvs.IsNulle())
				return null;

			string src = kvs.V("src");
			string srcSet = kvs.V("srcset");

			if (src.IsNulle())
				return null;

			var srcImg = new SrcSetImg() {
				Url = src,
				Size = kvs.V("width").ToInt(0),
				SizeType = SrcSizeType.Width
			};

			SrcSet srcset = new SrcSet() {
				Src = srcImg,
				Sizes = kvs.V("sizes"),
				SrcSets = SrcSet.ParseSrcSets(srcSet, out SrcSizeType sizeType),
				SrcSetsSizeType = sizeType
			};

			srcset.InitValues();

			return srcset;
		}

		/// <summary>
		/// Attempts to get all of the key-value attributes from this html-type tag.
		/// Importantly: input `tag` must be a single tag already, otherwise,
		/// while this will still work, it will nonetheless get all attributes from any tags in
		/// the input text. So it's up to the caller to isolate the tag, better to isolate that work
		/// on its own.
		/// </summary>
		public static Dictionary<string, string> GetHtmlTagAttributes(string tag)
		{
			if (tag.IsNulle())
				return null;

			var matches = _rxHtmlAttributeKVs
				.Matches(tag)
				.OfType<Match>()
				.Where(m => m.Groups.Count > 0)
				.ToArray();

			Dictionary<string, string> kvs = null;

			if (matches.Length > 0) {

				for (int i = 0; i < matches.Length; i++) {
					var m = matches[i];

					var grps = m.Groups;
					if ((grps?.Count ?? 0) == 3) {
						string attr = grps[1].Value;
						string val = grps[2].Value;
						if (attr.NotNulle()) {
							if (kvs == null)
								kvs = new Dictionary<string, string>();
							kvs[attr] = val;
						}
					}
				}
			}

			return kvs;
		}

		/// <summary>
		/// Unfortunately I forgot where I got this rx, somewhere on stackoverflow, just had to escape the 
		/// double-quotes for this string literal. 
		/// Can't find it here, but maybe: https://stackoverflow.com/questions/317053/regular-expression-for-extracting-tag-attributes
		/// </summary>
		static readonly Regex _rxHtmlAttributeKVs =
			new Regex(@"(\S+)\s*=\s*[\""']?((?:.(?![\""']?\s+(?:\S+)=|[>\""']))?[^\""']*)[\""']?",
			RegexOptions.Compiled);

		public static bool IsWebLink(string s, bool checkForWww = true)
		{
			if (s != null && s.Length > 9) { //http://bit.ly/ = 14 chars, www.msn.ly
				if (checkForWww &&
					s[0] == 'w' &&
					s[1] == 'w' &&
					s[2] == 'w' &&
					s[3] == '.') {
					return true;
				}
				else if (
					s[0] == 'h' &&
					s[1] == 't' &&
					s[2] == 't' &&
					s[3] == 'p') {

					if (
						(s[4] == ':' && s[5] == '/' && s[6] == '/') ||
						(s[4] == 's' && s[5] == ':' && s[6] == '/' && s[7] == '/')) {
						return true;
					}
				}
			}
			return false;
		}

	}
}

#region --- Old GetFirstImageTagFromHtmlText ---

//// found an image tag, now find its end
//int endImgIdx = html.IndexOf("/>", imgIdx);

//if (endImgIdx < 15)
//	return null; // no end to img tag found, return

//// get the full "<img ... />" tag
//string imgTag = html.Substring(imgIdx, endImgIdx - imgIdx + 2); // +2 recovers "/>"

// OLD:
//var imgAttributes = GetHtmlTagAttributes(imgTag);

//if (requiresIsWithinAnchorTag) {
//	if (imgIdx == 0) // gotta test to stay in range in below search `imgIdx - 1`
//		return null;

//	int anchorTagIdx = desc.LastIndexOf('<', imgIdx - 1); // search backwards from start of img tag for next open pbracket

//	if (anchorTagIdx < 0
//		|| desc[anchorTagIdx + 1] != 'a'
//		|| !desc[anchorTagIdx + 2].IsWhitespace())
//		return null;
//}

#endregion

/* --- */

//void OldCodeCategoryToLink(SFCategory cat)
//{
//	string value = cat.Value;

//	// this is for categories, not just links. 
//	// check greater than 10 to match the minimal link size (http://bit.ly/)
//	if (value.CountN() > 13) {

//		// -- move this outside of here if needed later... --
//		string linkVal = TextFuncs.RemoveBrackets(value);

//		if (TextFuncs.IsWebLink(linkVal, checkForWww: true)) {

//			if (linkVal[0] == 'w')
//				linkVal = "http://" + linkVal;

//			//AddLink(new SFLink() { Url = linkVal });

//			//if (_deleteCategoryIfConvertedToLink)
//			//return false;
//		}
//	}
//}

///// <summary>
///// Searches for the *first* <![CDATA[`<img ... />`]]> tag within the input
///// text field (ideally a content / description field within RSS item, containing html).
///// If found, we also try to find any parse any srcset information on the image tag.
///// </summary>
///// <param name="html">Content</param>
///// <param name="maxStartIndexOfImgTag">Allows one to require the img tag to be found
///// before a certain distance.</param>
///// <param name="requiresIsWithinAnchorTag">If true, found img tag must be 
///// a direct child of an anchor tag.</param>
