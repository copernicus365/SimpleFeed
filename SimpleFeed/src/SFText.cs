using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimpleFeedNS
{
	public class SFText
	{
		public string Type { get; set; }
		public string Value { get; set; }

		public override string ToString()
		{
			return "[" + this.Type + "] " + Value;
		}
	}
}
