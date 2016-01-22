namespace Orchard.MediaProcessing.Services {
    public interface IImageProcessingFileNameProvider : IDependency {
        string GetFileName(string profile, string path);
        void UpdateFileName(string profile, string path, string fileName);
    }
}
