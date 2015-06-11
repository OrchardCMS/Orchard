using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IDeliverable.Licensing;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace IDeliverable.Slides.Drivers
{
    public class SlideShowPartDriver : ContentPartDriver<SlideShowPart>
    {
        private readonly IOrchardServices _services;
        private readonly IEngineManager _engineManager;
        private readonly ISlidesProviderManager _providerManager;
        private readonly ILicenseValidator _licenseValidator;
        private readonly ILicenseAccessor _licenseAccessor;

        public SlideShowPartDriver(
            IOrchardServices services,
            IEngineManager engineManager,
            ISlidesProviderManager providerManager,
            ILicenseValidator licenseValidator,
            ILicenseAccessor licenseAccessor)
        {
            _services = services;
            _engineManager = engineManager;
            _providerManager = providerManager;
            _licenseValidator = licenseValidator;
            _licenseAccessor = licenseAccessor;
        }

        protected override DriverResult Editor(SlideShowPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(SlideShowPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (!_licenseValidator.ValidateLicense(_licenseAccessor.GetSlidesLicense()).IsValid)
                return ContentShape("Parts_SlideShow_Edit_InvalidLicense", () => shapeHelper.Parts_SlideShow_Edit_InvalidLicense());

            return ContentShape("Parts_SlideShow_Edit", () =>
            {
                var storage = new ContentPartStorage(part);
                var providerShapes = Enumerable.ToDictionary(_providerManager.BuildEditors(shapeHelper, storage, context: part), (Func<dynamic, string>)(x => (string)x.Provider.Name));

                var viewModel = new SlideShowPartViewModel
                {
                    Part = part,
                    ProfileId = part.ProfileId,
                    AvailableProfiles = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>().Profiles.ToList(),
                    ProviderName = part.ProviderName,
                    AvailableProviders = providerShapes,
                };

                if (updater != null)
                {
                    if (updater.TryUpdateModel(viewModel, Prefix, new[] { "ProfileId", "ProviderName" }, null))
                    {
                        providerShapes = Enumerable.ToDictionary(_providerManager.UpdateEditors(shapeHelper, storage, new Updater(updater, Prefix), context: part), (Func<dynamic, string>)(x => (string)x.Provider.Name));
                        part.ProfileId = viewModel.ProfileId;
                        part.ProviderName = viewModel.ProviderName;
                        viewModel.AvailableProviders = providerShapes;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.SlideShow", Prefix: Prefix, Model: viewModel);
            });
        }

        protected override DriverResult Display(SlideShowPart part, string displayType, dynamic shapeHelper)
        {
            if (!_licenseValidator.ValidateLicense(_licenseAccessor.GetSlidesLicense()).IsValid)
                return ContentShape("Parts_SlideShow_InvalidLicense", () => shapeHelper.Parts_SlideShow_InvalidLicense());

            return Combined(
                ContentShape("Parts_SlideShow", () =>
                {
                    var slideShapes = GetSlides(part, shapeHelper);
                    var engine = _engineManager.GetEngine(part.Profile);
                    var engineShape = engine.BuildDisplay(shapeHelper);

                    engineShape.Engine = engine;
                    engineShape.Slides = slideShapes;
                    engineShape.SlideShowId = part.Id.ToString(CultureInfo.InvariantCulture);

                    return shapeHelper.Parts_SlideShow(Slides: slideShapes, Engine: engineShape);
                }),
                ContentShape("Parts_SlideShow_Summary", () =>
                {
                    var slideShapes = GetSlides(part, shapeHelper);
                    return shapeHelper.Parts_SlideShow_Summary(Slides: slideShapes);
                }),
                ContentShape("Parts_SlideShow_SummaryAdmin", () =>
                {
                    var slideShapes = GetSlides(part, shapeHelper);
                    return shapeHelper.Parts_SlideShow_SummaryAdmin(Slides: slideShapes);
                }));
        }

        private IList<dynamic> GetSlides(SlideShowPart part, dynamic shapeHelper)
        {
            var provider = !String.IsNullOrWhiteSpace(part.ProviderName) ? _providerManager.GetProvider(part.ProviderName) : default(ISlidesProvider);
            var storage = new ContentPartStorage(part);
            return provider == null ? new List<dynamic>() : new List<dynamic>(provider.BuildSlides(shapeHelper, storage));
        }
    }
}