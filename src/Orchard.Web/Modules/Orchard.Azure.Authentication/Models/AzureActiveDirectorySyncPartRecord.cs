using Orchard.ContentManagement.Records;
using System;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Azure.Authentication.Models {
    public class AzureActiveDirectorySyncPartRecord : ContentPartRecord {
        [Required]
        public virtual int UserId { get; set; }

        public virtual DateTime LastSyncedUtc { get; set; }
    }
}