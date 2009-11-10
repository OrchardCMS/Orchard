using System;

namespace Orchard.Models {
    public sealed class ModelRoot : IModel {
        public ModelRoot(string modelType) {
            Welded = this;
            ModelType = modelType;
        }

        public IModel Welded { get; set; }

        public int Id { get; set; }
        public string ModelType { get; set; }

        bool IModel.Is<T>() {
            return this is T;
        }

        T IModel.As<T>() {
            return this as T;
        }

        public bool WeldedIs<T>() where T : class, IModel {
            return Welded.Is<T>();
        }

        public T WeldedAs<T>() where T : class, IModel {
            return Welded.As<T>();
        }

        void IModel.Weld(IModel model) {
            // this method is not called on root
        }
    }
}