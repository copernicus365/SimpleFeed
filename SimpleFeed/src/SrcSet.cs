using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetXtensions;

namespace SimpleFeedNS
{
	public class SrcSet
	{
		public SrcSetImg Src { get; set; }

		public SrcSizeType SrcSetsSizeType { get; set; }

		public List<SrcSetImg> SrcSets { get; set; }

		public string Sizes { get; set; }

		public Dictionary<string, int> SrcSetImageUrlsDict { get; set; }

		/// <summary>
		/// Orders the <see cref="SrcSets"/> list from smallest to largest sizes, 
		/// and then sets <see cref="SrcSetImageUrlsDict"/> dictionary from these values (or to null if nulle).
		/// </summary>
		public SrcSet InitValues()
		{
			SrcSetImageUrlsDict = null;

			if (SrcSets.NotNulle()) {

				SrcSets.Sort(v => v.Size);

				foreach (var img in SrcSets) {
					SrcSetImageUrlsDict = SrcSets.ToDictionaryIgnoreDuplicateKeys(
						v => v.Url, v => v.Size);
				}
			}

			return this;
		}

		/// <summary>
		/// Must have string source end with trailing comma though, so
		/// usually will have to alter it.
		/// </summary>
		static Regex _rxSrcSets =
			new Regex(@"(.*?)( )(\d+)(w|x)(,)", 
				// https://regex101.com/r/BUrSzJ/1
			RegexOptions.Compiled);

		public static List<SrcSetImg> ParseSrcSets(string srcSetStr, out SrcSizeType sizeType)
		{
			sizeType = SrcSizeType.None;

			if (srcSetStr.IsNulle())
				return null;

			if (srcSetStr.Last() != ',')
				srcSetStr += ','; // makes regex much easier / better, and allows consistent 

			var matches = _rxSrcSets
				.Matches(srcSetStr)
				.OfType<Match>()
				.Where(m => m.Groups.Count > 0)
				.ToArray();

			List<SrcSetImg> srcs = new List<SrcSetImg>();

			if (matches.Length > 0) {

				for (int i = 0; i < matches.Length; i++) {
					var m = matches[i];

					var grps = m.Groups;
					if ((grps?.Count ?? 0).InRange(5, 6)) { // final comma can be missing
						string url = grps[1].Value.NullIfEmptyTrimmed();
						int size = grps[3].Value.ToInt(-1);
						string sizeTypStr = grps[4].Value;

						if (url == null || size < 1 || sizeTypStr == null || sizeTypStr.Length != 1)
							continue;

						char sType = sizeTypStr[0];
						if (i == 0) {
							sizeType = SrcSetImg.CharForSrcSetType(sType);
							if (sizeType == SrcSizeType.None)
								return null;
						}

						SrcSetImg sImg = new SrcSetImg() {
							Url = url,
							SizeType = sizeType,
							Size = size
						};

						if (!sImg.SetNumberVal(size, sizeType, sizeTypStr[0]))
							continue;

						srcs.Add(sImg);
					}
				}
			}

			if (srcs.IsNulle())
				return null;

			return srcs;
		}

	}
}
