using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Security;
using TinyMce.Models;

namespace TinyMce.Drivers {
    [OrchardFeature("TinyMce.Settings")]
    public class TinyMceSettingsPartDriver : ContentPartDriver<TinyMceSettingsPart> {

        private const string TemplateName = "Parts/TinyMceSettings";
        private readonly IAuthorizer _authorizer;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public TinyMceSettingsPartDriver(IAuthorizer authorizer,
            IVirtualPathProvider virtualPathProvider) {
            _authorizer = authorizer;
            _virtualPathProvider = virtualPathProvider;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "TinyMceSettings"; } }

        protected override DriverResult Editor(TinyMceSettingsPart part, dynamic shapeHelper) {            
            if (string.IsNullOrWhiteSpace(part.TinyMceSettingsOverride)) {
                var filePath = "~/modules/tinymce/scripts/orchard-tinymce.js";
                if (_virtualPathProvider.FileExists(filePath)) {
                    var stream = _virtualPathProvider.OpenFile(filePath);
                    part.TinyMceSettingsOverride = File.ReadAllText(_virtualPathProvider.MapPath(filePath));
                }
            }
            return ContentShape("Parts_TinyMceSettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                    .OnGroup("tinymce");
        }

        protected override DriverResult Editor(TinyMceSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageTinyMceSettings))
                return null;
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
