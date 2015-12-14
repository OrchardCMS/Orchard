using System;

namespace Orchard.Widgets.Services {
    [Obsolete("Use Orchard.Conditions.Services.IConditionManager instead.")]
    public interface IRuleManager : IDependency {
        bool Matches(string expression);
    }
}