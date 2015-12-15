using System;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Orchard.ContentManagement.Records {
    public class ContentPartAlteration : IAutoMappingAlteration {
        public void Alter(AutoPersistenceModel model) {

            model.OverrideAll(mapping => {
                var recordType = mapping.GetType().GetGenericArguments().Single();

                if (Utility.IsPartRecord(recordType)) {
                    var type = typeof(ContentPartAlterationInternal<>).MakeGenericType(recordType);
                    var alteration = (IAlteration)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
                else if (Utility.IsPartVersionRecord(recordType)) {
                    var type = typeof(ContentPartVersionAlterationInternal<>).MakeGenericType(recordType);
                    var alteration = (IAlteration)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
            });

        }

        interface IAlteration {
            void Override(object mapping);
        }

        class ContentPartAlterationInternal<T> : IAlteration where T : ContentPartRecord {
            public void Override(object mappingObj) {
                var mapping = (AutoMapping<T>)mappingObj;

                mapping.Id(x => x.Id)
                    .GeneratedBy.Foreign("ContentItemRecord");

                mapping.HasOne(x => x.ContentItemRecord)
                    .Constrained();
            }
        }

        class ContentPartVersionAlterationInternal<T> : IAlteration where T : ContentPartVersionRecord {
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