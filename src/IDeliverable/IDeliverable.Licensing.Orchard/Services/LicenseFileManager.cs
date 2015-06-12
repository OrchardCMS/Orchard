using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IDeliverable.Licensing.Orchard.Models;
using Orchard.Caching;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Licensing.Orchard.Services
{
    public class LicenseFileManager : ILicenseFileManager
    {
        private readonly IAppDataFolder _appDataFolder;

        public LicenseFileManager(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
        }

        public LicenseFile Load(string extensionName)
        {
            var path = GetRelativePath(extensionName);
            if (!_appDataFolder.FileExists(path))
                return LicenseFile.CreateNullInstance(extensionName);

            using (var reader = new StreamReader(_appDataFolder.OpenFile(path)))
            {
                return Parse(extensionName, reader.ReadToEndAsync().Result);
            }
        }

        public void Save(LicenseFile file)
        {
            var path = GetRelativePath(file.ExtensionName);

            using (var writer = new StreamWriter(_appDataFolder.CreateFile(path)))
            {
                foreach (var setting in file.Settings)
                {
                    writer.WriteLine($"{setting.Key}: {setting.Value}");
                }
            }
        }

        public IVolatileToken WhenPathChanges(string extensionName)
        {
            return _appDataFolder.WhenPathChanges(GetRelativePath(extensionName));
        }

        public string GetRelativePath(string extensionName)
        {
            return $"Licenses/{extensionName}.lic";
        }

        public string GetPhysicalPath(string extensionName)
        {
            return _appDataFolder.MapPath(GetRelativePath(extensionName));
        }

        private static LicenseFile Parse(string name, string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return new LicenseFile(name);

            var lines = Regex.Split(text, "\n", RegexOptions.Multiline);
            var file = new LicenseFile(name);

            foreach (var line in lines.Where(x => !String.IsNullOrWhiteSpace(x)))
            {
                var pair = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var key = pair[0].Trim();
                var value = pair.Length > 1 ? pair[1] : "";

                file.Settings[key] = value.Trim();
            }

            return file;
        }
    }
}