namespace SFTests;

public class SFLinkTests : BaseTest
{
	const BasicMimeType mtypNONE = BasicMimeType.none;

	public SFLinkTests() : base() { }

	[Theory]
	[InlineData("http://howdy.com/cool.jpg", "jpg")]
	[InlineData("http://howdy.com/coolpic", null)]
	// I would like these not to fail, but they have been, and a change needs to be made
	[InlineData("//howdy.com/coolpic", null, "http://howdy.com/coolpic", false)]
	[InlineData("www.howdy.com/coolpic", null, "http://www.howdy.com/coolpic")]
	public void FixOrValidateInputUrl(string url, string ext = null, string fixedUrl = null, bool success = true)
	{
		ext = ext.NullIfEmptyTrimmed();
		fixedUrl = fixedUrl.NullIfEmptyTrimmed();

		bool _success = SFLink.FixOrValidateInputUrl(url, out string _url, out Uri _uri, out string _ext);
		True(success == _success);

		if(!_success)
			return;

		True(_uri != null);
		True(_url == (fixedUrl ?? url));
		True(ext == _ext);
	}

	[Theory]
	[InlineData("http://howdy.com/cool.jpg", null, null, mtypNONE, BasicMimeType.image_jpeg, null, true, SFRel.enclosure)]
	[InlineData("http://howdy.com/cool", null, null, BasicMimeType.image, BasicMimeType.image, null, true, SFRel.enclosure)]
	[InlineData("http://howdy.com/cool.jpg", null, "image/jpeg", mtypNONE, BasicMimeType.image_jpeg, null, true, SFRel.enclosure)]
	[InlineData("http://howdy.com/coolpic1", null, "image/jpeg", mtypNONE, BasicMimeType.image_jpeg, null, true, SFRel.enclosure)]
	[InlineData("http://howdy.com/coolpic1", null, null, BasicMimeType.image_jpeg, BasicMimeType.image_jpeg, null, true, SFRel.enclosure)]
	[InlineData("http://howdy.com/coolpic1", null, null, mtypNONE, mtypNONE, null, true, SFRel.enclosure)]
	public void Link1(
		string inUrl,
		string finUrl,
		string inMimeTypeStr,
		BasicMimeType inMimeType,
		BasicMimeType finMimeType,
		string inRel,
		bool inIsRssEncl,
		SFRel finRel)
	{
		var lk = new SFLink(inUrl, inMimeTypeStr, inRel, isRssEncl: inIsRssEncl, mimeType: inMimeType);

		True(lk.MimeType == finMimeType);
		True(lk.Rel == finRel);
		True(lk.Url == (finUrl.NullIfEmpty() ?? inUrl));
	}

	[Theory]
	[InlineData("https://i.vimeocdn.com/video/0e9-d_640", BasicMimeType.video, BasicMimeType.video_vimeo, true)]
	[InlineData("https://i.vimeocdn.com/video/0e9-d_640", BasicMimeType.none, BasicMimeType.video_vimeo, true)]
	[InlineData("https://i.vimeocdn.com/video/0e9-d_640", BasicMimeType.none, BasicMimeType.none, false)]
	[InlineData("https://i.vimeocdn.com/video/0e9-d_640", BasicMimeType.video, BasicMimeType.video, false)]
	[InlineData("https://i.vimeocdn.com/video/0e9-d_640", BasicMimeType.video_3gp, BasicMimeType.video_3gp, true)]
	[InlineData("https://i.vimeocdn.com/video/0e9-d_640", BasicMimeType.image, BasicMimeType.image, true)]
	public void LinkVimeoYTube(string url, BasicMimeType mime, BasicMimeType finalMime, bool detectYtVm)
	{
		var lk = new SFLink(url, mimeType: mime, detectYtubeVimeo: detectYtVm);

		True(lk.MimeType == finalMime);
	}
}
