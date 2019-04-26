﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DotNetXtensions; //using DotNetXtensionsPrivate;

namespace SimpleFeedNS
{
	public class SFLink : SFMediaDetails
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url"></param>
		/// <param name="mimeTypeStr">
		/// The mime type. If set, is higher priority than <paramref name="mimeType"/> enum
		/// (as that does not have all enum values).</param>
		/// <param name="rel"></param>
		/// <param name="title"></param>
		/// <param name="length"></param>
		/// <param name="isRssEncl"></param>
		/// <param name="mimeType">A default mime-type. <paramref name="mimeTypeStr"/> if set will override this value,
		/// but this will still be set as a default type if the string value doesn't have a matching enum value.</param>
		public SFLink(
			string url,
			string mimeTypeStr = null,
			string rel = null,
			string title = null,
			int length = 0,
			bool isRssEncl = false,
			BasicMimeType mimeType = BasicMimeType.none)
		{
			url = url.NullIfEmptyTrimmed();
			if (url.IsNulle())
				return;

			IsValid = false;

			if (!ExtraTextFuncs.IsWebLink(url, checkForWww: true))
				return;

			if (url[0] == 'w') // above check determines if [0] == 'w', then this == 'www.'
				url = "http://" + url;

			if (url.IndexOf(' ') > 0) // is pry a quicker check than always running the replace...
				url = url.Replace(" ", "%20"); // some links have spaces in them that invalidate the IsWellFormedOriginalString check below

			try {
				// note: this WILL throw an Ex for some things.
				// however, it also accepts ALL kinds of JUNK that 
				// obviously is not a valid Uri, I don't get it.
				// So we're still doing the IsWellFormedOriginalString
				// check, but that is largely useless in some of my tests

				Uri uri = new Uri(url);
				if (uri == null || !uri.IsWellFormedOriginalString())
					return;
				this.Uri = uri;
				Url = uri.AbsoluteUri; // do NOT do .ToString(), that is the 'canonical' form which ruins escapes, like replacing ' ' (space) for %20! dumb
			}
			catch {
				return;
			}

			// do NOT place this within the try/catch. there should be NO errors in this call
			// so if it does throw we need to know the bug
			UriPathInfo pathInfo = new UriPathInfo(this.Uri.AbsolutePath);

			Ext = pathInfo.Extension;
			//Ext = UriPathInfo.GetExtFromUrl(_uri.AbsolutePath); 
			// BasicMimeTypesX.GetExtFromUrl(_uri.AbsolutePath); //AbsolutePath removes any query string ending

			IsValid = true;

			Title = title.TrimIfNeeded().QQQ(null);
			Length = length.Min(0);

			// --- set Rel ---
			if (isRssEncl)
				Rel = SFRel.enclosure;
			else {
				Rel = SFRel.none;

				rel = rel.TrimIfNeeded();

				if (rel.NotNulle()) {
					if (SFRelTypesX.RelsDictionary.TryGetValue(rel, out SFRel _rl))
						Rel = _rl;
					else
						RelOther = rel;
				}
			}

			MimeType = GetMimeTypeFromTypeOrExtension(mimeTypeStr, Ext)
				.GetMostQualifiedMimeType(mimeType);

			// need to move this somewhere else at some time
			if (MimeType == BasicMimeType.none || (Ext.IsNulle() && mimeTypeStr.IsNulle())) { // hmmm, i guess let this override if these conditions met 
				string linkHost = Uri.Host;
				if (linkHost.CountN() > 5) { // && link.LinkType.IsAudio() == false) {
					if (linkHost.Contains("vimeo")) {
						MimeType = BasicMimeType.video_vimeo;
					}
					else if (linkHost.Contains("youtu")) {
						MimeType = BasicMimeType.video_youtube;
					}
				}
			}
		}

		public bool IsValid { get; }

		public static BasicMimeType GetMimeTypeFromTypeOrExtension(string inputMimeType, string extension)
		{
			inputMimeType = inputMimeType.NullIfEmptyTrimmed();
			BasicMimeType mime = BasicMimeType.none;

			if (inputMimeType.NotNulle()) {
				//TypeOriginal = inputMimeType;
				mime = BasicMimeType.none.GetMimeTypeFromString(inputMimeType, allowGenericMatchOnNotFound: true);
				//.LinkMimeTypesDictionary.V(typ, BasicMimeType.none);
			}
			if (extension.NotNulle() && mime.HasNoSubtypeOrNone()) {
				BasicMimeType _extMime = BasicMimeType.none.GetMimeTypeFromFileExtension(extension);
				mime = mime.GetMostQualifiedMimeType(_extMime);
			}

			return mime;
		}

		public string Url { get; }

		public Uri Uri { get; }

		public bool DiscoveredLink { get; set; }

		public string Ext { get; set; }

		/// <summary>
		/// The Type of link. This value is overwritten whenever LinkType is set.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// The original Link Type, to be set if the original type didn't 
		/// coorespond to one of our strongly typed types.
		/// </summary>
		public string TypeOriginal { get; set; }

		public int Height { get; set; }

		public int Width { get; set; }

		/// <summary>
		/// Returns the string version of the Rel enum, unless it is set to none,
		/// in which case RelOther is returns if not null, else null.
		/// </summary>
		public string RelString
			=> Rel == SFRel.none ? RelOther : Rel.ToStringFast();

		public string RelOther { get; set; }

		public int Length { get; set; }

		public SFRel Rel { get; set; } = SFRel.none;

