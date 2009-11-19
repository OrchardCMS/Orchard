namespace Orchard.Models.Driver {
    public class UpdateContentContext : GetEditorsContext {
        public UpdateContentContext(ContentItem contentItem, IUpdateModel updater) : base(contentItem) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; set; }
    }
}