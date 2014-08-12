using System;
using System.Globalization;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;

namespace Orchard.Localization.Services {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class AdminDirectionalityFactory : ShapeDisplayEvents {
        private readonly ILocalizationService _localizationService;
        private readonly ICultureManager _cultureManager;
        private readonly WorkContext _workContext;

        public AdminDirectionalityFactory(
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory,
            ILocalizationService localizationService,
            ICultureManager cultureManager) {
            _localizationService = localizationService;
            _cultureManager = cultureManager;
            _workContext = workContextAccessor.GetContext();
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        private bool IsActivable() {
            // activate on front-end only
            if (AdminFilter.IsApplied(new RequestContext(_workContext.HttpContext, new RouteData())))
                return true;

            return false;
        }

        public override void Displaying(ShapeDisplayingContext context) {
            context.ShapeMetadata.OnDisplaying(displayedContext => {
                if (!IsActivable()) {
                    return;
                }
                
                if (context.ShapeMetadata.Type != "Zone")
                    return;

                ContentItem contentItem = context.Shape.ContentItem;

                // if not, check for ContentPart 
                if (contentItem == null) {
                    ContentPart contentPart = context.Shape.ContentPart;
                    if (contentPart != null) {
                        contentItem = contentPart.ContentItem;
                    }
                }

                var culture = (contentItem != null) ? _localizationService.GetContentCulture(contentItem) : _cultureManager.GetSiteCulture();

                var className = "content-" + (CultureInfo.GetCultureInfo(culture).TextInfo.IsRightToLeft ? "rtl" : "ltr");

                if (!_workContext.Layout.Content.Classes.Contains(className))
                    _workContext.Layout.Content.Classes.Add(className);
            });
        }
    }
}