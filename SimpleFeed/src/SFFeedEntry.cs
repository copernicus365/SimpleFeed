using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using DotNetXtensions; //using DotNetXtensionsPrivate;

namespace SimpleFeedNS
{
	public class SFFeedEntry
	{
		SFAuthorFull m_AuthorFull;
		SFText _Content;
		List<SFLink> _Links;
		SFText _Summary;
		SFText _Title;

		[DebuggerStepThrough]
		public SFFeedEntry() { }

		[DebuggerStepThrough]
		public SFFeedEntry(SFFeedSettings settings)
		{
			ParentSettings = settings;
		}

		/// <summary>
		/// The entry Id.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The Updated time of the feed entry. When parsing a feed, when there is no updated 
		/// value available (as RSS does not have one), it is *in general*
		/// taken from the Published (rss: pubDate or alternatives) datetime if available (and vise-versa). 
		/// But please see documentation on Published for many more details on how SimpleFeed handles datetimes! 
		/// </summary>
		public DateTimeOffset Updated { get; set; }

		/// <summary>
		/// The Published date of the feed entry (atom: published, rss: pubDate or dc:date and so forth).
		/// For ATOM, when only the mandatory 'updated' element is present, this value will be taken from it.
		/// Also, when no date is available (for RSS, ATOM will always have an 'updated') 
		/// (OLD! strike out: a value from the parent channel is used if available)
		/// (NEW!): then this value will simply be set to Min (DateTimeOffset.MinValue).
		/// <para />
		/// When parsing an RSS feed, there are a number of possible dates that could be obtained,
		/// and we have to decide which gets precedence, on which see `SetRssDates`. 
		/// Things may change but currently we give precedence to `atom:published` then `dc:date`,
		/// and finally `pubDate` when available (NB! to be sure you must depend on the code and
		/// we don't make promises this complex set of options won't cause precedence changes).
		/// One thing is for sure though, SimpleFeed 
		/// will NEVER throw an exception based on bad date-time formats!!! In the worse case scenario, 
		/// when bad or just not available it will simply be set to DateTime.MinValue. 
		/// </summary>
		public DateTimeOffset Published { get; set; }

		/// <summary>
		/// Simply returns or sets AuthorFull.Name.
		/// </summary>
		public string Author {
			get { return AuthorFull.Name; }
			set { AuthorFull.Name = value; }
		}

		/// <summary>
		/// Always returns a non-null AuthorFull object, though its values (Name, Email, Uri) 
		/// are all null by default. 
		/// </summary>
		public SFAuthorFull AuthorFull {
			get {
				if (m_AuthorFull == null)
					m_AuthorFull = new SFAuthorFull();
				return m_AuthorFull;
			}
			set { m_AuthorFull = value; }
		}

		public SFFeedSettings ParentSettings { get; set; }

		/// <summary>
		/// The entry content. Returns / sets ContentFull.Value.
		/// </summary>
		public string Content {
			get { return ContentFull.Value; }
			set { ContentFull.Value = value; }
		}

		/// <summary>
		/// Content. Guaranteed non-null return.
		/// </summary>
		public SFText ContentFull {
			get {
				if (_Content == null)
					_Content = new SFText();
				return _Content;
			}
			set { _Content = value; }
		}

		/// <summary>
		/// Returns SummaryFull text value if its Value is not null or empty, else returns ContentFull.
		/// Guaranteed non-null return.
		/// </summary>
		public string SummaryOrContent(bool clearXmlTags = false, int? maxLen = null)
		{
			return clearVal(Summary.IsNulle() ? ContentFull : SummaryFull, clearXmlTags, maxLen);
		}

		string clearVal(SFText sfVal, bool clearXmlTags = false, int? maxLen = null)
		{
			if (sfVal == null)
				return null;
			else if (sfVal.Value.IsNulle())
				return sfVal.Value;
			else {
				string txt = sfVal.Value;
				txt = SimpleFeed.ClearXmlTagsIf(clearXmlTags, txt, trim: true, htmlDecode: true);

				if (maxLen > 0 && txt != null && txt.Length > maxLen)
					txt = txt.SubstringMax((int)maxLen, ellipsis: "...", tryBreakOnWord: true);

				return txt;
			}
		}

		/// <summary>
		/// The entry summary. Returns / sets SummaryFull.Value.
		/// </summary>
		public string Summary {
			get { return SummaryFull.Value; }
			set { SummaryFull.Value = value; }
		}

