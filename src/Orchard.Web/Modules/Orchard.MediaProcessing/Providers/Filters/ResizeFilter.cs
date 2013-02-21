using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.MediaProcessing.Descriptors.Filter;
using Orchard.MediaProcessing.Services;

namespace Orchard.MediaProcessing.Providers.Filters {
    public class ResizeFilter : IImageFilterProvider {
        public ResizeFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Transform", T("Transform"), T("Transform"))
                .Element("Resize", T("Resize"), T("Resizes to a fixed height and width"),
                         ApplyFilter,
                         DisplayFilter,
                         "ResizeFilter"
                );
        }

        public void ApplyFilter(FilterContext context) {
            var newHeight = int.Parse((string)context.State.Height);
            newHeight = newHeight > 0 ? newHeight : context.Image.Height;
            var heightFactor = (float) context.Image.Height/newHeight;

            var newWidth = int.Parse((string)context.State.Width);
            newWidth = newWidth > 0 ? newWidth : context.Image.Width;
            var widthFactor = context.Image.Width/newWidth;

            if (widthFactor != heightFactor) {
                if (widthFactor > heightFactor) {
                    newHeight = Convert.ToInt32(context.Image.Height/widthFactor);
                }
                else {
                    newWidth = Convert.ToInt32(context.Image.Width/heightFactor);
                }
            }

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage)) {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(context.Image, 0, 0, newWidth, newHeight);
            }

            context.Image = newImage;
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Resize to {0}px high x {1}px wide", context.State.Height, context.State.Width);
        }
    }

    public class ResizeFilterForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ResizeFilterForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = Shape.Form(
                        Id: "ImageResizeFilter",
                        _Height: Shape.Textbox(
                            Id: "height", Name: "Height",
                            Title: T("Height"),
                            Description: T("The height in pixels, 0 to allow any value."),
                            Classes: new[] {"text-small"}),
                        _Width: Shape.Textbox(
                            Id: "width", Name: "Width",
                            Title: T("Width"),
                            Description: T("The width in pixels, 0 to allow any value."),
                            Classes: new[] {"text-small"}));
                    return f;
                };

            context.Form("ResizeFilter", form);
        }
    }
}