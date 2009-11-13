using System.Collections.Generic;
using Orchard.UI.Models;

namespace Orchard.Models.Driver {
    public class GetModelEditorsContext {
        public GetModelEditorsContext(IModel model) {
            Instance = model;
            Editors= new List<ModelEditor>();
        }
        public IModel Instance { get; set; }
        public IList<ModelEditor> Editors { get; set; }
    }
}