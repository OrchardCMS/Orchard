//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using System.Web.Mvc;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.MetaData;
//using Orchard.Core.Common.Models;
//using Orchard.Localization;
//using Orchard.Logging;
//using Orchard.Security;
//using Orchard.Services;
//using Orchard.Settings;
//using Orchard.UI.Notify;

//namespace Orchard.ContentPreview.Controllers {
//    public class PreviewController : Controller, IUpdateModel {
//        private readonly IContentManager _contentManager;
//        private readonly IContentDefinitionManager _contentDefinitionManager;
//        private readonly ISiteService _siteService;
//        private readonly IContentItemDisplayManager _contentItemDisplayManager;
//        private readonly INotifier _notifier;
//        private readonly IClock _clock;
//        private readonly IAuthorizer _authorizer;


//        public PreviewController(
//            IContentManager contentManager,
//            IContentItemDisplayManager contentItemDisplayManager,
//            IContentDefinitionManager contentDefinitionManager,
//            ISiteService siteService,
//            INotifier notifier,
//            //ILogger<PreviewController> logger,
//            //IHtmlLocalizer<PreviewController> localizer,
//            IClock clock
//            ,
//            IAuthorizer authorizer) {
//            _clock = clock;
//            _notifier = notifier;
//            _siteService = siteService;
//            _contentManager = contentManager;
//            _contentDefinitionManager = contentDefinitionManager;
//            _authorizer = authorizer;

//            T = NullLocalizer.Instance;
//            Logger = NullLogger.Instance;
//        }

//        public ILogger Logger { get; set; }
//        public Localizer T { get; set; }

//        public ActionResult Index() {
//            return View();
//        }

//        [HttpPost]
//        public async Task<ActionResult> Render() {
//            if (!_authorizer.Authorize(Permissions.ContentPreview)) {
//                return new HttpUnauthorizedResult();
//            }

//            var contentItemType = Request.Form["ContentItemType"];
//            var contentItem = _contentManager.New(contentItemType);

//            // Assign the ids from the currently edited item so that validation thinks
//            // it's working on the same item. For instance if drivers are checking name unicity
//            // they need to think this is the same existing item (AutoroutePart).

//            var contentItemId = Request.Form["PreviewContentItemId"];
//            var contentItemVersionId = Request.Form["PreviewContentItemVersionId"];
//            int.TryParse(Request.Form["PreviewId"], out var contentId);

//            contentItem.As<CommonPart>().Id = contentId;
//            contentItem.ContentItemId = contentItemId;
//            contentItem.ContentItemVersionId = contentItemVersionId;
//            contentItem.As<CommonPart>().CreatedUtc = _clock.UtcNow;
//            contentItem.As<CommonPart>().ModifiedUtc = _clock.UtcNow;
//            contentItem.As<CommonPart>().PublishedUtc = _clock.UtcNow;

//            // TODO: we should probably get this value from the main editor as it might impact validators
//            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, true);

//            if (!ModelState.IsValid) {
//                var errors = new List<string>();
//                foreach (var modelState in ValidationHelpers.GetModelStateList(ViewData, false)) {
//                    for (var i = 0; i < modelState.Errors.Count; i++) {
//                        var modelError = modelState.Errors[i];
//                        var errorText = ValidationHelpers.GetModelErrorMessageOrDefault(modelError);
//                        errors.Add(errorText);
//                    }
//                }

//                return StatusCode(HttpStatusCode.InternalServerError, new { errors = errors });
//            }

//            model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "Detail");

//            return View(model);
//        }

//        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
//            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
//        }

//        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
//            ModelState.AddModelError(key, errorMessage.ToString());
//        }
//    }
//}