		/// <summary>
		/// Summary. Guaranteed non-null return.
		/// </summary>
		public SFText SummaryFull {
			get {
				if (_Summary == null)
					_Summary = new SFText();
				return _Summary;
			}
			set { _Summary = value; }
		}

		/// <summary>
		/// The entry title. Returns / sets TitleFull.Value.
		/// </summary>
		public string Title {
			get { return TitleFull.Value; }
			set { TitleFull.Value = value; }
		}

		public string SubTitle { get; set; }

		public string Series { get; set; }

		#region -- Categories --

		//public Dictionary<string, string> Categories { get; set; } = new Dictionary<string, string>();

		public List<SFCategory> Categories { get; set; } = new List<SFCategory>();

		public List<SFFeedMeta> Metas { get; set; } = new List<SFFeedMeta>();

		/// <summary>
		/// Returns a concatenated string of all the categories, listing 
		/// </summary>
		public string Keywords
			=> Categories?.JoinToString(c => c.ToStringNoScheme(), ", ").NullIfEmpty();

		public void AddCategoriesCommaSeparatedString(string value)
		{
			if (value.IsNulle())
				return;

			foreach (string v in value.Split(_splitChar, StringSplitOptions.RemoveEmptyEntries)) {
				AddCategory(new SFCategory() { Value = v });
			}
		}

		static char[] _splitChar = { ',' };

		//public bool AddCategory(SFCategory category)
		//{

		//}

		public bool AddCategory(SFCategory cat) //string value, string scheme = null, string term = null)
		{
			if (cat != null && cat.PrepValues()) {
				Categories.Add(cat);
				return true;
			}
			return false;
		}

		public void AddCategories(XElement item)
		{
			if (item != null) {
				// gets any 'category' elements or any 'atom:category' elements
				foreach (var xcat in item.Elements().Where(e => e.Name == "category" || e.Name == SimpleFeed.xname_Atom_Category)) { // .Elements("category")) {
					if (xcat != null) {
						string val = xcat.ValueN().TrimIfNeeded(); // MUST trim this one here for next step
						SFCategory cat = null;

						// note that 'term' is a REQUIRED attribute for ATOM
						if (xcat.HasAttributes) {

							if (val.IsNulle())
								val = xcat.Attributes().FirstOrDefault(a => a.Name == "term").ValueN().TrimIfNeeded();

							if (val.IsNulle())
								return;

							cat = new SFCategory() {
								Scheme = xcat.Attributes().FirstOrDefault(a => a.Name == "domain" || a.Name == "scheme").ValueN(),
								Label = xcat.Attribute("label").ValueN(),
							};
						}
						else
							cat = new SFCategory();

						if (val.IsNulle())
							return;

						cat.Value = val;
						AddCategory(cat);
					}
				}
			}
		}

		public void AddMeta(XElement item)
		{
			var f = ParentSettings?.ParentFeed;
			if (item == null || f == null)
				return;

			// rawvoice
			// http://www.rawvoice.com/services/tools-and-resources/rawvoice-rss-2-0-module-xmlns-namespace-rss2/
			/* metamark, if present as a sub-item of <item> and <item> includes an <enclosure> item, 
			specifies additional meta information that may complement the enclosure and/or may be used 
			during the playback of the enclosure’s media. It has four attributes:
			type, link, position and duration and may contain a value. */
			if (f.HasRawVoice) {
				foreach (var metaX in item.Elements(SimpleFeed.xname_rawvoice_metamark)) {
					var meta = new SFFeedMeta() {
						Source = "rawvoice.meta",
						Type = metaX.Attribute("type")?.Value?.Trim(), //.ValueN().TrimN();
						Url = metaX.Attribute("link")?.Value?.Trim(), //.ValueN().TrimN();
						Value = metaX?.Value?.Trim()
					};
					if (meta.Value.NotNulle() || meta.Url.NotNulle()) {
						Metas.Add(meta);

						//BasicMimeType typ = isLinkRVMeta.V(meta.Type, BasicMimeType.none);
						string url = TextFuncs.IsWebLink(meta.Url)
							? meta.Url
							: (TextFuncs.IsWebLink(meta.Value) ? meta.Value : null);

						if (url != null) {
							var lnk = new SFLink(url, meta.Type);
							if (lnk != null && lnk.IsValid)
								AddLink(lnk);
						}
					}
				}
			}
		}

		private static Dictionary<string, BasicMimeType> isLinkRVMeta = new Dictionary<string, BasicMimeType>()
			.AddN("video", BasicMimeType.video)
			.AddN("audio", BasicMimeType.audio)
			.AddN("image", BasicMimeType.image);

		#endregion

