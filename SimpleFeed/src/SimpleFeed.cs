using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DotNetXtensions; //using DotNetXtensionsPrivate;
using DotNetXtensions.Text;

namespace SimpleFeedNS
{

	public class SimpleFeed : SFFeedEntry, IList<SFFeedEntry>
	{
		// future analysis: http://www.feedforall.com/itune-tutorial-tags.htm#category

		#region STATIC / HELPERS

		public static string LocalIdTag = "tag:id,2001:";

		public static readonly XNamespace ns_Atom = "http://www.w3.org/2005/Atom";
		public static readonly XNamespace ns_DC = "http://purl.org/dc/elements/1.1/";
		/// <summary>http://www.rssboard.org/rss-profile#namespace-elements-content-encoded</summary>
		public static readonly XNamespace ns_Content = "http://purl.org/rss/1.0/modules/content/";
		public static readonly XNamespace ns_iTunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";
		public static readonly XNamespace ns_rawvoice = "http://www.rawvoice.com/rawvoiceRssModule/";
		/// <summary>http://www.rssboard.org/media-rss#media-content http://www.rssboard.org/media-rss</summary>
		public static readonly XNamespace ns_yahoomrss = "http://search.yahoo.com/mrss/";

		public static readonly XName xname_Atom_Updated = ns_Atom + "updated";
		public static readonly XName xname_Atom_Published = ns_Atom + "published";
		public static readonly XName xname_DC_Date = ns_DC + "date";
		public static readonly XName xname_DC_Creator = ns_DC + "creator";
		public static readonly XName xname_Content_Encoded = ns_Content + "encoded";
		public static readonly XName xname_iTunes_Author = ns_iTunes + "author";

		/// <summary>
		/// This is not a normal 'category' that the user could fill in, it is a limited set list of 
		/// basically itunes set 'genres', that can be nested one deep. WE are going to mark
		/// these herein as having the domain: <see cref="ns_iTunes"/> domain + "#category",
		/// and will also combine any subcategories.
		/// </summary>
		public static readonly XName xname_iTunes_Category = ns_iTunes + "category";
		public static readonly XName xname_iTunes_Duration = ns_iTunes + "duration";
		public static readonly XName xname_iTunes_Image = ns_iTunes + "image";
		public static readonly XName xname_iTunes_Keywords = ns_iTunes + "keywords";
		public static readonly XName xname_iTunes_Subtitle = ns_iTunes + "subtitle";
		public static readonly XName xname_iTunes_Summary = ns_iTunes + "summary";
		public static readonly XName xname_Atom_Link = ns_Atom + "link";
		public static readonly XName xname_Atom_Category = ns_Atom + "category";
		public static readonly XName xname_rawvoice_metamark = ns_rawvoice + "metamark";
		public static readonly XName xname_yahoomrss_content = ns_yahoomrss + "content";

		// helpers
		public string[] m_AuthorRssSplitter = { " (" };

		#endregion

		#region FIELDS / PROPERTIES

		string m_IdBase;
		SFFeedSettings settings = new SFFeedSettings();

		/// <summary>
		/// Feed settings. Guaranteed not null (null set allowed,
		/// but will revert to a new settings instance).
		/// </summary>
		public SFFeedSettings Settings {
			get { return settings; }
			set { settings = value ?? new SFFeedSettings(); }
		}

		public XElement Document { get; private set; }
		public bool Error { get; private set; }

		/// <summary>
		/// This means the feed has the ATOM namespace, which can
		/// be true for RSS feeds (always true for ATOM feeds of course).
		/// </summary>
		public bool HasAtom { get; private set; }

		public bool IsRss { get; private set; }

		public bool IsAtom { get; private set; }

		public bool HasDublinCore { get; private set; }

		public bool HasRawVoice { get; private set; }

		public bool HasYahooMRss { get; private set; }

		public bool HasITunes { get; private set; }

		// we won't instantiate this resource if it hasn't been used
		private StringBuilder _messages;
		private StringBuilder messages {
			get {
				if (_messages == null) _messages = new StringBuilder(1024);
				return _messages;
			}
		}

		public string Messages {
			get {
				if (_messages == null || _messages.Length == 0) return null;
				return _messages.ToString();
			}
		}

		public string IdBase {
			get {
				return (m_IdBase != null) ? m_IdBase : LocalIdTag;
			}
			set { m_IdBase = value; }
		}
		/// <summary>
		/// The 'rights' (ATOM) or 'copyright' (RSS) value. 
		/// </summary>

