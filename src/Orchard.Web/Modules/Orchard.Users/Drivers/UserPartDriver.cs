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
            part.Record.Email = context.Attribute(part.PartDefinition.Name, "Email");
            part.Record.EmailChallengeToken = context.Attribute(part.PartDefinition.Name, "EmailChallengeToken");
            part.Record.EmailStatus = (UserStatus)Enum.Parse(typeof(UserStatus), context.Attribute(part.PartDefinition.Name, "EmailStatus"));
            part.Record.HashAlgorithm = context.Attribute(part.PartDefinition.Name, "HashAlgorithm");
            part.Record.NormalizedUserName = context.Attribute(part.PartDefinition.Name, "NormalizedUserName");
            part.Record.Password = context.Attribute(part.PartDefinition.Name, "Password");
            part.Record.PasswordFormat = (MembershipPasswordFormat)Enum.Parse(typeof(MembershipPasswordFormat), context.Attribute(part.PartDefinition.Name, "PasswordFormat"));
            part.Record.PasswordSalt = context.Attribute(part.PartDefinition.Name, "PasswordSalt");
            part.Record.RegistrationStatus = (UserStatus)Enum.Parse(typeof(UserStatus), context.Attribute(part.PartDefinition.Name, "RegistrationStatus"));
            part.Record.UserName = context.Attribute(part.PartDefinition.Name, "UserName");
        }

        protected override void Exporting(UserPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Email", part.Record.Email);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EmailChallengeToken", part.Record.EmailChallengeToken);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EmailStatus", part.Record.EmailStatus);
            context.Element(part.PartDefinition.Name).SetAttributeValue("HashAlgorithm", part.Record.HashAlgorithm);
            context.Element(part.PartDefinition.Name).SetAttributeValue("NormalizedUserName", part.Record.NormalizedUserName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Password", part.Record.Password);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PasswordFormat", part.Record.PasswordFormat);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PasswordSalt", part.Record.PasswordSalt);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RegistrationStatus", part.Record.RegistrationStatus);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserName", part.Record.UserName);
        }
    }
}