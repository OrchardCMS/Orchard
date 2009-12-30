using System;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Orchard.ContentManagement.Records {
    public class ContentPartVersionRecordAlteration : IAutoMappingAlteration {
        public void Alter(AutoPersistenceModel model) {

            model.OverrideAll(mapping => {
                var genericArguments = mapping.GetType().GetGenericArguments();
                if (!genericArguments.Single().IsSubclassOf(typeof(ContentPartVersionRecord))) {
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

        class Alteration<T> : IAlteration where T : ContentPartVersionRecord {
            public void Override(object mappingObj) {
                var mapping = (AutoMapping<T>)mappingObj;

                mapping.Id(x => x.Id)
                    .GeneratedBy.Foreign("ContentItemVersionRecord");

                mapping.HasOne(x => x.ContentItemVersionRecord)
                    .Constrained();
            }
        }
    }
}