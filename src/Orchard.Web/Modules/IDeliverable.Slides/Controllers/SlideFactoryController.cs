using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IDeliverable.Slides.Filters;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.ViewModels;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.UI.Admin;

namespace IDeliverable.Slides.Controllers
{
    using ContentItemElement = Orchard.Layouts.Elements.ContentItem;

    [Admin]
    [Dialog]
    public class SlideFactoryController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IElementFactory _elementFactory;
        private readonly ILayoutSerializer _layoutSerializer;
        private readonly IElementManager _elementManager;
        private readonly IObjectStore _objectStore;
        private readonly ILayoutEditorFactory _layoutEditorFactory;
        private readonly ILayoutModelMapper _layoutModelMapper;

        public SlideFactoryController(
            IContentManager contentManager,
            IElementFactory elementFactory,
            ILayoutSerializer layoutSerializer,
            IElementManager elementManager,
            IObjectStore objectStore, 
            ILayoutEditorFactory layoutEditorFactory, 
            ILayoutModelMapper layoutModelMapper)
        {
            _contentManager = contentManager;
            _elementFactory = elementFactory;
            _layoutSerializer = layoutSerializer;
            _elementManager = elementManager;
            _objectStore = objectStore;
            _layoutEditorFactory = layoutEditorFactory;
            _layoutModelMapper = layoutModelMapper;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Create(string returnUrl)
        {
            var viewModel = new SlideEditorViewModel
            {
                ReturnUrl = returnUrl,
                LayoutEditor = _layoutEditorFactory.Create(null, _objectStore.GenerateKey())
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(SlideEditorViewModel viewModel)
        {
            var slides = new[] { new Slide { LayoutData = _layoutSerializer.Serialize(_layoutModelMapper.ToLayoutModel(viewModel.LayoutEditor.Data, DescribeElementsContext.Empty)) } };
            var slidesKey = viewModel.LayoutEditor.SessionKey;

            _objectStore.Set(slidesKey, slides);
            return RedirectToEditor(viewModel.ReturnUrl, slidesKey);
        }

        public ActionResult MediaSlides(string returnUrl)
        {
            var viewModel = new ImageSlidesFactoryViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult MediaSlides(string returnUrl, string imageIds)
        {
            if (imageIds == null)
            {
                ModelState.AddModelError("imageIds", T("No images were selected.").Text);
                return View(new ImageSlidesFactoryViewModel
                {
                    ReturnUrl = returnUrl
                });
            }

            var mediaItems = LoadContentItems<MediaPart>(imageIds).ToList();
            var slides = mediaItems.Select(CreateSlide).ToList();
            var slidesKey = _objectStore.GenerateKey();

            _objectStore.Set(slidesKey, slides);
            return RedirectToEditor(returnUrl, slidesKey);
        }

        public ActionResult ContentSlides(string returnUrl)
        {
            var viewModel = new ContentSlidesFactoryViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ContentSlides(ContentSlidesFactoryViewModel viewModel)
        {
            if (viewModel.ContentIds == null)
            {
                ModelState.AddModelError("contentIds", T("No content items were selected.").Text);
                return View(viewModel);
            }

            var contentItems = LoadContentItems<IContent>(viewModel.ContentIds).ToList();
            var slides = contentItems.Select(x => CreateSlide(x, viewModel.DisplayType)).ToList();
            var slidesKey = _objectStore.GenerateKey();

            _objectStore.Set(slidesKey, slides);
            return RedirectToEditor(viewModel.ReturnUrl, slidesKey);
        }

        private Slide CreateSlide(MediaPart mediaItem)
        {
            var element = CreateElementFromMediaItem(mediaItem);
            
            return new Slide
            {
                LayoutData = _layoutSerializer.Serialize(new[] { CreateCanvas(element) }),
            };
        }

        private Element CreateElementFromMediaItem(MediaPart mediaItem)
        {
            if (mediaItem.Is<ImagePart>())
            {
                return _elementManager.ActivateElement<Image>(x => x.MediaId = mediaItem.Id);
            }
            else if (mediaItem.Is<VectorImagePart>())
            {
                return _elementManager.ActivateElement<VectorImage>(x => x.MediaId = mediaItem.Id);
            }

            return _elementManager.ActivateElement<MediaItem>(x => x.MediaItemIds = new[] { mediaItem.Id });
        }

        private Slide CreateSlide(IContent content, string displayType)
        {
            var elementDescriptor = _elementManager.GetElementDescriptorByType<ContentItemElement>();
            var element = (ContentItemElement)_elementFactory.Activate(elementDescriptor);

            element.ContentItemIds = new[] { content.Id };
            element.DisplayType = displayType;
            
            return new Slide
            {
                LayoutData = _layoutSerializer.Serialize(new[] { CreateCanvas(element) }),
            };
        }

        private IEnumerable<TPart> LoadContentItems<TPart>(string ids) where TPart : class, IContent
        {
            if (String.IsNullOrWhiteSpace(ids))
            {
                return Enumerable.Empty<TPart>();
            }

            var itemIds = ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse);
            return _contentManager.GetMany<TPart>(itemIds, VersionOptions.Latest, QueryHints.Empty);
        }

        private RedirectResult RedirectToEditor(string returnUrl, string slidesSessionKey)
        {
            var url = returnUrl.AppendQueryString(new { slides = slidesSessionKey });
            return Redirect(url);
        }

        private Canvas CreateCanvas(params Element[] elements)
        {
            var canvasDescriptor = _elementManager.GetElementDescriptorByType<Canvas>();
            var canvas = (Canvas)_elementFactory.Activate(canvasDescriptor);

            foreach (var element in elements) {
                canvas.Elements.Add(element);
            }

            return canvas;
        }
    }
}