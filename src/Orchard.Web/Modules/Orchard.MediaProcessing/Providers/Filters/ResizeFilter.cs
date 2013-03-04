using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using ImageResizer;
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
                .Element("Resize", T("Resize"), T("Resizes using predefined height or width."),
                         ApplyFilter,
                         DisplayFilter,
                         "ResizeFilter"
                );
        }

        public void ApplyFilter(FilterContext context) {
            int witdh = context.State.Width;
            int height = context.State.Height;

            var settings = new ResizeSettings {
                Mode = FitMode.Max,
                Height = height,
                Width = witdh
            };

            var result = new MemoryStream();
            ImageBuilder.Current.Build(context.Media, result, settings);
            context.Media = result;
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
                        _Width: Shape.Textbox(
                            Id: "width", Name: "Width",
                            Title: T("Width"),
                            Description: T("The width in pixels, 0 to allow any value."),
                            Classes: new[] {"text-small"}),
                        _Height: Shape.Textbox(
                            Id: "height", Name: "Height",
                            Title: T("Height"),
                            Description: T("The height in pixels, 0 to allow any value."),
                            Classes: new[] {"text-small"})
                            );
                    return f;
                };

            context.Form("ResizeFilter", form);
        }
    }
}