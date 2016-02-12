using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;

namespace TTS.TalaveraBase.Gravatar {

    internal static class XElementExtensions {
        public static string ElementValueOrDefault(this XElement xElement, string elementName, string defaultValue) {
            if (null == xElement) throw new ArgumentNullException("xElement");

            var element = xElement.Element(elementName);
            if (element == null) return defaultValue;
            return element.Value;
        }
    }

    /// <summary>
    /// A field of a Gravatar profile that may have more than one instance per profile.
    /// </summary>
    public class GrofilePluralField {

        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Gets a flag indicating whether or not this field is the primary instance
        /// of all other similar fields.
        /// </summary>
        public bool Primary { get; private set; }

        /// <summary>
        /// Creates a new instance of the field.
        /// </summary>
        public GrofilePluralField() : this(null) { }

        /// <summary>
        /// Creates a new instance of the field.
        /// </summary>
        /// <param name="value">The value of the field.</param>
        public GrofilePluralField(string value) : this(value, null) { }

        /// <summary>
        /// Creates a new instance of the Gravatar profile field.
        /// </summary>
        /// <param name="value">The value of the field.</param>
        /// <param name="type">The type of the field.</param>
        public GrofilePluralField(string value, string type) : this(value, type, false) { }

        /// <summary>
        /// Creates a new instance of the Gravatar profile field.
        /// </summary>
        /// <param name="value">The value of the field.</param>
        /// <param name="primary">A flag indicating whether or not this field is the primary instance of all other similar fields.</param>
        public GrofilePluralField(string value, bool primary) : this(value, null, primary) { }

        /// <summary>
        /// Creates a new instance of the Gravatar profile field.
        /// </summary>
        /// <param name="value">The value of the field.</param>
        /// <param name="type">The type of the field.</param>
        /// <param name="primary">A flag indicating whether or not this field is the primary instance of all other similar fields.</param>
        public GrofilePluralField(string value, string type, bool primary) {
            Value = value;
            Type = type;
            Primary = primary;
        }

        /// <summary>
        /// Gets a string representation of the field.
        /// </summary>
        /// <returns>The field value as a string.</returns>
        public override string ToString() {
            return Value;
        }
    }

    /// <summary>
    /// A URL field in a Gravatar profile.
    /// </summary>
    public class GrofileUrl : GrofilePluralField {

        /// <summary>
        /// Gest the title of the URL.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Creates a new instance of the URL Gravatar profile field.
        /// </summary>
        /// <param name="title">The title of the URL.</param>
        /// <param name="value">The URL value.</param>
        public GrofileUrl(string title, string value)
            : base(value) {
            Title = title;
        }
    }

    /// <summary>
    /// An email field in a Gravatar profile.
    /// </summary>
    public class GrofileEmail : GrofilePluralField {

        /// <summary>
        /// Creates a new instance of the email field.
        /// </summary>
        /// <param name="value">The email address.</param>
        public GrofileEmail(string value) : base(value, false) { }

        /// <summary>
        /// Creates a new instance of the email field.
        /// </summary>
        /// <param name="value">The email address.</param>
        /// <param name="primary">A flag indicating whether or not this is the primary email address of the profile.</param>
        public GrofileEmail(string value, bool primary) : base(value, primary) { }
    }

    /// <summary>
    /// A photo field in a Gravatar profile.
    /// </summary>
    public class GrofilePhoto : GrofilePluralField {

        /// <summary>
        /// Creates a new instance of the photo field
        /// </summary>
        /// <param name="value">The URI of the photo.</param>
        public GrofilePhoto(string value) : this(value, null) { }

        /// <summary>
        /// Creates a new instance of the photo field.
        /// </summary>
        /// <param name="value">The URI of the photo.</param>
        /// <param name="type">The type of the photo.</param>
        public GrofilePhoto(string value, string type) : base(value, type) { }
    }

    /// <summary>
    /// A field in a Gravatar profile that provides information about a user's account.
    /// </summary>
    public class GrofileAccount : GrofilePluralField {

        /// <summary>
        /// Gets the domain of this account.
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// Gets the username of this account.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets a display string for this account.
        /// </summary>
        public string Display { get; private set; }

        /// <summary>
        /// Gets the URL of this account.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the short name of this account.
        /// </summary>
        public string Shortname { get; private set; }

        /// <summary>
        /// Gets a flag indicating whether or not this account has been verified.
        /// </summary>
        public bool Verified { get; private set; }

