using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Models {
    public interface IModelManager : IDependency {
        IModel New(string modelType);
        IModel Get(int id);
        void Create(IModel model);
        IEnumerable<ModelEditor> GetEditors(IModel model);
        IEnumerable<ModelEditor> UpdateEditors(IModel model, IModelUpdater updater);
    }
}
