using System.Collections.Generic;

using DotNetXtensions;

namespace SimpleFeedNS
{
	public static class SFRelTypesX
	{
		public static Dictionary<string, SFRel> RelsDictionary = new Dictionary<string, SFRel>()
			.AddN("alternate", SFRel.alternate)
			.AddN("enclosure", SFRel.enclosure)
			.AddN("related", SFRel.related)

			// -- self types --
			.AddN("self", SFRel.self)
			.AddN("alternate home", SFRel.self)
			.AddN("home", SFRel.self)

			.AddN("hub", SFRel.hub)
			.AddN("via", SFRel.via)
			.AddN("src", SFRel.src);

		public static Dictionary<SFRel, string> RelsDictionaryReverse = RelsDictionary.ReverseDictionary(
			ignore: new string[] { "alternate home", "home" });

	}
}
