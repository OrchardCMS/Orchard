using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Core.Shapes {
    public static class ShapeTemplateHelper {
        private static string IdentifierToFriendlyCasingAndSpacing(string name) {
            if (string.IsNullOrEmpty(name)) {
                return name;
            }
            var dotIndex = name.LastIndexOf('.');
            if (dotIndex != -1) {
                name = name.Substring(dotIndex + 1);
            }
            // This is a 'usually correct' way of converting a property name into a display name. In cases where
            // it isn't right, the property should just define a name with [Display(Name = "")].
            // It inserts a space between upper characters, except adjacent upper case characters,
            // and it makes all but the first upper character lower case, unless part of adjacent uppers.
            // e.g.
            // FooBar => Foo bar
            // FooBarBazBBQ => Foo bar baz BBQ
            var needSpace = false;
            var inSequence = false;
            var sb = new StringBuilder(name.Length + 3);
            for (var i = 0; i < name.Length; i++) {
                var c = name[i];
                if (char.IsUpper(c)) {
                    inSequence = inSequence || ((i < name.Length - 1) && char.IsUpper(name[i + 1]));
                    if (!inSequence && needSpace) {
                        sb.Append(' ');
                        sb.Append(c.ToString().ToLowerInvariant());
                    }
                    else {
                        sb.Append(c);
                    }
                    needSpace = false;
                }
                else {
                    sb.Append(c);
                    inSequence = false;
                    needSpace = true;
                }
            }
            return sb.ToString();
        }

        public static void ForwardTemplateContextToShape(HtmlHelper html, ViewContext context, dynamic shape, string defaultErrorMessage) {
            // Gather what information we can and forward/translate it onto the given Orchard Shape.
            shape.Value = context.ViewData.TemplateInfo.FormattedModelValue;
            shape.Id = html.FieldIdFor("");  // empty string in a template is special meaning in MVC for 'this model'

            var name = html.FieldNameFor("");
            shape.Attributes["Name"] = name;

            // MVC templated inputs have to opt-into shape rendering so we don't break all EditorFor(), etc, uses.
            shape.EnableWrapper = false;

            var metadata = context.ViewData.ModelMetadata;
            var localizer = LocalizationUtilities.Resolve(context, metadata.ModelType.FullName);

            // forward all metadata additional values into the shape as shape properties
            CopyProperties(html, metadata.AdditionalValues, shape);
            // forward all additional viewdata into the shape as shape properties, these take priority if collision
            CopyProperties(html, context.ViewData, shape);

            // add wrapper information that comes from natural metadata or needs to be localized
            var displayNameStr = metadata.DisplayName ?? IdentifierToFriendlyCasingAndSpacing((string)shape.Name);
            if (!string.IsNullOrEmpty(displayNameStr)) {
                shape.Title = shape.Title == null ? localizer(displayNameStr) : shape.Title;
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

        private static void CopyProperties(HtmlHelper html, IEnumerable<KeyValuePair<string, object>> items, dynamic shape) {
            foreach (var pair in items) {
                if ("DisplayName".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)) {
                    shape.Title = shape.Title == null ? pair.Value : shape.Title;
                }
                else if ("EnabledBy".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)) {
                    var value = Convert.ToString(pair.Value);
                    if (!string.IsNullOrEmpty(value)) {
                        // In the UIOptions attribute, EnabledBy points to another property name on the model.
                        // But the shape expects the HTML id of the controlling element.
                        shape[pair.Key] = GetFieldIdFromParentModel(html, Convert.ToString(pair.Value));
                    }
                }
                else {
                    shape[pair.Key] = pair.Value;
                }
            }
        }

        private static string GetFieldIdFromParentModel(HtmlHelper html, string propertyName) {
            var selfId = html.FieldIdFor("");
            var separatorIndex = selfId.LastIndexOf('_');
            if (separatorIndex == -1) {
                return propertyName;
            }
            var otherId = selfId.Substring(0, separatorIndex) + "_" + propertyName;
            return otherId;
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