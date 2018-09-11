using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DotNetXtensions; //using DotNetXtensionsPrivate;

namespace SimpleFeedNS
{
	public class SFAuthorFull
	{
		public string Name { get; set; }
		public string Uri { get; set; }
		public string Email { get; set; }

		public SFAuthorFull Copy()
		{
			return new SFAuthorFull() {
				Name = this.Name,
				Uri = this.Uri,
				Email = this.Email
			};
		}

		public override string ToString()
		{
			return Name + " (" + Email + ")";
		}

	}
}
