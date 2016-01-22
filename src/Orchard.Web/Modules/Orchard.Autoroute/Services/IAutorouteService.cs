using System.Collections.Generic;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Settings;

namespace Orchard.Autoroute.Services {

    /// <summary>
    /// Provides main services for Autoroute module
    /// </summary>
    public interface IAutorouteService : IDependency {

        string GenerateAlias(AutoroutePart part);
        void PublishAlias(AutoroutePart part);
        void RemoveAliases(AutoroutePart part);
        void CreatePattern(string contentType, string name, string pattern, string description, bool makeDefault);
        RoutePattern GetDefaultPattern(string contentType);
        IEnumerable<RoutePattern> GetPatterns(string contentType);
        bool ProcessPath(AutoroutePart part);
        bool IsPathValid(string slug);
    }
}
