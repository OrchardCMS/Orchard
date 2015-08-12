using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Drivers {
    public class ContentPartElementDriver : ElementDriver<ContentPart> {
        private readonly IContentPartDisplay _contentPartDisplay;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly ITransactionManager _transactionManager;

        public ContentPartElementDriver(IContentPartDisplay contentPartDisplay, ICultureAccessor cultureAccessor, ITransactionManager transactionManager) {
            _contentPartDisplay = contentPartDisplay;
            _cultureAccessor = cultureAccessor;
            _transactionManager = transactionManager;
        }

        protected override void OnDisplaying(ContentPart element, ElementDisplayingContext context) {
            // Content is optional context, so if it's null, we can't render the part element.
            // This typically only happens when the layout editor is used outside the context of
            // a content item and still renders the various content part elements as part of the toolbox.
            if (context.Content == null)
                return;

            var contentItem = context.Content.ContentItem;
            var contentPartName = (string)element.Descriptor.StateBag["ElementTypeName"];
            var contentPart = contentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == contentPartName);

            if ((contentItem.Id == 0 || context.DisplayType == "Design") && context.Updater != null) {
                // The content item hasn't been stored yet, so bind form values with the content part to represent actual state.
                var controller = (Controller)context.Updater;
                var oldValueProvider = controller.ValueProvider;

                controller.ValueProvider = context.Element.Data.ToValueProvider(_cultureAccessor.CurrentCulture);
                _contentPartDisplay.UpdateEditor(contentPart, context.Updater);
                _transactionManager.Cancel();
                controller.ValueProvider = oldValueProvider;
            }

            var contentPartShape = _contentPartDisplay.BuildDisplay(contentPart, displayType: "Layout");

            context.ElementShape.ContentPart = contentPart;
            context.ElementShape.Content = contentPartShape;
        }
    }
}