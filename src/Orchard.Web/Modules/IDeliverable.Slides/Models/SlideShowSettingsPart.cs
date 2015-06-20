using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Models
{
    public class SlideshowSettingsPart : ContentPart
    {
        private IReadOnlyList<SlideshowProfile> _profiles;

        /// <summary>
        /// Expose direct access to the underlying data so that it becomes part of the Site export recipe step.
        /// </summary>
        public string ProfilesData
        {
            get { return Retrieve<string>("SlideshowProfiles"); }
            set { Store("SlideshowProfiles", value); }
        }

        public IReadOnlyList<SlideshowProfile> Profiles
        {
            get
            {
                if (_profiles == null)
                {
                    var json = ProfilesData;

                    try
                    {
                        var profiles = !String.IsNullOrWhiteSpace(json) ? JsonConvert.DeserializeObject<List<SlideshowProfile>>(json) : new List<SlideshowProfile>();
                        _profiles = profiles.AsReadOnly();
                    }
                    catch (JsonSerializationException)
                    {
                        return new ReadOnlyCollection<SlideshowProfile>(new List<SlideshowProfile>(0));
                    }
                }
                return _profiles;
            }
            private set
            {
                var json = JsonConvert.SerializeObject(value.ToList());
                ProfilesData = json;
                _profiles = value;
            }
        }

        public int NextId()
        {
            var seed = Retrieve<int>("Seed");
            Store("Seed", ++seed);
            return seed;
        }

        public void StoreProfile(SlideshowProfile profile)
        {
            var profiles = Profiles.ToDictionary(x => x.Id);
            profiles[profile.Id] = profile;
            Profiles = profiles.Select(x => x.Value).ToList();
        }

        public void DeleteProfile(int id)
        {
            var profiles = Profiles.ToDictionary(x => x.Id);
            profiles.Remove(id);
            Profiles = profiles.Select(x => x.Value).ToList();
        }

        public SlideshowProfile GetProfile(int id)
        {
            return Profiles.SingleOrDefault(x => x.Id == id);
        }
    }
}