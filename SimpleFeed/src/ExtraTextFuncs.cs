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


		public List<SrcSetImg> GetImageTagFromRssDescriptionHtml(string desc, int maxStartIndexOfImgTag)
		{
			if (desc.IsNulle())
				return null;

			if (maxStartIndexOfImgTag > desc.Length)
				maxStartIndexOfImgTag = desc.Length;

			int imgIdx = desc.IndexOf("<img ", 0, maxStartIndexOfImgTag);

			return null;
		}
	}
}
