using System;
using Orchard.Data.Conventions;

namespace Orchard.DynamicForms.Models {
    public class Submission {
        public virtual int Id { get; set; }
        public virtual string FormName { get; set; }

        [StringLengthMax]
        public virtual string FormData { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
    }
}