using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PackageIndexReferenceImplementation.Services;

namespace PackageIndexReferenceImplementation.Controllers
{
    public class MediaController : Controller
    {  
        private readonly MediaStorage _mediaStorage;

        public MediaController() {
            _mediaStorage = new MediaStorage();
        }

        public ActionResult Resource(string id, string contentType)
        {
            return new StreamResult(contentType, _mediaStorage.GetMedia(id + ":" + contentType));
        }
    }

    public class StreamResult : ActionResult {
        public string ContentType { get; set; }
        public Stream Stream { get; set; }

        public StreamResult(string contentType, Stream stream) {
            ContentType = contentType;
            Stream = stream;
        }

        public override void ExecuteResult(ControllerContext context) {
            context.HttpContext.Response.ContentType = ContentType;
            Stream.CopyTo(context.HttpContext.Response.OutputStream);
        }
    }
}
