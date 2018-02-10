using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;

namespace Orchard.ContentManagement {
    public interface IGlobalCriteriaProvider : IDependency {
        void AddCriteria(ICriteria criteria);
    }
}
