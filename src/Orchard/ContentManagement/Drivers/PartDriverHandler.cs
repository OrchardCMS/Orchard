using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers {
    [UsedImplicitly]
    public class PartDriverHandler : IContentHandler {
        private readonly IEnumerable<IPartDriver> _drivers;

        public PartDriverHandler(IEnumerable<IPartDriver> drivers) {
            _drivers = drivers;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        System.Collections.Generic.IEnumerable<ContentType> IContentHandler.GetContentTypes() {
            return Enumerable.Empty<ContentType>();
        }

        void IContentHandler.Activating(ActivatingContentContext context) { }

        void IContentHandler.Activated(ActivatedContentContext context) { }

        void IContentHandler.Creating(CreateContentContext context) { }

        void IContentHandler.Created(CreateContentContext context) { }

        void IContentHandler.Loading(LoadContentContext context) { }

        void IContentHandler.Loaded(LoadContentContext context) { }

        void IContentHandler.Versioning(VersionContentContext context) { }

        void IContentHandler.Versioned(VersionContentContext context) { }

        void IContentHandler.Removing(RemoveContentContext context) { }

        void IContentHandler.Removed(RemoveContentContext context) { }

        void IContentHandler.GetItemMetadata(GetItemMetadataContext context) { }

        void IContentHandler.BuildDisplayModel(BuildDisplayModelContext context) {
            _drivers.Invoke(driver => {
                                var result = driver.BuildDisplayModel(context);
                                if (result != null)
                                    result.Apply(context);
                            }, Logger);
        }

        void IContentHandler.BuildEditorModel(BuildEditorModelContext context) {
            _drivers.Invoke(driver => {
                                var result = driver.BuildEditorModel(context);
                                if (result != null)
                                    result.Apply(context);
                            }, Logger);
        }

        void IContentHandler.UpdateEditorModel(UpdateEditorModelContext context) {
            _drivers.Invoke(driver => {
                                var result = driver.UpdateEditorModel(context);
                                if (result != null)
                                    result.Apply(context);
                            }, Logger);
        }

    }
}