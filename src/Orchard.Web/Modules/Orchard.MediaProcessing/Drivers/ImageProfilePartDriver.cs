using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using Orchard.MediaProcessing.ViewModels;
using Orchard.Utility.Extensions;

namespace Orchard.MediaProcessing.Drivers {

    public class ImageProfilePartDriver : ContentPartDriver<ImageProfilePart> {
        private readonly IImageProfileService _imageProfileService;

        private const string TemplateName = "Parts.MediaProcessing.ImageProfilePart";

        public ImageProfilePartDriver(IImageProfileService imageProfileService) {
            _imageProfileService = imageProfileService;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "MediaProcessing"; }
        }

        protected override DriverResult Display(ImageProfilePart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_MediaProcessing_ImageProfile",
                                () => shapeHelper.Parts_MediaProcessing_ImageProfile(Name: part.Name));
        }

        protected override DriverResult Editor(ImageProfilePart part, dynamic shapeHelper) {
            var viewModel = new ImageProfileViewModel {
                Name = part.Name
            };
            return ContentShape("Parts_MediaProcessing_ImageProfile_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(ImageProfilePart part, IUpdateModel updater, dynamic shapeHelper) {
            var currentName = part.Name;
            var viewModel = new ImageProfileViewModel();
            
            // It would be nice if IUpdateModel provided access to the IsValid property of the Controller, instead of having to track a local flag.
            var isValid = updater.TryUpdateModel(viewModel, Prefix, null, null);
            if (String.IsNullOrWhiteSpace(viewModel.Name)) {
                updater.AddModelError("Name", T("The Name can't be empty."));
                isValid = false;
            }
            if (currentName != viewModel.Name && _imageProfileService.GetImageProfileByName(viewModel.Name) != null) {
                updater.AddModelError("Name", T("A profile with the same Name already exists."));
                isValid = false;
            }
            if (viewModel.Name != viewModel.Name.ToSafeName()) {
                updater.AddModelError("Name", T("The Name can only contain letters and numbers without spaces"));
                isValid = false;
            }

            if (isValid) {
                part.Name = viewModel.Name;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(ImageProfilePart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element.Add(
                new XAttribute("Name", part.Name),
                new XElement("Filters",
                             part.Filters.Select(filter =>
                                                 new XElement("Filter",
                                                              new XAttribute("Description", filter.Description ?? ""),
                                                              new XAttribute("Category", filter.Category ?? ""),
                                                              new XAttribute("Type", filter.Type ?? ""),
                                                              new XAttribute("Position", filter.Position),
                                                              new XAttribute("State", filter.State ?? "")
                                                     )
                                 )
                    )
                );
        }

        protected override void Importing(ImageProfilePart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            var element = context.Data.Element(part.PartDefinition.Name);

            part.Name = element.Attribute("Name").Value;

            var filterRecords = element.Element("Filters").Elements("Filter").Select(filter => new FilterRecord {
                Description = filter.Attribute("Description").Value,
                Category = filter.Attribute("Category").Value,
                Type = filter.Attribute("Type").Value,
                Position = Convert.ToInt32(filter.Attribute("Position").Value),
                State = filter.Attribute("State").Value
            });

            foreach (var result in filterRecords) {
                part.Record.Filters.Add(result);
            }
        }
    }
}