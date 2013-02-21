using System;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.MediaProcessing.Descriptors.Filter;
using Orchard.MediaProcessing.Services;

namespace Orchard.MediaProcessing.Providers.Filters {
    public class ImageFormatFilter : IImageFilterProvider {
        public ImageFormatFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Transform", T("Transform"), T("Transform"))
                .Element("ImageFormat", T("ImageFormat"), T("Changes the image file format"),
                         ApplyFilter,
                         DisplayFilter,
                         "ImageFormatFilter"
                );
        }

        public void ApplyFilter(FilterContext context) {
            context.ImageFormat = ImageFormatConverter.ToImageFormat((ImageFormats)Enum.Parse(typeof (ImageFormats), (string)context.State.ImageFormat));
            context.FilePath = Path.ChangeExtension(context.FilePath, context.ImageFormat.ToString().ToLower());
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Convert to {0}", context.State.ImageFormat.ToString());
        }
    }

    public class ImageFormatFilterForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ImageFormatFilterForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = Shape.Form(
                        Id: "ImageFormatFilter",
                        _ImageFormat: Shape.SelectList(
                            Id: "imageformat",
                            Name: "ImageFormat"
                            ));

                    foreach (var item in Enum.GetValues(typeof (ImageFormats))) {
                        var name = Enum.GetName(typeof (ImageFormats), item);
                        f._ImageFormat.Add(new SelectListItem {Value = item.ToString(), Text = name});
                    }

                    return f;
                };

            context.Form("ImageFormatFilter", form);
        }
    }

    public enum ImageFormats {
        Bmp,
        Gif,
        Jpeg,
        Png
    }

    public class ImageFormatConverter {
        public static ImageFormat ToImageFormat(ImageFormats format) {
            switch (format) {
                case ImageFormats.Bmp:
                    return ImageFormat.Bmp;
                case ImageFormats.Gif:
                    return ImageFormat.Gif;
                case ImageFormats.Jpeg:
                    return ImageFormat.Jpeg;
                case ImageFormats.Png:
                    return ImageFormat.Png;
            }
            return ImageFormat.Jpeg;
        }
    }
}