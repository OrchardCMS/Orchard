using System;

namespace Orchard.DynamicForms.Services {
    public interface IValidationRuleFactory : IDependency {
        T Create<T>(Action<T> setup = null) where T : ValidationRule, new();
        T Create<T>(string errorMessage = null, Action<T> setup = null) where T : ValidationRule, new();
    }
}