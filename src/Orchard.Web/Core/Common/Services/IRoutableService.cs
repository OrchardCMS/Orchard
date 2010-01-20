using System;
using System.Collections.Generic;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.Services {
    public interface IRoutableService : IDependency {
        void FillSlug<TModel>(TModel model) where TModel : RoutableAspect;
        void FillSlug<TModel>(TModel model, Func<string, string> generateSlug) where TModel : RoutableAspect;
        string GenerateUniqueSlug(string slugCandidate, IEnumerable<string> existingSlugs);
    }
}