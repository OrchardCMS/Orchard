namespace Orchard.Models.Driver {
    public class UpdateContentContext : GetEditorsContext {
        public UpdateContentContext(IContent content, IUpdateModel updater) : base(content) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; set; }
    }
}