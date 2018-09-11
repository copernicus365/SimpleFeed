using DotNetXtensions; //using DotNetXtensionsPrivate;

namespace SimpleFeedNS
{
	public class SFCategory
	{
		/// <summary>
		/// Equivalent to the RSS category tag value, and to the ATOM 'term' attribute value.
		/// Note that 'term' is the chief value in ATOM feeds for category tags and the only required
		/// attribute.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// The RSS 'domain' or ATOM 'scheme' attribute value (if any).
		/// </summary>
		public string Scheme { get; set; }

		/// <summary>
		/// The ATOM 'label' attribute value (if any).
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// This is an extra field that can be used as an extra identifier
		/// for this category instance. Most feeds do not cary through 
		/// the system's internal ids used for a given category or tag,
		/// but if so, that can be saved here (e.g. a numeric number,
		/// a slug, etc).
		/// </summary>
		public string ExtraId { get; set; }

		public SFCategory() { }

		public SFCategory(string value, string scheme = null, string label = null)
		{
			Value = value;
			Scheme = scheme;
			Label = label;
		}

		public bool PrepValues()
		{
			if (Value.IsTrimmable())
				Value = Value.Trim();
			if (Scheme.IsTrimmable())
				Scheme = Scheme.Trim();
			if (Label.IsTrimmable())
				Label = Label.Trim();
			return Value.NotNulle();
		}

		public string ToStringNoScheme()
		{
			if (Label == null)
				return Value;
			if (Value == null)
				return Label;
			return $"{Value} ('{Label}')";
			//if (Label == null) {
			//	return Scheme == null
			//		? Value
			//		: $"{Value} ({Scheme})";
			//}
			//return $"{Value} ({Label}, {Scheme})";
		}

		/// <summary>
		/// Currently override returns <see cref="ToStringNoScheme"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => ToStringNoScheme();
	}
}
