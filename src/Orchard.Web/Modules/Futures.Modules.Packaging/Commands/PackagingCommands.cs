using System;
using System.IO;
using System.Net;
using Orchard.Commands;
using Orchard.Packaging;

namespace Futures.Modules.Packaging.Commands {
    public class PackagingCommands : DefaultOrchardCommandHandler {
        private readonly IPackageManager _packageManager;

        public PackagingCommands(IPackageManager packageManager) {
            _packageManager = packageManager;
        }

        [CommandHelp("harvest <moduleName>\r\n\t" + "Package a module into a distributable")]
        [CommandName("harvest")]
        public void PackageCreate(string moduleName) {
            var packageData = _packageManager.Harvest(moduleName);
            if (packageData.PackageStream.CanSeek)
                packageData.PackageStream.Seek(0, SeekOrigin.Begin);

            const int chunk = 512;
            var dataBuffer = new byte[3 * chunk];
            var charBuffer = new char[4 * chunk + 2];
            for (; ; ) {
                var dataCount = packageData.PackageStream.Read(dataBuffer, 0, dataBuffer.Length);
                if (dataCount <= 0)
                    return;

                var charCount = Convert.ToBase64CharArray(dataBuffer, 0, dataCount, charBuffer, 0);
                Context.Output.Write(charBuffer, 0, charCount);
            }
        }

        [CommandHelp("harvest post <moduleName> <feedUrl>\r\n\t" + "Package a module into a distributable and push it to a feed server.")]
        [CommandName("harvest post")]
        public void PackageCreate(string moduleName, string feedUrl) {
            var packageData = _packageManager.Harvest(moduleName);
            _packageManager.Push(packageData, feedUrl);

            try {
                _packageManager.Push(packageData, feedUrl);
                Context.Output.WriteLine("Success");
            }
            catch (WebException webException) {
                var text = new StreamReader(webException.Response.GetResponseStream()).ReadToEnd();
                throw new ApplicationException(text);
            }
        }
    }
}