		/// <summary>
		/// Title.
		/// </summary>
		public SFText TitleFull {
			get {
				if (_Title == null)
					_Title = new SFText();
				return _Title;
			}
			set { _Title = value; }
		}

		/// <summary>
		/// The original XElement of this FeedEntry if this entry was generated by
		/// parsing XML feed entry. SimpleFeed.KeepXmlDocument must be true for this to
		/// be saved automattically.
		/// </summary>
		public XElement XmlEntry { get; set; }

		#region LINKS

		/// <summary>
		/// Returns first link whose Rel == SFRel.src (see notes on SFRel.src).
		/// </summary>
		public SFLink ContentSrc {
			get {
				return Links.FirstOrDefault(l => l.Rel == SFRel.src);
			}
		}

		/// <summary>
		/// Returns links where lnk.IsEnclosureLinkType is true. See notes on that property 
		/// (LinkType has priority over Rel type).
		/// </summary>
		public IEnumerable<SFLink> Enclosures()
		{
			return Links.Where(l => l.IsEnclosureLinkType);
		}

		/// <summary>
		/// Returns all links within Links where LinkType.IsImage().
		/// </summary>
		public IEnumerable<SFLink> Images()
		{
			return Links.Where(l => l.MimeType.IsImage());
		}

		/// <summary>
		/// Gets first item of GetWebLinks (see notes there).
		/// </summary>
		public SFLink GetFirstWebLink_AlternateWins()
		{
			// OLD:
			//return _Links.NotNulle()
			//	? GetWebLinks().FirstOrDefault()
			//	: null;

			if (_Links.IsNulle())
				return null;
			else {
				SFLink wLnk = null;
				int cnt = _Links.Count;
				for (int i = 0; i < cnt; i++) { // (SFLink lnk in _Links) {
					SFLink lnk = _Links[i];
					if (lnk != null && lnk.Rel.IsNotEnclosureOrSelf() && lnk.MimeType.IsWebPageOrNone()) {


						if (wLnk == null)
							wLnk = lnk;


						else {


							// #1 --- REL IS THE SAME ---
							if (lnk.Rel == wLnk.Rel) {

								// If this link's mime is explicitly text_html and the other was not, it wins
								if (lnk.MimeType != wLnk.MimeType && lnk.MimeType == BasicMimeType.text_html) {
									wLnk = lnk;
								}
								else
									continue;
							}



							// #2 --- REL is NOT the same ---
							else {

								// a) IF last wLnk is alternate, and this one is not, last one wins, continue
								if (wLnk.Rel == SFRel.alternate)
									continue;

								// b) If this lnk IS alternate, then last wasn't (see above), so this one wins
								else if (lnk.Rel == SFRel.alternate)
									wLnk = lnk;

								// c) Neither rels are alternate, so look now compare mimes
								else if (lnk.MimeType != wLnk.MimeType && lnk.MimeType == BasicMimeType.text_html) {
									wLnk = lnk; // last could not be because above winnowed they aren't the same
								}
								else
									continue;
							}
						}
						if (wLnk.Rel == SFRel.alternate && wLnk.MimeType == BasicMimeType.text_html)
							return wLnk;
					}
				}
				return wLnk;
			}
		}

		/// <summary>
		/// Gets all links whose linkType IsNoneOrWebPage (<c>l.LinkType.IsNoneOrWebPage()</c>)
		/// and whose Rel is neither enclosure or self (<c>l.Rel.IsNotEnclosureOrSelf()</c>). 
		/// </summary>
		public IEnumerable<SFLink> GetWebLinks()
		{
			if (_Links.IsNulle())
				yield break;

			foreach (SFLink l in _Links) {
				if (l != null && l.Rel.IsNotEnclosureOrSelf() && l.MimeType.IsWebPageOrNone())
					yield return l;
			}
		}

		/// <summary>
		/// This prefers the first link that has a Rel type of either enclosure or src,
		/// AND whose LinkType is not IsNoneOrTextType, unless the predicate is sent in in which 
		/// case that is used instead. If none have both, LinkType 
		/// predominates then Rel, else null.
		/// </summary>
		public SFLink GetFirstEnclosure(Func<BasicMimeType, bool> linkType = null, bool returnFirstEnclosureIfNoLinkTypeMatches = false)
		{
			if (_Links.IsNulle())
				return null;

			SFLink encRel = null;

			if (linkType == null)
				linkType = l => l.IsTextOrNone() == false;

			foreach (SFLink l in _Links) {
				if (l != null) {
					if (linkType(l.MimeType))
						return l;

					if (l.Rel == SFRel.enclosure)
						encRel = l;
				}
			}

			return returnFirstEnclosureIfNoLinkTypeMatches
				? encRel
				: null;
		}

