using DotNetXtensions;

namespace SimpleFeedNS
{

	public class SFAlterFeedLinks
	{
		/// <summary>
		/// If a given link matches a vimeo or youtube url host type, set their 
		/// mime types to own of our custom vimeo or youtube mime types (true by default).
		/// </summary>
		public bool SetYoutubeVimeoLinkMimeTypes { get; set; } = true; //SetYoutubeVimeoValues

		/// <summary>
		/// If you override this function, it is up to you to call 
		/// <see cref="AlterLink_VimeoYoutube"/> if <see cref="SetYoutubeVimeoLinkMimeTypes"/> is true.
		/// </summary>
		public virtual SFLink AlterLink(SFLink link)
		{
			if(link == null || link.Url.IsNulle())
				return null;
			if(SetYoutubeVimeoLinkMimeTypes) {
				bool vimYtAltered = AlterLink_VimeoYoutube(link);
			}
			return link;
		}

		/// <summary>
		/// If the link url has a url.hostname that matches vimeo or youtube
		/// values, this sets the LinkType accordingly, and then returns true.
		/// Otherwise returns false.
		/// </summary>
		public bool AlterLink_VimeoYoutube(SFLink link)
		{
			// in the future could have a dictionary of all possible vimeo / youtube full hosts.
			// for the moment, if the host part has those in it's name we'll assume it is.
			// but we certainly could not do this with the full url value (e.g. someone could have 'vimeo'
			// in a url or blog title)

			if(link != null) {
				string linkHost = link.Uri.Host;
				if(linkHost.CountN() > 5) { // && link.LinkType.IsAudio() == false) {
					if(linkHost.Contains("vimeo")) {
						link.MimeType = BasicMimeType.video_vimeo;
					}
					else if(linkHost.Contains("youtu")) {
						link.MimeType = BasicMimeType.video_youtube;
					}
					else
						return false;
					return true;
				}
			}
			return false;
		}

	}
}
