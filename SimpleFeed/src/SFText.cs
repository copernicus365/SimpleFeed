namespace SimpleFeedNS
{
	public class SFText
	{
		public string Type { get; set; }
		public string Value { get; set; }

		public override string ToString() => $"[{Type}] {Value}";
	}
}
