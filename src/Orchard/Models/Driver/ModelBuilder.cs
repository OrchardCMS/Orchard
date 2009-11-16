using System;

namespace Orchard.Models.Driver {
    public class ModelBuilder {
        private IModel _instance;

        public ModelBuilder(string modelType) {
            _instance = new ModelRoot(modelType);
        }

        public IModel Build() {
            return _instance;
        }

        public ModelBuilder Weld<TPart>() where TPart : class, IModel, new() {
            var part = new TPart();
            part.Weld(_instance);
            _instance = part;
            return this;
        }
    }
}
