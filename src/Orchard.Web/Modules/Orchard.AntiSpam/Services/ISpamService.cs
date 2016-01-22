using System.Collections.Generic;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Services {
    public interface ISpamService : IDependency {
        SpamStatus CheckForSpam(CommentCheckContext text, SpamFilterAction action, IContent content);
        SpamStatus CheckForSpam(SpamFilterPart part);

        /// <summary>
        /// Explicitely report some content as spam in order to improve the service.
        /// </summary>
        /// <param name="context">The comment context to report as spam.</param>
        void ReportSpam(CommentCheckContext context);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="part">The content part to report as ham (false positive).</param>
        void ReportSpam(SpamFilterPart part);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="context">The comment context to report as ham (false positive).</param>
        void ReportHam(CommentCheckContext context);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="part">The content part to report as ham (false positive).</param>
        void ReportHam(SpamFilterPart part);

        IEnumerable<ISpamFilter> GetSpamFilters();
    }
}