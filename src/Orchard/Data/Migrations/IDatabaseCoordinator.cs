using System.Collections.Generic;
using NHibernate;
using Orchard.Environment;

namespace Orchard.Data.Migrations {
    public interface IDatabaseCoordinator {
        bool CanConnect();
        void CreateDatabase();
        
        /// <summary>
        /// Should only be called in a development or evaluation environment. Automatic schema migration
        /// not a really safe practice on production data sources.
        /// </summary>
        /// <param name="recordDescriptors">Set of known records to be applied</param>
        void UpdateSchema(IEnumerable<RecordDescriptor> recordDescriptors);

        ISessionFactory BuildSessionFactory(IEnumerable<RecordDescriptor> recordDescriptors);
    }
}
