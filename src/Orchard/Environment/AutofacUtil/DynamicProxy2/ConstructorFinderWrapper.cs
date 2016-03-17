using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace Orchard.Environment.AutofacUtil.DynamicProxy2 {
    class ConstructorFinderWrapper : IConstructorFinder {
        private readonly IConstructorFinder _constructorFinder;
        private readonly DynamicProxyContext _dynamicProxyContext;

        public ConstructorFinderWrapper(IConstructorFinder constructorFinder, DynamicProxyContext dynamicProxyContext) {
            _constructorFinder = constructorFinder;
            _dynamicProxyContext = dynamicProxyContext;
        }

        public ConstructorInfo[] FindConstructors(Type targetType) {
            Type proxyType;
            if (_dynamicProxyContext.TryGetProxy(targetType, out proxyType)) {
                return _constructorFinder.FindConstructors(proxyType);
            }
            return _constructorFinder.FindConstructors(targetType);
        }
    }
}
