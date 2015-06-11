using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Comments.Models;

namespace Orchard.Comments.Projections {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class CommentsFilter : IFilterProvider {

        public CommentsFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Comments", T("Comments"), T("Comments"))
                .Element("HasComments", T("Has Comments"), T("Commented content items"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    null
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;
            context.Query = query.Where(x => x.ContentPartRecord<CommentsPartRecord>(), x => x.IsNotEmpty("CommentPartRecords"));
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Has comments");
        }
    }
}