﻿using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class VectorImagePartDriver : ContentPartDriver<VectorImagePart> {

        protected override DriverResult Display(VectorImagePart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_VectorImage_Metadata", () => shapeHelper.Parts_VectorImage_Metadata()),
                ContentShape("Parts_VectorImage_Summary", () => shapeHelper.Parts_VectorImage_Summary()),
                ContentShape("Parts_VectorImage", () => shapeHelper.Parts_VectorImage()),
                ContentShape("Parts_VectorImage_SummaryAdmin", () => shapeHelper.Parts_VectorImage_SummaryAdmin())
            );
        }
    }
}