using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace PackageIndexReferenceImplementation.Services {
    public class MediaStorage {
        public void StoreMedia(string identifier, Stream data) {
            if (!Directory.Exists(HostingEnvironment.MapPath("~/App_Data/Media")))
                Directory.CreateDirectory(HostingEnvironment.MapPath("~/App_Data/Media"));

            var safeIdentifier = GetSafeIdentifier(identifier);
            var filePath = HostingEnvironment.MapPath("~/App_Data/Media/" + safeIdentifier);
            using (var destination = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                data.Seek(0, SeekOrigin.Begin);
                data.CopyTo(destination);
            }
        }

        public Stream GetMedia(string identifier) {
            if (!Directory.Exists(HostingEnvironment.MapPath("~/App_Data/Media")))
                Directory.CreateDirectory(HostingEnvironment.MapPath("~/App_Data/Media"));

            var safeIdentifier = GetSafeIdentifier(identifier);
            var filePath = HostingEnvironment.MapPath("~/App_Data/Media/" + safeIdentifier);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        static string GetSafeIdentifier(string identifier) {
            var invalidFileNameChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct();
            var safeIdentifier = identifier.Replace("^", string.Format("^{0:X2}", (int)'^'));
            foreach (var ch in invalidFileNameChars) {
                safeIdentifier = safeIdentifier.Replace(new string(ch, 1), string.Format("^{0:X2}", (int)ch));
            }
            return safeIdentifier;
        }
    }
}