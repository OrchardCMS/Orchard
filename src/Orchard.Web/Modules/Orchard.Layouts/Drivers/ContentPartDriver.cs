using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Drivers {
    public class ContentPartDriver : ElementDriver<ContentPart> {
        private readonly IContentPartDisplay _contentPartDisplay;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly ITransactionManager _transactionManager;

        public ContentPartDriver(IContentPartDisplay contentPartDisplay, ICultureAccessor cultureAccessor, ITransactionManager transactionManager) {
            _contentPartDisplay = contentPartDisplay;
            _cultureAccessor = cultureAccessor;
            _transactionManager = transactionManager;
        }

        protected override void OnDisplaying(ContentPart element, ElementDisplayContext context) {
            var contentItem = context.Content.ContentItem;
            var contentPartName = (string)element.Descriptor.StateBag["ElementTypeName"];
            var contentPart = contentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == contentPartName);

            if ((contentItem.Id == 0 || context.DisplayType == "Design") && context.Updater != null) {
                // The content item hasn't been stored yet, so bind form values with the content part to represent actual state.
                var controller = (Controller)context.Updater;
                var oldValueProvider = controller.ValueProvider;

                controller.ValueProvider = context.Element.State.ToValueProvider(_cultureAccessor.CurrentCulture);
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