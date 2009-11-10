using System;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Orchard.Models.Records {
    public abstract class ModelPartRecord {
        public virtual int Id { get; set; }
        public virtual ModelRecord Model { get; set; }
    }


    public class ModelPartRecordAlteration : IAutoMappingAlteration {
        public void Alter(AutoPersistenceModel model) {

            model.OverrideAll(mapping => {
                var genericArguments = mapping.GetType().GetGenericArguments();
                if (!genericArguments.Single().IsSubclassOf(typeof(ModelPartRecord))) {
                    return;
                }

                var type = typeof(Alteration<>).MakeGenericType(genericArguments);
                var alteration = (IAlteration)Activator.CreateInstance(type);
                alteration.Override(mapping);
            });

        }

        interface IAlteration {
            void Override(object mapping);
        }

        class Alteration<T> : IAlteration where T : ModelPartRecord {
            public void Override(object o) {
                var mapping = (AutoMapping<T>)o;
                mapping.Id(x => x.Id).GeneratedBy.Foreign("Model");
                mapping.HasOne(x => x.Model);
            }
        }
    }

}
