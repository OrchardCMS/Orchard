using System;

namespace Orchard.Tests.Records {
    public class FooRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime? Timespan { get; set; }
    }
}