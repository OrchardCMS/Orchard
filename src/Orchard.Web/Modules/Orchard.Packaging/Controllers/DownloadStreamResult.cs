using System.IO;
using System.Web.Mvc;

namespace Orchard.Packaging.Controllers {
    public class DownloadStreamResult : ActionResult {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream Stream { get; set; }

        public DownloadStreamResult(string fileName, string contentType, Stream stream) {
            FileName = fileName;
            ContentType = contentType;
            Stream = stream;
        }

        public override void ExecuteResult(ControllerContext context) {
            context.HttpContext.Response.ContentType = ContentType;
            context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=\"" + FileName + "\"");
            Stream.Seek(0, SeekOrigin.Begin);
            Stream.CopyTo(context.HttpContext.Response.OutputStream);
        }
    }
}