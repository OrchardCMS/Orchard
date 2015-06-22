﻿using System;
using System.Web.Security;
using Orchard.ContentManagement.Drivers;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;
using Orchard.ContentManagement;

namespace Orchard.Users.Drivers
{
    /// <summary>
    /// This class intentionnaly has no Display method to prevent external access to this information through standard 
    /// Content Item display methods.
    /// </summary>
    public class UserPartDriver : ContentPartDriver<UserPart>
    {

        protected override DriverResult Display(UserPart part, string displayType, dynamic shapeHelper)
        {
            UserIndexViewModel viewModel = new UserIndexViewModel();
            viewModel.MapPart(part);

            if (displayType == "SummaryAdmin")
            {
                return Combined(new DriverResult[]{
                    ContentShape("Parts_User", () => shapeHelper.Parts_UserSummaryAdmin(Model: viewModel)),
                    ContentShape("User_AdminActions", () => shapeHelper.Parts_UserAdminActions(Model: viewModel))
                });
            }

            return ContentShape("Parts_User", () => shapeHelper.Parts_UserSummaryAdmin(
                Model: viewModel
                ));
        }

        protected override DriverResult Editor(UserPart part, dynamic shapeHelper)
        {
            return ContentShape("", () => shapeHelper.EditorTemplate());
        }

        protected override DriverResult Editor(UserPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return Editor(part, shapeHelper);
        }

        protected override void Importing(UserPart part, ContentManagement.Handlers.ImportContentContext context)
        {
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
        }
    }
}