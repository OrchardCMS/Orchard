using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Services;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class OEmbedController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public OEmbedController(
            IOrchardServices services,
            IMediaLibraryService mediaManagerService) {
            Services = services;
            _mediaLibraryService = mediaManagerService;
        }

        public IOrchardServices Services { get; set; }

        public ActionResult Index(string folderPath, string type) {
            var viewModel = new OEmbedViewModel {
                FolderPath = folderPath,
                Type = type
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        [ValidateInput(false)]
        public ActionResult IndexPOST(string folderPath, string url, string type, string title, string html, string thumbnail, string width, string height, string description) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            // Check permission.
            var rootMediaFolder = _mediaLibraryService.GetRootMediaFolder();
            if (!Services.Authorizer.Authorize(Permissions.ManageMediaContent) && !_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var viewModel = new OEmbedViewModel {
                Url = url,
                FolderPath = folderPath
            };

            var webClient = new WebClient {Encoding = Encoding.UTF8};
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
                            var content = webClient.DownloadString(Server.HtmlDecode(href));
                            viewModel.Content = XDocument.Parse(content);
                        }
                        catch {
                            // bubble exception
                        }
                    }
                }
                if (viewModel.Content == null) {
                    viewModel.Content = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XElement("oembed")
                        );
                }
                var root = viewModel.Content.Root;
                if (!String.IsNullOrWhiteSpace(url)) {
                    root.El("url", url);
                }
                if (!String.IsNullOrWhiteSpace(type)) {
                    root.El("type", type.ToLowerInvariant());
                }
                if (!String.IsNullOrWhiteSpace(title)) {
                    root.El("title", title);
                }
                if (!String.IsNullOrWhiteSpace(html)) {
                    root.El("html", html);
                }
                if (!String.IsNullOrWhiteSpace(thumbnail)) {
                    root.El("thumbnail", thumbnail);
                }
                if (!String.IsNullOrWhiteSpace(width)) {
                    root.El("width", width);
                }
                if (!String.IsNullOrWhiteSpace(height)) {
                    root.El("height", height);
                }
                if (!String.IsNullOrWhiteSpace(description)) {
                    root.El("description", description);
                }
                Response.AddHeader("X-XSS-Protection", "0"); // Prevents Chrome from freaking out over embedded preview
            }
            catch {
                return View(viewModel);
            }

            return View(viewModel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult MediaPost(string folderPath, string url, string document) {
            var content = XDocument.Parse(document);
            var oembed = content.Root;

            var part = Services.ContentManager.New<MediaPart>("OEmbed");
                        
            part.MimeType = "text/html";
            part.FolderPath = folderPath;
            part.LogicalType = "OEmbed";

            if (oembed.Element("title") != null) {
                part.Title = oembed.Element("title").Value;
            }
            else {
                part.Title = oembed.Element("url").Value;
            }
            if (oembed.Element("description") != null) {
                part.Caption = oembed.Element("description").Value;
            }

            var oembedPart = part.As<OEmbedPart>();

            if (oembedPart != null) {

                oembedPart.Source = url;

                foreach (var element in oembed.Elements()) {
                    oembedPart[element.Name.LocalName] = element.Value;
                }

                Services.ContentManager.Create(oembedPart);
            }

            var viewModel = new OEmbedViewModel {
                FolderPath = folderPath
            };

            return View("Index", viewModel);
        }
    }
}