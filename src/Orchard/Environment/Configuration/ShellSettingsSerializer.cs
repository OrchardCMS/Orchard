using System;
using System.IO;
using System.Text;

namespace Orchard.Environment.Configuration {
    public class ShellSettingsSerializer {
        public const char Separator = ':';
        public const string EmptyValue = "null";
        public const char ThemesSeparator = ';';

        public static ShellSettings ParseSettings(string text) {
            var shellSettings = new ShellSettings();
            if (String.IsNullOrEmpty(text))
                return shellSettings;

            var settings = new StringReader(text);
            string setting;
            while ((setting = settings.ReadLine()) != null) {
                if (string.IsNullOrWhiteSpace(setting)) continue;
                var separatorIndex = setting.IndexOf(Separator);
                if (separatorIndex == -1) {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                if (!value.Equals(EmptyValue, StringComparison.OrdinalIgnoreCase)) {
                    switch (key) {
                        case "Name":
                            shellSettings.Name = value;
                            break;
                        case "DataProvider":
                            shellSettings.DataProvider = value;
                            break;
                        case "State":
                            TenantState state;
                            shellSettings.State = Enum.TryParse(value, true, out state) ? state : TenantState.Uninitialized;
                            break;
                        case "DataConnectionString":
                            shellSettings.DataConnectionString = value;
                            break;
                        case "DataPrefix":
                            shellSettings.DataTablePrefix = value;
                            break;
                        case "RequestUrlHost":
                            shellSettings.RequestUrlHost = value;
                            break;
                        case "RequestUrlPrefix":
                            shellSettings.RequestUrlPrefix = value;
                            break;
                        case "EncryptionAlgorithm":
                            shellSettings.EncryptionAlgorithm = value;
                            break;
                        case "EncryptionKey":
                            shellSettings.EncryptionKey = value;
                            break;
                        case "HashAlgorithm":
                            shellSettings.HashAlgorithm = value;
                            break;
                        case "HashKey":
                            shellSettings.HashKey = value;
                            break;
                        case "Themes":
                            shellSettings.Themes = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            break;
                        case "Modules":
                            shellSettings.Modules = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            break;
                        default:
                            shellSettings[key] = value;
                            break;
                    }
                }
            }

            return shellSettings;
        }

        public static string ComposeSettings(ShellSettings settings) {
            if (settings == null)
                return "";

            var sb = new StringBuilder();
            foreach (var key in settings.Keys) {
                sb.AppendLine(key + ": " + (settings[key] ?? EmptyValue));
            }

            return sb.ToString();
        }
    }
}