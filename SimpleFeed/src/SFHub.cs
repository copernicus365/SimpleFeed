using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DotNetXtensions; //using DotNetXtensionsPrivate;

namespace SimpleFeedNS
{
	public class SFHub
	{
		public SFHub() { }

		public SFHub(SFLink hubLink, SFLink selfLink = null)
		{
			if (hubLink.UrlN().IsNulle())
				throw new ArgumentNullException();

			HubLink = hubLink;
			if (selfLink.UrlN().NotNulle())
				SelfLink = selfLink;
		}

		public static SFHub GetHubFromLinksOrNull(IEnumerable<SFLink> links)
		{
			if (links == null)
				return null;

			var hubLink = links.FirstN(l => l.Rel == SFRel.hub && l.Url.NotNulle());
			if (hubLink == null)
				return null;

			var selfLink = links.FirstN(l => l.Rel == SFRel.self && l.Url.NotNulle());

			return new SFHub(hubLink, selfLink);
		}

		public string HubUrl {
			get { return _hubUrl ?? HubLink?.Url; }
			set { _hubUrl = value; }
		}
		string _hubUrl;

		public string SelfUrl {
			get { return _selfUrl ?? SelfLink?.Url; }
			set { _selfUrl = value; }
		}
		string _selfUrl;

		public BasicMimeType? SelfType {
			get { return _selfType ?? SelfLink?.MimeType; }
			set { _selfType = value; }
		}
		BasicMimeType? _selfType;

		public SFLink HubLink { get; set; }

		public SFLink SelfLink { get; set; }
	}
}
