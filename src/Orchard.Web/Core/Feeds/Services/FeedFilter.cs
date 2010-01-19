using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;


namespace Orchard.Core.Feeds.Services {
    [UsedImplicitly]
    public class FeedFilter : FilterProvider, IResultFilter {
        private readonly IFeedManager _feedManager;

        public FeedFilter(IFeedManager feedManager) {
            _feedManager = feedManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var model = filterContext.Controller.ViewData.Model as BaseViewModel;
            if (model == null) {
                return;
            }

            model.Zones.AddAction("head:after", html => html.ViewContext.Writer.Write(_feedManager.GetRegisteredLinks(html)));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}