		public SFText Rights { get; set; }

		//public SFLink Hub => Links.FirstN(l => l.Rel == SFRel.hub);

		//public SFLink Self => Links.FirstN(l => l.Rel == SFRel.self);

		public SFHub Hub => SFHub.GetHubFromLinksOrNull(Links);

		public SFFeedType SourceFeedType { get; private set; }

		/// <summary>
		/// This is only set when SetAllCategoriesDictionary is manually called.
		/// It will also not stay updated when feed changes are made.
		/// </summary>
		public Dictionary<string, SFCategory> AllCategoriesDictionary { get; set; }

		/// <summary>
		/// Gets all categories into a dictionary. Duplicates are simply overwritten with the last one of a given value.
		/// </summary>
		/// <param name="includeFeedCategories"></param>
		/// <param name="includeChildItemCategories"></param>
		public Dictionary<string, SFCategory> SetAllCategoriesDictionary(bool includeFeedCategories = true, bool includeChildItemCategories = true)
		{
			IEnumerable<SFCategory> cats = new SFCategory[0];

			if (includeFeedCategories && Categories.NotNulle())
				cats = cats.Concat(Categories.E());

			cats = includeChildItemCategories && Items.NotNulle()
				? Items.SelectMany(e => e.Categories)
				: new SFCategory[0];

			Dictionary<string, SFCategory> catsD = new Dictionary<string, SFCategory>();

			foreach (var cat in cats) {
				if (cat != null && cat.Value.NotNulle())
					catsD[cat.Value] = cat;
			}

			AllCategoriesDictionary = catsD;

			return catsD;
		}

		public IEnumerable<SFFeedEntry> ItemsAndFeed()
		{
			yield return this;

			if (Items.NotNulle()) {
				foreach (var item in Items)
					if (item != null)
						yield return item;
			}
		}

		public IEnumerable<SFFeedEntry> ItemsAndFeed(Func<SFFeedEntry, bool> predicate)
		{
			foreach (var item in ItemsAndFeed())
				if (predicate(item))
					yield return item;
		}

		#endregion

		#region ITEMS 
		// (guaranteed always non-null)

		static readonly SFFeedEntry[] _emptyItems = new SFFeedEntry[] { };
		IList<SFFeedEntry> m_Items = _emptyItems;

		public IList<SFFeedEntry> Items {
			get { return m_Items; }
			set {
				if (value == null)
					m_Items = _emptyItems;
				else {
					if (value is List<SFFeedEntry>)
						m_Items = value;
					else
						m_Items = value.ToList();
				}
			}
		}

		#endregion

		/*
		 <link rel="hub" href="http://etsy.superfeedr.com/"/>
		 <link rel="hub" href="http://pubsubhubbub.superfeedr.com/"/>
		 <atom10:link xmlns:atom10="http://www.w3.org/2005/Atom" rel="hub" href="http://pubsubhubbub.appspot.com/" />
			 */


		#region ====== MAIN ======


		// ====== START ======

		public SimpleFeed() { }
		public SimpleFeed(SFFeedSettings settings)
		{
			Settings = settings;
		}


		public IList<SFFeedEntry> Parse(byte[] data)
		{
			if (data == null || data.Length < 12) {
				Error = true;
				messages.AppendFormat("Input feed byte array was null or too small ({0}) to be valid.", data == null ? "NULL" : data.Length.ToString());
				return null;
			}

			try {
				bool bom = DataStartsWIthBOM(data);

				int substract = bom ? 3 : 0;

				// note: Parse of string also handles BOM (new feature!), but that requires a stinking full substring,
				// whereas here with bytes, if there is a BOM, we can just by-pass it up front when calling GetString
				string feedStr = Encoding.UTF8.GetString(
					data,
					bom ? substract : 0,
					data.Length - substract);

				return Parse(feedStr);
			}
			catch (Exception ex) {
				Error = true;
				messages.AppendLine("Exception was caught: \r\n" + ex.ToString());
				return null;
			}
		}

		public static bool DataStartsWIthBOM(byte[] data)
		{
			if (data == null || data.Length < 3)
				return false;

			// See also: Encoding.UTF8.GetPreamble()
			bool hasBom = (data[0] == 239 && data[1] == 187 && data[2] == 191); // Detect BOM, 239, 187, 191

			return hasBom;
		}

