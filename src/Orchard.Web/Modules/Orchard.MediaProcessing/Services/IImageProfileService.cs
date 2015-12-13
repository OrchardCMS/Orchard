using System.Collections.Generic;
using Orchard.MediaProcessing.Models;

namespace Orchard.MediaProcessing.Services {
    public interface IImageProfileService : IDependency {
        ImageProfilePart GetImageProfile(int id);
        ImageProfilePart GetImageProfileByName(string name);
        IEnumerable<ImageProfilePart> GetAllImageProfiles();
        ImageProfilePart CreateImageProfile(string name);
        void DeleteImageProfile(int id);
        void MoveUp(int filterId);
        void MoveDown(int filterId);
    }
}