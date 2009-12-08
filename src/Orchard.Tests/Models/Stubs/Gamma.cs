using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Tests.Models.Records;

namespace Orchard.Tests.Models.Stubs {
    public class Gamma : ContentPart<GammaRecord> {
    }


    public class GammaHandler : ContentHandler {
        public override System.Collections.Generic.IEnumerable<Orchard.Models.ContentType> GetContentTypes() {
            return new[] { new ContentType { Name = "gamma" } };
        }

        public GammaHandler(IRepository<GammaRecord> repository) {
            Filters.Add(new ActivatingFilter<Gamma>(x => x == "gamma"));
            Filters.Add(new StorageFilter<GammaRecord>(repository));
        }
    }


    //public class ContentItemRecordAlteration : IAutoMappingAlteration {
    //    public void Alter(AutoPersistenceModel model) {
    //        model.OverrideAll(mapping => {
    //                              var genericArguments = mapping.GetType().GetGenericArguments();
    //                              if (!genericArguments.Single().IsSubclassOf(typeof (ContentPartRecord))) {
    //                                  return;
    //                              }
    //                          });

    //        model.Override<ContentItemRecord>(mapping => mapping.HasOne(record => (GammaRecord)record["GammaRecord"]).Access.NoOp().Fetch.Select());


    //    }

    //    interface IAlteration {
    //        void Override(object mapping);
    //    }

    //    class Alteration<T> : IAlteration where T : ContentPartRecord {
    //        public void Override(object mappingObj) {
    //            var mapping = (AutoMapping<T>)mappingObj;
    //            mapping.Id(x => x.Id).GeneratedBy.Foreign("ContentItem");
    //            mapping.HasOne(x => x.ContentItem).Constrained();
    //        }
    //    }
    //}
}
