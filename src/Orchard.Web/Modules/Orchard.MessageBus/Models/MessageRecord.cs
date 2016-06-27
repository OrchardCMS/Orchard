using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Orchard.MessageBus.Models {

    [OrchardFeature("Orchard.MessageBus.SqlServerServiceBroker")]
    public class MessageRecord {

        public virtual int Id { get; set; }
        public virtual string Publisher { get; set; }
        public virtual string Channel { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        
        [StringLengthMax]
        public virtual string Message { get; set; }
    }
}