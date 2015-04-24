using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;

namespace Orchard.Data {
    /// <summary>
    /// Base class for session configuration
    /// </summary>
    public class DefaultSessionConfigurationEvents : SessionConfigurationEventsWithParameters {
        /// <summary>
        /// Called when an empty fluent configuration object has been created, 
        /// before applying any default Orchard config settings (alterations, conventions etc.).
        /// </summary>
        /// <param name="cfg">Empty fluent NH configuration object.</param>
        /// <param name="defaultModel">Default persistence model that is about to be used.</param>
        public override void Created(FluentConfiguration cfg, AutoPersistenceModel defaultModel) {
            defaultModel.OverrideAll(map => {
                map.IgnoreProperties(x => x.MemberInfo.IsDefined(typeof(DoNotMapAttribute), false));
            });
        }
    }
}

