using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Providers;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Services;

namespace IDeliverable.Slides.Drivers
{
    public class SlideShowElementDriver : ElementDriver<SlideShow>
    {
        private readonly IOrchardServices _services;
        private readonly ISlideShowPlayerEngineManager _engineManager;
        private readonly IClock _clock;
        private readonly ISlidesProviderManager _providerManager;

        public SlideShowElementDriver(
            IOrchardServices services,
            ISlideShowPlayerEngineManager engineManager,
            IClock clock,
            ISlidesProviderManager providerManager)
        {
            _services = services;
            _engineManager = engineManager;
            _clock = clock;
            _providerManager = providerManager;
        }

        public string Prefix => "SlideShowElement";

        protected override EditorResult OnBuildEditor(SlideShow element, ElementEditorContext context)
        {
            if (!LicenseValidationHelper.GetLicenseIsValid())
                return Editor(context, context.ShapeFactory.Slides_InvalidLicense());

            var storage = new ElementStorage(element);
            var slidesProvidercontext = new SlidesProviderContext(context.Content, element, storage, context.Session);
            var providerShapes = Enumerable.ToDictionary(_providerManager.BuildEditors(context.ShapeFactory, slidesProvidercontext), (Func<dynamic, string>)(x => (string)x.Provider.Name));

            var viewModel = new SlideShowElementViewModel
            {
                Element = element,
                ProfileId = element.ProfileId,
                SessionKey = context.Session,
                AvailableProfiles = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>().Profiles.ToList(),
                ProviderName = element.ProviderName,
                AvailableProviders = providerShapes,
            };

            if (context.Updater != null)
            {
                if (context.Updater.TryUpdateModel(viewModel, Prefix, new[] { "ProfileId", "ProviderName", "SlidesData" }, null))
                {
                    // The element editor only provides the posted form values (for the ValueProvider), so we need to fetch the slides data ourselves in order to not lose it.
                    if (context.ElementData.ContainsKey("SlideShowSlides"))
                        storage.StoreSlidesData(context.ElementData["SlideShowSlides"]);

                    providerShapes = Enumerable.ToDictionary(_providerManager.UpdateEditors(context.ShapeFactory, slidesProvidercontext, new Updater(context.Updater, Prefix)), (Func<dynamic, string>)(x => (string)x.Provider.Name));
                    element.ProfileId = viewModel.ProfileId;
                    element.ProviderName = viewModel.ProviderName;
                    viewModel.AvailableProviders = providerShapes;
                }
            }

            var slidesEditor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.SlideShow", Prefix: Prefix, Model: viewModel);

            //viewModel.Slides = element.Slides.Select(x => _layoutManager.RenderLayout(x.LayoutData)).ToArray();
            slidesEditor.Metadata.Position = "Slides:0";
            return Editor(context, slidesEditor);
        }

        protected override void OnDisplaying(SlideShow element, ElementDisplayContext context)
        {
            if (!LicenseValidationHelper.GetLicenseIsValid())
            {
                context.ElementShape.Metadata.Alternates.Clear();
                context.ElementShape.Metadata.Alternates.Add($"Elements_SlideShow_InvalidLicense");
                context.ElementShape.Metadata.Alternates.Add($"Elements_SlideShow_InvalidLicense_{context.DisplayType}");
                return;
            }

            var slideShapes = GetSlides(element, context);
            var engine = _engineManager.GetEngine(element.Profile);
            var engineShape = engine.BuildDisplay(_services.New);

            engineShape.Engine = engine;
            engineShape.Slides = slideShapes;
            engineShape.SlideShowId = _clock.UtcNow.Ticks + "[" + element.Index + "]"; // TODO: Come up with a better, deterministic way to determine the slide show id. Perhaps elements should have a unique ID (unique within the layout, at least).

            context.ElementShape.Slides = slideShapes;
            context.ElementShape.Engine = engineShape;
        }

        private IList<dynamic> GetSlides(SlideShow element, ElementDisplayContext context)
        {
            var provider = !String.IsNullOrWhiteSpace(element.ProviderName) ? _providerManager.GetProvider(element.ProviderName) : default(ISlidesProvider);
            var storage = new ElementStorage(element);
            var slidesProviderContext = new SlidesProviderContext(context.Content, element, storage);
            return provider == null ? new List<dynamic>() : new List<dynamic>(provider.BuildSlides(_services.New, slidesProviderContext));
        }
    }
}