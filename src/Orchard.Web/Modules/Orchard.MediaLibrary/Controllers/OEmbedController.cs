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
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class OEmbedController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;

        public OEmbedController(
            IOrchardServices services,
            IMediaLibraryService mediaManagerService) {
            _mediaLibraryService = mediaManagerService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(string folderPath, string type, int? replaceId) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath))
                return new HttpUnauthorizedResult();

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var viewModel = new OEmbedViewModel {
                FolderPath = folderPath,
            };

            if (replaceId != null) {
                var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId.Value);
                if (replaceMedia == null)
                    return HttpNotFound();

                viewModel.Replace = replaceMedia;

                if (!replaceMedia.TypeDefinition.Name.Equals("OEmbed"))
                    Services.Notifier.Error(T("Cannot replace {0} with OEmbed", replaceMedia.ContentItem.TypeDefinition.Name));
            }

            return View(viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        [ValidateInput(false)]
        public ActionResult IndexPOST(string folderPath, string url, string type, string title, string html, string thumbnail, string width, string height, string description, int? replaceId) {
            var viewModel = new OEmbedViewModel {
                Url = url,
                FolderPath = folderPath,
            };

            if (replaceId != null) {
                var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId.Value);
                if (replaceMedia == null)
                    return HttpNotFound();

                viewModel.Replace = replaceMedia;

                if (!replaceMedia.ContentItem.TypeDefinition.Name.Equals("OEmbed")) {
                    Services.Notifier.Error(T("Cannot replace {0} with OEmbed", replaceMedia.ContentItem.TypeDefinition.Name));
                    return View(viewModel);
                }
            }

            var webClient = new WebClient { Encoding = Encoding.UTF8 };
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
        public ActionResult Import(string folderPath, string url, string document) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath)) {
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

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
                Services.Notifier.Information(T("Media imported successfully."));
            }

            return RedirectToAction("Index", new { folderPath = folderPath });
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Replace(int replaceId, string url, string document) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia)) {
                return new HttpUnauthorizedResult();
            }

            var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId);
            if (replaceMedia == null)
                return HttpNotFound();

            // Check permission
            if (!(_mediaLibraryService.CheckMediaFolderPermission(Permissions.EditMediaContent, replaceMedia.FolderPath) && _mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, replaceMedia.FolderPath)) 
                && !_mediaLibraryService.CanManageMediaFolder(replaceMedia.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            var content = XDocument.Parse(document);
            var oembed = content.Root;

            if (oembed.Element("title") != null) {
                replaceMedia.Title = oembed.Element("title").Value;
            }
            else {
                replaceMedia.Title = oembed.Element("url").Value;
            }
            if (oembed.Element("description") != null) {
                replaceMedia.Caption = oembed.Element("description").Value;
            }

            var oembedPart = replaceMedia.As<OEmbedPart>();

            if (oembedPart != null) {
                replaceMedia.ContentItem.Record.Infoset.Element.Element("OEmbedPart").Remove();

                oembedPart.Source = url;

                foreach (var element in oembed.Elements()) {
                    oembedPart[element.Name.LocalName] = element.Value;
                }

                Services.ContentManager.Publish(oembedPart.ContentItem);
                Services.Notifier.Information(T("Media replaced successfully."));
            }

            return RedirectToAction("Index", new { folderPath = replaceMedia.FolderPath, replaceId = replaceMedia.Id });
        }
    }
}