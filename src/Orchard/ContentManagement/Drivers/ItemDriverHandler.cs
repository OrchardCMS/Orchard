using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers {
    [UsedImplicitly]
    public class ItemDriverHandler : IContentHandler {
        private readonly IEnumerable<IItemDriver> _drivers;

        public ItemDriverHandler(IEnumerable<IItemDriver> drivers) {
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

        void IContentHandler.GetItemMetadata(GetItemMetadataContext context) {
            _drivers.Invoke(driver => {
                driver.GetItemMetadata(context);
            }, Logger);
        }

        void IContentHandler.BuildDisplayModel(BuildDisplayModelContext context) { }

        void IContentHandler.BuildEditorModel(BuildEditorModelContext context) { }

        void IContentHandler.UpdateEditorModel(UpdateEditorModelContext context) { }

    }

}
