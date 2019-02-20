using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetXtensions;

namespace SimpleFeedNS
{
	class ExtraTextFuncs
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

		void OldCodeCategoryToLink(SFCategory cat)
		{
			string value = cat.Value;

			// this is for categories, not just links. 
			// check greater than 10 to match the minimal link size (http://bit.ly/)
			if (value.CountN() > 13) {

				// -- move this outside of here if needed later... --
				string linkVal = TextFuncs.RemoveBrackets(value);

				if (TextFuncs.IsWebLink(linkVal, checkForWww: true)) {

					if (linkVal[0] == 'w')
						linkVal = "http://" + linkVal;

					//AddLink(new SFLink() { Url = linkVal });

					//if (_deleteCategoryIfConvertedToLink)
					//return false;
				}
			}
		}


		public SrcSet GetImageTagFromRssDescriptionHtml(
			string desc, 
			int maxStartIndexOfImgTag,
			bool requiresIsWithinAnchorTag = true)
		{
			if (desc.IsNulle())
				return null;

			if (maxStartIndexOfImgTag > desc.Length)
				maxStartIndexOfImgTag = desc.Length;

			int imgIdx = desc.IndexOf("<img ", 0, maxStartIndexOfImgTag);

			if (imgIdx >= 0) {

				int endImgIdx = desc.IndexOf("/>", imgIdx);

				if (endImgIdx < 15)
					return null;

				string imgTag = desc.Substring(imgIdx, endImgIdx - imgIdx + 2);

				if (requiresIsWithinAnchorTag) {
					if (imgIdx == 0)
						return null;

					int anchorTagIdx = desc.LastIndexOf('<', imgIdx - 1);

					if (anchorTagIdx < 0
						|| desc[anchorTagIdx + 1] != 'a' 
						|| !desc[anchorTagIdx + 2].IsWhitespace())
						return null;
				}

				var imgAttributes = parseAttributesFromHtmlTag(imgTag);

				if (imgAttributes.NotNulle()) {
					string src = imgAttributes.V("src");
					string srcSet = imgAttributes.V("srcset");

					if (src.IsNulle())
						return null;

					var srcImg = new SrcSetImg() {
						Url = src,
						Size = imgAttributes.V("width").ToInt(0),
						SizeType = SrcSizeType.Width
					};

					SrcSet srcset = new SrcSet() {
						Src = srcImg,
						Sizes = imgAttributes.V("sizes"),
						SrcSets = SrcSet.ParseSrcSets(srcSet, out SrcSizeType sizeType),
						SrcSetsSizeType = sizeType
					};

					srcset.InitValues();

					return srcset;
				}
			}

			return null;
		}

		static Regex _rxHtmlAttributeKVs = 
			new Regex(@"(\S+)\s*=\s*[\""']?((?:.(?![\""']?\s+(?:\S+)=|[>\""']))?[^\""']*)[\""']?",
			RegexOptions.Compiled);

		static Dictionary<string, string> parseAttributesFromHtmlTag(string tag)
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
	}
}
