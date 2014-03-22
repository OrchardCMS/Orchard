namespace Orchard.Azure.MediaServices.Services.Wams {
	public class WamsLocatorInfo {
		public  WamsLocatorInfo(string id, string url) {
			Id = id;
			Url = url;
		}

		public string Id { get; private set; }
		public string Url { get; private set; }
	}
}