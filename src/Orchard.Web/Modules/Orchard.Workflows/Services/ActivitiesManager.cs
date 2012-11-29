using System.Collections.Generic;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.Workflows.Models.Descriptors;

namespace Orchard.Workflows.Services {
    public class ActivitiesManager : IActivitiesManager{
        private const string SignalName = "Orchard.Workflows.Services.ActivitiesManager";

        private readonly ITokenizer _tokenizer;
        private readonly IEnumerable<IActivityProvider> _activityProviders;
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public ActivitiesManager(
            ITokenizer tokenizer,
            IEnumerable<IActivityProvider> activityProviders,
            IContentManager contentManager,
            ICacheManager cacheManager,
            ISignals signals) {
            _tokenizer = tokenizer;
            _activityProviders = activityProviders;
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<TypeDescriptor<ActivityDescriptor>> DescribeActivities() {
            return _cacheManager.Get("activities", ctx => {
                MonitorSignal(ctx);
                
                var context = new DescribeActivityContext();

                foreach (var provider in _activityProviders) {
                    provider.Describe(context);
                }
                return context.Describe();
            });
        }

        private void MonitorSignal(AcquireContext<string> ctx) {
            ctx.Monitor(_signals.When(SignalName));
        }

        private void TriggerSignal() {
            _signals.Trigger(SignalName);
        }
    }
}