using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Http;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System.Linq;
using Orchard.Security;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Controllers {
    public class TagsController : ApiController {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentManager _contentManager;
	    private readonly IAuthorizer _authorizer;
        public Localizer T { get; set; }
        protected ILogger Logger { get; set; }

        public TagsController(
            ITaxonomyService taxonomyService,
            IContentManager contentManager,
			IAuthorizer authorizer) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
	        _authorizer = authorizer;
            Logger = NullLogger.Instance;
        }

        public IEnumerable<Tag> Get(int taxonomyId, bool leavesOnly, string query) {
	        if (!_authorizer.Authorize(StandardPermissions.AccessAdminPanel)) {
		        throw new UnauthorizedAccessException("Can't access the admin");
	        }
            var allTerms = leavesOnly
                               ? _taxonomyService.GetTerms(taxonomyId).ToList()
                               : new List<TermPart>();

            var matchingTerms = _contentManager.Query<TermPart, TermPartRecord>()
                                               .Where(t => t.TaxonomyId == taxonomyId)
                                               .Join<TitlePartRecord>();

            if (!string.IsNullOrEmpty(query))
                matchingTerms = matchingTerms.Where(r => r.Title.Contains(query));

            var resultingTerms = matchingTerms.List()
                                              .Select(t => BuildTag(t, leavesOnly, allTerms))
                                              .OrderBy(t => t.Label)
                                              .ToList();

            return resultingTerms;
        }

        private static Tag BuildTag(TermPart term, bool leavesOnly, IEnumerable<TermPart> terms) {
            return new Tag {
                Value = term.Id,
                Label = term.Name,
                Disabled = !term.Selectable || (leavesOnly && terms.Any(t => t.Path.Contains(term.Path + term.Id))),
                Levels = term.GetLevels()
            };
        }
    }
}