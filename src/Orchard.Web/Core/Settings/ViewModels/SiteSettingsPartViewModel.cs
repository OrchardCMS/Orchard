using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Core.Settings.Models;
using Orchard.Mvc;
using Orchard.Settings;

namespace Orchard.Core.Settings.ViewModels {
    public class SiteSettingsPartViewModel  {
        public SiteSettingsPart Site { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public IEnumerable<TimeZoneInfo> TimeZones { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Site.ContentItem.Id; }
        }

        [UIOptions(EnableWrapper = true)]
        public string PageTitleSeparator {
            get { return Site.PageTitleSeparator; }
            set { Site.PageTitleSeparator = value; }
        }

        [UIOptions(EnableWrapper = true)]
        public string SiteName {
            get { return Site.SiteName; }
            set { Site.SiteName = value; }
        }

        [UIOptions(EnableWrapper = true, Template = "SelectList", DisplayName = "Default Site Culture", ActionLink = "Culture", ActionLinkText = "Add or remove supported cultures for the site.")]
        public string SiteCulture {
            get { return Site.SiteCulture; }
            set { Site.SiteCulture = value; }
        }

        [UIOptions(EnableWrapper = true, Description = "Enter an existing account name, or nothing if you don't want a Super user account")]
        public string SuperUser {
            get { return Site.SuperUser; }
            set { Site.SuperUser = value; }
        }

        [UIOptions(EnableWrapper = true, Template = "SelectList", Description = "Determines whether scripts and stylesheets load in their debuggable or minified form.")]
        public ResourceDebugMode ResourceDebugMode {
            get { return Site.ResourceDebugMode; }
            set { Site.ResourceDebugMode = value; }
        }

        [UIOptions(EnableWrapper = true, Template = "Integer", DisplayName = "Default number of items per page", Description = "Determines the default number of items that are shown per page.")]
        public int PageSize {
            get { return Site.PageSize; }
            set { Site.PageSize = value; }
        }

        [UIOptions(EnableWrapper = true, Template = "MediumString", Description = @"Enter the fully qualified base url of your website.
e.g., http://localhost:30320/orchardlocal, http://www.yourdomain.com")]
        public string BaseUrl {
            get { return Site.BaseUrl; }
            set { Site.BaseUrl = value; }
        }

        [UIOptions(EnableWrapper = true, Template = "SelectList", DisplayName = "Default Time Zone", Description = "Determines the default time zone which will should be used to display date and times.")]
        public string TimeZone {
            get { return Site.SiteTimeZone; }
            set { Site.SiteTimeZone = value; }
        }
    }
}
