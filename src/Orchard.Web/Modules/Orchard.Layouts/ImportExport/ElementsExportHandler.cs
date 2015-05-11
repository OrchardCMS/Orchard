using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Data;
using Orchard.Events;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    public class ElementsExportHandler : IExportEventHandler {
        private readonly IRepository<ElementBlueprint> _repository;

        public ElementsExportHandler(IRepository<ElementBlueprint> repository) {
            _repository = repository;
        }

        public void Exporting(dynamic context) {
        }

        public void Exported(dynamic context) {

            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("LayoutElements")) {
                return;
            }

            var elements = _repository.Table.ToList();

            if (!elements.Any()) {
                return;
            }

            var root = new XElement("LayoutElements");
            context.Document.Element("Orchard").Add(root);

            foreach (var element in elements) {
                root.Add(new XElement("Element",
                    new XAttribute("ElementTypeName", element.ElementTypeName),
                    new XAttribute("BaseElementTypeName", element.BaseElementTypeName),
                    new XAttribute("ElementDisplayName", element.ElementDisplayName),
                    new XAttribute("ElementDescription", element.ElementDescription),
                    new XAttribute("ElementCategory", element.ElementCategory),
                    new XElement("BaseElementState", new XCData(element.BaseElementState))));
            }
        }
    }
}

