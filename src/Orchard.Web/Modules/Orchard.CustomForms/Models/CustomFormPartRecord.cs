using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.CustomForms.Models {
    public class CustomFormPartRecord : ContentPartRecord {
        [StringLength(255)]
        public virtual string ContentType { get; set; }

        [StringLengthMax]
        public virtual string Message { get; set; }
        public virtual bool CustomMessage { get; set; }
        public virtual bool SaveContentItem { get; set; }
        [StringLengthMax]
        public virtual string RedirectUrl { get; set; }
        public virtual bool Redirect { get; set; }

        public virtual string SubmitButtonText { get; set; }
    }
}