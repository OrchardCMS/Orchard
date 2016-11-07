using System.Collections.Generic;
using Orchard.Data.Migration.Schema;

namespace Orchard.Data.Migration.Generator {
    public interface ISchemaCommandGenerator : IDependency {
        /// <summary>
        /// Returns a set of <see cref="SchemaCommand"/> instances to execute in order to create the tables requiered by the specified feature. 
        /// </summary>
        /// <param name="feature">The name of the feature from which the tables need to be created.</param>
        /// <param name="drop">Whether to generate drop commands for the created tables.</param>
        IEnumerable<SchemaCommand> GetCreateFeatureCommands(string feature, bool drop);
        
        /// <summary>
        /// Automatically updates the tables in the database.
        /// </summary>
        void UpdateDatabase();

        /// <summary>
        /// Creates the tables in the database.
        /// </summary>
        void CreateDatabase();

    }
}