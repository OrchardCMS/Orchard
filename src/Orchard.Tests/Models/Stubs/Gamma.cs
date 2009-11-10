using System;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Tests.Models.Stubs {
    public class Gamma : ModelPart<GammaRecord> {
    }

    public class GammaRecord {
        public virtual int Id { get; set; }
        public virtual ModelRecord Model { get; set; }
        public virtual string Frap { get; set; }
    }

    public class GammaRecordOverride : IAutoMappingOverride<GammaRecord> {
        public void Override(AutoMapping<GammaRecord> mapping) {
            mapping.HasOne(x => x.Model);
        }
    } 

    public class GammaDriver : ModelDriver<GammaRecord> {
        public GammaDriver(IRepository<GammaRecord> repository) : base(repository) {
        }

        protected override void New(NewModelContext context) {
            if(context.ModelType == "gamma") {
                WeldModelPart<Gamma>(context);
            }
        }
    }
}
