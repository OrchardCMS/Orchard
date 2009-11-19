using System;
using Orchard.Models.Records;

namespace Orchard.Core.Common.Records {
    public class CommonRecord : ContentPartRecordBase {
        public virtual int OwnerId { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual DateTime? ModifiedUtc { get; set; }
    }
}
