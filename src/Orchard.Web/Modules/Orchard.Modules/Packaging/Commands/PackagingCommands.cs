using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Modules.Packaging.Services;

namespace Orchard.Modules.Packaging.Commands {
    [OrchardFeature("Orchard.Modules.Packaging")]
    public class PackagingCommands : DefaultOrchardCommandHandler {
        private readonly IPackageBuilder _packageBuilder;

        public PackagingCommands(IPackageBuilder packageBuilder) {
            _packageBuilder = packageBuilder;
        }

        [CommandHelp("harvest <moduleName>\r\n\t" + "Package a module into a distributable")]
        [CommandName("harvest")]
        public void PackageCreate(string moduleName) {
            var stream = _packageBuilder.Create(moduleName);
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            using(var fileStream = new FileStream(HostingEnvironment.MapPath("~/Modules/" + moduleName + ".zip"), FileMode.Create, FileAccess.Write)) {

                const int chunk = 512;
                var dataBuffer = new byte[3*chunk];
                var charBuffer = new char[4*chunk + 2];
                for (;;) {
                    var dataCount = stream.Read(dataBuffer, 0, dataBuffer.Length);
                    if (dataCount <= 0)
                        return;

                    fileStream.Write(dataBuffer, 0, dataCount);

                    var charCount = Convert.ToBase64CharArray(dataBuffer, 0, dataCount, charBuffer, 0);
                    Context.Output.Write(charBuffer, 0, charCount);
                }

            }
        }

        [CommandHelp("harvest post <moduleName> <feedUrl>\r\n\t" + "Package a module into a distributable and push it to a feed server.")]
        [CommandName("harvest post")]
        public void PackageCreate(string moduleName, string feed) {
            var stream = _packageBuilder.Create(moduleName);
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            var request = WebRequest.Create(feed);
            request.Method = "POST";
            request.ContentType = "application/x-package";
            using (var requestStream = request.GetRequestStream()) {
                stream.CopyTo(requestStream);
            }
            try {
                using (var response = request.GetResponse()) {
                    Context.Output.Write("Success: {0}", response.ResponseUri);
                }
            }
            catch (WebException webException) {
                var text = new StreamReader(webException.Response.GetResponseStream()).ReadToEnd();
                throw new ApplicationException(text);
            }
        }
    }
}

