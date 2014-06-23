using System.Linq;
using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Services;

namespace Orchard.Lists.ListViews {
    public class CondensedListView : ListViewProviderBase {
        private readonly IContentManager _contentManager;
        public CondensedListView(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public override int Priority {
            get { return 1; }
        }

        public override async Task<dynamic> BuildDisplayAsync(BuildListViewDisplayContext context) {
            var pagerShape = context.New.Pager(context.Pager).TotalItemCount(context.ContentQuery.Count());
            var pageOfContentItems = context.ContentQuery.Slice(context.Pager.GetStartIndex(), context.Pager.PageSize);
            
            var shapeTasks = pageOfContentItems.Select(x => _contentManager.BuildDisplayAsync(x, "SummaryAdmin")).ToList();

            await Task.WhenAll(shapeTasks);
            return context.New.ListView_Condensed()
                .Container(context.Container)
                .ContainerDisplayName(context.ContainerDisplayName)
                .ContentItems(shapeTasks.Select(task => task.Result))
                .Pager(pagerShape);
        }
    }
}