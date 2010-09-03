namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorModelContext : BuildEditorModelContext {
        public UpdateEditorModelContext(IContent content, IUpdateModel updater)
            : base(content) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}