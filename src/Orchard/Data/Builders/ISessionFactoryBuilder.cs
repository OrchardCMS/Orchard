using System.Collections.Generic;
using NHibernate;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;

namespace Orchard.Data.Builders {

    public interface ISessionFactoryBuilder : IDependency {
        ISessionFactory BuildSessionFactory(SessionFactoryParameters sessionFactoryParameters);
    }

    public class SessionFactoryParameters {
        public string Provider { get; set; }
        public string DataFolder { get; set; }
        public string ConnectionString { get; set; }

        public bool CreateDatabase { get; set; }
        public bool UpdateSchema { get; set; }

        public IEnumerable<RecordTopology> RecordDescriptors { get; set; }
    }
}
