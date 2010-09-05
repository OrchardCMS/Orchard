using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeDescriptorAlteration {
        private readonly IList<Action<ShapeDescriptor>> _configurations;

        public ShapeDescriptorAlteration(string shapeType, FeatureDescriptor feature, IList<Action<ShapeDescriptor>> configurations) {
            _configurations = configurations;
            ShapeType = shapeType;
            Feature = feature;
        }

        public string ShapeType { get; private set; }
        public FeatureDescriptor Feature { get; private set; }
        public void Alter(ShapeDescriptor descriptor) {
            foreach (var configuration in _configurations) {
                configuration(descriptor);
            }
        }
    }
}