        /// <summary>
        /// Creates a new instance of a Gravatar profile account.
        /// </summary>
        /// <param name="domain">The domain of the account.</param>
        /// <param name="username">The account username.</param>
        /// <param name="display">The string to display for the account.</param>
        /// <param name="url">The URL of the account.</param>
        /// <param name="shortname">The short name of the account.</param>
        /// <param name="verified">A flag indicating whether or not the account is verified.</param>
        public GrofileAccount(string domain, string username, string display, string url, string shortname, bool verified) {
            Domain = domain;
            Username = username;
            Display = display;
            Url = url;
            Shortname = shortname;
            Verified = verified;
        }

        /// <summary>
        /// Gets a string representation of the account.
        /// </summary>
        /// <returns>The account string to display.</returns>
        public override string ToString() {
            return Display;
        }
    }

    /// <summary>
    /// The name field of a Gravatar profile.
    /// </summary>
    public class GrofileName {

        /// <summary>
        /// Gets the full name, including all middle names, titles, and suffixes as appropriate, formatted for display (e.g. Mr. Joseph Robert Smarr, Esq.). This is the Primary Sub-Field for this field, for the purposes of sorting and filtering. 
        /// </summary>
        public string Formatted { get; private set; }

        /// <summary>
        /// Gets the family name of this Contact, or "Last Name" in most Western languages (e.g. Smarr given the full name Mr. Joseph Robert Smarr, Esq.). 
        /// </summary>
        public string FamilyName { get; private set; }

        /// <summary>
        /// Gets the given name of this Contact, or "First Name" in most Western languages (e.g. Joseph given the full name Mr. Joseph Robert Smarr, Esq.). 
        /// </summary>
        public string GivenName { get; private set; }

        /// <summary>
        /// Gets the middle name(s) of this Contact (e.g. Robert given the full name Mr. Joseph Robert Smarr, Esq.). 
        /// </summary>
        public string MiddleName { get; private set; }

        /// <summary>
        /// Gets the honorific prefix(es) of this Contact, or "Title" in most Western languages (e.g. Mr. given the full name Mr. Joseph Robert Smarr, Esq.). 
        /// </summary>
        public string HonorificPrefix { get; private set; }

        /// <summary>
        /// Gets the honorifix suffix(es) of this Contact, or "Suffix" in most Western languages (e.g. Esq. given the full name Mr. Joseph Robert Smarr, Esq.). 
        /// </summary>
        public string HonorificSuffix { get; private set; }

        /// <summary>
        /// Creates a new instance of the Gravatar profile name.
        /// </summary>
        /// <param name="formatted">The full formatted name.</param>
        /// <param name="familyName">The family name or surname.</param>
        /// <param name="givenName">The given name or first name.</param>
        /// <param name="middleName">The middle name.</param>
        /// <param name="honorificPrefix">The prefix of the name.</param>
        /// <param name="honorificSuffix">The suffix of the name.</param>
        public GrofileName(string formatted, string familyName, string givenName, string middleName, string honorificPrefix, string honorificSuffix) {
            Formatted = formatted;
            FamilyName = familyName;
            GivenName = givenName;
            MiddleName = middleName;
            HonorificPrefix = honorificPrefix;
            HonorificSuffix = honorificSuffix;
        }

        /// <summary>
        /// Gets the name formatted as a string.
        /// </summary>
        /// <returns>The string formatted name.</returns>
        public override string ToString() {
            return Formatted;
        }
    }

    /// <summary>
    /// Interface for Gravatar profile information objects.
    /// </summary>
    public interface IGrofileInfo {

        /// <summary>
        /// Gets the ID value of this profile.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets this profile hash.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// Gets this profile request hash.
        /// </summary>
        string RequestHash { get; }

        /// <summary>
        /// Gets the URL of this profile.
        /// </summary>
        string ProfileUrl { get; }

        /// <summary>
        /// Gets the preferred username of this profile's owner.
        /// </summary>
        string PreferredUsername { get; }

        /// <summary>
        /// Gets the URL of the thumbnail for this profile.
        /// </summary>
        string ThumbnailUrl { get; }

        /// <summary>
        /// Gets the name to display for the owner of this profile.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets information about the owner of this profile.
        /// </summary>
        string AboutMe { get; }

        /// <summary>
        /// Gets the current location value of this profile.
        /// </summary>
        string CurrentLocation { get; }

        /// <summary>
        /// Gets the profile name section of this profile.
        /// </summary>
        GrofileName Name { get; }

        /// <summary>
        /// Gets a collection of URLs associated with this profile.
        /// </summary>
        IEnumerable<GrofileUrl> Urls { get; }

        /// <summary>
        /// Gets a collection of emails associated with this profile.
        /// </summary>
        IEnumerable<GrofileEmail> Emails { get; }

        /// <summary>
        /// Gets a collection of photos associated with this profile.
        /// </summary>
        IEnumerable<GrofilePhoto> Photos { get; }

