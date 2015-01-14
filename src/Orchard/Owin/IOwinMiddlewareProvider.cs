using System.Collections.Generic;

namespace Orchard.Owin {
    /// <summary>
    /// Represents a provider that makes Owin middlewares available for the Orchard Owin pipeline.
    /// </summary>
    /// <remarks>
    /// If you want to write an Owin middleware and inject it into the Orchard Owin pipeline, implement this interface. For more information
    /// about Owin <see cref="!:http://owin.org/">http://owin.org/</see>.
    /// </remarks>
    public interface IOwinMiddlewareProvider : IDependency {
        /// <summary>
        /// Gets Owin middlewares that will be injected into the Orchard Owin pipeline.
        /// </summary>
        /// <returns>Owin middlewares that will be injected into the Orchard Owin pipeline.</returns>
        IEnumerable<OwinMiddleware> GetOwinMiddlewares();
    }
}