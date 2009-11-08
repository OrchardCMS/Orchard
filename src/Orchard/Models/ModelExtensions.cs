using System;

namespace Orchard.Models {
    public static class ModelExtensions {
        public static T New<T>(this IModelManager manager, string modelType) where T : class, IModel {
            var t = manager.New(modelType).As<T>();
            if (t == null)
                throw new InvalidCastException();
            return t;
        }

    }
}
