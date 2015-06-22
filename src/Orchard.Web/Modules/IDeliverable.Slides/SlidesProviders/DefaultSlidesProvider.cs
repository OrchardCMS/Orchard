using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Localization;

namespace IDeliverable.Slides.SlidesProviders
{
    public class DefaultSlidesProvider : SlidesProvider
    {
        private readonly ISlidesSerializer _slidesSerializer;
        private readonly ILayoutManager _layoutManager;
        private readonly ILayoutSerializer _layoutSerializer;
        private readonly IElementManager _elementManager;

        public DefaultSlidesProvider(ISlidesSerializer slidesSerializer, ILayoutManager layoutManager, ILayoutSerializer layoutSerializer, IElementManager elementManager)
        {
            _slidesSerializer = slidesSerializer;
            _layoutManager = layoutManager;
            _layoutSerializer = layoutSerializer;
            _elementManager = elementManager;
        }

        public override LocalizedString DisplayName => T("Default");

        public override dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context)
        {
            return UpdateEditor(shapeFactory, context, updater: null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater)
        {
            var slides = LoadSlides(context.Storage);
            var slideShapes = slides.Select(x => _layoutManager.RenderLayout(x.LayoutData, content: context.Content)).ToList();
            var viewModel = new DefaultSlidesProviderViewModel
            {
                Slideshow = context.Slideshow,
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
                    SaveSlides(context.Storage, newSlides);
                }
            }

            var templateName = context.Slideshow is SlideshowPart ? "SlidesProviders.Slides.Part" : "SlidesProviders.Slides.Element";
            return shapeFactory.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: Prefix);
        }

        public override IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context)
        {
            var slides = LoadSlides(context.Storage);
            var slideShapes = slides.Select(x => _layoutManager.RenderLayout(x.LayoutData, content: context.Content));
            return slideShapes;
        }

        public override void Exporting(SlidesProviderExportContext context)
        {
            var slides = LoadSlides(context.Storage);

            foreach (var slide in slides)
            {
                ExportSlide(context, slide);
            }

            var slidesData = _slidesSerializer.Serialize(slides);
            context.Element.Add(new XElement("Slides", slidesData));
        }

        public override void Importing(SlidesProviderImportContext context)
        {
            var slidesElement = context.Element.Element("Slides");

            if (slidesElement == null)
                return;

            var slidesData = slidesElement.Value;
            context.Storage.StoreSlidesData(slidesData);

            var slides = LoadSlides(context.Storage);

            foreach (var slide in slides)
            {
                ImportSlide(context, slide);
            }

            SaveSlides(context.Storage, slides);
        }

        private IList<Slide> LoadSlides(IStorage storage)
        {
            var slidesData = storage.RetrieveSlidesData();
            var slides = _slidesSerializer.Deserialize(slidesData).ToList();

            return slides;
        }

        private void SaveSlides(IStorage storage, IEnumerable<Slide> slides)
        {
            storage.StoreSlidesData(_slidesSerializer.Serialize(slides));
        }

        private void ExportSlide(SlidesProviderExportContext context, Slide slide)
        {
            var describeContext = new DescribeElementsContext { Content = context.Content };
            var elementTree = _layoutSerializer.Deserialize(slide.LayoutData, describeContext).ToList();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Exporting(elements, new ExportLayoutContext());
            slide.LayoutData = _layoutSerializer.Serialize(elementTree);
        }

        private void ImportSlide(SlidesProviderImportContext context, Slide slide)
        {
            var describeContext = new DescribeElementsContext { Content = context.Content };
            var elementTree = _layoutSerializer.Deserialize(slide.LayoutData, describeContext).ToList();
            var elements = elementTree.Flatten().ToArray();

            _elementManager.Importing(elements, new ImportLayoutContext { Session = context.Session });
            slide.LayoutData = _layoutSerializer.Serialize(elementTree);
        }
    }
}