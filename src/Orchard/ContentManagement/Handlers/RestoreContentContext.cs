namespace Orchard.ContentManagement.Handlers {
    public class RestoreContentContext : ContentContextBase {
        public VersionOptions VersionOptions { get; set; }

        public RestoreContentContext(ContentItem contentItem, VersionOptions versionOptions) : base(contentItem) {
            VersionOptions = versionOptions;
        }
    }
}
