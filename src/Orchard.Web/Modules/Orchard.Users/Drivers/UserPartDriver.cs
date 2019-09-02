using System;
using System.Web.Security;
using Orchard.ContentManagement.Drivers;
using Orchard.Users.Models;

namespace Orchard.Users.Drivers {
    /// <summary>
    /// This class intentionnaly has no Display method to prevent external access to this information through standard 
    /// Content Item display methods.
    /// </summary>
    public class UserPartDriver : ContentPartDriver<UserPart> {

        protected override void Importing(UserPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            part.Email = context.Attribute(part.PartDefinition.Name, "Email");
            part.EmailChallengeToken = context.Attribute(part.PartDefinition.Name, "EmailChallengeToken");
            part.EmailStatus = (UserStatus)Enum.Parse(typeof(UserStatus), context.Attribute(part.PartDefinition.Name, "EmailStatus"));
            part.HashAlgorithm = context.Attribute(part.PartDefinition.Name, "HashAlgorithm");
            part.NormalizedUserName = context.Attribute(part.PartDefinition.Name, "NormalizedUserName");
            part.Password = context.Attribute(part.PartDefinition.Name, "Password");
            part.PasswordFormat = (MembershipPasswordFormat)Enum.Parse(typeof(MembershipPasswordFormat), context.Attribute(part.PartDefinition.Name, "PasswordFormat"));
            part.PasswordSalt = context.Attribute(part.PartDefinition.Name, "PasswordSalt");
            part.RegistrationStatus = (UserStatus)Enum.Parse(typeof(UserStatus), context.Attribute(part.PartDefinition.Name, "RegistrationStatus"));
            part.UserName = context.Attribute(part.PartDefinition.Name, "UserName");
            part.LastPasswordChangeUtc = DateTime.Parse(context.Attribute(part.PartDefinition.Name, "LastPasswordChangeUtc"));
        }

        protected override void Exporting(UserPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Email", part.Email);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EmailChallengeToken", part.EmailChallengeToken);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EmailStatus", part.EmailStatus);
            context.Element(part.PartDefinition.Name).SetAttributeValue("HashAlgorithm", part.HashAlgorithm);
            context.Element(part.PartDefinition.Name).SetAttributeValue("NormalizedUserName", part.NormalizedUserName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Password", part.Password);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PasswordFormat", part.PasswordFormat);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PasswordSalt", part.PasswordSalt);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RegistrationStatus", part.RegistrationStatus);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserName", part.UserName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LastPasswordChangeUtc", part.LastPasswordChangeUtc);
        }
    }
}