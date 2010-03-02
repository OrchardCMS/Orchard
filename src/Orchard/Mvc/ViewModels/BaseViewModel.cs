using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.UI.Zones;

namespace Orchard.Mvc.ViewModels {
    public class BaseViewModel : IZoneContainer {
        private ZoneCollection _zones = new ZoneCollection();
        private IList<NotifyEntry> _messages = new List<NotifyEntry>();

        public virtual ZoneCollection Zones { get { return _zones; } set { _zones = value; } }
        public virtual IList<NotifyEntry> Messages { get { return _messages; } set { _messages = value; } }
        public virtual IUser CurrentUser { get; set; }
        public virtual IEnumerable<MenuItem> Menu { get; set; }

        public static BaseViewModel From(ViewDataDictionary viewData) {
            var model = viewData.Model as BaseViewModel;
            return model ?? new AdaptedViewModel(viewData);
        }

        public static BaseViewModel From(ActionResult actionResult) {
            var viewResult = actionResult as ViewResult;
            return viewResult == null ? null : From(viewResult.ViewData);
        }
    }

    [Obsolete("Please change your code to use BaseViewModel, as AdminViewModel will likely be removed in the near future.")]
    public class AdminViewModel : BaseViewModel { }
}