        /// <summary>
        /// Gets a collection of accounts associated with this profile.
        /// </summary>
        IEnumerable<GrofileAccount> Accounts { get; }
    }

    internal class GrofileInfoXml : IGrofileInfo {

        private readonly XElement _Entry;

        private GrofileName GetName() {
            var formatted = Entry.ElementValueOrDefault("formatted", null);
            var familyName = Entry.ElementValueOrDefault("familyName", null);
            var givenName = Entry.ElementValueOrDefault("givenName", null);
            var middleName = Entry.ElementValueOrDefault("middleName", null);
            var honorificPrefix = Entry.ElementValueOrDefault("honorificPrefix", null);
            var honorificSuffix = Entry.ElementValueOrDefault("honorificSuffix", null);

            return new GrofileName(formatted, familyName, givenName, middleName, honorificPrefix, honorificSuffix);
        }

        private IEnumerable<GrofileUrl> GetUrls() {
            var list = new List<GrofileUrl>();
            var elements = Entry.Elements("urls");
            foreach (var element in elements) {
                var title = element.ElementValueOrDefault("title", null);
                var value = element.ElementValueOrDefault("value", null);
                list.Add(new GrofileUrl(title, value));
            }
            return list.AsEnumerable();
        }

        private IEnumerable<GrofileEmail> GetEmails() {
            var list = new List<GrofileEmail>();
            var elements = Entry.Elements("emails");
            foreach (var element in elements) {
                var value = element.ElementValueOrDefault("value", null);
                var primary = element.ElementValueOrDefault("primary", "false");
                var primaryValue = false;
                bool.TryParse(primary, out primaryValue);
                list.Add(new GrofileEmail(value, primaryValue));
            }
            return list.AsEnumerable();
        }

        private IEnumerable<GrofileAccount> GetAccounts() {
            var list = new List<GrofileAccount>();
            var elements = Entry.Elements("accounts");
            foreach (var element in elements) {
                var domain = element.ElementValueOrDefault("domain", null);
                var username = element.ElementValueOrDefault("username", null);
                var display = element.ElementValueOrDefault("display", null);
                var url = element.ElementValueOrDefault("url", null);
                var shortname = element.ElementValueOrDefault("shortname", null);
                var verified = element.ElementValueOrDefault("verified", "false");
                var verifiedValue = false;
                bool.TryParse(verified, out verifiedValue);
                list.Add(new GrofileAccount(domain, username, display, url, shortname, verifiedValue));
            }
            return list.AsEnumerable();
        }

        private IEnumerable<GrofilePhoto> GetPhotos() {
            var list = new List<GrofilePhoto>();
            var elements = Entry.Elements("photos");
            foreach (var element in elements) {
                var value = element.ElementValueOrDefault("value", null);
                var type = element.ElementValueOrDefault("type", null);
                list.Add(new GrofilePhoto(value, type));
            }
            return list.AsEnumerable();
        }

        public XElement Entry { get { return _Entry; } }

        public string Id { get; private set; }
        public string Hash { get; private set; }
        public string RequestHash { get; private set; }
        public string ProfileUrl { get; private set; }
        public string PreferredUsername { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public string DisplayName { get; private set; }
        public string AboutMe { get; private set; }
        public string CurrentLocation { get; private set; }

        public GrofileName Name { get; private set; }
        public IEnumerable<GrofileUrl> Urls { get; private set; }
        public IEnumerable<GrofileEmail> Emails { get; private set; }
        public IEnumerable<GrofilePhoto> Photos { get; private set; }
        public IEnumerable<GrofileAccount> Accounts { get; private set; }

        public GrofileInfoXml(XElement entry) {
            if (null == entry) throw new ArgumentNullException("entry");
            _Entry = entry;

            Id = Entry.ElementValueOrDefault("id", null);
            Hash = Entry.ElementValueOrDefault("hash", null);
            RequestHash = Entry.ElementValueOrDefault("requestHash", null);
            ProfileUrl = Entry.ElementValueOrDefault("profileUrl", null);
            PreferredUsername = Entry.ElementValueOrDefault("preferredUsername", null);
            ThumbnailUrl = Entry.ElementValueOrDefault("thumbnailUrl", null);
            DisplayName = Entry.ElementValueOrDefault("displayName", null);
            AboutMe = Entry.ElementValueOrDefault("aboutMe", null);
            CurrentLocation = Entry.ElementValueOrDefault("currentLocation", null);

            Name = GetName();
            Urls = GetUrls();
            Emails = GetEmails();
            Photos = GetPhotos();
            Accounts = GetAccounts();
        }

        public override string ToString() {
            return DisplayName;
        }
    }
}
