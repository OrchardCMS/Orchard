using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Lists.Services;
using Orchard.Localization;

namespace Orchard.Lists.Projections {
    public class ListFilter : Component, IFilterProvider {
        private readonly IContentManager _contentManager;

        public ListFilter(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Describe(dynamic describe) {
            describe.For("Lists", T("Lists"), T("Lists"))
                .Element("List", T("List"), T("Specific list"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "ListFilter"
                );
        }

        public void ApplyFilter(dynamic context) {
            var selectedList = (string)context.State.ListId;

            if (String.IsNullOrWhiteSpace(selectedList))
                return;

            var query = (IHqlQuery)context.Query;
            var listId = Int32.Parse(selectedList);
            context.Query = query.Where(alias => alias.ContentPartRecord<CommonPartRecord>(), item => item.Eq("Container", listId));
        }

        public LocalizedString DisplayFilter(dynamic context) {
            var selectedList = (string)context.State.ListId;

            if (String.IsNullOrWhiteSpace(selectedList))
                return T("No list was selected");

            var listId = Int32.Parse(selectedList);
            var list = _contentManager.Get<ContainerPart>(listId, VersionOptions.Latest, QueryHints.Empty);
            var listName = _contentManager.GetItemMetadata(list).DisplayText;
            return T("Show items from the following list: {0}", listName);
        }
    }
}