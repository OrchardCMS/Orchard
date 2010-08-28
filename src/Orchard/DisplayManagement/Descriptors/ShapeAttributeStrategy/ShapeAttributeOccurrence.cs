using System;
using System.Reflection;
using Autofac.Core;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy {
    public class ShapeAttributeOccurrence {
        private readonly Func<FeatureDescriptor> _feature;

        public ShapeAttributeOccurrence(ShapeAttribute shapeAttribute, MethodInfo methodInfo, IComponentRegistration registration, Func<FeatureDescriptor> feature) {
            ShapeAttribute = shapeAttribute;
            MethodInfo = methodInfo;
            Registration = registration;
            _feature = feature;
        }

        public ShapeAttribute ShapeAttribute { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
        public IComponentRegistration Registration { get; private set; }
        public FeatureDescriptor Feature { get { return _feature(); } }
    }
}