		/// <summary>
		/// You can get this or test it originally with: Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())[0];
		/// But no need, this way we have a constant. Equals, as bytes: `239, 187, 191`
		/// </summary>
		public const char BOMChar = (char)65279;

		public static bool FixBOMIfNeeded(ref string str)
		{
			if (str == null || str.Length < 3)
				return false;

			bool hasBom = str[0] == BOMChar;
			if (hasBom)
				str = str.Substring(1);

			return hasBom;
		}

		public IList<SFFeedEntry> Parse(string feed)
		{
			Error = false;
			if (feed.IsNulle()) {
				Error = true;
				return null;
			}

			try {
				bool hadBOM = FixBOMIfNeeded(ref feed);

				XElement doc = XElement.Parse(feed); // XLinqToXml.GetNamespaceIgnorantXElement(feed);  FREEZEs, takes like many minutes, when bad XML input. Does not immediately throw, need to diagnose why, but until then, use the slower other method

				if (this.settings.KeepXmlDocument)
					Document = doc;

				HasAtom = doc.Name.Namespace == ns_Atom || (doc.GetPrefixOfNamespace(ns_Atom).IsNulle() ? false : true);
				HasDublinCore = doc.GetPrefixOfNamespace(ns_DC).IsNulle() ? false : true;
				HasRawVoice = doc.GetPrefixOfNamespace(ns_rawvoice).IsNulle() ? false : true;
				HasYahooMRss = doc.GetPrefixOfNamespace(ns_yahoomrss).IsNulle() ? false : true;
				HasITunes = doc.GetPrefixOfNamespace(ns_iTunes).IsNulle() ? false : true;

				doc.ClearDefaultNamespace();
				if (!doc.Name.NamespaceName.IsNulle()) {
					Error = true;
					messages.AppendLine("Namespace did not clear.");
					return null;
				}

				Settings.ParentFeed = this;
				this.ParentSettings = Settings;

				int limit = Settings.NumberOfEntriesToParseLimit ?? -1;

				switch (doc.Name.ToString()) {
					case "rss": {
						IsRss = true;
						XElement channel = doc.Element("channel");
						if (channel != null) {
							SourceFeedType = SFFeedType.RSS;
							RssItemToFeedEntry(channel, this, true);

							Items = channel
								.Elements("item")
								.TakeIf(limit > 0, limit)
								.Select(xItem => RssItemToFeedEntry(
									xItem
								// HUH?! REMOVED! defaultDateTime: Published - TimeSpan.FromSeconds(i++) 
								// for the record, as absurd as it is why the default date would have been
								// whatever the default time (like UtcNow) plus a 1 second increment for each 
								// item, dumb, but at least the idea was you would get an auto-increment
								// and thus an ascending order of dates giving the items a natural sort
								// by datetime as their items actually occurred. Keep in mind ATOM has no such
								// problems because it's an invalid feed without at least I think an `updated` 
								// value. Anyways, this is way to messy, just needs to report 0 time when
								// no time exists, period
								))
								.ToList();
						}
					}
					break;
					case "feed": {
						IsAtom = true; //HasAtom = true; // do NOT set this to true, the true namespace MUST be available
						SourceFeedType = SFFeedType.ATOM;
						AtomEntryToFeedEntry(doc, this, true);
						Items = doc
							.Elements("entry")
							.TakeIf(limit > 0, limit)
							.Select(e => AtomEntryToFeedEntry(e)).ToList();
					}
					break;
					default: {
						Error = true;
						messages.AppendFormat("Document root element name ({0}) is not a valid feed type ('rss' or 'atom').", doc.Name);
						return null;
					}
				}

				if (Items.NotNulle()) {
					if (Settings.AlterEntryOnComplete != null) {
						var alter = Settings.AlterEntryOnComplete;
						Items = Items.Select(itm => alter(itm)).Where(itm => itm != null).ToList();
					}
					if (Settings.GenerateIdForEntriesWithNoId != null) {
						var generateId = Settings.GenerateIdForEntriesWithNoId;
						string idBase = IdBase;
						foreach (var itm in Items)
							if (itm.Id.IsNulle())
								itm.Id = generateId(itm, idBase);
					}
				}

				//SetHub();

				return Items;
			}
			catch (Exception ex) {
				Error = true;
				messages.AppendLine("Exception was caught: \r\n" + ex.ToString());
				return null;
			}
		}

		//void SetHub() { }

