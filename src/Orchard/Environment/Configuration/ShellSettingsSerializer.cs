using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
                if (string.IsNullOrWhiteSpace(setting)) continue; ;
                var separatorIndex = setting.IndexOf(Separator);
                if (separatorIndex == -1) {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                if (value != EmptyValue) {
                    switch (key) {
                        case "Name":
                            shellSettings.Name = value;
                            break;
                        case "DataProvider":
                            shellSettings.DataProvider = value;
                            break;
                        case "State":
                            shellSettings.State = new TenantState(value);
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
                    }
                }
            }

            return shellSettings;
        }

        public static string ComposeSettings(ShellSettings settings) {
            if (settings == null)
                return "";

            return string.Format("Name: {0}\r\nDataProvider: {1}\r\nDataConnectionString: {2}\r\nDataPrefix: {3}\r\nRequestUrlHost: {4}\r\nRequestUrlPrefix: {5}\r\nState: {6}\r\nEncryptionAlgorithm: {7}\r\nEncryptionKey: {8}\r\nHashAlgorithm: {9}\r\nHashKey: {10}\r\nThemes: {11}\r\n",
                                 settings.Name,
                                 settings.DataProvider,
                                 settings.DataConnectionString ?? EmptyValue,
                                 settings.DataTablePrefix ?? EmptyValue,
                                 settings.RequestUrlHost ?? EmptyValue,
                                 settings.RequestUrlPrefix ?? EmptyValue,
                                 settings.State != null ? settings.State.ToString() : String.Empty,
                                 settings.EncryptionAlgorithm ?? EmptyValue,
                                 settings.EncryptionKey ?? EmptyValue,
                                 settings.HashAlgorithm ?? EmptyValue,
                                 settings.HashKey ?? EmptyValue,
                                 String.Join(";", settings.Themes ?? new string[0])
                );
        }
    }
}