using System.Collections.Generic;
using Orchard.Alias.Implementation.Map;

namespace Orchard.Alias.Implementation.Holder {
    /// <summary>
    /// Holds every alias in a tree structure, indexed by area
    /// </summary>
    public interface IAliasHolder : ISingletonDependency {

        /// <summary>
        /// Returns an <see cref="AliasMap"/> for a specific area
        /// </summary>
        AliasMap GetMap(string areaName);

        /// <summary>
        /// Returns all <see cref="AliasMap"/> instances
        /// </summary>
        IEnumerable<AliasMap> GetMaps();

        /// <summary>
        /// Adds or updates an alias in the tree
        /// </summary>
        void SetAlias(AliasInfo alias);

        /// <summary>
        /// Adds or updates a set of aliases in the tree
        /// </summary>
        void SetAliases(IEnumerable<AliasInfo> aliases);
        
        /// <summary>
        /// Removes an alias from the tree based on its path
        /// </summary>
        void RemoveAlias(AliasInfo aliasInfo);
    }
}