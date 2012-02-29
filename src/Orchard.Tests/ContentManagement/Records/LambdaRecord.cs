using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Records {
    public class LambdaRecord : ContentPartRecord {
        public LambdaRecord() {
            DateTimeStuff = new DateTime(1980,1,1);
        }

        public virtual int IntegerStuff { get; set; }
        public virtual long LongStuff { get; set; }
        public virtual bool BooleanStuff { get; set; }
        public virtual float FloatStuff { get; set; }
        public virtual double DoubleStuff { get; set; }
        public virtual Decimal DecimalStuff { get; set; }
        public virtual string StringStuff { get; set; }
        public virtual DateTime DateTimeStuff { get; set; }
    }
}