		/// <summary>
		/// The MIME type of this link, cooresponding to a (limited) strongly coded
		/// version of RSS's Type value. Setting this resets the Type value.
		/// </summary>
		public BasicMimeType MimeType {
			get { return _mimeType; }
			set {
				_mimeType = value;
				this.Type = value.ToMimeTypeString();
			}
		}
		BasicMimeType _mimeType = BasicMimeType.none;

		/// <summary>
		/// This effectively allows the LinkType (BasicMimeType) value to 
		/// override in importance whether this link is considered an enclosure or not.
		/// If LinkType == BasicMimeType.none, returns if Rel == SFRel.enclosure,
		/// otherwise LinkType setting dominates, returning if LinkType != BasicMimeType.text_html.
		/// </summary>
		public bool IsEnclosureLinkType {
			get {
				return MimeType == BasicMimeType.none
					? Rel == SFRel.enclosure
					: MimeType != BasicMimeType.text_html;
			}
		}


		/// <summary>
		/// Converts the purportedly XML link tag to a Link. One can also
		/// send in a content element, and null will be returned if src attribute is not there
		/// or empty.
		/// RETURNS NULL if Url was empty or null.
		/// </summary>
		/// <param name="xLink">Supposed to be an XML tag representing an ATOM Link.</param>
		public static SFLink AtomXmlLinkToLink(XElement xLink)
		{
			if (xLink == null)
				return null;

			string url = null;
			if (xLink.Name.LocalName == "link")
				url = ((string)xLink.Attribute("href"));
			else if (xLink.Name.LocalName == "content")
				url = ((string)xLink.Attribute("src"));
			else
				return null;

			return GetLink(xLink, url: url, isAtom: true);
		}

		public static SFLink YahooMRSSXmlMediaElementToLink(XElement xLink)
		{
			if (xLink == null)
				return null;

			string _type = xLink.Attribute("type").ValueN().TrimN();
			if (_type.IsNulle()) {
				_type = xLink.Attribute("medium").ValueN().TrimN();
				//if(_type.NotNulle() && !_type.Contains("/")) // not needed, our type parser will handle this (I think)
				//	_type = _type + "/null";
			}

			string url = xLink.Attribute("url").ValueN().NullIfEmptyTrimmed();
			if (url.IsNulle())
				return null;

			var lnk = new SFLink(
				url: url,
				mimeTypeStr: _type,
				length: xLink.Attribute("fileSize").ToInt(
					xLink.Attribute("duration").ToInt(0).Min(0)), // duration is the number of seconds the media object plays. It is an optional attribute.
				rel: null,
				title: null,
				isRssEncl: false,
				mimeType: BasicMimeType.audio // though the "Media RSS Spec" can be for pics or vids too, I doubt it's used much ever for that, presumption of audio is good one (I think)
				);

			if (!lnk.IsValid)
				return null;

			lnk.Height = xLink.Attribute("height").ToInt().Min(0);
			lnk.Width = xLink.Attribute("width").ToInt().Min(0);

			return lnk;
			/*
		<media:content
  url: url="http://www.foo.com/movie.mov"
  length: fileSize="12216320"
  type: type="video/quicktime"
  duration="185"
  height="200"
  width="300"

  // nada
  medium="video"
  isDefault="true"
  expression="full"
  bitrate="128"
  framerate="25"
  samplingrate="44.1"
  channels="2"
  lang="en" />	 
			 */
		}

		public static SFLink RssXmlLinkOrEnclosureToLink(XElement xLink)
		{
			if (xLink == null)
				return null;

			bool isLinkNotEnclosure = xLink.Name == "link";
			if (isLinkNotEnclosure == false) {
				if (xLink.Name != "enclosure")
					return null;
			}
			string url = isLinkNotEnclosure
				? xLink.Value
				: xLink.Attribute("url").ValueN();

			return GetLink(xLink, url, isAtom: false, isRssEncl: !isLinkNotEnclosure);
		}

		public static SFLink GetLink(XElement xLink, string url, bool isAtom, bool isRssEncl = false)
		{
			if (xLink == null || url.IsNulle())
				return null;

			url = url.NullIfEmptyTrimmed();
			if (url == null)
				return null;

			return new SFLink(url,
				xLink.Attribute("type").ValueN(),
				xLink.Attribute("rel").ValueN(),
				xLink.Attribute("title").ValueN(),
				xLink.Attribute("length").ToInt(0),
				isRssEncl);
		}

		public bool IsImage()
		{
			return MimeType.IsImage();
		}

		public SFLink Copy()
		{
			return (SFLink)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}",
				Rel,
				MimeType,
				Url ?? "-");
		}
	}

	public class SFMediaDetails
	{
		/// <summary>
		/// Alternative text.
		/// </summary>
		public string AltText { get; set; }

		/// <summary>
		/// Caption.
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Fuller description of this resource.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// A URL related to this image, either the source page from which it came,
		/// or just somehow related in some way to this media / image link.
		/// </summary>
		public string RelUrl { get; set; }

		/// <summary>
		/// Title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Returns the first non-null value of 
		/// the many text description values in this SFMediaDetail, in order: 
		/// Title, AltText, Caption , Description.
		/// </summary>
		public string GetFirstAltText()
		{
			if (Title.NotNulle())
				return Title;
			if (AltText.NotNulle())
				return AltText;
			if (Caption.NotNulle())
				return Caption;
			if (Description.NotNulle())
				return Description;
			return null;
		}

	}
}
