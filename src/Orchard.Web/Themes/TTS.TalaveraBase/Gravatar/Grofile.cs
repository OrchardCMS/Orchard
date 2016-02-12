using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;

namespace TTS.TalaveraBase.Gravatar {

    internal interface IGrofileHelper {
        XDocument LoadXml(string uri);
    }

    internal class GrofileHelper : IGrofileHelper {
        public XDocument LoadXml(string uri) {
            return XDocument.Load(uri);
        }
    }

    /// <summary>
    /// Class to provide functionality for dealing with Gravatar profile information.
    /// </summary>
    public class Grofile {

        #region Private Members
        private readonly IGrofileHelper _Helper = new GrofileHelper();
        #endregion

        #region Internal Members
        internal IGrofileHelper Helper { get { return _Helper; } }

        internal string GetXmlLink(string email) {
            return GetLink(email) + ".xml";
        }

        internal string GetJsonLink(string email) {
            return GetJsonLink(email, null);
        }

        internal string GetJsonLink(string email, string callback) {
            var link = GetLink(email) + ".json";
            if (!string.IsNullOrEmpty(callback))
                link += "?callback=" + callback;
            return link;
        }

        internal Grofile(IGrofileHelper helper) {
            if (null == helper) throw new ArgumentNullException("helper");
            _Helper = helper;
        }

        internal XDocument GetXDocument(string email) {
            return Helper.LoadXml(GetXmlLink(email));
        }
        #endregion

        #region Public Members

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public Grofile() { }

        /// <summary>
        /// Gets the URL that links to the Gravatar profile of the specified <paramref name="email"/>.
        /// </summary>
        /// <param name="email">The email whose profile should be linked.</param>
        /// <returns>The URL of the profile for the specified <paramref name="email"/>.</returns>
        public string GetLink(string email) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(email.Trim()))
                throw new ArgumentException("The email cannot be empty.", "email");

            return "http://www.gravatar.com/" + new Gremail(email).Hash();
        }

        /// <summary>
        /// Parses Gravatar profile information for the specified <paramref name="email"/> into an object.
        /// </summary>
        /// <param name="email">The email whose profile information should be returned.</param>
        /// <returns>An object that contains information about the Gravatar profile for the specified <paramref name="email"/>.</returns>
        public IGrofileInfo GetInfo(string email) {
            var xdoc = GetXDocument(email);
            var entry = xdoc.Descendants("entry").FirstOrDefault();
            if (entry == null) return null;
            return new GrofileInfoXml(entry);
        }

        /// <summary>
        /// Creates a script tag that can be included in an HTML page to process a Gravatar profile on the client.
        /// </summary>
        /// <param name="email">The email whose profile should be processed.</param>
        /// <param name="callback">
        /// The JavaScript callback function which should be called after the profile information is loaded. The profile
        /// information will be passed as a paramter to this callback.
        /// </param>
        /// <returns>A rendered script tag that can be included in an HTML page.</returns>
        public string RenderScript(string email, string callback) {
            var src = GetJsonLink(email, callback);
            var tag = "<script type=\"text/javascript\" src=\"" + src + "\"></script>";
            return tag;
        }
        #endregion
    }
}
