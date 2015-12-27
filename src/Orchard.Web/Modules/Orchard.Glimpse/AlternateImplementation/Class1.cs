//using System.Collections.Generic;
//using Orchard.ContentManagement.Drivers;
//using Orchard.ContentManagement.Handlers;
//using Orchard.ContentManagement.MetaData;
//using Orchard.Glimpse.Extensions;
//using Orchard.Glimpse.Services;
//using Orchard.Glimpse.Tabs.Parts;

//namespace Orchard.Glimpse.AlternateImplementation
//{
//    public class GlimpseContentPartDriver : IDecorator<IContentPartDriver>, IContentPartDriver
//    {
//        private readonly IContentPartDriver _decoratedService;
//        private readonly IGlimpseService _glimpseService;

//        public GlimpseContentPartDriver(IContentPartDriver decoratedService, IGlimpseService glimpseService) {
//            _decoratedService = decoratedService;
//            _glimpseService = glimpseService;
//        }

//        public DriverResult BuildDisplay(BuildDisplayContext context) {
//            var result =  _glimpseService.PublishTimedAction(() => _decoratedService.BuildDisplay(context), (r, t) => {
//                return new PartMessage {
//                    //ContentId = context.ContentItem.Id,
//                    //ContentName = context.ContentItem.GetContentName(),
//                    //ContentType = context.ContentItem.ContentType,
//                    //DisplayType = context.DisplayType,
//                    //PartDefinition = context.ContentPart == null ? null : context.ContentPart.PartDefinition
//                };
//            }, TimelineCategories.Widgets, "Build Display: "/* + context.ContentPart == null ? context.ContentItem.ContentType : context.ContentPart.PartDefinition.Name*/, context.ContentItem.GetContentName(), driverResult => driverResult != null).ActionResult;
//            //}, TimelineCategories.Widgets, "Build Display: " + context.ContentPart == null ? context.ContentItem.ContentType : context.ContentPart.PartDefinition.Name, context.ContentItem.GetContentName(), driverResult => driverResult != null).ActionResult;

//            return result; 
//        }

//        public DriverResult BuildEditor(BuildEditorContext context) {
//            return _decoratedService.BuildEditor(context);
//        }

//        public void Exported(ExportContentContext context) {
//            _decoratedService.Exported(context);
//        }

//        public void Exporting(ExportContentContext context) {
//            _decoratedService.Exporting(context);
//        }

//        public void GetContentItemMetadata(GetContentItemMetadataContext context) {
//            _decoratedService.GetContentItemMetadata(context);
//        }

//        public IEnumerable<ContentPartInfo> GetPartInfo() {
//            return _decoratedService.GetPartInfo();
//        }

//        public void Imported(ImportContentContext context) {
//            _decoratedService.Imported(context);
//        }

//        public void Importing(ImportContentContext context) {
//            _decoratedService.Importing(context);
//        }

//        public DriverResult UpdateEditor(UpdateEditorContext context) {
//            return _decoratedService.UpdateEditor(context);
//        }
//    }
//}