		// ====== CHIEF ENTRY PARSERS ======


		public SFFeedEntry RssItemToFeedEntry(
			XElement x,
			// REMOVED! -- DateTime? defaultDateTime = null, 
			SFFeedEntry e = null,
			bool isRssChannel = false)
		{
			if (e == null) // for when parent feed IS the entry
				e = new SFFeedEntry(Settings);

			// LINK
			e.SetLinksFromXmlRssItem(x);

			// DATETIME
			SetRssDates(x, e, isRssChannel);

			// ID
			e.Id = GetRssId(x); // e.GetFirstWebLink()?.Url); //e.GetFirstWebLink()?.Url.QQQ(e.GetFirstEnclosure()?.Url));

			SetItunes(x, e);

			//if (e.Categories.IsNulle())
			e.AddCategories(x); // -- before if itunes categories were setting, we would stop there... hmmm, what to do about duplicates?

			e.AddMeta(x);

			var cntStg = settings.ContentSettings;

			// TITLE
			if (e.Title.IsNulle()) { // itunes overrides
				string title = x.Element("title").ValueN().NullIfEmptyTrimmed();
				e.Title = ClearHtmlTagsIf(title, cntStg.TitleTag); //, true, settings.HtmlDecodeTextValues); // settings.ConvertHtmlContentInTitleTag
			}

			// AUTHOR
			if (e.Author.IsNulle()) // itunes overrides
				e.AuthorFull = GetAuthorFromXmlRssEntry(x);

			//! CONTENT / DESCRIPTION

			string description = x.Element("description").ValueN().NullIfEmptyTrimmed();
			string content_enc = x.Element(xname_Content_Encoded).ValueN().NullIfEmptyTrimmed();

			bool summaryIsFromItunes = e.Summary.NotNulle();
			bool contentFromPurlContentEncoded = content_enc != null;
			//bool contentFromDescriptionTag = false; // <-- not using this variable right now, but we might later...

			if (content_enc == null && description != null) {

				// See: https://stackoverflow.com/questions/7220670/difference-between-description-and-contentencoded-tags-in-rss2/54905457#54905457 (https://stackoverflow.com/a/54905457/264031)

				//contentFromDescriptionTag = true;
				content_enc = description;
				description = null;
			}

			if (description != null && !summaryIsFromItunes) {
				// itunes summary DEFINITELY must win for summary (!!), 
				// given the outlandish uncertainty of RSS on description tag
				e.Summary = ClearHtmlTagsIf(description, cntStg.SummaryTag); // settings.ConvertHtmlContentInSummaryTag
			}

			if (content_enc.NotNulle()) {
				if (contentFromPurlContentEncoded) // then we KNOW content field will be html content
					e.ContentFull.Type = "text/html";

				e.ContentFull.Value = ClearHtmlTagsIf(content_enc, cntStg.ContentTag); // settings.ConvertHtmlContentInContentTag
			}

			// NOTE: both content and summary variables should be UNCHANGED from the XML values
			// (so should NOT have been htmlTagCleared yet, no fear)
			SetImageUrlsFromContentImgTag(e, content_enc ?? description);

			if (settings.KeepXmlDocument)
				e.XmlEntry = x;

			ConvertUrlsToLinks(e);

			return e;
		}

		public SFFeedEntry AtomEntryToFeedEntry(XElement x, SFFeedEntry e = null, bool isAtomDocument = false)
		{
			// Unlike RSS, no 'DateTime? defaultDateTime' is needed, because we will always have the 'updated' element 
			if (e == null)
				e = new SFFeedEntry(Settings);

			e.Id = (string)x.Element("id");

			__SET_ENTRY_DATES(e, (string)x.Element("published"), (string)x.Element("updated"));

			SetItunes(x, e);

			e.AddCategories(x); // -- before if itunes categories were setting, we would stop there... hmmm, what to do about duplicates?

			e.AddMeta(x);

			if (e.Author.IsNulle()) // let itunes override
				e.AuthorFull = GetAuthorFromXmlAtomEntry(x);

			(string titleVal, string titleMType) = AtomTextAndTypeFromXElem(x, "title");
			(string summaryVal, string summaryMType) = AtomTextAndTypeFromXElem(x, "summary");
			(string contentVal, string contentMType) = AtomTextAndTypeFromXElem(x, "content");

			var cntStg = settings.ContentSettings;

			if (titleVal.NotNulle()) // let the ATOM 'title' overrule the itunes one
				e.TitleFull = AtomTextTypeToText(titleVal, titleMType, cntStg.TitleTag); // settings.ConvertHtmlContentInTitleTag);

			if (summaryVal.NotNulle()) // let the ATOM 'summary' overrule the itunes one
				e.SummaryFull = AtomTextTypeToText(summaryVal, summaryMType, cntStg.SummaryTag); // , settings.ConvertHtmlContentInSummaryTag);

			if (contentVal.NotNulle())
				e.ContentFull = AtomTextTypeToText(contentVal, contentMType, cntStg.ContentTag); // , settings.ConvertHtmlContentInContentTag);

			SetImageUrlsFromContentImgTag(e, contentVal ?? summaryVal);

			// LINKS
			e.SetLinksFromXmlAtomEntry(x);

			if (settings.KeepXmlDocument)
				e.XmlEntry = x;

			ConvertUrlsToLinks(e);

			return e;
		}


