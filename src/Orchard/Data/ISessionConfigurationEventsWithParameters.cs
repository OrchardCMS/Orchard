using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using Orchard.Utility;
using Orchard.Data.Providers;

namespace Orchard.Data {
    /// <summary>
    /// Add ability for the configuration event handler be aware of parameters
    /// </summary>
    /// <param name="parameters"></param>
    public interface ISessionConfigurationEventsWithParameters : ISessionConfigurationEvents {
        SessionFactoryParameters Parameters { set; get; }
    }
}

// usage sample
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using FluentNHibernate.Automapping;
//using FluentNHibernate.Cfg;
//using FluentNHibernate.Conventions;
//using FluentNHibernate.Conventions.Instances;
//using NHibernate.Cfg;
//using Orchard.Data;
//using Orchard.Environment.ShellBuilders.Models;
//using Orchard.Utility;

//namespace usage_example {

//    public class PersistenceConfiguration : ISessionConfigurationEventsWithParameters {
//        Orchard.Data.Providers.SessionFactoryParameters _parameters;

//        public PersistenceConfiguration() {
//        }

//        public void SetParameters(Orchard.Data.Providers.SessionFactoryParameters parameters) {
//            _parameters = parameters;
//        }

//        public void Created(FluentConfiguration cfg, AutoPersistenceModel defaultModel) {
//            Dictionary<Type, RecordBlueprint> descriptors = _parameters.RecordDescriptors.ToDictionary(d => d.Type);
//            defaultModel.Conventions.Add(new IbnJoinedSubclassConvention(descriptors));
//            defaultModel.OverrideAll(map => {
//                map.IgnoreProperties(x => x.MemberInfo.IsDefined(typeof(DoNotMapAttribute), false));
//            });
//        }

//        public void Prepared(FluentConfiguration cfg) {
//        }

//        public void Building(Configuration cfg) {
//        }

//        public void Finished(Configuration cfg) {
//        }

//        public void ComputingHash(Hash hash) {
//        }
//    }


//    public class IbnJoinedSubclassConvention : IJoinedSubclassConvention {
//        private readonly Dictionary<Type, RecordBlueprint> _descriptors;

//        public IbnJoinedSubclassConvention(Dictionary<Type, RecordBlueprint> descriptors) {
//            _descriptors = descriptors;
//        }

//        public void Apply(IJoinedSubclassInstance instance) {
//            if (instance.EntityType.FullName.StartsWith("Ibn")) {
//                instance.Key.Column("Id");
//                RecordBlueprint desc;
//                if (_descriptors.TryGetValue(instance.EntityType, out desc)) {
//                    instance.Table(desc.TableName);
//                }
//            }
//        }
//    }


//    public class DoNotMapAttribute : Attribute {
//    }

//}