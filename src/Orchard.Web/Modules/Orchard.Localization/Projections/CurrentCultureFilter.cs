using System;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization.Models;
using Orchard.Localization.Services;

namespace Orchard.Localization.Projections {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class CurrentCultureFilter : IFilterProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICultureManager _cultureManager;

        public CurrentCultureFilter(IWorkContextAccessor workContextAccessor, ICultureManager cultureManager) {
            _workContextAccessor = workContextAccessor;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Localization", T("Localization"), T("Localization"))
                .Element("ForCurrentCulture", T("For current culture"), T("Localized content items for current culture"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    null
                );
        }

        public void ApplyFilter(dynamic context) {
            string currentCulture = _workContextAccessor.GetContext().CurrentCulture;
            var currentCultureId = _cultureManager.GetCultureByName(currentCulture).Id;

            var query = (IHqlQuery)context.Query;
            context.Query = query.Where(x => x.ContentPartRecord<LocalizationPartRecord>(), x => x.Eq("CultureId", currentCultureId));
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("For current culture");
        }
    }
}