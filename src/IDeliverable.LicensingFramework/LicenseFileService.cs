using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Caching;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Licensing
{
    public class LicenseFileService : ILicenseFileService
    {
        private readonly IAppDataFolder _appDataFolder;

        public LicenseFileService(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
        }

        public LicenseFile Load(string name)
        {
            var path = GetPath(name);
            if (!_appDataFolder.FileExists(path))
                return new LicenseFile(name);

            using (var reader = new StreamReader(_appDataFolder.OpenFile(path)))
            {
                return Parse(name, reader.ReadToEndAsync().Result);
            }
        }

        public void Save(LicenseFile file)
        {
            var path = GetPath(file.Name);

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
            return _appDataFolder.WhenPathChanges(GetPath(extensionName));
        }

        private string GetPath(string moduleName)
        {
            return $"Licenses/{moduleName}.lic";
        }

        private LicenseFile Parse(string name, string text)
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