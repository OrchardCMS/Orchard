using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models.Descriptors;

namespace Orchard.Workflows.Services {
    public interface IActivity : IDependency {

        string Name { get; }
        LocalizedString Category { get; }
        LocalizedString Description { get; }
        bool IsEvent { get; }
        bool CanStartWorkflow { get; }
        string Form { get; }

        /// <summary>
        /// List of possible outcomes when the activity is executed.
        /// </summary>
        IEnumerable<LocalizedString> GetPossibleOutcomes(ActivityContext context);

        /// <summary>
        /// Whether the activity can transition to the next outcome. Can prevent the activity from being transitioned
        /// because a condition is not valid.
        /// </summary>
        bool CanExecute(ActivityContext context);

        /// <summary>
        /// Executes the current activity
        /// </summary>
        /// <returns>The names of the resulting outcomes.</returns>
        IEnumerable<LocalizedString> Execute(ActivityContext context);

        /// <summary>
        /// Called on blocking activities when they are reached
        /// </summary>
        void Touch(dynamic workflowState);
    }
}