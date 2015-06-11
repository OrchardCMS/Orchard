using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Helpers;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.Projections.Services;

namespace IDeliverable.Slides.Providers
{
    [OrchardFeature("IDeliverable.Slides.Projections")]
    public class ProjectionSlidesProvider : SlidesProvider
    {
        private readonly IProjectionManager _projectionManager;
        private readonly IContentManager _contentManager;

        public ProjectionSlidesProvider(IProjectionManager projectionManager, IOrchardServices services)
        {
            _projectionManager = projectionManager;
            _contentManager = services.ContentManager;
        }

        public override LocalizedString DisplayName
        {
            get { return T("Projection"); }
        }

        public override dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context)
        {
            return UpdateEditor(shapeFactory, context: context, updater: null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater)
        {
            var viewModel = new ProjectionSlidesProviderViewModel
            {
                QueryOptions = GetQueries().ToList(),
                SelectedQueryId = context.Storage.RetrieveQueryId(),
                DisplayType = context.Storage.RetrieveProjectionSlidesDisplayType()
            };

            if (updater != null)
            {
                if (updater.TryUpdateModel(viewModel, Prefix, new[] { "SelectedQueryId", "DisplayType" }, null))
                {
                    context.Storage.StoreQueryId(viewModel.SelectedQueryId);
                    context.Storage.StoreProjectionSlidesDisplayType(viewModel.DisplayType.TrimSafe());
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "SlidesProviders.Projection", Model: viewModel, Prefix: Prefix);
        }

        public override IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context)
        {
            var queryId = context.Storage.RetrieveQueryId();

            if (queryId == null)
                yield break;

            var query = _contentManager.Get<QueryPart>(queryId.Value);
            var displayType = context.Storage.RetrieveProjectionSlidesDisplayType();
            var contentItems = _projectionManager.GetContentItems(query.Id).ToList();

            foreach (var contentItem in contentItems)
            {
                var contentShape = _contentManager.BuildDisplay(contentItem, displayType);
                yield return contentShape;
            }
        }

        private IEnumerable<SelectListItem> GetQueries()
        {
            var queries = _contentManager.Query<QueryPart, QueryPartRecord>().Join<TitlePartRecord>().OrderBy(x => x.Title).List().ToArray();

            foreach (var query in queries.OrderBy(x => x.Name))
            {
                yield return new SelectListItem { Text = query.Name, Value = query.Id.ToString() };
            }
        }
    }
}