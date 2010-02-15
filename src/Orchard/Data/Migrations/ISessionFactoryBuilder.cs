using System.Collections.Generic;
using NHibernate;
using Orchard.Environment;

namespace Orchard.Data.Migrations {

    public interface ISessionFactoryBuilder : IDependency {
        ISessionFactory BuildSessionFactory(SessionFactoryParameters sessionFactoryParameters);
    }

    public class SessionFactoryParameters {
        public IEnumerable<RecordDescriptor> RecordDescriptors { get; set; }
        public bool UpdateSchema { get; set; }

        public string Provider { get; set; }
        public string DataFolder { get; set; }
        public string ConnectionString { get; set; }
    }
}
