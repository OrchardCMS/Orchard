using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Orchard.UI.Models {
    public class ModelEditor {
        public string PartialName { get; set; }
        public ViewDataDictionary ViewData { get; set; }

        public static ModelEditor For<TModel>(string partialName, TModel model) {
            return new ModelEditor {
                PartialName = partialName,
                ViewData = new ViewDataDictionary<TModel>(model)
            };
        }
    }
}