		void __SET_ENTRY_DATES(SFFeedEntry e, string publishedDateStr, string updatedDateStr)
		{
			if (publishedDateStr.IsNulle() && updatedDateStr.IsNulle()) {
				e.Updated = e.Published = DateTimeOffset.MinValue;
				return;
			}

			TimeZoneInfo tzi = settings.AlterUTCDatesToThisTimezone;

			if (publishedDateStr == null)
				publishedDateStr = updatedDateStr;
			else if (updatedDateStr == null)
				updatedDateStr = publishedDateStr;

			if (publishedDateStr == null) // then both are null by now, return
				return;

			bool areTheSame = publishedDateStr == updatedDateStr;

			(bool success, bool hadOffset, DateTimeOffset result) = XDateTimes.ParseDateTimeWithOffsetInfo(
				dateStr: updatedDateStr,
				localTimeZone: tzi,
				treatNoOffsetAsLocalTime: true, // !!
				handleObsoleteUSTimeZones: true // !!
			);

			e.Updated = result;

			if (areTheSame) {
				e.Published = e.Updated;
			}
			else {
				(success, hadOffset, result) = XDateTimes.ParseDateTimeWithOffsetInfo(
					dateStr: publishedDateStr,
					localTimeZone: tzi,
					treatNoOffsetAsLocalTime: true, // !!
					handleObsoleteUSTimeZones: true // !!
				);

				e.Published = result;
			}
		}

		ExtraTextFuncs ex = new ExtraTextFuncs();

		public void ConvertUrlsToLinks(SFFeedEntry e)
		{
			if (e == null) return;

			bool convertCats = settings.ConvertCategoryUrlsToLinks;
			bool convertAllUrls = settings.ConvertContentUrlsToLinks;
			if (!convertCats && !convertAllUrls)
				return;

			if (convertAllUrls) // IF ConvertContentUrlsToLinks, then always do ConvertCategoryUrlsToLinks as well
				convertCats = true;

			if (convertCats) {

				string fields = e.Keywords; // NOTE! this property concats all categories ...

				if (convertAllUrls) {
					fields =
						(new string[] { fields, e.Title, e.SubTitle, e.Series, e.Summary, e.Content })
						.Where(v => v.NotNulle())
						.JoinToString(" ;!; ");
				}

				string[] links = ex.GetLinks(fields);

				if (links.NotNulle()) {

					var srcsetsDict = e.SrcSet?.SrcSetImageUrlsDict;
					bool hasSrcSets = srcsetsDict.NotNulle();

					// for perf, let's only make this dictionary if it seems worth it
					Dictionary<string, bool> tempLinksDict = e.Links.Count > 0 && links.Length > 2
						? e.Links.ToDictionaryIgnoreDuplicateKeys(kv => kv.Url, kv => false)
						: null;

					for (int i = 0; i < links.Length; i++) {

						string url = links[i];

						if (hasSrcSets) {
							if (srcsetsDict.ContainsKey(url))
								continue;
						}

						if (tempLinksDict != null) {
							if (tempLinksDict.ContainsKey(url))
								continue;
						}
						else if (e.Links.Any(lk => lk.Url == url))
							continue;

						var lnk = new SFLink(url) { DiscoveredLink = true };
						if (lnk != null && lnk.IsValid)
							e.AddLink(lnk);
					}
				}
			}
		}

