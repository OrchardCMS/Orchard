using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Drivers;
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

        public override dynamic BuildEditor(dynamic shapeFactory, IStorage storage, dynamic context = null)
        {
            return UpdateEditor(shapeFactory, storage, updater: null, context: context);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IStorage storage, IUpdateModel updater, dynamic context = null)
        {
            var slidesData = storage.RetrieveSlidesData();
            var slides = _serializer.Deserialize(slidesData).ToList();
            var slideShapes = slides.Select(x => _layoutManager.RenderLayout(x.LayoutData)).ToList();
            var elementContext = context as ElementEditorContext;
            var viewModel = new DefaultSlidesProviderViewModel
            {
                Part = context as SlideShowPart,
                Element = elementContext != null ? (SlideShow)elementContext.Element : default(SlideShow),
                SessionKey = elementContext != null ? elementContext.Session : default(string),
                Slides = slideShapes
            };

            if (updater != null)
            {
                if (updater.TryUpdateModel(viewModel, Prefix, new[] { "Indices", "SlidesData" }, null))
                {
                    var currentSlides = slides;
                    var newSlides = new List<Slide>(currentSlides.Count);

                    newSlides.AddRange(viewModel.Indices.Select(index => currentSlides[index]));
                    storage.StoreSlidesData(_serializer.Serialize(newSlides));
                }
            }

            var templateName = viewModel.Part != null ? "SlidesProviders.Slides.Part" : "SlidesProviders.Slides.Element";
            return shapeFactory.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: Prefix);
        }

        public override IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, IStorage storage)
        {
            var slidesData = storage.RetrieveSlidesData();
            var slides = _serializer.Deserialize(slidesData).ToList();
            var slideShapes = slides.Select(x => _layoutManager.RenderLayout(x.LayoutData));
            return slideShapes;
        }
    }
}