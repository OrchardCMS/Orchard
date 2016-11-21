using System;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Providers;
using Orchard.Security;

namespace Orchard.MediaLibrary.Implementation {
    public class DefaultMediaUsername : IMediaFolderProvider {
        public virtual string GetFolderName(IUser content) {
                string folder = "";
                foreach (char c in content.UserName) {
                    if (char.IsLetterOrDigit(c)) {
                        folder += c;
                    }
                    else
                        folder += "_" + String.Format("{0:X}", Convert.ToInt32(c));
                }
                return folder;
        }
    }
}