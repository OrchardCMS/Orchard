using System;
using System.IO;
using System.Web.Mvc;

namespace PackageIndexReferenceImplementation.Controllers {
    public class StreamResult : ActionResult {
        public string ContentType { get; set; }
        public Stream Stream { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public StreamResult(string contentType, Stream stream) : this(contentType, stream, null) {
        }

        public StreamResult(string contentType, Stream stream, DateTime? lastModified) {
            ContentType = contentType;
            Stream = stream;
            LastModifiedDate = lastModified;
        }

        public override void ExecuteResult(ControllerContext context) {
            var response = context.HttpContext.Response;
            response.ContentType = ContentType;
            if (LastModifiedDate.HasValue) {
                response.Cache.SetLastModified(LastModifiedDate.Value);
                response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
            }
            Stream.CopyTo(response.OutputStream);
        }
    }
}