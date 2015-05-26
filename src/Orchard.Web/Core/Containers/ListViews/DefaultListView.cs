using System.Linq;
using System.Threading.Tasks;
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

        public override async Task<dynamic> BuildDisplayAsync(BuildListViewDisplayContext context) {
            var pagerShape = context.New.Pager(context.Pager).TotalItemCount(context.ContentQuery.Count());
            var pageOfContentItems = context.ContentQuery.Slice(context.Pager.GetStartIndex(), context.Pager.PageSize);
            var itemTasks = pageOfContentItems.Select(x => _contentManager.BuildDisplayAsync(x, "SummaryAdmin")).ToList();

            await Task.WhenAll(itemTasks);

            var items = itemTasks.Select(task => task.Result).ToList();

            return context.New.ListView_Default()
                .Container(context.Container)
                .ContainerDisplayName(context.ContainerDisplayName)
                .ContentItems(items)
                .Pager(pagerShape);
        }
    }
}