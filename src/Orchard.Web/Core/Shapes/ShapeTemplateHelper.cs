using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Core.Shapes {
    public static class ShapeTemplateHelper {
        public static void ForwardTemplateContextToShape(HtmlHelper html, ViewContext context, dynamic shape, string defaultErrorMessage) {
            // Gather what information we can and forward/translate it onto the given Orchard Shape.
            var name = html.FieldNameFor("");
            shape.Value = context.ViewData.TemplateInfo.FormattedModelValue;
            shape.Id = html.FieldIdFor("");  // empty string in a template is special meaning in MVC for 'this model'
            shape.Name = html.FieldNameFor("");

            // forward all viewdata into the shape as shape properties
            foreach (var pair in context.ViewData) {
                shape[pair.Key] = pair.Value;
            }
            // forward all metadata additional values into the shape as shape properties
            var metadata = context.ViewData.ModelMetadata;
            foreach (var pair in metadata.AdditionalValues) {
                shape[pair.Key] = pair.Value;
            }

            var localizer = LocalizationUtilities.Resolve(context, metadata.ModelType.FullName);

            // add wrapper information that comes from natural metadata or needs to be localized
            var displayNameStr = metadata.DisplayName;
            if (!string.IsNullOrEmpty(displayNameStr)) {
                shape.DisplayName = localizer(displayNameStr);
            }

            var descriptionStr = metadata.Description;
            if (!string.IsNullOrEmpty(descriptionStr)) {
                shape.Description = localizer(descriptionStr);
            }

            var actionLinkText = metadata.AdditionalValues.ContainsKey("ActionLinkText") ? metadata.AdditionalValues["ActionLinkText"] as string : null;
            if (!string.IsNullOrEmpty(actionLinkText)) {
                shape.ActionLinkValue = localizer(actionLinkText);
            }

            var action = metadata.AdditionalValues.ContainsKey("ActionLink") ? metadata.AdditionalValues["ActionLink"] : null;
            var actionController = metadata.AdditionalValues.ContainsKey("ActionLinkController") ? metadata.AdditionalValues["ActionLinkController"] : null;
            var actionArea = metadata.AdditionalValues.ContainsKey("ActionLinkArea") ? metadata.AdditionalValues["ActionLinkArea"] : null;
            if ((action ?? actionController ?? actionArea) != null) {
                var rvd = new RouteValueDictionary();
                rvd["Action"] = action;
                if (actionController != null) {
                    rvd["Controller"] = actionController;
                }
                if (actionArea != null) {
                    rvd["Area"] = actionArea;
                }
                shape.ActionLink = rvd;
            }

            // Set IsValid = false if there's a ModelError for this, and get the validation message.
            // Unfortunately, this required some duplication since there's no public way to get
            // the error message the same way MVC itself does it.
            var modelState = context.ViewData.ModelState[name];
            var modelErrors = (modelState == null) ? null : modelState.Errors;
            var modelError = ((modelErrors == null) || (modelErrors.Count == 0)) ? null : modelErrors[0];
            if (modelError != null) {
                shape.IsValid = false;
                // if no error message was given, get it from the model error
                if (shape.ErrorMessage == null) {
                    if (!string.IsNullOrEmpty(modelError.ErrorMessage)) {
                        shape.ErrorMessage = modelError.ErrorMessage;
                    }
                    else {
                        // use a default error message like MVC does
                        var attemptedValue = modelState.Value != null ? modelState.Value.AttemptedValue : null;
                        shape.ErrorMessage = string.Format(CultureInfo.CurrentCulture, defaultErrorMessage, attemptedValue);
                    }
                }
            }
        }

        private static object GetSelectProperty(object obj, string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                return obj.ToString();
            }
            var pi = obj.GetType().GetProperty(propertyName);
            return pi.GetValue(obj, new object[] { });
        }

        public static void ForwardTemplateContextToSelect(HtmlHelper html, ViewContext context, dynamic display, dynamic shape, string defaultErrorMessage) {
            ForwardTemplateContextToShape(html, context, shape, defaultErrorMessage);

            var items = shape.Items as IEnumerable;
            if (items != null && !(items is IEnumerable<SelectListItem>)) {
                // if Items is provided and isn't already a IEnumerable<SelectListItem>,
                // we convert it to one so we can also run the Text value through the localizer.
                var selectItems = new List<SelectListItem>();
                string dataTextField = shape.DataTextField;
                string dataValueField = shape.DataValueField;
                var localizer = LocalizationUtilities.Resolve(context, context.ViewData.ModelMetadata.ModelType.FullName);
                foreach (dynamic item in items) {
                    var selectItem = item as SelectListItem;
                    if (selectItem == null) {
                        selectItem = new SelectListItem();
                        if (item is string) {
                            var itemStr = (string) item;
                            selectItem.Text = localizer(itemStr).Text;
                            selectItem.Selected = (itemStr == shape.Value);
                        }
                        else {
                            selectItem.Text = localizer(Convert.ToString(GetSelectProperty(item, dataTextField))).Text;
                            if (dataValueField != null) {
                                var value = GetSelectProperty(item, dataValueField);
                                selectItem.Value = Convert.ToString(value);
                                selectItem.Selected = (value == shape.Value);
                            }
                        }
                    }
                    selectItems.Add(selectItem);
                }
                shape.Items.Clear();
                shape.Items.AddRange(selectItems);
            }
        }
    }
}