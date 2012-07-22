using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Diagnostics;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Event.Default;
using NHibernate.Persister.Entity;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Providers {
    [Serializable]
    public abstract class AbstractDataServicesProvider : IDataServicesProvider {

        public abstract IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase);

        public Configuration BuildConfiguration(SessionFactoryParameters parameters) {
            var database = GetPersistenceConfigurer(parameters.CreateDatabase);
            var persistenceModel = CreatePersistenceModel(parameters.RecordDescriptors.ToList());

            return Fluently.Configure()
                .Database(database)
                .Mappings(m => m.AutoMappings.Add(persistenceModel))
                .ExposeConfiguration(cfg => cfg.EventListeners.LoadEventListeners = new ILoadEventListener[] { new OrchardLoadEventListener() })
                .BuildConfiguration()
                ;
        }

        public static AutoPersistenceModel CreatePersistenceModel(ICollection<RecordBlueprint> recordDescriptors) {
            if(recordDescriptors == null) {
                throw new ArgumentNullException("recordDescriptors");
            }

            return AutoMap.Source(new TypeSource(recordDescriptors))
                // Ensure that namespaces of types are never auto-imported, so that 
                // identical type names from different namespaces can be mapped without ambiguity
                .Conventions.Setup(x => x.Add(AutoImport.Never()))
                .Conventions.Add(new RecordTableNameConvention(recordDescriptors))
                .Conventions.Add(new CacheConvention(recordDescriptors))
                .Alterations(alt => {
                    foreach (var recordAssembly in recordDescriptors.Select(x => x.Type.Assembly).Distinct()) {
                        alt.Add(new AutoMappingOverrideAlteration(recordAssembly));
                    }
                    alt.AddFromAssemblyOf<DataModule>();
                    alt.Add(new ContentItemAlteration(recordDescriptors));
                })
                .Conventions.AddFromAssemblyOf<DataModule>();
        }

        [Serializable]
        class TypeSource : ITypeSource {
            private readonly IEnumerable<RecordBlueprint> _recordDescriptors;

            public TypeSource(IEnumerable<RecordBlueprint> recordDescriptors) { _recordDescriptors = recordDescriptors; }

            public IEnumerable<Type> GetTypes() { return _recordDescriptors.Select(descriptor => descriptor.Type); }
            
            public void LogSource(IDiagnosticLogger logger) {
                throw new NotImplementedException();
            }

            public string GetIdentifier() {
                throw new NotImplementedException();
            }
        }

        [Serializable]
        class OrchardLoadEventListener : DefaultLoadEventListener, ILoadEventListener {

            public new void OnLoad(LoadEvent @event, LoadType loadType) {
                var source = (ISessionImplementor)@event.Session;
                IEntityPersister entityPersister;
                if (@event.InstanceToLoad != null) {
                    entityPersister = source.GetEntityPersister(null, @event.InstanceToLoad);
                    @event.EntityClassName = @event.InstanceToLoad.GetType().FullName;
                }
                else
                    entityPersister = source.Factory.GetEntityPersister(@event.EntityClassName);
                if (entityPersister == null)
                    throw new HibernateException("Unable to locate persister: " + @event.EntityClassName);

                //a hack to handle unused ContentPartRecord proxies on ContentItemRecord or ContentItemVersionRecord.
                //I don't know why it actually works, or how to do it right

                //if (!entityPersister.IdentifierType.IsComponentType)
                //{
                //    Type returnedClass = entityPersister.IdentifierType.ReturnedClass;
                //    if (returnedClass != null && !returnedClass.IsInstanceOfType(@event.EntityId))
                //        throw new TypeMismatchException(string.Concat(new object[4]
                //    {
                //      (object) "Provided id of the wrong type. Expected: ",
                //      (object) returnedClass,
                //      (object) ", got ",
                //      (object) @event.EntityId.GetType()
                //    }));
                //}

                var keyToLoad = new EntityKey(@event.EntityId, entityPersister, source.EntityMode);

                if (loadType.IsNakedEntityReturned) {
                    @event.Result = Load(@event, entityPersister, keyToLoad, loadType);
                }
                else if (@event.LockMode == LockMode.None) {
                    @event.Result = ProxyOrLoad(@event, entityPersister, keyToLoad, loadType);
                }
                else {
                    @event.Result = LockAndLoad(@event, entityPersister, keyToLoad, loadType, source);
                }
            }
        }
    }
}