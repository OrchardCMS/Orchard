using System;
using System.IO;
using System.Net;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Packaging.Services;

namespace Orchard.Packaging.Commands {
    [OrchardFeature("Gallery")]
    public class GalleryCommands : DefaultOrchardCommandHandler {
        private readonly IPackageManager _packageManager;

        [OrchardSwitch]
        public string User { get; set; }

        [OrchardSwitch]
        public string Password { get; set; }

        public GalleryCommands(IPackageManager packageManager) {
            _packageManager = packageManager;
        }

#if false
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
#endif

        [CommandHelp("gallery submit module <moduleName> <feedUrl> /User:<user> /Password:<password>\r\n\t" + "Package a module into a distributable and push it to a feed server.")]
        [CommandName("gallery submit module")]
        [OrchardSwitches("User,Password")]
        public void SubmitModule(string moduleName, string feedUrl) {
            var packageData = _packageManager.Harvest(moduleName);

            if ( String.IsNullOrWhiteSpace(User) ) {
                Context.Output.WriteLine("Missing or incorrect User");
                return;
            }

            if ( String.IsNullOrWhiteSpace(Password) ) {
                Context.Output.WriteLine("Missing or incorrect Password");
                return;
            }

            try {
                _packageManager.Push(packageData, feedUrl, User, Password);
                Context.Output.WriteLine("Success");
            }
            catch (WebException webException) {
                var text = new StreamReader(webException.Response.GetResponseStream()).ReadToEnd();
                throw new ApplicationException(text);
            }
        }

        [CommandHelp("gallery submit package <filePath> <feedUrl> /User:<user> /Password:<password>\r\n\t" + "Push a packaged module to a feed server.")]
        [CommandName("gallery submit package")]
        [OrchardSwitches("User,Password")]
        public void SubmitPackage(string filePath, string feedUrl) {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read)) {
                var packageData = new PackageData {
                    PackageStream =  stream
                };

                if ( String.IsNullOrWhiteSpace(User) ) {
                    Context.Output.WriteLine("Missing or incorrect User");
                    return;
                }

                if ( String.IsNullOrWhiteSpace(Password) ) {
                    Context.Output.WriteLine("Missing or incorrect Password");
                    return;
                }

                try {
                    _packageManager.Push(packageData, feedUrl, User, Password);
                    Context.Output.WriteLine("Success");
                }
                catch (WebException webException) {
                    var text = new StreamReader(webException.Response.GetResponseStream()).ReadToEnd();
                    throw new ApplicationException(text);
                }
            }
        }
    }
}

