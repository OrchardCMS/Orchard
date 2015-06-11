using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Models
{
    public class SlideShowSettingsPart : ContentPart
    {
        private IReadOnlyList<SlideShowProfile> _profiles;

        public IReadOnlyList<SlideShowProfile> Profiles
        {
            get
            {
                if (_profiles == null)
                {
                    var json = Retrieve<string>("Profiles");

                    try
                    {
                        var profiles = !String.IsNullOrWhiteSpace(json) ? JsonConvert.DeserializeObject<List<SlideShowProfile>>(json) : new List<SlideShowProfile>();
                        _profiles = profiles.AsReadOnly();
                    }
                    catch (JsonSerializationException)
                    {
                        return new ReadOnlyCollection<SlideShowProfile>(new List<SlideShowProfile>(0));
                    }
                }
                return _profiles;
            }
            private set
            {
                var json = JsonConvert.SerializeObject(value.ToList());
                Store("Profiles", json);
                _profiles = value;
            }
        }

        public int NextId()
        {
            var seed = Retrieve<int>("Seed");
            Store("Seed", ++seed);
            return seed;
        }

        public void StoreProfile(SlideShowProfile profile)
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

        public SlideShowProfile GetProfile(int id)
        {
            return Profiles.SingleOrDefault(x => x.Id == id);
        }

        public string LicenseKey
        {
            get { return this.Retrieve(x => x.LicenseKey); }
            set { this.Store(x => x.LicenseKey, value); }
        }
    }
}