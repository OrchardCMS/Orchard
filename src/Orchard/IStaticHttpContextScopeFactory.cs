using System;

namespace Orchard {
    /// <summary>
    /// A factory class that creates an <see cref="StaticHttpContextScope"/> instance and initializes the HttpContext.Current property with that instance until the scope is disposed of.
    /// This is useful when rendering views from a background thread, as some Html Helpers access HttpContext.Current directly, thus preventing a NullReferenceException.
    /// </summary>
    public interface IStaticHttpContextScopeFactory : IDependency {
        /// <summary>
        /// Creates a disposable static HttpContext scope. This is safe to use even if there is an actual HttpContext.Current instance.
        /// </summary>
        /// <returns></returns>
        IDisposable CreateStaticScope();
    }
}
