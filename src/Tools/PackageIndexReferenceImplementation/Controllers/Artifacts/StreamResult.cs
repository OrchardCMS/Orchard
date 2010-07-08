using System.IO;
using System.Web.Mvc;

namespace PackageIndexReferenceImplementation.Controllers {
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