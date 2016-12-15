using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Taxonomies.Events;
using Orchard.Taxonomies.Models;
using Orchard.UI.Notify;

namespace Orchard.Taxonomies.EventHandlers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class TermMovingEventHandler : ITermLocalizationEventHandler {
        private readonly INotifier _notifier;
        public TermMovingEventHandler(INotifier notifier) {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public void MovingTerms(MoveTermsContext context) {
            bool termsRemoved = false;
            if (context.ParentTerm != null) {
                if (context.ParentTerm.Has<LocalizationPart>()) {
                    var parentTermCulture = context.ParentTerm.As<LocalizationPart>().Culture;
                    for (int i = context.Terms.Count() - 1; i >= 0; i--) {
                        if (context.Terms[i].Has<LocalizationPart>()) {
                            var termCulture = context.Terms[i].As<LocalizationPart>().Culture;
                            if (termCulture != null && termCulture != parentTermCulture) {
                                context.Terms.RemoveAt(i);
                                termsRemoved = true;
                            }
                        }
                    }
                }
            }
            if (termsRemoved)
                _notifier.Warning(T("Some terms were not moved because their culture was different from the culture of the selected parent."));
        }
    }
}