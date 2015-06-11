using System.Collections.Generic;

namespace IDeliverable.Licensing
{
    public class LicenseFile
    {
        public LicenseFile(string name)
        {
            Name = name;
            Settings = new Dictionary<string, string>();
        }

        public string Name { get; set; }
        public IDictionary<string, string> Settings { get; set; }

        public string Key
        {
            get { return GetValue("Key"); }
            set { SetValue("Key", value); }
        }

        public string LicenseValidationToken
        {
            get { return GetValue("ValidationToken"); }
            set { SetValue("ValidationToken", value); }
        }

        private string GetValue(string key)
        {
            return Settings.ContainsKey(key) ? Settings[key] : null;
        }

        private void SetValue(string key, string value)
        {
            Settings[key] = value;
        }
    }
}