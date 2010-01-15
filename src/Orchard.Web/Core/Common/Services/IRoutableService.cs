using System.Collections.Generic;

namespace Orchard.Core.Common.Services {
    public interface IRoutableService : IDependency {
        string Slugify(string title);
        string GenerateUniqueSlug(string slugCandidate, IEnumerable<string> existingSlugs);
    }
}