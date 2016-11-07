using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Feeds.Models;
using Orchard.Logging;

namespace Orchard.Core.Feeds.Controllers {
    public class FeedController : Controller {
        private readonly IEnumerable<IFeedBuilderProvider> _feedFormatProviders;
        private readonly IEnumerable<IFeedQueryProvider> _feedQueryProviders;
        private readonly IFeedItemBuilder _feedItemBuilder;

        public FeedController(
            IEnumerable<IFeedQueryProvider> feedQueryProviders,
            IEnumerable<IFeedBuilderProvider> feedFormatProviders,
            IFeedItemBuilder feedItemBuilder) {
            _feedQueryProviders = feedQueryProviders;
            _feedFormatProviders = feedFormatProviders;
            _feedItemBuilder = feedItemBuilder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ActionResult Index(string format) {
            var context = new FeedContext(ValueProvider, format);

            var bestFormatterMatch = _feedFormatProviders
                .Select(provider => provider.Match(context))
                .Where(match => match != null && match.FeedBuilder != null)
                .OrderByDescending(match => match.Priority)
                .FirstOrDefault();

            if (bestFormatterMatch == null || bestFormatterMatch.FeedBuilder == null)
                return HttpNotFound();

            context.Builder = bestFormatterMatch.FeedBuilder;

            var bestQueryMatch = _feedQueryProviders
                .Select(provider => provider.Match(context))
                .Where(match => match != null && match.FeedQuery != null)
                .OrderByDescending(match => match.Priority)
                .FirstOrDefault();

            if (bestQueryMatch == null || bestQueryMatch.FeedQuery == null)
                return HttpNotFound();

            return context.Builder.Process(context, () => {
                bestQueryMatch.FeedQuery.Execute(context);
                _feedItemBuilder.Populate(context);
                foreach (var contextualizer in context.Response.Contextualizers) {
                    if (ControllerContext != null &&
                        ControllerContext.RequestContext != null) {
                        contextualizer(ControllerContext.RequestContext);
                    }
                }
            });
        }
    }
}
