using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace Orchard.Projections.Models {
    public class QueryPart : ContentPart<QueryPartRecord> {

        public string Name {
            get { return this.As<TitlePart>().Title;  }
            set { this.As<TitlePart>().Title = value; }
        }

        public IList<SortCriterionRecord> SortCriteria {
            get { return Record.SortCriteria; }
        }

        public IList<FilterGroupRecord> FilterGroups {
            get { return Record.FilterGroups; }
        }

        public IList<LayoutRecord> Layouts {
            get { return Record.Layouts; }
        }
    }
}