		public void SetImageUrlsFromContentImgTag(SFFeedEntry e, string content)
		{
			if (e == null || !settings.GetImageUrlsFromContentImgTag || content.IsNulle())
				return;

			var srcset = ex.GetFirstImageTagFromHtmlText(
				content,
				maxStartIndexOfImgTag: 2048);
				//requiresIsWithinAnchorTag: true);

			var src = srcset?.Src;

			if (srcset != null && src != null) {

				if (!e.Images().Any(img => img.Url.EqualsIgnoreCase(src.Url))) {
					//SFLink.GetMimeTypeFromTypeOrExtension(null,
					e.AddLink(new SFLink(src.Url, mimeType: BasicMimeType.image));
				}

				if (srcset.SrcSets.NotNulle()) {
					e.SrcSet = srcset;
				}
			}
		}

		#endregion



		#region RSS

		public string GetAuthorFromItunesOrDC(XElement item, bool getFeedAuthorIfNull = true)
		{
			string name = null;
			if (HasITunes) {
				name = (string)item.Element(xname_iTunes_Author);
				if (!name.IsNulle())
					return name;
			}
			if (HasDublinCore) {
				name = (string)item.Element(xname_DC_Creator);
				if (!name.IsNulle())
					return name;
			}
			if (getFeedAuthorIfNull && !Author.IsNulle())
				return Author;
			return null;
		}

		public SFAuthorFull GetAuthorFromXmlRssEntry(XElement item)
		{
			SFAuthorFull authorFull = new SFAuthorFull();

			authorFull.Name = GetAuthorFromItunesOrDC(item);

			string authorValue = (string)item.Element("author");

			if (!authorValue.IsNulle()) {
				if (authorValue[authorValue.Length - 1] == ')') {
					string[] split = authorValue.Split(m_AuthorRssSplitter, StringSplitOptions.RemoveEmptyEntries);
					if (authorFull.Name == null &&
						split == null ||
						split.Length != 2 ||
						split[0].IsNulle() ||
						split[1].IsNulle()) {
						// if all these are true it means: 
						// a) name is still null (dc:creator has priority otherwise), and
						// b) split is invalid, so we set the entire Name value to <author> value, forget split
						authorFull.Name = authorValue;
					}
					else {
						if (split[0].IndexOf('@') > 0) // keep to highly performant rudimentary email check
							authorFull.Email = split[0].Trim();
						if (AuthorFull.Name == null)
							authorFull.Name = split[1].Substring(0, split[1].Length - 1);
					}
				}
			}

			if (authorFull.Name != null)
				return authorFull;

			if (authorFull.Name == null && authorFull.Email == null) {
				// only try set from document author if no values thus far
				authorFull = this.AuthorFull.Copy();
			}

			// Lastly, the split above could have set an email, but no name.
			// problem is RSS totally can have author value as just the email, by the spec actually
			// So we have to duplicate Email to Name if is a Email value but no Name value:

			if (authorFull.Name == null && authorFull.Email != null)
				authorFull.Name = authorFull.Email;

			return authorFull;
		}

		void SetRssDates(
			XElement item,
			SFFeedEntry e,
			// REMOVED! -- DateTime? defaultDateTime = null, 
			bool isRssChannel = false)
		{
			// DATE (PUBLISHED / UPDATED)
			string publishedDateStr = null;
			string updatedDateStr = (string)item.Element(xname_Atom_Updated);

			if (isRssChannel) {
				if (updatedDateStr.IsNulle())
					updatedDateStr = (string)item.Element("lastBuildDate");
			}

			if (HasAtom)
				publishedDateStr = (string)item.Element(xname_Atom_Published);

			if (HasDublinCore && publishedDateStr == null)
				publishedDateStr = (string)item.Element(xname_DC_Date);

			if (publishedDateStr == null)
				publishedDateStr = (string)item.Element("pubDate");

			__SET_ENTRY_DATES(e, publishedDateStr, updatedDateStr);
		}

		string GetRssId(XElement rss)
		{
			// string defLinkForId) no more default link, let the caller handle these if wanted with 
			// Settings.GenerateIdForEntriesWithNoId
			string id = (string)rss.Element("guid");

			if (id.IsNulle())
				return null;

			return GetRssId(id, IdBase);
		}

