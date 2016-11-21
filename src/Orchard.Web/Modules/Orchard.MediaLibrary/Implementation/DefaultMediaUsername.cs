using System;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Providers;
using Orchard.Users.Models;

namespace Orchard.MediaLibrary.Implementation {
    public class DefaultMediaUsername : IMediaFolderProvider {
        public virtual string GetFolderName(ContentItem content) {
            if (content.As<UserPart>() != null) {
                string folder = "";
                foreach (char c in content.As<UserPart>().UserName) {
                    if (char.IsLetterOrDigit(c)) {
                        folder += c;
                    }
                    else
                        folder += "_" + String.Format("{0:X}", Convert.ToInt32(c));
                }
                return folder;
            }
            else {
                return null;
            }
        }
    }
}