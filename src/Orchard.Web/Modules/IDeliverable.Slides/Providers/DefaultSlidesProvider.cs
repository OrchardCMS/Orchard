using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard.ContentManagement;
using Orchard.Layouts.Services;
using Orchard.Localization;

namespace IDeliverable.Slides.Providers
{
    public class DefaultSlidesProvider : SlidesProvider
    {
        private readonly ISlidesSerializer _serializer;
        private readonly ILayoutManager _layoutManager;

        public DefaultSlidesProvider(ISlidesSerializer serializer, ILayoutManager layoutManager)
        {
            _serializer = serializer;
            _layoutManager = layoutManager;
        }

        public override LocalizedString DisplayName
        {
            get { return T("Default"); }
        }

        public override dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context)
        {
            return UpdateEditor(shapeFactory, context, updater: null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater)
        {
            var slidesData = context.Storage.RetrieveSlidesData();
            var slides = _serializer.Deserialize(slidesData).ToList();
            var slideShapes = slides.Select(x => _layoutManager.RenderLayout(x.LayoutData, content: context.Content)).ToList();
            var viewModel = new DefaultSlidesProviderViewModel
            {
                SlideShow = context.SlideShow,
                SessionKey = context.ElementSessionKey,
                Slides = slideShapes
            };

            if (updater != null)
            {
                if (updater.TryUpdateModel(viewModel, Prefix, new[] { "Indices", "SlidesData" }, null))
                {
                    var currentSlides = slides;
                    var newSlides = new List<Slide>(currentSlides.Count);

                    newSlides.AddRange(viewModel.Indices.Select(index => currentSlides[index]));
                    context.Storage.StoreSlidesData(_serializer.Serialize(newSlides));
                }
            }

            var templateName = context.SlideShow is SlideShowPart ? "SlidesProviders.Slides.Part" : "SlidesProviders.Slides.Element";
            return shapeFactory.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: Prefix);
        }

        public override IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context)
        {
            var slidesData = context.Storage.RetrieveSlidesData();
            var slides = _serializer.Deserialize(slidesData).ToList();
            var slideShapes = slides.Select(x => _layoutManager.RenderLayout(x.LayoutData, content: context.Content));
            return slideShapes;
        }
    }
}