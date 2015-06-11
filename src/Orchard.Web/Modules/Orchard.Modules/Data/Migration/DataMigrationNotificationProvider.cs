using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Data.Migration;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Modules.Extensions;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Modules.Data.Migration {
    /// <summary>
    /// This implementation overrides the Core notification provider. It has been replaced
    /// because some links to the features had to be displayed, links that are pointing to 
    /// this module. So in case the module is removed, the notifications would still be displayed.
    /// </summary>
    [OrchardSuppressDependency("Orchard.Data.Migration.DataMigrationNotificationProvider")]
    public class DataMigrationNotificationProvider : INotificationProvider {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly WorkContext _workContext;

        public DataMigrationNotificationProvider(IDataMigrationManager dataMigrationManager, IWorkContextAccessor workContextAccessor) {
            _dataMigrationManager = dataMigrationManager;
            _workContext = workContextAccessor.GetContext();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var features = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            if (features.Any()) {
                UrlHelper urlHelper = new UrlHelper(_workContext.HttpContext.Request.RequestContext);

                yield return new NotifyEntry { Message = T("Some features need to be upgraded: {0}", 
                    T(string.Join(", ", features
                        .Select(feature =>
                            string.Format("<a href=\"{0}#{1}\">{2}</a>", urlHelper.Action("Features", "Admin", new RouteValueDictionary { { "area", "Orchard.Modules" } }), feature.AsFeatureId(n => T(n)), feature))))),
                    Type = NotifyType.Warning };
            }
        }
    }
}