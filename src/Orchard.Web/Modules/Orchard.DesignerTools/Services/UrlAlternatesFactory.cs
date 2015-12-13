using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Mvc;

namespace Orchard.DesignerTools.Services {
    [OrchardFeature("UrlAlternates")]
    public class UrlAlternatesFactory : ShapeDisplayEvents {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Lazy<List<string>> _urlAlternates;

        public UrlAlternatesFactory(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;

            _urlAlternates = new Lazy<List<string>>(() => {
                var httpContext = _httpContextAccessor.Current();

                if (httpContext == null) {
                    return null;
                }

                var request = httpContext.Request;

                if (request == null) {
                    return null;
                }

                // extract each segment of the url
                var urlSegments = VirtualPathUtility.ToAppRelative(request.Path.ToLower())
                    .Split('/')
                    .Skip(1) // ignore the heading ~ segment 
                    .Select(url => url.Replace("-", "__").Replace(".", "_")) // format the alternate
                    .ToArray();

                if (String.IsNullOrWhiteSpace(urlSegments[0])) {
                    urlSegments[0] = "homepage";
                }

                return Enumerable.Range(1, urlSegments.Count()).Select(range => String.Join("__", urlSegments.Take(range))).ToList();
            });
        }

        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata.OnDisplaying(displayedContext => {

                if (_urlAlternates.Value == null || !_urlAlternates.Value.Any()) {
                    return;
                }

                // prevent applying alternate again, c.f. https://github.com/OrchardCMS/Orchard/issues/2125
                if(displayedContext.ShapeMetadata.Alternates.Any(x => x.Contains("__url__"))) {
                    return;
                }

                // appends Url alternates to current ones
                displayedContext.ShapeMetadata.Alternates = displayedContext.ShapeMetadata.Alternates.SelectMany(
                    alternate => new [] { alternate }.Union(_urlAlternates.Value.Select(a => alternate + "__url__" + a))
                    ).ToList();

                // appends [ShapeType]__url__[Url] alternates
                displayedContext.ShapeMetadata.Alternates = _urlAlternates.Value.Select(url => displayedContext.ShapeMetadata.Type + "__url__" + url)
                    .Union(displayedContext.ShapeMetadata.Alternates)
                    .ToList();
            });

        }
    }
}