using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Services;

namespace Orchard.Core.Containers.ListViews {
    public class DefaultListView : ListViewProviderBase {
        private readonly IContentManager _contentManager;
        public DefaultListView(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public override int Priority {
            get { return 0; }
        }

        public override dynamic BuildDisplay(BuildListViewDisplayContext context) {
            var pagerShape = context.New.Pager(context.Pager).TotalItemCount(context.ContentQuery.Count());
            var pageOfContentItems = context.ContentQuery.Slice(context.Pager.GetStartIndex(), context.Pager.PageSize).Select(x => _contentManager.BuildDisplay(x, "SummaryAdmin")).ToList();
            return context.New.ListView_Default()
                .Container(context.Container)
                .ContainerDisplayName(context.ContainerDisplayName)
                .ContentItems(pageOfContentItems)
                .Pager(pagerShape);
        }
    }
}