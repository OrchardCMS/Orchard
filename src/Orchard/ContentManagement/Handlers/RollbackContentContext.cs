namespace Orchard.ContentManagement.Handlers {
    public class RollbackContentContext : ContentContextBase {
        public VersionOptions VersionOptions { get; set; }

        public RollbackContentContext(ContentItem contentItem, VersionOptions versionOptions) : base(contentItem) {
            VersionOptions = versionOptions;
        }
    }
}
