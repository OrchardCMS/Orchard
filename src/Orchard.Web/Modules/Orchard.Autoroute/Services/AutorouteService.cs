using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Alias;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Tokens;
using Orchard.Localization.Services;
using Orchard.Mvc;
using System.Web;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Autoroute.Services {
    public class AutorouteService : Component, IAutorouteService {

        private readonly IAliasService _aliasService;
        private readonly ITokenizer _tokenizer;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IRouteEvents _routeEvents;
        private readonly ICultureManager _cultureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string AliasSource = "Autoroute:View";

        public AutorouteService(
            IAliasService aliasService,
            ITokenizer tokenizer,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IRouteEvents routeEvents,
            ICultureManager cultureManager,
            IHttpContextAccessor httpContextAccessor) {

            _aliasService = aliasService;
            _tokenizer = tokenizer;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _routeEvents = routeEvents;
            _cultureManager = cultureManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateAlias(AutoroutePart part) {

            if (part == null) {
                throw new ArgumentNullException("part");
            }
            var settings = part.TypePartDefinition.Settings.GetModel<AutorouteSettings>();
            var itemCulture = _cultureManager.GetSiteCulture();

            // If we are editing an existing content item.
            if (part.Record.Id != 0) {
                ContentItem contentItem = _contentManager.Get(part.Record.ContentItemRecord.Id);
                var aspect = contentItem.As<ILocalizableAspect>();

                if (aspect != null) {
                    itemCulture = aspect.Culture;
                }
            }

            if (settings.UseCulturePattern) {
                // TODO: Refactor the below so that we don't need to know about Request.Form["Localization.SelectedCulture"].
                // If we are creating from a form post we use the form value for culture.
                var context = _httpContextAccessor.Current();
                var selectedCulture = context.Request.Form["Localization.SelectedCulture"];
                if (!String.IsNullOrEmpty(selectedCulture)) {
                    itemCulture = selectedCulture;
                }
            }

            var pattern = GetDefaultPattern(part.ContentItem.ContentType, itemCulture).Pattern;

            // String.Empty forces pattern based generation.
            if (part.UseCustomPattern && (!String.IsNullOrWhiteSpace(part.CustomPattern))) {
                pattern = part.CustomPattern;
            }

            // Convert the pattern and route values via tokens.
            var path = _tokenizer.Replace(pattern, BuildTokenContext(part.ContentItem), new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });

            // Removing trailing slashes in case the container is empty, and tokens are base on it (e.g. home page).
            while (path.StartsWith("/")) {
                path = path.Substring(1);
            }

            return path;
        }

        public void PublishAlias(AutoroutePart part) {
            var displayRouteValues = _contentManager.GetItemMetadata(part).DisplayRouteValues;
            _aliasService.Replace(part.DisplayAlias, displayRouteValues, AliasSource, true);
            _routeEvents.Routed(part, part.DisplayAlias);
        }

        private IDictionary<string, object> BuildTokenContext(IContent item) {
            return new Dictionary<string, object> { { "Content", item } };
        }

        public void CreatePattern(string contentType, string name, string pattern, string description, bool makeDefault) {
            var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);

            if (contentDefinition == null) {
                throw new OrchardException(T("Unknown content type: {0}", contentType));
            }

            var settings = contentDefinition.Settings.GetModel<AutorouteSettings>();

            var routePattern = new RoutePattern {
                Description = description,
                Name = name,
                Pattern = pattern,
                Culture = _cultureManager.GetSiteCulture()
            };

            var patterns = settings.Patterns;
            patterns.Add(routePattern);
            settings.Patterns = patterns;

            // Define which pattern is the default.
            if (makeDefault || settings.Patterns.Count == 1) {
                settings.DefaultPatterns = new List<DefaultPattern> { new DefaultPattern { PatternIndex = "0", Culture = settings.Patterns[0].Culture } };
            }

            _contentDefinitionManager.AlterTypeDefinition(contentType, builder => builder.WithPart("AutoroutePart", settings.Build));
        }

        public IEnumerable<RoutePattern> GetPatterns(string contentType) {
            var settings = GetTypePartSettings(contentType).GetModel<AutorouteSettings>();
            return settings.Patterns;
        }

        public RoutePattern GetDefaultPattern(string contentType, string culture) {
            var settings = GetTypePartSettings(contentType).GetModel<AutorouteSettings>();

            if (settings.UseCulturePattern) {
                var defaultPatternIndex = "0";

                if (!settings.DefaultPatterns.Any(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
                    // If no default pattern exists for the language return the default culture neutral pattern if it exists, else a generic pattern
                    if (settings.Patterns.Any(x => String.IsNullOrEmpty(x.Culture))) {
                        defaultPatternIndex = GetDefaultPatternIndex(contentType, null);
                        return settings.Patterns.Where(x => String.IsNullOrEmpty(x.Culture)).ElementAt(Convert.ToInt32(defaultPatternIndex));
                    }
                    else {
                        return new RoutePattern { Name = "Title", Description = "my-title", Pattern = "{Content.Slug}", Culture = culture };
                    }
                }

                // If patterns for the specified culture exist search one of them, else search a culture neutral pattern
                var patternCultureSearch = settings.Patterns.Any(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase)) ? culture : null;
                defaultPatternIndex = GetDefaultPatternIndex(contentType, patternCultureSearch);

                if (settings.Patterns.Any()) {
                    if (settings.Patterns.Where(x => x.Culture == patternCultureSearch).ElementAt(Convert.ToInt32(defaultPatternIndex)) != null) {
                        return settings.Patterns.Where(x => x.Culture == patternCultureSearch).ElementAt(Convert.ToInt32(defaultPatternIndex));
                    }
                }
            }
            else {
                // Using the culture neutral pattern
                if (settings.Patterns.Any(x => String.IsNullOrEmpty(x.Culture))) {
                    var defaultPatternIndex = GetDefaultPatternIndex(contentType, null);

                    // If no default culture neutral pattern exist use the default pattern
                    if (!settings.DefaultPatterns.Any(x => String.IsNullOrEmpty(x.Culture))) {
                        var patternIndex = String.IsNullOrWhiteSpace(settings.DefaultPatternIndex) ? "0" : settings.DefaultPatternIndex;
                        settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = patternIndex, Culture = null });
                    }

                    return settings.Patterns.Where(x => String.IsNullOrEmpty(x.Culture)).ElementAt(Convert.ToInt32(defaultPatternIndex));
                }
            }

            // Return a default pattern if none is defined
            return new RoutePattern { Name = "Title", Description = "my-title", Pattern = "{Content.Slug}", Culture = culture };
        }

        public void RemoveAliases(AutoroutePart part) {
            _aliasService.Delete(part.Path, AliasSource);
        }

        public string GenerateUniqueSlug(AutoroutePart part, IEnumerable<string> existingPaths) {
            if (existingPaths == null || !existingPaths.Contains(part.Path))
                return part.Path;

            var version = existingPaths.Select(s => GetSlugVersion(part.Path, s)).OrderBy(i => i).LastOrDefault();

            return version != null
                ? String.Format("{0}-{1}", part.Path, version)
                : part.Path;
        }

        public IEnumerable<AutoroutePart> GetSimilarPaths(string path) {
            return
                _contentManager.Query<AutoroutePart, AutoroutePartRecord>()
                    .Where(part => part.DisplayAlias != null && part.DisplayAlias.StartsWith(path))
                    .List();
        }

        public bool IsPathValid(string slug) {
            return String.IsNullOrWhiteSpace(slug) || Regex.IsMatch(slug, @"^[^:?#\[\]@!$&'()*+,.;=\s\""\<\>\\\|%]+$");
        }

        public bool ProcessPath(AutoroutePart part) {
            var pathsLikeThis = GetSimilarPaths(part.Path).ToArray();

            // Don't include *this* part in the list
            // of slugs to consider for conflict detection.
            pathsLikeThis = pathsLikeThis.Where(p => p.ContentItem.Id != part.ContentItem.Id).ToArray();

            if (pathsLikeThis.Any()) {
                var originalPath = part.Path;
                var newPath = GenerateUniqueSlug(part, pathsLikeThis.Select(p => p.Path));
                part.DisplayAlias = newPath;

                if (originalPath != newPath)
                    return false;
            }

            return true;
        }

        private SettingsDictionary GetTypePartSettings(string contentType) {
            var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);

            if (contentDefinition == null) {
                throw new OrchardException(T("Unknown content type: {0}", contentType));
            }

            return contentDefinition.Parts.First(x => x.PartDefinition.Name == "AutoroutePart").Settings;
        }

        private string GetDefaultPatternIndex(string contentType, string culture) {
            var settings = GetTypePartSettings(contentType).GetModel<AutorouteSettings>();

            DefaultPattern defaultPattern = null;
            if (String.IsNullOrEmpty(culture))
                defaultPattern = settings.DefaultPatterns.FirstOrDefault(x => String.IsNullOrEmpty(x.Culture));
            else
                defaultPattern = settings.DefaultPatterns.FirstOrDefault(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase));

            var defaultPatternIndex = defaultPattern != null ? defaultPattern.PatternIndex : "0";

            if (String.IsNullOrWhiteSpace(defaultPatternIndex))
                defaultPatternIndex = "0";

            return defaultPatternIndex;
        }

        private static int? GetSlugVersion(string path, string potentialConflictingPath) {
            int v;
            var slugParts = potentialConflictingPath.Split(new[] { path }, StringSplitOptions.RemoveEmptyEntries);

            if (slugParts.Length == 0)
                return 2;

            return Int32.TryParse(slugParts[0].TrimStart('-'), out v)
                ? (int?)++v
                : null;
        }
    }
}
