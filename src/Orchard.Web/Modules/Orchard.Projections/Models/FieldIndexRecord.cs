using System.ComponentModel.DataAnnotations;

namespace Orchard.Projections.Models {
    public abstract class FieldIndexRecord {
        public virtual int Id { get; set; }
        public virtual string PropertyName { get; set; }
    }

    public class StringFieldIndexRecord : FieldIndexRecord {
        public virtual string Value { get; set; }
    }

    public class IntegerFieldIndexRecord : FieldIndexRecord {
        public virtual long? Value { get; set; }
    }

    public class DoubleFieldIndexRecord : FieldIndexRecord {
        public virtual double? Value { get; set; }
    }

    public class DecimalFieldIndexRecord : FieldIndexRecord {
        public virtual decimal? Value { get; set; }
    }

}