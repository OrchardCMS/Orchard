using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public abstract class AutomaticPartDriver<TPart> : PartDriver<TPart> where TPart : class, IContent {
        protected override string Prefix {
            get {
                return (typeof (TPart).Name);
            }
        }
        protected override DriverResult Display(TPart part, string displayType) {
            return PartialView(part);
        }
        protected override DriverResult Editor(TPart part) {
            return PartialView(part);
        }
        protected override DriverResult Editor(TPart part, IUpdateModel updater) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return PartialView(part);
        }
    }
}