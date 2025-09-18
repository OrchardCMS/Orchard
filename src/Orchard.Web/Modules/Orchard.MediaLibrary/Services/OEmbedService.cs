using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Orchard.MediaLibrary.Services {
    public class OEmbedService : IOEmbedService {
        public XDocument DownloadMediaData(string url) {
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            XDocument doc = null;
            try {
                // <link rel="alternate" href="http://vimeo.com/api/oembed.xml?url=http%3A%2F%2Fvimeo.com%2F23608259" type="text/xml+oembed">
                var source = webClient.DownloadString(url);

                // seek type="text/xml+oembed" or application/xml+oembed
                var oembedSignature = source.IndexOf("type=\"text/xml+oembed\"", StringComparison.OrdinalIgnoreCase);
                if (oembedSignature == -1) {
                    oembedSignature = source.IndexOf("type=\"application/xml+oembed\"", StringComparison.OrdinalIgnoreCase);
                }
                if (oembedSignature != -1) {
                    var tagStart = source.Substring(0, oembedSignature).LastIndexOf('<');
                    var tagEnd = source.IndexOf('>', oembedSignature);
                    var tag = source.Substring(tagStart, tagEnd - tagStart);
                    var matches = new Regex("href=\"([^\"]+)\"").Matches(tag);
                    if (matches.Count > 0) {
                        var href = matches[0].Groups[1].Value;
                        try {
                            var content = webClient.DownloadString(HttpUtility.HtmlDecode(href));
                            doc = XDocument.Parse(content);
                        } catch {
                            // bubble exception
                        }
                    }
                }
            } catch {
                doc = null;
            }

            if (doc == null) {
                doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("oembed")
                    );
            }

            return doc;
        }
    }
}