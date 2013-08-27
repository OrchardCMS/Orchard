using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;

namespace Orchard.Environment.Configuration {

	/// <summary>
	/// Provides logic to override individual shell settings with values read from platform configuration.
	/// </summary>
	/// <remarks>
	/// This class is used by IShellSettingsManager implementations to apply overrides of individual
	/// shell settings from corresponding values in platform configuration. For each setting found
	/// in the shell settings, this class looks for a corresponding platform setting named
	/// Orchard.TenantName.SettingName in platform configuration. Platform configuration refers to
	/// anywhere the <see cref="Microsoft.WindowsAzure.CloudConfigurationManager"/> class looks.
	/// </remarks>
	public static class PlatformShellSettings {

		private const string _prefix = "Orchard";
		private const string _emptyValueString = "null";

		/// <summary>
		/// Applies platform configuration overrides to the specified ShellSettings objects.
		/// </summary>
		/// <param name="shellSettingsList">A list of ShellSettings objects to which platform configuration overrides will be applied.</param>
		public static void ApplyTo(IEnumerable<ShellSettings> shellSettingsList) {
			foreach (var settings in shellSettingsList) {
				foreach (var key in settings.Keys.ToArray()) {
					var cloudConfigurationKey = String.Format("{0}.{1}.{2}", _prefix, settings.Name, key);
					var cloudConfigurationValue = ParseValue(CloudConfigurationManager.GetSetting(cloudConfigurationKey));
					if (cloudConfigurationValue != null)
						settings[key] = cloudConfigurationValue;
				}
			}
		}

		private static string ParseValue(string value) {
			if (value == _emptyValueString || String.IsNullOrWhiteSpace(value))
				return null;
			return value;
		}
	}
}
