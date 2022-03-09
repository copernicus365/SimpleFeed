namespace SimpleFeedNS
{
	public class SFFeedMeta
	{
		public string Value { get; set; }

		public string Type { get; set; }

		public string Url { get; set; }

		public string Source { get; set; }

		public string ExtraInfo { get; set; }

		public int Length { get; set; }

		public override string ToString()
		{
			return $"({Source}:{Type}) {Value} [{Url}]";
		}
	}
}
