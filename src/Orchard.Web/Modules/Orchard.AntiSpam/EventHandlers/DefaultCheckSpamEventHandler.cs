using System.Linq;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Services;
using Orchard.AntiSpam.Settings;

namespace Orchard.AntiSpam.EventHandlers {
    public class DefaultCheckSpamEventHandler : ICheckSpamEventHandler {
        private readonly ISpamService _spamService;

        public DefaultCheckSpamEventHandler(ISpamService spamService) {
            _spamService = spamService;
        }

        public void CheckSpam(dynamic context) {
            if(!_spamService.GetSpamFilters().Any()) {
                return;
            }

            context.Checked = true;
            
            if(string.IsNullOrWhiteSpace(context.Text)) {
                return;
            }

            context.IsSpam = _spamService.CheckForSpam(context.Text, SpamFilterAction.One) == SpamStatus.Spam;
        }
    }
}