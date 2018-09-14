﻿using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Projections.Models;

namespace Orchard.Projections.Descriptors.SortCriterion {
    public class SortCriterionContext {
        public SortCriterionContext() {
            Tokens = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }
        public IHqlQuery Query { get; set; }

        public QueryPartRecord QueryPartRecord { get; set; }
        public string GetSortColumnName() {
            return QueryPartRecord != null && QueryPartRecord.VersionScope == QueryVersionScopeOptions.Latest ? "LatestValue" : "Value";
        }
    }
}