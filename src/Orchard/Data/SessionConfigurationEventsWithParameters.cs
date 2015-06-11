using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using Orchard.Data.Providers;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Utility;

namespace Orchard.Data
{
    /// <summary>
    /// Base class for session configuration
    /// </summary>
    public abstract class SessionConfigurationEventsWithParameters : ISessionConfigurationEventsWithParameters
    {
        /// <summary>
        /// The parameters are set before any of the functions are called.
        /// </summary>
        public SessionFactoryParameters Parameters { set; get; }

        /// <summary>
        /// Returns the BlueprintDescriptors - translating from type to DB Table names
        /// </summary>
        public Dictionary<Type, RecordBlueprint> BlueprintDescriptors {
            get {
                if (_descriptors == null)
                    _descriptors = Parameters.RecordDescriptors.ToDictionary(d => d.Type);
                return _descriptors;
            }
        }
        protected Dictionary<Type, RecordBlueprint> _descriptors = null;

        /// <summary>
        /// Called when an empty fluent configuration object has been created, 
        /// before applying any default Orchard config settings (alterations, conventions etc.).
        /// </summary>
        /// <param name="cfg">Empty fluent NH configuration object.</param>
        /// <param name="defaultModel">Default persistence model that is about to be used.</param>
        public virtual void Created(FluentConfiguration cfg, AutoPersistenceModel defaultModel) { }

        /// <summary>
        /// Called when fluent configuration has been prepared but not yet built. 
        /// </summary>
        /// <param name="cfg">Prepared fluent NH configuration object.</param>
        public virtual void Prepared(FluentConfiguration cfg) { }

        /// <summary>
        /// Called when raw NHibernate configuration is being built, after applying all customizations.
        /// Allows applying final alterations to the raw NH configuration.
        /// </summary>
        /// <param name="cfg">Raw NH configuration object being processed.</param>
        public virtual void Building(Configuration cfg) { }

        /// <summary>
        /// Called when NHibernate configuration has been built or read from cache storage (mappings.bin file by default).
        /// </summary>
        /// <param name="cfg">Final, raw NH configuration object.</param>
        public virtual void Finished(Configuration cfg) { }

        /// <summary>
        /// Called when configuration hash is being computed. If hash changes, configuration will be rebuilt and stored in mappings.bin.
        /// This method allows to alter the default hash to take into account custom configuration changes.
        /// </summary>
        /// <remarks>
        /// It's a developer responsibility to make sure hash is correctly updated when config needs to be rebuilt.
        /// Otherwise the cached configuration (mappings.bin file) will be used as long as default Orchard configuration 
        /// is unchanged or until the file is manually removed.
        /// </remarks>
        /// <param name="hash">Current hash object</param>
        public virtual void ComputingHash(Hash hash) { }
    }

}

