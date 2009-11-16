using System;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Tests.Models.Stubs {
    public class Gamma : ModelPartWithRecord<GammaRecord> {
    }

    public class GammaRecord : ModelPartRecord {
        public virtual string Frap { get; set; }
    }


    public class GammaDriver : ModelDriverWithRecord<GammaRecord> {
        public GammaDriver(IRepository<GammaRecord> repository)
            : base(repository) {
        }

        protected override void New(NewModelContext context) {
            if (context.ModelType == "gamma") {
                context.Builder.Weld<Gamma>();
            }
        }
    }
}
