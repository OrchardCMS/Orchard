using System;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.AntiSpam.Rules {
    public interface IEventProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class AntiSpamEvents : IEventProvider {
        public AntiSpamEvents() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            Func<dynamic, bool> alwaysTrue = c => true;

            describe.For("AntiSpam", T("Anti-Spam"), T("Anti-Spam"))
                .Element("Spam", T("Spam Reported"), T("Content is categorized as spam."), alwaysTrue, (Func<dynamic, LocalizedString>)(context => T("When content is categorized as spam.")), null)
                .Element("Ham", T("Ham Reported"), T("Content is categorized as ham."), alwaysTrue, (Func<dynamic, LocalizedString>)(context => T("When content is categorized as ham.")), null)
                ;
        }
    }
}