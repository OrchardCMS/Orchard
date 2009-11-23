using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Comments {
    public class Permissions : IPermissionProvider {
        public static readonly Permission AddComment = new Permission { Description = "Adding a Comment", Name = "AddComment" };
        public static readonly Permission AddCommentWithoutValidation = new Permission { Description = "Adding a Comment without validation", Name = "AddCommentWithoutValidation" };
        public static readonly Permission EnableComment = new Permission { Description = "Enabling Comments on content items", Name = "EnableComment" };
        public static readonly Permission CloseComment = new Permission { Description = "Closing Comments", Name = "CloseComment" };
        public static readonly Permission CloseCommentOnOwnItems = new Permission { Description = "Closing Comments on own items", Name = "CloseCommentOnOwnItems" };
        public static readonly Permission ModerateComment = new Permission { Description = "Moderating Comments", Name = "ModerateComment" };
        public static readonly Permission ModerateCommentOnOwnItems = new Permission { Description = "Moderating Comments On Own Items", Name = "ModerateCommentOnOwnItems" };

        public string PackageName {
            get {
                return "Comments";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                AddComment,
                AddCommentWithoutValidation,
                EnableComment,
                CloseComment,
                CloseCommentOnOwnItems,
                ModerateComment,
                ModerateCommentOnOwnItems
            };
        }
    }
}
