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
        private readonly List<string> _urlAlternates;

        public UrlAlternatesFactory(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;

            var request = _httpContextAccessor.Current().Request;

                // extract each segment of the url
                var urlSegments = VirtualPathUtility.ToAppRelative(request.Path.ToLower())
                    .Split('/')
                    .Skip(1) // ignore the heading ~ segment 
                    .Select(url => url.Replace("-", "__").Replace(".", "_")) // format the alternate
                    .ToArray();

                if ( String.IsNullOrWhiteSpace(urlSegments[0]) ) {
                    urlSegments[0] = "homepage";
                }

                _urlAlternates = Enumerable.Range(1, urlSegments.Count()).Select(range => String.Join("__", urlSegments.Take(range))).ToList();
        }

        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata.OnDisplaying(displayedContext => {
                // appends Url alternates to current ones
                displayedContext.ShapeMetadata.Alternates = displayedContext.ShapeMetadata.Alternates.SelectMany(
                    alternate => new [] { alternate }.Union(_urlAlternates.Select(a => alternate + "__url__" + a))
                    ).ToList();

                // appends [ShapeType]__url__[Url] alternates
                displayedContext.ShapeMetadata.Alternates = _urlAlternates.Select(url => displayedContext.ShapeMetadata.Type + "__url__" + url)
                    .Union(displayedContext.ShapeMetadata.Alternates)
                    .ToList();
            });

        }
    }
}