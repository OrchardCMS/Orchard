namespace Orchard.Models.Driver {
    public class UpdateModelContext : GetModelEditorsContext {
        public UpdateModelContext(ContentItem contentItem, IModelUpdater updater) : base(contentItem) {
            Updater = updater;
        }

        public IModelUpdater Updater { get; set; }
    }
}