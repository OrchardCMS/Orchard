using System.Collections.Generic;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Models {
    public interface IModelManager : IDependency {
        IModel New(string modelType);
        IModel Get(int id);
        void Create(IModel model);
        IEnumerable<ModelTemplate> GetEditors(IModel model);
        IEnumerable<ModelTemplate> UpdateEditors(IModel model, IModelUpdater updater);
    }
}
