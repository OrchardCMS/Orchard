using System.Collections.Generic;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Settings;

namespace Orchard.AntiSpam.Services {
    public interface ISpamService : IDependency {
        SpamStatus CheckForSpam(string text, SpamFilterAction action);
        SpamStatus CheckForSpam(SpamFilterPart part);

        /// <summary>
        /// Explicitely report some content as spam in order to improve the service.
        /// </summary>
        /// <param name="text">The text to report as spam.</param>
        void ReportSpam(string text);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="part">The content part to report as ham (false positive).</param>
        void ReportSpam(SpamFilterPart part);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="text">The text to report as ham (false positive).</param>
        void ReportHam(string text);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="part">The content part to report as ham (false positive).</param>
        void ReportHam(SpamFilterPart part);

        IEnumerable<ISpamFilter> GetSpamFilters();
    }
}