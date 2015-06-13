using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IDeliverable.ThemeSettings.Models;
using Newtonsoft.Json;
using NHibernate;
using Orchard;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace IDeliverable.ThemeSettings.Services
{
    public class ThemeSettingsService : Component, IThemeSettingsService
    {
        private readonly ISessionLocator _sessionLocator;
        private readonly ICacheManager _cacheManager;
        private readonly IFeatureManager _featureManager;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public ThemeSettingsService(
            ISessionLocator sessionLocator,
            ICacheManager cacheManager,
            IFeatureManager featureManager,
            IVirtualPathProvider virtualPathProvider)
        {

            _sessionLocator = sessionLocator;
            _cacheManager = cacheManager;
            _featureManager = featureManager;
            _virtualPathProvider = virtualPathProvider;
        }

        private ISession Session
        {
            get { return _sessionLocator.For(typeof(ThemeProfileRecord)); }
        }

        public ThemeProfile GetProfile(string themeId, string profileName)
        {
            var profiles = GetProfiles(themeId);
            return profiles.SingleOrDefault(x => String.Equals(x.Name, profileName, StringComparison.OrdinalIgnoreCase));
        }

        public ExtensionDescriptor GetTheme(string themeId)
        {
            var query = from x in _featureManager.GetEnabledFeatures()
                        where DefaultExtensionTypes.IsTheme(x.Extension.ExtensionType) && x.Id == themeId
                        select x.Extension;

            return query.Distinct().SingleOrDefault();
        }

        public ThemeProfile GetProfile(int id)
        {
            return Map(Session.Get<ThemeProfileRecord>(id));
        }

        public ThemeProfile GetProfile(string profileName)
        {
            var query = Session.QueryOver<ThemeProfileRecord>().Where(x => x.Name == profileName);
            return query.Future<ThemeProfileRecord>().Select(Map).FirstOrDefault();
        }

        public ThemeProfile GetCurrentProfile()
        {
            var profile = Session.QueryOver<ThemeProfileRecord>().Where(x => x.IsCurrent).List().FirstOrDefault() ?? Session.QueryOver<ThemeProfileRecord>().Take(1).SingleOrDefault();
            return Map(profile);
        }

        public IEnumerable<ThemeProfile> GetAllProfiles()
        {
            var query = Session.QueryOver<ThemeProfileRecord>();
            return query.Future<ThemeProfileRecord>().Select(Map);
        }

        public IEnumerable<ThemeProfile> GetProfiles(string themeId)
        {
            var query = Session.QueryOver<ThemeProfileRecord>().Where(x => x.Theme == themeId);
            return query.Future<ThemeProfileRecord>().Select(Map);
        }

        public ThemeSettingsManifest GetSettingsManifest(string themeId)
        {
            var key = String.Format("ThemeSettingsManifest-{0}", themeId);
            return _cacheManager.Get(key, context =>
            {
                var theme = GetTheme(themeId);
                var path = _virtualPathProvider.Combine(theme.Location, theme.Id, "Settings.json");

                if (!_virtualPathProvider.FileExists(path))
                    return null;

                var json = ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<ThemeSettingsManifest>(json);

                return settings;
            });
        }

        public bool HasSettingsManifest(string themeId)
        {
            return GetSettingsManifest(themeId) != null;
        }

        public void SaveProfile(ThemeProfile profile)
        {
            ThemeProfileRecord record;

            if (profile.Id == 0)
            {
                record = new ThemeProfileRecord();
                Session.Save(record);
                profile.Id = record.Id;
            }
            else
            {
                record = Session.Get<ThemeProfileRecord>(profile.Id);
            }

            record.Name = profile.Name;
            record.Description = profile.Description;
            record.Theme = profile.Theme;
            record.Settings = SerializeSettings(profile.Settings);
            record.IsCurrent = profile.IsCurrent;

            // If the specified profile is marked as current, we need to mark any previous profile to be not current.
            if (record.IsCurrent)
            {
                var previousCurrentProfiles = Session.QueryOver<ThemeProfileRecord>().Where(x => x.IsCurrent && x.Id != record.Id).List();

                foreach (var previousCurrentProfile in previousCurrentProfiles)
                {
                    previousCurrentProfile.IsCurrent = false;
                }
            }
        }

        public ThemeProfile CloneProfile(ThemeProfile profile)
        {
            var clone = new ThemeProfile
            {
                Name = String.Format("{0} - clone", profile.Name),
                Description = profile.Description,
                Theme = profile.Theme,
                Settings = profile.Settings.ToDictionary(x => x.Key, x => new ThemeSetting { Name = x.Value.Name, Value = x.Value.Value }),
                IsCurrent = false
            };

            return clone;
        }

        public void DeleteProfile(int id)
        {
            var record = Session.Get<ThemeProfileRecord>(id);
            Session.Delete(record);
        }

        public string SerializeSettings(IEnumerable<KeyValuePair<string, ThemeSetting>> settings)
        {
            return JsonConvert.SerializeObject(settings.Select(x => x.Value));
        }

        public IDictionary<string, ThemeSetting> DeserializeSettings(string settings)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<ThemeSetting>>(settings).ToDictionary(x => x.Name);
            }
            catch (JsonSerializationException ex)
            {
                Logger.Error(ex, "Failed to deserialize JSON serialized theme profile settings.");
            }
            return new Dictionary<string, ThemeSetting>();
        }

        private ThemeProfile Map(ThemeProfileRecord record)
        {
            if (record == null)
                return null;

            return new ThemeProfile
            {
                Id = record.Id,
                Name = record.Name,
                Description = record.Description,
                Theme = record.Theme,
                Settings = DeserializeSettings(record.Settings),
                IsCurrent = record.IsCurrent
            };
        }

        private string ReadAllText(string virtualPath)
        {
            using (var reader = new StreamReader(_virtualPathProvider.OpenFile(virtualPath)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}