using System;

namespace Orchard.Models {
    public class ModelPart : IModel {
        protected IModel Next { get; set; }
        protected ModelRoot Root { get; set; }

        void IModel.Weld(IModel model) {
            Next = model;
            Root = model.As<ModelRoot>();
            Root.Welded = this;
        }

        public int Id { get { return Root.Id; } }
        public string ModelType { get { return Root.ModelType; } }

        bool IModel.Is<T>() {
            return this is T ? true : Next.Is<T>();
        }

        T IModel.As<T>() {
            return this is T ? this as T : Next.As<T>();
        }

        public bool Is<T>() where T : class, IModel {
            return Root.WeldedIs<T>();
        }

        public T As<T>() where T : class, IModel {
            return Root.WeldedAs<T>();
        }
    }
}