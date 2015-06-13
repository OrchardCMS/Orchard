# Documentation

To provide theme settings, follow the following steps:

1. Create a file called "Settings.json" in the root folder of your theme (or copy "Settings.json.sample" from this project and rename to "Settings.json").
2. Update the "Settings.json" file with your theme-specific settings.
3. To use/query custom settings in your theme, create a Razor template called something like "ThemeSettings.cshtml" into the __Views__ folder of your theme. The sample "ThemeSettings.cshtml.sample" file demonstrates a typical use case where an inline style is rendered in the HEAD section of the HTML document leveraging custom theme settings.