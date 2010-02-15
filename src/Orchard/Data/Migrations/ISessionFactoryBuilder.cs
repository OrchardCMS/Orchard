using System.Collections.Generic;
using NHibernate;
using Orchard.Environment;

namespace Orchard.Data.Migrations {
    public interface ISessionFactoryBuilder {
        ISessionFactory BuildSessionFactory(SessionFactoryBuilderParameters parameters);
    }

    public class SessionFactoryBuilderParameters {
        public IEnumerable<RecordDescriptor> RecordDescriptors { get; set; }
        public bool CreateDatabase { get; set; }
        public bool UpdateSchema { get; set; }
    }
}