		public static string GetRssId(string id, string idBase)
		{
			if (id.IsNulle())
				return null;

			if (idBase.IsNulle())
				return id;

			bool needsTag = true;
			if (id.Length > 5) {
				if (id[0] == 'h' && (id.StartsWith("http") && id[4] == ':' || (id[4] == 's' && id[5] == ':')))
					needsTag = false;
				else if (id[0] == 't' && id.StartsWith("tag:"))
					needsTag = false;
				else if (id[0] == 'u' && id.StartsWith("urn:"))
					needsTag = false;
				else
					needsTag = true;
			}
			if (!needsTag)
				return id;

			id = idBase + id; // Uri.EscapeUriString(id);
			return id;
		}

		#endregion

		public static bool ClearHtmlTagsWithOlderSimpleClearXmlTags = false;

		public string ClearHtmlTagsIf(
			string value,
			SFContentConversionType convType,
			bool? htmlDecode = null)
		{		
			return ClearHtmlTagsIfStatic(
				convType,
				value,
				htmlDecode: htmlDecode, // settings.HtmlDecodeTextValues,
				htmlToMDConverter: _htmlToMDConverter);
		}

		OnePassHtmlToMarkdown _htmlToMDConverter = new OnePassHtmlToMarkdown();

		public static string ClearHtmlTagsIfStatic(
			SFContentConversionType convType,
			string value,
			bool? htmlDecode = null,
			OnePassHtmlToMarkdown htmlToMDConverter = null)
		{
			// but note, at the moment, we are NEVER sending in this value, so effectively we're 
			// always html-decoding right now
			bool _htmlDecode = htmlDecode ?? true;

			if (value.IsNulle())
				return null;

			switch (convType) {
				case SFContentConversionType.HtmlToMarkdown:
				case SFContentConversionType.SimpleHtmlTagStrip:

					//if (!XmlTextFuncs.StringContainsAnyXmlTagsQuick(value))
					//	return value.NullIfEmptyTrimmed();

					value = (htmlToMDConverter ?? new OnePassHtmlToMarkdown())
						.ConvertHtmlToMD(
							value,
							onlyCleanHtmlTags: convType == SFContentConversionType.SimpleHtmlTagStrip, //!convertWithMarkdown,
							htmlDecode: _htmlDecode,
							checkAndReturnIfNoXmlTags: true);
					break;

				case SFContentConversionType.None:
					if (_htmlDecode)
						value = System.Net.WebUtility.HtmlDecode(value);
					break;

				case SFContentConversionType.OldHtmlTagStrip:

					value = _htmlDecode
						? XmlTextFuncs.ClearXmlTagsAndHtmlDecode(value, trim: true)
						: XmlTextFuncs.ClearXmlTags(value, trim: true);
					break;
			}

			return value.NullIfEmptyTrimmed() ?? "";
		}

		public SFFeedEntry SetItunes(XElement entry, SFFeedEntry e)
		{
			if (HasITunes) {

				e.AddCategoriesCommaSeparatedString(
					entry.Element(xname_iTunes_Keywords).ValueN());

				e.Author = entry.Element(xname_iTunes_Author).ValueN().TrimIfNeeded();
				e.SubTitle = entry.Element(xname_iTunes_Subtitle).ValueN().TrimIfNeeded();
				e.Summary = entry.Element(xname_iTunes_Summary).ValueN().TrimIfNeeded();

				//bool htmlDecode = settings.HtmlDecodeTextValues;

				// REMOVING THIS: We never were setting `e.Content` in this itunes stuff anyways! // e.Content = ClearHtmlTagsIf(settings.ClearXmlContent_ContentTag, e.Content); //, true, htmlDecode);
				e.Summary = ClearHtmlTagsIf(e.Summary, SFContentConversionType.None); //, true, htmlDecode); // settings.ConvertHtmlContentInSummaryTag
				e.SubTitle = ClearHtmlTagsIf(e.SubTitle, SFContentConversionType.None); //, true, htmlDecode); // settings.ConvertHtmlContentInTitleTag

				//int? duration = entry.Element(xname_iTunes_Duration).ToIntN();

				XElement itunesImg = entry.Element(xname_iTunes_Image);
				if (itunesImg != null) {
					string imageUrl = itunesImg.AttributeN("href").ValueN().TrimIfNeeded();
					if (imageUrl.NotNulle()) {
						var imgLink = new SFLink(imageUrl, mimeType: BasicMimeType.image) { Rel = SFRel.enclosure };
						if (imgLink.IsValid)
							e.AddLink(imgLink);
					}
				}
			}
			return e;
		}

		#region ATOM

