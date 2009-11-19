using System;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Orchard.Models.Records {
    public abstract class ContentPartRecord {
        public virtual int Id { get; set; }
        public virtual ContentItemRecord ContentItem { get; set; }
    }


    public class ContentPartRecordAlteration : IAutoMappingAlteration {
        public void Alter(AutoPersistenceModel model) {

            model.OverrideAll(mapping => {
                var genericArguments = mapping.GetType().GetGenericArguments();
                if (!genericArguments.Single().IsSubclassOf(typeof(ContentPartRecord))) {
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

        class Alteration<T> : IAlteration where T : ContentPartRecord {
            public void Override(object o) {
                var mapping = (AutoMapping<T>)o;
                mapping.Id(x => x.Id).GeneratedBy.Foreign("ContentItem");
                mapping.HasOne(x => x.ContentItem);
            }
        }
    }

}
