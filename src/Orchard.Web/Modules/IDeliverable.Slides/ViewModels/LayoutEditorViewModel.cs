using System.Collections.Generic;
using Orchard.Layouts.Models;

namespace IDeliverable.Slides.ViewModels
{
    public class LayoutEditorViewModel
    {
        public IList<LayoutPart> Templates { get; set; }
        public int? SelectedTemplateId { get; set; }
        public string State { get; set; }
        public dynamic LayoutRoot { get; set; }
        public string SessionKey { get; set; }
        public string ConfigurationData { get; set; }
    }
}