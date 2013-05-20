using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Taxonomies.Services;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.ContentManagement;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class OEmbedController : Controller {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IMediaLibraryService _mediaLibraryService;

        public OEmbedController(
            ITaxonomyService taxonomyService, 
            IMediaLibraryService mediaManagerService, 
            IOrchardServices services) {
            _taxonomyService = taxonomyService;
            _mediaLibraryService = mediaManagerService;
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        public ActionResult Index(int id) {
            var viewModel = new OEmbedViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(int id, string url) {
            var viewModel = new OEmbedViewModel {
                Url = url,
                Id = id
            };

            try {
                // <link rel="alternate" href="http://vimeo.com/api/oembed.xml?url=http%3A%2F%2Fvimeo.com%2F23608259" type="text/xml+oembed">

                var source = new WebClient().DownloadString(url);

                // seek type="text/xml+oembed"
                var oembedSignature = source.IndexOf("type=\"text/xml+oembed\"", StringComparison.OrdinalIgnoreCase);
                if (oembedSignature != -1) {
                    var tagStart = source.Substring(0, oembedSignature).LastIndexOf('<');
                    var tagEnd = source.IndexOf('>', oembedSignature);
                    var tag = source.Substring(tagStart, tagEnd - tagStart);
                    var matches = new Regex("href=\"([^\"]+)\"").Matches(tag);
                    if (matches.Count > 0) {
                        var href = matches[0].Groups[1].Value;
                        try {   
                            var content = new WebClient().DownloadString(href);
                            viewModel.Content = XDocument.Parse(content);
                        }
                        catch {
                            // bubble exception
                        }
                    }
                }
            }
            catch {
                return View(viewModel);
            }

            return View(viewModel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult MediaPost(int id, string url, string document) {
            var termPart = _taxonomyService.GetTerm(id);

            if (termPart == null) {
                return HttpNotFound();
            }

            var content = XDocument.Parse(document);
            var oembed = content.Root;

            var part = Services.ContentManager.New<MediaPart>("OEmbed");
            part.TermPart = _taxonomyService.GetTerm(id);

            part.Resource = url;
            part.MimeType = "text/html";
            part.Title = oembed.Element("title").Value;
            if (oembed.Element("description") != null) {
                part.Caption = oembed.Element("description").Value;
            }
            var oembedPart = part.As<OEmbedPart>();

            foreach (var element in oembed.Elements()) {
                oembedPart[element.Name.LocalName] = element.Value;
            }

            
            Services.ContentManager.Create(oembedPart);

            var viewModel = new OEmbedViewModel {
                Id = id
            };

            return View("Index", viewModel);
        }
    }
}