using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Conventions;

namespace Orchard.TaskLease.Models
{
    public class TaskLeaseRecord
    {
        public virtual int Id { get; set; }
        public virtual string TaskName { get; set; }
        public virtual string MachineName { get; set; }
        public virtual DateTime UpdatedUtc { get; set; }
        public virtual DateTime ExpiredUtc { get; set; }

        [StringLengthMax]
        public virtual string State { get; set; }
    }
}