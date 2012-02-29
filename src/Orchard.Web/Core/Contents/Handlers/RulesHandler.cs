using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Events;

namespace Orchard.Core.Contents.Handlers {

    public interface IRulesManager : IEventHandler {
        void TriggerEvent(string category, string type, Func<Dictionary<string, object>> tokensContext);
    }

    public class RulePartHandler : ContentHandler {
        public RulePartHandler(IRulesManager rulesManager) {

            OnPublished<ContentPart>(
                (context, part) =>
                    rulesManager.TriggerEvent("Content", "Published",
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

            OnRemoved<ContentPart>(
                (context, part) =>
                    rulesManager.TriggerEvent("Content", "Removed",
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

            OnVersioned<ContentPart>(
                (context, part1, part2) =>
                    rulesManager.TriggerEvent("Content", "Versioned",
                    () => new Dictionary<string, object> { { "Content", part1.ContentItem } }));

            OnCreated<ContentPart>(
                (context, part) =>
                    rulesManager.TriggerEvent("Content", "Created",
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

        }
    }
}