
namespace SimpleFeedNS
{

	/// <summary>
	/// Rel types. In RSS, it is really simple, the only values
	/// will be none, alternate, and self, . As for ATOM it
	/// "contains five values: 'alternate', 'related', 'self', 'enclosure', and 'via'"
	/// to which we add "src" as another rel type (see notes on src below).
	/// For ATOM see: https://tools.ietf.org/html/rfc4287#section-4.2.7.2
	/// </summary>
	public enum SFRel
	{
		/// <summary>
		/// No rel value was provided.
		/// </summary>
		none = 0,

		/// <summary>
		/// ATOM: "identifies an alternate version of the resource described by the containing element."
		/// Note that by "alternate version" they mean alternate *in comparison to the xml feed*.
		/// Typically, that means the web page that displays the original post, broadcast, etc.
		/// </summary>
		alternate = 1,

		/// <summary>
		/// ATOM: "identifies a resource *related* to the resource described by the containing element."
		/// </summary>
		related = 2,

		/// <summary>
		/// ATOM: "The value "via" signifies that the IRI in the value of the href
		/// attribute identifies a resource that is the source of the
		/// information provided in the containing element." - https://tools.ietf.org/html/rfc4287#section-4.2.7.2
		/// </summary>
		via = 3,

		/// <summary>
		/// ATOM: To be used in ATOM feeds when the 'content' 
		/// element has a 'src' url tag. Technically, 
		/// ATOM doesn't have a 'rel' type of src.
		/// However, when ATOM's 'content' element has a 'src' attribute
		/// it *cannot* have content in the element (only the src url value can be used),
		/// making that really a special kind (/rel) of link, which we
		/// find is best indicated by a Rel value.
		/// </summary>
		src = 4,

		/// <summary>
		/// RSS and ATOM: RSS 'enclosure' elements are converted to a SFLink
		/// with Rel type 'enclosure. For ATOM: "identifies a related resource that is potentially 
		/// large in size and might require special handling. For atom:link
		/// elements with rel='enclosure', the length attribute SHOULD be provided."
		/// In practise, typically this indicates a binary type file, such as media files, 
		/// PDFs, docs, etc.
		/// </summary>
		enclosure = 5,

		/// <summary>
		/// RSS and ATOM: All rel types that reference the XML feed itself.
		/// In atom of value "self", in RSS sometimes
		/// "home" and "alternate home" is used.
		/// </summary>
		self = 6,

		/// <summary>
		/// pubsubhubbub 
		/// <code>
		/// <![CDATA[<atom10:link xmlns:atom10="http://www.w3.org/2005/Atom" rel="hub" href="http://pubsubhubbub.appspot.com/" />]]>
		/// </code>
		/// </summary>
		hub = 10,

	}
}
