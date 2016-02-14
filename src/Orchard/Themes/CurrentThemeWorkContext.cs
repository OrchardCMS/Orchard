using System;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;

namespace Orchard.Themes
{
    public class CurrentThemeWorkContext : IWorkContextStateProvider
    {
        private readonly IThemeManager _themeManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentThemeWorkContext(IThemeManager themeManager, IHttpContextAccessor httpContextAccessor)
        {
            _themeManager = themeManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public Func<WorkContext, T> Get<T>(string name)
        {
            if (name == "CurrentTheme") {
                var context = _httpContextAccessor.Current();
                var currentTheme = context != null && context.Request != null
                    ? _themeManager.GetRequestTheme(context.Request.RequestContext)
                    : null;

                return ctx => (T)(object)currentTheme;
            }
            return null;
        }
    }
}
