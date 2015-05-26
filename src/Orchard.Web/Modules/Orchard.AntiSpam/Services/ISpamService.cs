using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement;

namespace Orchard.AntiSpam.Services {
    public interface ISpamService : IDependency {
        Task<SpamStatus> CheckForSpam(CommentCheckContext text, SpamFilterAction action, IContent content);
        Task<SpamStatus> CheckForSpam(SpamFilterPart part);

        /// <summary>
        /// Explicitely report some content as spam in order to improve the service.
        /// </summary>
        /// <param name="context">The comment context to report as spam.</param>
        Task ReportSpam(CommentCheckContext context);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="part">The content part to report as ham (false positive).</param>
        Task ReportSpam(SpamFilterPart part);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="context">The comment context to report as ham (false positive).</param>
        Task ReportHam(CommentCheckContext context);

        /// <summary>
        /// Explicitely report some content as ham in order to improve the service.
        /// </summary>
        /// <param name="part">The content part to report as ham (false positive).</param>
        Task ReportHam(SpamFilterPart part);

        IEnumerable<ISpamFilter> GetSpamFilters();
    }
}