namespace SimpleFeedNS
{
	public class SrcSetImg
	{
		public int Size { get; set; }

		public SrcSizeType SizeType { get; set; }

		public string Url { get; set; }


		public override string ToString()
			=> $"{Url} {Size}{SrcSetTypeChar}";

		public static SrcSizeType CharForSrcSetType(char c)
		{
			switch(c) {
				case 'w': return SrcSizeType.Width;
				case 'x': return SrcSizeType.PixelDensity;
				default: return SrcSizeType.None;
			}
		}

		public char SrcSetTypeChar {
			get {
				switch(SizeType) {
					case SrcSizeType.Width: return 'w';
					case SrcSizeType.PixelDensity: return 'x';
					default:
						return '-';
				}
			}
		}

		public bool SetNumberVal(int size, SrcSizeType expectedSizeType, char sizeType)
		{
			var type = CharForSrcSetType(sizeType);

			if(type != expectedSizeType)
				return false;

			Size = size;

			return true;
		}

	}

	public enum SrcSizeType
	{
		None = 0,
		Width = 1,
		PixelDensity = 2
	}
}
