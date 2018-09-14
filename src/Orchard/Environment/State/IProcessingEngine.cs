using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;

namespace Orchard.Environment.State
{
    public interface IProcessingEngine
    {
        /// <summary>
        /// Init a new tasks list in the http context or in a new logical context.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Queue an event to fire inside of an explicitly decribed shell context
        /// </summary>        
        string AddTask(
            ShellSettings shellSettings, 
            ShellDescriptor shellDescriptor, 
            string messageName, 
            Dictionary<string, object> parameters);

        /// <summary>
        /// Called by a component responsible for causing tasks to execute. Can be called from 
        /// anyplace which needs to know if work needs to be performed.
        /// </summary>
        bool AreTasksPending();

        /// <summary>
        /// Called by a component responsible for causing tasks to execute. Must only be called
        /// at a point where a full context-specific transaction scope may run. (*Not* inside the processing
        /// of a request)
        /// </summary>
        void ExecuteNextTask();
    }
}
