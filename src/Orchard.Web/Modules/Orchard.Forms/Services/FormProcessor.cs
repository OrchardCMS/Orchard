using System;
using Orchard.DisplayManagement;

namespace Orchard.Forms.Services {
    public static class FormNodesProcessor {
        public static void ProcessForm(dynamic shape, Action<object> process) {
            // if it's not a shape, ignore
            // e.g., SelectListItem
            if (!(shape is IShape)) {
                return;
            }

            // execute external code on this node
            process(shape);

            // recursively process child nodes
            if (shape.Items != null) {
                foreach (var item in shape.Items) {
                    ProcessForm(item, process);
                }
            }

        }
    }
}