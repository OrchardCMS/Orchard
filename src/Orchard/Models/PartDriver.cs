using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Models {
    public interface IPartDriver : IEvents {
        DriverResult BuildDisplayModel(BuildDisplayModelContext context);
        DriverResult BuildEditorModel(BuildEditorModelContext context);
        DriverResult UpdateEditorModel(UpdateEditorModelContext context);
    }


    public abstract class PartDriver<TPart> : IPartDriver where TPart : class, IContent {
        DriverResult IPartDriver.BuildDisplayModel(BuildDisplayModelContext context) {
            var part = context.ContentItem.As<TPart>();
            return part == null ? null : Display(part, context.GroupName, context.DisplayType);
        }

        DriverResult IPartDriver.BuildEditorModel(BuildEditorModelContext context) {
            var part = context.ContentItem.As<TPart>();
            return part == null ? null : Editor(part, context.GroupName);
        }

        DriverResult IPartDriver.UpdateEditorModel(UpdateEditorModelContext context) {
            var part = context.ContentItem.As<TPart>();
            return part == null ? null : Editor(part, context.GroupName, context.Updater);
        }

        protected virtual DriverResult Display(TPart part, string groupName, string displayType) {
            return null;
        }

        protected virtual DriverResult Editor(TPart part, string groupName) {
            return null;
        }

        protected virtual DriverResult Editor(TPart part, string groupName, IUpdateModel updater) {
            return null;
        }

        protected virtual string Prefix { get { return ""; } }

        public TemplateResult PartialView(object model) {
            return new TemplateResult(model, null, Prefix);
        }
        public TemplateResult PartialView(object model, string template) {
            return new TemplateResult(model, template, Prefix);
        }
        public TemplateResult PartialView(object model, string template, string prefix) {
            return new TemplateResult(model, template, prefix);
        }
    }

    public class DriverResult {
        public virtual void Apply(BuildDisplayModelContext context) { }
        public virtual void Apply(BuildEditorModelContext context) { }
    }

    public class TemplateResult : DriverResult {
        public object Model { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }

        public TemplateResult(object model, string templateName, string prefix) {
            Model = model;
            TemplateName = templateName;
            Prefix = prefix;
        }

        public override void Apply(BuildDisplayModelContext context) {
            context.AddDisplay(new TemplateViewModel(Model, Prefix) { TemplateName = TemplateName });
        }

        public override void Apply(BuildEditorModelContext context) {
            context.AddEditor(new TemplateViewModel(Model, Prefix) { TemplateName = TemplateName });
        }
    }

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
