namespace Orchard.Azure.MediaServices.Services.Wams {
    public class MediaProcessorName {
        public static readonly MediaProcessorName MediaEncoderStandard = new MediaProcessorName("Media Encoder Standard");

        private MediaProcessorName(string name) {
            Name = name;
        }

        public string Name { get; set; }
    }
}