		static (string value, string type) AtomTextAndTypeFromXElem(XElement elem, string elemName)
		{
			XElement e = elem.Element(elemName);
			if (e != null) {
				string val = e.Value;
				if (val.NotNulle()) {
					string typ = e.Attribute("type").ValueN().NullIfEmptyTrimmed();
					return (val, typ);
				}
			}
			return (null, null);
		}

		SFText AtomTextTypeToText(string val, string mtype, SFContentConversionType convType) 
			// bool clearHtmlTags = false)
		{
			if (val.NotNulle()) {
				val = ClearHtmlTagsIf(val, convType); //, true, settings.HtmlDecodeTextValues);

				return new SFText() { Value = val, Type = mtype };
			}
			return null;
		}

		public SFAuthorFull GetAuthorFromXmlAtomEntry(XElement entry)
		{
			// #1) Check if has ATOM author
			var xAuthor = entry.Element("author");
			if (xAuthor != null) {
				SFAuthorFull authorFull = new SFAuthorFull() {
					Name = (string)xAuthor.Element("name"),
					Uri = (string)xAuthor.Element("uri"),
					Email = (string)xAuthor.Element("email"),
				};
				return authorFull; // spec says if has author elem, name child is required, we don't error check that, assume is correct (per SimpleFeed's guidlines)
			}

			string name = GetAuthorFromItunesOrDC(entry);
			if (!name.IsNulle())
				return new SFAuthorFull() { Name = name };

			return this.AuthorFull.Copy();
		}

		#endregion

		#region IList / ICollection / IEnumerable

		public int IndexOf(SFFeedEntry item)
		{
			return Items.IndexOf(item);
		}

		public void Insert(int index, SFFeedEntry item)
		{
			Items.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			Items.RemoveAt(index);
		}

		public SFFeedEntry this[int index] {
			get { return Items[index]; }
			set { Items[index] = value; }
		}

		public void Add(SFFeedEntry item)
		{
			Items.Add(item);
		}

		public void Clear()
		{
			Items.Clear();
		}

		public bool Contains(SFFeedEntry item)
		{
			return Items.Contains(item);
		}

		public void CopyTo(SFFeedEntry[] array, int arrayIndex)
		{
			Items.CopyTo(array, arrayIndex);
		}

		public int Count {
			get { return Items.Count; }
		}

		public bool IsReadOnly {
			get { return Items.IsReadOnly; }
		}

		public bool Remove(SFFeedEntry item)
		{
			return Items.Remove(item);
		}

		public IEnumerator<SFFeedEntry> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return (Items as IEnumerable).GetEnumerator();
		}

		#endregion

	}
}

#region --- Old 'ClearHtmlTagsIfStatic' ---

//public static string ClearHtmlTagsIfStatic(
//	SFContentConversionType convType, // bool conditional,
//	string value,
//	bool htmlDecode,
//	bool convertWithMarkdown = true,
//	bool basicXmlTagStrip = false,
//	OnePassHtmlToMarkdown htmlToMDConverter = null)
//{
//	bool trim = true;

//	if (value.IsNulle())
//		return null;

//	if (trim && !conditional && !htmlDecode) // if (conditional && htmlDecode), then trim is already ran, so would be double hit
//		value = value.TrimIfNeeded();

//	if (!conditional)
//		return value;

//	if (basicXmlTagStrip) {
//		value = htmlDecode
//			? XmlTextFuncs.ClearXmlTagsAndHtmlDecode(value, trim)
//			: XmlTextFuncs.ClearXmlTags(value, trim);
//	}
//	else {
//		//if (!XmlTextFuncs.StringContainsAnyXmlTagsQuick(value))
//		//	return value.NullIfEmptyTrimmed();

//		value = (htmlToMDConverter ?? new OnePassHtmlToMarkdown())
//			.ConvertHtmlToMD(
//				value,
//				onlyCleanHtmlTags: !convertWithMarkdown,
//				htmlDecode: htmlDecode,
//				checkAndReturnIfNoXmlTags: true);
//	}
//	return value;
//}


// ---

//SFText AtomTextTypeToText(XElement elem, string elemName, bool clearHtmlTags = false)
//{
//	(string val, string mtype) = AtomTextAndTypeFromXElem(elem, elemName);

//	if (val.NotNulle()) {
//		val = ClearHtmlTagsIf(clearHtmlTags, val); //, true, settings.HtmlDecodeTextValues);

//		return new SFText() { Value = val, Type = mtype };
//	}
//	return null;
//}

#endregion
