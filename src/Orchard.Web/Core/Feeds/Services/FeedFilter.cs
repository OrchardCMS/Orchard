using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Mvc.Filters;
using Orchard.UI;

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
            IPage page  =_workContextAccessor.GetContext(filterContext).Page;
            page.Zones["Head"].Add((HtmlHelper html) => html.ViewContext.Writer.Write(_feedManager.GetRegisteredLinks(html)), ":after");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}