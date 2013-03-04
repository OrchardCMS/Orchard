using System;
using System.Web.Mvc;
using ImageResizer;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.MediaProcessing.Descriptors.Filter;
using Orchard.MediaProcessing.Services;

namespace Orchard.MediaProcessing.Providers.Filters {
    public class ConstrainFilter : IImageFilterProvider {
        public ConstrainFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Transform", T("Transform"), T("Transform"))
                .Element("Constrain", T("Constrain"), T("Constrains the dimensions of an image either by with of by height"),
                         ApplyFilter,
                         DisplayFilter,
                         "ContrainImageFilter"
                );
        }

        public void ApplyFilter(FilterContext context) {
            var value = (int)context.State.Value;
            var axis = (string)context.State.Axis;



            var settings = new ResizeSettings {
                Mode = FitMode.Max
            };

            switch (axis) {
                case "width":
                    settings.Width = value;
                    break;
                case "height":
                    settings.Height = value;
                    break;
            }
            

            context.Media = result;
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            var value = (int)context.State.Value;
            var axis = (string)context.State.Axis;

            return axis == "height"
                       ? T("Constrain to {0}px high", value)
                       : T("Constrain to {0}px wide", value);
        }
    }

    public class ConstrainFilterFilterForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ConstrainFilterFilterForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = Shape.Form(
                        Id: "ContrainImageFilter",
                        _Axis: Shape.SelectList(
                            Id: "axis", Name: "Axis",
                            Title: T("Axis"),
                            Size: 1,
                            Multiple: false
                        ),
                        _Height: Shape.Textbox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Description: T("The value in pixel the selected axis should be constrained to. Mandatory."),
                            Classes: new[] {"text-small"})
                        );

                    f._Axis.Add(new SelectListItem { Value = "height", Text = T("Height").Text });
                    f._Axis.Add(new SelectListItem { Value = "width", Text = T("Width").Text });
                    
                    return f;
                };

            context.Form("ContrainImageFilter", form);
        }
    }
}