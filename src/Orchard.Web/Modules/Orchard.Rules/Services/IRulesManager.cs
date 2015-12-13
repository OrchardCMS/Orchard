using System;
using System.Collections.Generic;
using Orchard.Events;
using Orchard.Rules.Models;

namespace Orchard.Rules.Services {
    public interface IRulesManager : IEventHandler {
        IEnumerable<TypeDescriptor<EventDescriptor>> DescribeEvents();
        IEnumerable<TypeDescriptor<ActionDescriptor>> DescribeActions();

        /// <summary>
        /// Triggers a specific Event, and provides the tokens context if the event is 
        /// actually executed
        /// </summary>
        /// <param name="category">The category of the event to trigger, e.g. Content</param>
        /// <param name="type">The type of the event to trigger, e.g. Publish</param>
        /// <param name="tokensContext">An object containing the tokens context</param>
        void TriggerEvent(string category, string type, Func<Dictionary<string, object>> tokensContext);

        /// <summary>
        /// Forces the execution of a set of actions
        /// </summary>
        /// <param name="actions">The actions to execute</param>
        /// <param name="tokens">A dictionary containing the tokens </param>
        void ExecuteActions(IEnumerable<ActionRecord> actions, Dictionary<string, object> tokens);
    }

}