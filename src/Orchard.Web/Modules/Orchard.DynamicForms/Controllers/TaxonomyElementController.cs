using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.Layouts.Services;
using Orchard.Security;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Themes;
using Orchard.DynamicForms.Helpers;
using Orchard.Tokens;
using System;
using Orchard.ContentManagement;

namespace Orchard.DynamicForms.Controllers
{
    [Themed(false)]
    public class TaxonomyElementController : Controller
    {
        private readonly ILayoutManager _layoutManager;
        private readonly IFormService _formService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITokenizer _tokenizer;
        private readonly IContentManager _contentManager; 

        public TaxonomyElementController(ILayoutManager layoutManager, 
                                        IFormService formService, 
                                        ITaxonomyService taxonomyService, 
                                        ITokenizer tokenizer,
                                        IAuthenticationService authenticationService,
                                        IAuthorizationService authorizationService,
                                        IContentManager contentManager) {
            _layoutManager = layoutManager;
            _formService = formService;
            _taxonomyService = taxonomyService;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _tokenizer = tokenizer;
            _contentManager = contentManager;
        }

        public ActionResult GetChildrenTerms(int contentId, string formName, string elementName, string parentTermIds) {
            if (string.IsNullOrWhiteSpace(parentTermIds))
                return new HttpNotFoundResult();
            var layoutPart = _layoutManager.GetLayout(contentId);
            if (layoutPart == null)
                return new HttpNotFoundResult();
            var form = _formService.FindForm(layoutPart, formName);
            if (form == null)
                return new HttpNotFoundResult();
            var element = _formService.GetFormElements(form).FirstOrDefault(e => e.Name == elementName) as Taxonomy;
            if (element == null || !element.TaxonomyId.HasValue)
                return new HttpNotFoundResult();
            
            var user = _authenticationService.GetAuthenticatedUser();
            
            if (!_authorizationService.TryCheckAccess(Permissions.SubmitAnyForm, user, layoutPart.ContentItem, formName)
                &&
                Permissions.GetOwnerVariation(Permissions.SubmitAnyForm) != null
                &&
                !(_authorizationService.TryCheckAccess(Permissions.GetOwnerVariation(Permissions.SubmitAnyForm), user, layoutPart.ContentItem, formName))
                )
                return new HttpUnauthorizedResult();

            var parentTerms = _contentManager.GetMany<TermPart>(parentTermIds.Split(',').Select(int.Parse).Where(id => id > 0),VersionOptions.Published, QueryHints.Empty).Where(t=>t.TaxonomyId == element.TaxonomyId.Value);
            
            if (parentTerms == null || !parentTerms.Any())
                return new HttpNotFoundResult();

            List<TermPart> terms = new List<TermPart>();
            foreach (var parentTerm in parentTerms) {
                terms.AddRange(_taxonomyService.GetChildren(parentTerm, false, element.LevelsToRender.GetValueOrDefault()));
            }
            var projection = terms.GetSelectListItems(element, _tokenizer);
            var result = new List<object>();

            foreach (var item in projection) {
                result.Add(new {
                    text = item.Text,
                    value = item.Value,                    
                });
            }

            // Return JSON
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}