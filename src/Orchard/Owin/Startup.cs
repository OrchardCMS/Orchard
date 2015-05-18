using Owin;

namespace Orchard.Owin {
    /// <summary>
    /// The entry point for the Owin pipeline that doesn't really do anything on its own but is necessary for bootstrapping.
    /// </summary>
    /// <remarks>Also, startups are hip nowadays.</remarks>
    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.Use((context, next) => {
                context.Response.Headers.Append("X-Generator", "Orchard");
                return next();
            });
        }
    }
}