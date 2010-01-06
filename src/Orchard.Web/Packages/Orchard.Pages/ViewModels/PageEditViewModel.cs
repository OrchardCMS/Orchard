using System;
using Orchard.Pages.Models;
using Orchard.Pages.Services.Templates;
using Orchard.Mvc.ViewModels;

namespace Orchard.Pages.ViewModels {
    public enum PageEditCommand {
        Undefined,
        PublishNow,
        PublishLater,
        SaveDraft,
    }
    public class PageEditViewModel : AdminViewModel {
        public PageEditCommand Command { get; set; }
        public DateTime? PublishLaterDate { get; set; }

        public PageRevision Revision { get; set; }
        public TemplateDescriptor Template { get; set; }

        public bool CanDeleteDraft { get; set; }
    }
}
