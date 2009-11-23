using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.UI.Notify;

namespace Orchard.Tags.Services {
    public interface ITagService : IDependency {
        IEnumerable<Tag> GetTags();
        void CreateTag(Tag tag);
    }

    public class TagService : ITagService {
        private readonly IRepository<Tag> _tagRepository;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public TagService(IRepository<Tag> tagRepository, IAuthorizer authorizer, INotifier notifier) {
            _tagRepository = tagRepository;
            _authorizer = authorizer;
            _notifier = notifier;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public ISite CurrentSite { get; set; }

        #region ITagService Members

        public IEnumerable<Tag> GetTags() {
            return from comment in _tagRepository.Table.ToList() select comment;
        }

        public void CreateTag(Tag tag) {
            _tagRepository.Create(tag);
        }

        #endregion
    }
}
