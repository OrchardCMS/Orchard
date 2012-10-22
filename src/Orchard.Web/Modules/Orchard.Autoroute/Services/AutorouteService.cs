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
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;

namespace Orchard.Autoroute.Services {
    public class AutorouteService : IAutorouteService {

        private readonly IAliasService _aliasService;
        private readonly ITokenizer _tokenizer;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IRouteEvents _routeEvents;
        private const string AliasSource = "Autoroute:View";

        public AutorouteService(
            IAliasService aliasService,
            ITokenizer tokenizer,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IRouteEvents routeEvents) {
                _aliasService = aliasService;
                _tokenizer = tokenizer;
                _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _routeEvents = routeEvents;

            Logger = NullLogger.Instance;
                T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public string GenerateAlias(AutoroutePart part) {

            if (part == null) {
                throw new ArgumentNullException("part");
            }

            string pattern = GetDefaultPattern(part.ContentItem.ContentType).Pattern;
            
            // String.Empty forces pattern based generation. "/" forces homepage
            if(part.UseCustomPattern 
                && (!String.IsNullOrWhiteSpace(part.CustomPattern) || String.Equals(part.CustomPattern, "/"))) {
                pattern = part.CustomPattern;
            }

            // Convert the pattern and route values via tokens
            var path = _tokenizer.Replace(pattern, BuildTokenContext(part.ContentItem), new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });

            // removing trailing slashes in case the container is empty, and tokens are base on it (e.g. home page)
            while(path.StartsWith("/")) {
                path = path.Substring(1);
            }

            return path;
        }

        public void PublishAlias(AutoroutePart part) {
            var displayRouteValues = _contentManager.GetItemMetadata(part).DisplayRouteValues;

            _aliasService.Replace(part.DisplayAlias, displayRouteValues, AliasSource);

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
                Pattern = pattern
            };

            var patterns = settings.Patterns;
            patterns.Add(routePattern);
            settings.Patterns = patterns;

            // define which pattern is the default
            if (makeDefault || settings.Patterns.Count == 1) {
                settings.DefaultPatternIndex = settings.Patterns.IndexOf(routePattern);
            }

            _contentDefinitionManager.AlterTypeDefinition(contentType, builder => builder.WithPart("AutoroutePart", settings.Build));
        }

        public IEnumerable<RoutePattern> GetPatterns(string contentType) {
            var settings = GetTypePartSettings(contentType).GetModel<AutorouteSettings>();
            return settings.Patterns;
        }

        public RoutePattern GetDefaultPattern(string contentType) {
            var settings = GetTypePartSettings(contentType).GetModel<AutorouteSettings>();

            // return a default pattern if none is defined
            if(settings.DefaultPatternIndex < settings.Patterns.Count) {
                return settings.Patterns.ElementAt(settings.DefaultPatternIndex);    
            }

            return new RoutePattern {Name = "Title", Description = "my-title", Pattern = "{Content.Slug}"};
        }

        public void RemoveAliases(AutoroutePart part) {
            _aliasService.Delete(part.Path, AliasSource);
        }

        private SettingsDictionary GetTypePartSettings(string contentType) {
            var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            
            if (contentDefinition == null) {
                throw new OrchardException(T("Unknown content type: {0}", contentType));
            }
            
            return contentDefinition.Parts.First(x => x.PartDefinition.Name == "AutoroutePart").Settings;
        }

        public string GenerateUniqueSlug(AutoroutePart part, IEnumerable<string> existingPaths) {
            if (existingPaths == null || !existingPaths.Contains(part.Path))
                return part.Path;

            int? version = existingPaths.Select(s => GetSlugVersion(part.Path, s)).OrderBy(i => i).LastOrDefault();

            return version != null
                ? string.Format("{0}-{1}", part.Path, version)
                : part.Path;
        }

        private static int? GetSlugVersion(string path, string potentialConflictingPath) {
            int v;
            string[] slugParts = potentialConflictingPath.Split(new[] { path }, StringSplitOptions.RemoveEmptyEntries);

            if (slugParts.Length == 0)
                return 2;

            return int.TryParse(slugParts[0].TrimStart('-'), out v)
                       ? (int?)++v
                       : null;
        }

        public IEnumerable<AutoroutePart> GetSimilarPaths(string path) {
            return
                _contentManager.Query<AutoroutePart, AutoroutePartRecord>()
                    .Where(part => part.DisplayAlias != null && part.DisplayAlias.StartsWith(path))
                    .List();
        }

        public bool IsPathValid(string slug) {
            return String.IsNullOrWhiteSpace(slug) || Regex.IsMatch(slug, @"^[^:?#\[\]@!$&'()*+,;=\s\""\<\>\\\|]+$") && !(slug.StartsWith(".") || slug.EndsWith("."));
        }

        public bool ProcessPath(AutoroutePart part) {

            var pathsLikeThis = GetSimilarPaths(part.Path).ToArray();

            // Don't include *this* part in the list
            // of slugs to consider for conflict detection
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
    }
}