		/// <summary>
		/// All the links within this entry. Link, ContentSrc, Enclosures and Images
		/// all simply iterate through this collection to find their value(s).
		/// </summary>
		public List<SFLink> Links {
			get {
				if (_Links == null)
					_Links = new List<SFLink>();
				return _Links;
			}
			set { _Links = value; }
		}

		public List<SrcSetImg> SrcSetImages { get; set; }

		public void AddLink(SFLink link)
		{
			if (link.UrlN().NotNulle()) {
				if (ParentSettings != null && ParentSettings.AlterFeedLinks != null)
					link = ParentSettings.AlterFeedLinks.AlterLink(link);

				if (link.UrlN().NotNulle()) {
					if (Links.NotNulle()) {
						var lLink = Links.FirstN(lnk => lnk.Url.EqualsIgnoreCase(link.Url));
						if (lLink != null)
							link = _mergeLinks(link, lLink);
					}
					Links.Add(link);
				}
			}
		}

		SFLink _mergeLinks(SFLink primary, SFLink secondary)
		{
			var p = primary;
			var s = secondary;
			if (p == null)
				return s;
			if (s == null)
				return p;

			if (p.MimeType != s.MimeType) {
				BasicMimeType mtye = p.MimeType.GetMostQualifiedMimeType(s.MimeType);
				return p.MimeType == mtye ? p : s;
			}

			if (p.Rel != s.Rel)
				return p.Rel == SFRel.none ? s : p;

			if (p.DiscoveredLink != s.DiscoveredLink)
				return p.DiscoveredLink ? s : p;
			return p;
		}

		public bool WinnowLinks(params string[] urlsToWinnow)
		{
			if (_Links.CountN() > 0 && urlsToWinnow.NotNulle()) {

				urlsToWinnow = urlsToWinnow.Where(l => l.NotNulle()).ToArray();
				int arrCnt = urlsToWinnow.Length;

				if (arrCnt > 0) {
					int origLinksCnt = _Links.Count;

					if (arrCnt == 1) {
						string url1 = urlsToWinnow[0];
						_Links = _Links.Where(l => l.UrlN() != url1).ToList();
					}
					else
						_Links = _Links.Where(l => urlsToWinnow.Contains(l.Url) == false).ToList();

					return _Links.Count != origLinksCnt;
				}
			}
			return false;
		}

		#endregion

		#region SET LINKS

		/// <summary>
		/// Sets Links based on the inputed ATOM xml.
		/// </summary>
		public void SetLinksFromXmlAtomEntry(XElement xEntry)
		{
			foreach (var xLink in xEntry.Elements("link")) {
				SFLink link = SFLink.AtomXmlLinkToLink(xLink);
				AddLink(link);
			}

			var contentSrcElem = xEntry
				.Elements("content")
				.FirstOrDefault(e => e.Attribute("src") != null);
			//.Where(e => e.Attribute("src") != null)
			//var contentSrcElem = xEntry.Element("content", "src");

			if (contentSrcElem != null) {
				var cSrcLnk = SFLink.AtomXmlLinkToLink(contentSrcElem);
				if (cSrcLnk != null) {
					cSrcLnk.Rel = SFRel.src;
					AddLink(cSrcLnk);
				}
			}
		}

		/// <summary>
		/// Sets Links based on the inputed RSS xml.
		/// </summary>
		/// <param name="xEntry">RSS item.</param>
		public void SetLinksFromXmlRssItem(XElement xEntry)
		{
			if (xEntry != null) {
				XElement[] elems = xEntry.Elements().ToArray();

				foreach (var xLink in elems.Where(e => e.Name == "link" || e.Name == "enclosure")) {
					SFLink link = SFLink.RssXmlLinkOrEnclosureToLink(xLink);
					AddLink(link);
				}
				foreach (var xLink in elems.Where(e => e.Name == SimpleFeed.xname_Atom_Link)) {
					SFLink link = SFLink.AtomXmlLinkToLink(xLink);
					AddLink(link);
				}
				foreach (var xLink in elems.Where(e => e.Name == SimpleFeed.xname_yahoomrss_content)) {
					SFLink link = SFLink.YahooMRSSXmlMediaElementToLink(xLink);
					AddLink(link);
				}
			}
		}

		#endregion

		public override string ToString()
		{
			return $"{Title.SubstringMax(14)} (Links:{Links.CountN()} / Cats:{Categories.CountN()})";
		}
	}
}
