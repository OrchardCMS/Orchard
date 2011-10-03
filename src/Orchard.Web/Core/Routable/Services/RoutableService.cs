using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Events;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.Services {
    public class RoutableService : IRoutableService {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<ISlugEventHandler> _slugEventHandlers;
        private readonly IRoutablePathConstraint _routablePathConstraint;

        public RoutableService(IContentManager contentManager, IEnumerable<ISlugEventHandler> slugEventHandlers, IRoutablePathConstraint routablePathConstraint) {
            _contentManager = contentManager;
            _slugEventHandlers = slugEventHandlers;
            _routablePathConstraint = routablePathConstraint;
        }

        public void FixContainedPaths(IRoutableAspect part) {
            var items = _contentManager.Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(cr => cr.Container.Id == part.Id)
                .List()
                .Select(item => item.As<IRoutableAspect>()).Where(item => item != null);

            foreach (var itemRoute in items) {
                var route = itemRoute.As<IRoutableAspect>();
                
                if(route == null) {
                    continue;
                }

                var path = route.Path;
                route.Path = route.GetPathWithSlug(route.Slug);

                // if the path has changed by having the slug changed on the way in (e.g. user input) or to avoid conflict
                // then update and publish all contained items
                if (path != route.Path) {
                    _routablePathConstraint.RemovePath(path);
                    FixContainedPaths(route);
                }

                if (!string.IsNullOrWhiteSpace(route.Path))
                    _routablePathConstraint.AddPath(route.Path);

            }
        }

        public static string RemoveDiacritics(string slug) {
            string stFormD = slug.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++) {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark) {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public void FillSlugFromTitle<TModel>(TModel model) where TModel : IRoutableAspect {
            if ((model.Slug != null && !string.IsNullOrEmpty(model.Slug.Trim())) || string.IsNullOrEmpty(model.Title))
                return;

            FillSlugContext slugContext = new FillSlugContext(model.Title);

            foreach (ISlugEventHandler slugEventHandler in _slugEventHandlers) {
                slugEventHandler.FillingSlugFromTitle(slugContext);
            }

            if (!slugContext.Adjusted) {
                var disallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s\""\<\>\\]+");

                slugContext.Slug = disallowed.Replace(slugContext.Slug, "-").Trim('-');

                if (slugContext.Slug.Length > 1000)
                    slugContext.Slug = slugContext.Slug.Substring(0, 1000);

                // dots are not allowed at the begin and the end of routes
                slugContext.Slug = RemoveDiacritics(slugContext.Slug.Trim('.').ToLower());
            }

            foreach (ISlugEventHandler slugEventHandler in _slugEventHandlers) {
                slugEventHandler.FilledSlugFromTitle(slugContext);
            }

            model.Slug = slugContext.Slug;
        }

        public string GenerateUniqueSlug(IRoutableAspect part, IEnumerable<string> existingPaths) {
            if (existingPaths == null || !existingPaths.Contains(part.Path))
                return part.Slug;

            int? version = existingPaths.Select(s => GetSlugVersion(part.Path, s)).OrderBy(i => i).LastOrDefault();

            return version != null
                ? string.Format("{0}-{1}", part.Slug, version)
                : part.Slug;
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

        public IEnumerable<IRoutableAspect> GetSimilarPaths(string path) {
            return
                _contentManager.Query<RoutePart, RoutePartRecord>()
                    .List()
                    .Select(i => i.As<RoutePart>())
                    .Where(routable => routable.Path != null && routable.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }

        public bool IsSlugValid(string slug) {
            return String.IsNullOrWhiteSpace(slug) || Regex.IsMatch(slug, @"^[^:?#\[\]@!$&'()*+,;=\s\""\<\>\\]+$") && !(slug.StartsWith(".") || slug.EndsWith("."));
        }

        public bool ProcessSlug(IRoutableAspect part) {
            FillSlugFromTitle(part);

            if (string.IsNullOrEmpty(part.Slug))
                return true;

            part.Path = part.GetPathWithSlug(part.Slug);
            var pathsLikeThis = GetSimilarPaths(part.Path);

            // Don't include *this* part in the list
            // of slugs to consider for conflict detection
            pathsLikeThis = pathsLikeThis.Where(p => p.ContentItem.Id != part.ContentItem.Id);

            if (pathsLikeThis.Count() > 0) {
                var originalSlug = part.Slug;
                var newSlug = GenerateUniqueSlug(part, pathsLikeThis.Select(p => p.Path));
                part.Path = part.GetPathWithSlug(newSlug);
                part.Slug = newSlug;

                if (originalSlug != newSlug)
                    return false;
            }

            return true;
        }
    }

    public static class RoutableAspectExtensions {
        public static string GetContainerPath(this IRoutableAspect routableAspect) {
            var commonAspect = routableAspect.As<ICommonPart>();
            if (commonAspect != null && commonAspect.Container != null) {
                var routable = commonAspect.Container.As<IRoutableAspect>();
                if (routable != null)
                    return routable.Path;
            }
            return null;
        }

        public static string GetPathWithSlug(this IRoutableAspect routableAspect, string slug) {
            var containerPath = routableAspect.GetContainerPath();
            return !string.IsNullOrEmpty(containerPath)
                ? string.Format("{0}/{1}", containerPath, slug)
                : slug;
        }

        public static string GetChildPath(this IRoutableAspect routableAspect, string slug) {
            return string.Format("{0}/{1}", routableAspect.Path, slug);
        }

        public static string GetEffectiveSlug(this IRoutableAspect routableAspect) {
            var containerPath = routableAspect.GetContainerPath();

            if (string.IsNullOrWhiteSpace(routableAspect.Path))
                return "";

            var slugParts = routableAspect.Path.Split(new []{string.Format("{0}/", containerPath)}, StringSplitOptions.RemoveEmptyEntries);
            return slugParts.FirstOrDefault();
        }
    }
}