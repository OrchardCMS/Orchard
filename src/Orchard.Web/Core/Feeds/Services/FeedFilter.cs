using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Mvc.Filters;

namespace Orchard.Core.Feeds.Services {
    [UsedImplicitly]
    public class FeedFilter : FilterProvider, IResultFilter {
        private readonly IFeedManager _feedManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public FeedFilter(IFeedManager feedManager, IWorkContextAccessor workContextAccessor) {
            _feedManager = feedManager;
            _workContextAccessor = workContextAccessor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            _workContextAccessor.GetContext(filterContext).CurrentPage.Zones["Head"].Add(html => html.ViewContext.Writer.Write(_feedManager.GetRegisteredLinks(html)), ":after");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}