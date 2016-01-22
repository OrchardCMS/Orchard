using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.MediaProcessing.Models;

namespace Orchard.MediaProcessing.Services {
    public class ImageProfileService : IImageProfileService {
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<FilterRecord> _filterRepository;
        private readonly ISignals _signals;

        public ImageProfileService(
            IContentManager contentManager, 
            ICacheManager cacheManager,
            IRepository<FilterRecord> filterRepository,
            ISignals signals) {
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _filterRepository = filterRepository;
            _signals = signals;
        }

        public Localizer T { get; set; }

        public ImageProfilePart GetImageProfile(int id) {
            return _contentManager.Get<ImageProfilePart>(id);
        }

        public ImageProfilePart GetImageProfileByName(string name) {

            var profileId = _cacheManager.Get("ProfileId_" + name, true, ctx => {
                var profile = _contentManager.Query<ImageProfilePart, ImageProfilePartRecord>()
                    .Where(x => x.Name == name)
                    .Slice(0, 1)
                    .FirstOrDefault();

                if (profile == null) {
                    return -1;
                }

                return profile.Id;
            });

            if (profileId == -1) {
                return null;
            }

            return _contentManager.Get<ImageProfilePart>(profileId);
        }

        public IEnumerable<ImageProfilePart> GetAllImageProfiles() {
            return _contentManager.Query<ImageProfilePart, ImageProfilePartRecord>().List();
        }

        public ImageProfilePart CreateImageProfile(string name) {
            var contentItem = _contentManager.New("ImageProfile");
            var profile = contentItem.As<ImageProfilePart>();
            profile.Name = name;

            _contentManager.Create(contentItem);

            return profile;
        }

        public void DeleteImageProfile(int id) {
            var profile = _contentManager.Get(id);

            if (profile != null) {
                _contentManager.Remove(profile);
            }
        }

        public void MoveUp(int filterId) {
            var filter = _filterRepository.Get(filterId);

            // look for the previous action in order in same rule
            var previous = _filterRepository.Table
                .Where(x => x.Position < filter.Position && x.ImageProfilePartRecord.Id == filter.ImageProfilePartRecord.Id)
                .OrderByDescending(x => x.Position)
                .FirstOrDefault();

            _signals.Trigger("MediaProcessing_Saved_" + filter.ImageProfilePartRecord.Name);

            // nothing to do if already at the top
            if (previous == null) {
                return;
            }

            // switch positions
            var temp = previous.Position;
            previous.Position = filter.Position;
            filter.Position = temp;
        }

        public void MoveDown(int filterId) {
            var filter = _filterRepository.Get(filterId);

            // look for the next action in order in same rule
            var next = _filterRepository.Table
                .Where(x => x.Position > filter.Position && x.ImageProfilePartRecord.Id == filter.ImageProfilePartRecord.Id)
                .OrderBy(x => x.Position)
                .FirstOrDefault();

            // nothing to do if already at the end
            if (next == null) {
                return;
            }

            // switch positions
            var temp = next.Position;
            next.Position = filter.Position;
            filter.Position = temp;
        }
    }
}