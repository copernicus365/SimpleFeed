using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotNetXtensions;

namespace SimpleFeedNS
{
	public static class XSimpleFeed
	{

		#region FeedEntry Related

		public static IEnumerable<SFFeedEntry> GetIncompleteEntries(
			this IEnumerable<SFFeedEntry> entries,
			bool updatedOrPublishedMustBeSet = true,
			bool mustHaveAuthor = false)
		{
			return entries.Where(e => !e.IsComplete(updatedOrPublishedMustBeSet, mustHaveAuthor));
		}

		public static bool IsComplete(
			this SFFeedEntry entry,
			bool updatedOrPublishedMustBeSet = true,
			bool mustHaveAuthor = false)
		{
			if (entry == null)
				return false;

			if (entry.Id.IsNulle() ||
				entry.Title.IsNulle())
				return false;

			if (updatedOrPublishedMustBeSet) {
				if(entry.Published == DateTimeOffset.MinValue && entry.Updated == DateTimeOffset.MinValue)
					return false;
			}

			if (mustHaveAuthor && entry.Author.IsNulle())
				return false;

			return true;
		}
		
		[DebuggerStepThrough]
		public static string UrlN(this SFLink link)
		{
			return link == null ? null : link.Url;
		}

		#endregion

		public static bool IsNotEnclosureOrSelf(this SFRel rel)
		{
			return rel != SFRel.enclosure && rel != SFRel.self;
		}

		public static string ToStringFast(this SFRel rel)
		{
			return BasicMimeTypesSFX.RelsDictionaryReverse[rel];
		}
	}
}

//#region --- REMOVED Lenient-DateTime parsing, now calling DNX ---

//public static DateTime ParseDateTimeLenientDefault(string date, DateTime? defaultTime = null)
//{
//	DateTime result;
//	if (date != null && TryParseDateTimeLenient(date, out result))
//		return result;
//	else
//		return defaultTime == null ? DateTime.MinValue : (DateTime)defaultTime;
//}

//public static bool TryParseDateTimeLenient(string date, out DateTime result)
//{
//	if (date == null) {
//		result = DateTime.MinValue;
//		return false;
//	}

//	if (DateTime.TryParse(date, out result)) {
//		return true;
//	}
//	int len = date.Length;
//	if (len > 8) {
//		if (date[3] == ',' && date[4] == ' ') {
//			date = date.Substring(5, len - 5);
//			len -= 5;
//		}

//		if (date[len - 4] == ' ' && date[len - 1] == 'T') {
//			string tz = date.Substring(len - 3, 3);
//			date = date.Substring(0, len - 4);
//			if (DateTime.TryParse(date, out result)) {
//				double offset;
//				if (USTimeZoneDateTimeOffsets.TryGetValue(tz, out offset))
//					result = result.Add(TimeSpan.FromHours(offset));
//				return true;
//			}
//		}
//	}

//	result = DateTime.MinValue;
//	return false;
//}

//// http://www.timeanddate.com/library/abbreviations/timezones/
//static readonly Dictionary<string, double> USTimeZoneDateTimeOffsets = new Dictionary<string, double>() {
//	{ "CDT", -5.0 }, 
//	{ "CST", -6.0 }, 
//	{ "EDT", -4.0 }, 
//	{ "EST", -5.0 }, 
//	{ "MDT", -6.0 }, 
//	{ "MST", -7.0 }, 
//	{ "PDT", -7.0 }, 
//	{ "PST", -8.0 }, 
//	{ "GMT", -0.0 }, 
//	{ "UT", -0.0 }
//};

//#endregion
