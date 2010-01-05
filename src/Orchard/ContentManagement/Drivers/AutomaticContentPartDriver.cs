namespace Orchard.ContentManagement.Drivers {
    public abstract class AutomaticContentPartDriver<TPart> : ContentPartDriver<TPart> where TPart : class, IContent {
        protected override string Prefix {
            get {
                return (typeof (TPart).Name);
            }
        }

        protected override DriverResult Display(TPart part, string displayType) {
            return ContentPartTemplate(part);
        }
        
        protected override DriverResult Editor(TPart part) {
            return ContentPartTemplate(part);
        }
        
        protected override DriverResult Editor(TPart part, IUpdateModel updater) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return ContentPartTemplate(part);
        }
    }
}