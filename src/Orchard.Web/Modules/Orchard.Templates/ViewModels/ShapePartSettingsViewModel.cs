using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Templates.Services;

namespace Orchard.Templates.ViewModels {
    public class ShapePartSettingsViewModel {

        [UIHint("TemplateProcessorPicker")]
        public string Processor { get; set; }
        public IList<ITemplateProcessor> AvailableProcessors { get; set; }
    }
}