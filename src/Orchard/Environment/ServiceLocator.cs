using System;

namespace Orchard.Environment {
    public static class ServiceLocator {
        private static Func<Type, object> _locator;

        public static void SetLocator(Func<Type, object> locator) {
            _locator = locator;
        }

        public static T Resolve<T>() {
            return (T)_locator(typeof(T));
        }

    }
}
