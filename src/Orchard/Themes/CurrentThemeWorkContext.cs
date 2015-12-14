using System;
using Orchard.Mvc;

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
            if (name == "CurrentTheme")
            {
                var currentTheme = _themeManager.GetRequestTheme(_httpContextAccessor.Current().Request.RequestContext);
                return ctx => (T)(object)currentTheme;
            }
            return null;
        }
    }
}
