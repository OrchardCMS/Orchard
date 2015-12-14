using System;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
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
            string mode = context.State.Mode;
            string alignment = context.State.Alignment;
            string padcolor = context.State.PadColor;

            var settings = new ResizeSettings {
                Mode = FitMode.Max,
                Height = height,
                Width = witdh
            };

            switch (mode) {
                case "max": settings.Mode = FitMode.Max; break;
                case "pad": 
                    settings.Mode = FitMode.Pad; 
                    settings.Scale = ScaleMode.Both; 
                    break;
                case "crop": settings.Mode = FitMode.Crop; break;
                case "stretch": settings.Mode = FitMode.Stretch; break;
            }

            switch (alignment) {
                case "topleft": settings.Anchor = ContentAlignment.TopLeft; break;
                case "topcenter": settings.Anchor = ContentAlignment.TopCenter; break;
                case "topright": settings.Anchor = ContentAlignment.TopRight; break;
                case "middleleft": settings.Anchor = ContentAlignment.MiddleLeft; break;
                case "middlecenter": settings.Anchor = ContentAlignment.MiddleCenter; break;
                case "middleright": settings.Anchor = ContentAlignment.MiddleRight; break;
                case "bottomleft": settings.Anchor = ContentAlignment.BottomLeft; break;
                case "bottomcenter": settings.Anchor = ContentAlignment.BottomCenter; break;
                case "bottomright": settings.Anchor = ContentAlignment.BottomRight; break;
            }

            if (!String.IsNullOrWhiteSpace(padcolor)) {
                if (padcolor.StartsWith("#")) {
                    settings.BackgroundColor = ColorTranslator.FromHtml(padcolor);
                }
                else {
                    settings.BackgroundColor = Color.FromName(padcolor);
                }
            }

            var result = new MemoryStream();
            if (context.Media.CanSeek) {
                context.Media.Seek(0, SeekOrigin.Begin);
            }
            ImageBuilder.Current.Build(context.Media, result, settings, true);
            context.Media = result;
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            string mode = context.State.Mode;

            switch (mode) {
                case "pad": return T("Pad to {0}x{1}", context.State.Height, context.State.Width);
                case "crop": return T("Crop to {0}x{1}", context.State.Height, context.State.Width);
                case "stretch": return T("Stretch to {0}x{1}", context.State.Height, context.State.Width);
                default: return T("Resize to {0}x{1}", context.State.Height, context.State.Width); 

            } 
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
                            Value: 0,
                            Description: T("The width in pixels."),
                            Classes: new[] {"text small"}),
                        _Height: Shape.Textbox(
                            Id: "height", Name: "Height",
                            Title: T("Height"),
                            Value: 0,
                            Description: T("The height in pixels."),
                            Classes: new[] {"text small"}),
                        _Mode: Shape.SelectList(
                            Id: "mode", Name: "Mode",
                            Title: T("Mode"),
                            Description: T("How the image should be resized.<br />Max: adjusts to the max given width or height, keeping image ratio.<br />Pad: adds a padding so that the target image is exactly of width and height.<br />Crop: removes part of the image to fit with given height and width.<br />Stretch: stretches the image to fit within height and width."),
                            Size: 1,
                            Multiple: false),
                        _Alignment: Shape.SelectList(
                            Id: "alignment", Name: "Alignment",
                            Title: T("Alignment"),
                            Description: T("Select the alignment for Crop and Pad modes."),
                            Size: 1,
                            Multiple: false),
                        _PadColor: Shape.Textbox(
                            Id: "padcolor", Name: "PadColor",
                            Title: T("Pad Color"),
                            Value: "",
                            Description: T("The background color to use to pad the image e.g., #ffffff, red. Leave empty to keep transparency."),
                            Classes: new[] {"text small"})
                        );

                    f._Mode.Add(new SelectListItem { Value = "max", Text = T("Max").Text });
                    f._Mode.Add(new SelectListItem { Value = "pad", Text = T("Pad").Text });
                    f._Mode.Add(new SelectListItem { Value = "crop", Text = T("Crop").Text });
                    f._Mode.Add(new SelectListItem { Value = "stretch", Text = T("Stretch").Text });

                    f._Alignment.Add(new SelectListItem { Value = "topleft", Text = T("Top Left").Text });
                    f._Alignment.Add(new SelectListItem { Value = "topcenter", Text = T("Top Center").Text });
                    f._Alignment.Add(new SelectListItem { Value = "topright", Text = T("Top Right").Text });
                    f._Alignment.Add(new SelectListItem { Value = "middleleft", Text = T("Middle Left").Text });
                    f._Alignment.Add(new SelectListItem { Value = "middlecenter", Text = T("Middle Center").Text });
                    f._Alignment.Add(new SelectListItem { Value = "middleright", Text = T("Middle Right").Text });
                    f._Alignment.Add(new SelectListItem { Value = "bottomleft", Text = T("Bottom Left").Text });
                    f._Alignment.Add(new SelectListItem { Value = "bottomcenter", Text = T("Bottom Center").Text });
                    f._Alignment.Add(new SelectListItem { Value = "bottomright", Text = T("Bottom Right").Text });

                    return f;
                };

            context.Form("ResizeFilter", form);
        }
    }
}