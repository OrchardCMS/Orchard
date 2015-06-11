using System.Linq;
using System.Web.Mvc;
using IDeliverable.Slides.Filters;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.ViewModels;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Layouts.Helpers;

namespace IDeliverable.Slides.Controllers
{
    [Admin]
    [Dialog]
    public class SlideController : Controller, IUpdateModel
    {
        private readonly INotifier _notifier;
        private readonly IObjectStore _objectStore;
        private readonly IElementManager _elementManager;
        private readonly ILayoutModelMapper _mapper;
        private readonly ILayoutSerializer _serializer;
        private readonly ILayoutEditorFactory _layoutEditorFactory;

        public SlideController(
            INotifier notifier, 
            IObjectStore objectStore,
            IElementManager elementManager, 
            ILayoutModelMapper mapper,
            ILayoutSerializer serializer, 
            ILayoutEditorFactory layoutEditorFactory)
        {
            _notifier = notifier;
            _objectStore = objectStore;
            _elementManager = elementManager;
            _mapper = mapper;
            _serializer = serializer;
            _layoutEditorFactory = layoutEditorFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Edit(string id, string returnUrl)
        {
            var slide = _objectStore.Get<Slide>(id);
            var viewModel = new SlideEditorViewModel
            {
                ReturnUrl = returnUrl,
                LayoutEditor = _layoutEditorFactory.Create(slide.LayoutData, id, slide.TemplateId)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(SlideEditorViewModel viewModel)
        {
            var elementInstances = _mapper.ToLayoutModel(viewModel.LayoutEditor.Data, DescribeElementsContext.Empty).ToArray();
            var context = new LayoutSavingContext {
                Updater = this,
                Elements = elementInstances,
            };

            _elementManager.Saving(context);
            var slide = new Slide {
                LayoutData = _serializer.Serialize(elementInstances),
                TemplateId = viewModel.LayoutEditor.TemplateId
            };
            _objectStore.Set(viewModel.LayoutEditor.SessionKey, slide);

            return RedirectToEditor(viewModel.ReturnUrl, T("That slide has been updated."));
        }

        private RedirectResult RedirectToEditor(string returnUrl, LocalizedString notification)
        {
            _notifier.Information(notification);
            return Redirect(returnUrl);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}