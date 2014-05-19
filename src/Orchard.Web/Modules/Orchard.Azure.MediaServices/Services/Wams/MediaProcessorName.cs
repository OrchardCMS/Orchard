namespace Orchard.Azure.MediaServices.Services.Wams {
    public class MediaProcessorName {
        public static readonly MediaProcessorName WindowsAzureMediaEncoder = new MediaProcessorName("Windows Azure Media Encoder");
        public static readonly MediaProcessorName WindowsAzureMediaPackager = new MediaProcessorName("Windows Azure Media Packager");
        public static readonly MediaProcessorName WindowsAzureMediaEncryptor = new MediaProcessorName("Windows Azure Media Encryptor");
        public static readonly MediaProcessorName StorageDecryption = new MediaProcessorName("Storage Decryption");

        private MediaProcessorName(string name) {
            Name = name;
        }

        public string Name { get; set; }
    }
}