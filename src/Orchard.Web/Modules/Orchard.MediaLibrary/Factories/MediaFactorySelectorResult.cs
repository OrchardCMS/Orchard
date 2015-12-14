namespace Orchard.MediaLibrary.Factories {
    public class MediaFactorySelectorResult {
        public int Priority { get; set; }
        public IMediaFactory MediaFactory { get; set; }
    }
}