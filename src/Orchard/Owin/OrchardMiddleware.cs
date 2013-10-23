using System;
using System.Threading.Tasks;
using Owin;

namespace Orchard.Owin {
    public static class OrchardMiddleware {
        public static IAppBuilder UseOrchard(this IAppBuilder app) {
            app.Use(async (context, next) => {
                var handler = context.Environment["orchard.Handler"] as Func<Task>;

                if (handler == null) {
                    throw new ArgumentException("orchard.Handler can't be null");
                }
                await handler();
            });

            return app;
        }
    }
}
