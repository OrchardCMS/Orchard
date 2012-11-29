using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Workflows.Models.Descriptors;

namespace Orchard.Workflows.Services {
    public interface IActivity : IDependency {

        string Name { get; }
        LocalizedString Category { get; }
        LocalizedString Description { get; }
        bool IsBlocking { get; }
        string Form { get; }

        IEnumerable<LocalizedString> GetPossibleOutcomes(ActivityContext context);
        bool CanTransition(ActivityContext context);
        LocalizedString Transition(ActivityContext context);
    }
}