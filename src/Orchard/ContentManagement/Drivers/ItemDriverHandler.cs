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

        IEnumerable<ContentType> IContentHandler.GetContentTypes() {
            var contentTypes = new List<ContentType>();
            _drivers.Invoke(driver=>contentTypes.AddRange(driver.GetContentTypes()), Logger);
            return contentTypes;
        }

        void IContentHandler.Activating(ActivatingContentContext context) { }

        void IContentHandler.Activated(ActivatedContentContext context) { }

        void IContentHandler.Creating(CreateContentContext context) { }

        void IContentHandler.Created(CreateContentContext context) { }

        void IContentHandler.Loading(LoadContentContext context) { }

        void IContentHandler.Loaded(LoadContentContext context) { }

        void IContentHandler.GetItemMetadata(GetItemMetadataContext context) {
            _drivers.Invoke(driver => driver.GetItemMetadata(context), Logger);
        }

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
