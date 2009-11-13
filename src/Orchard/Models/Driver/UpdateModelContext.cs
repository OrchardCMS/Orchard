using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.UI.Models;

namespace Orchard.Models.Driver {
    public class UpdateModelContext : GetModelEditorsContext {
        public UpdateModelContext(IModel model, IModelUpdater updater) : base(model) {
            Updater = updater;
        }

        public IModelUpdater Updater { get; set; }
    }
}