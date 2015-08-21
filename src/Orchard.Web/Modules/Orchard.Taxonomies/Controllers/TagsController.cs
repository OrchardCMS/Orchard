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

namespace Orchard.Taxonomies.Controllers {
    public class TagsController : ApiController {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentManager _contentManager;
        public Localizer T { get; set; }
        protected ILogger Logger { get; set; }

        public TagsController(
            ITaxonomyService taxonomyService,
            IContentManager contentManager) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public IEnumerable<TagDto> Get(int taxonomyId, bool leavesOnly, string query) {
            if (string.IsNullOrEmpty(query)) return new List<TagDto>();
            var allTerms = leavesOnly
                               ? _taxonomyService.GetTerms(taxonomyId).ToList()
                               : new List<TermPart>();
            var matchingTerms = _contentManager.Query<TermPart, TermPartRecord>()
                                               .Where(t => t.TaxonomyId == taxonomyId)
                                               .Join<TitlePartRecord>()
                                               .Where(r => r.Title.Contains(query))
                                               .List()
                                               .Select(t => CreateTagDto(t, leavesOnly, allTerms))
                                               .OrderBy(t => t.label)
                                               .ToList();
            return matchingTerms;
        }

        private static TagDto CreateTagDto(TermPart term, bool leavesOnly, IEnumerable<TermPart> terms) {
            return new TagDto {
                value = term.Id,
                label = term.Name,
                disabled = !term.Selectable || (leavesOnly && terms.Any(t => t.Path.Contains(term.Path + term.Id))),
                levels = term.GetLevels()
            };
        }
    }
    public class TagDto {
        public string label { get; set; }
        public int value { get; set; }
        public int levels { get; set; }
        public bool disabled { get; set; }
    }
}