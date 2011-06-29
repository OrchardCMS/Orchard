using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Mvc;
using Orchard.UI.Resources;

// ReSharper disable InconsistentNaming

namespace Orchard.Core.Shapes {
    public class EditorShapes : IShapeTableProvider {
        private readonly ITagBuilderFactory _tagBuilderFactory;

        public EditorShapes(ITagBuilderFactory tagBuilderFactory) {
            _tagBuilderFactory = tagBuilderFactory;
        }

        public void Discover(ShapeTableBuilder builder) {
            // hack: This is important when using the Input shape directly, but it doesn't come into play
            // when using a 'master' shape yet.
            builder.Describe("Input").Configure(descriptor => descriptor.Wrappers.Add("InputWrapper"));
            builder.Describe("SelectList").Configure(descriptor => descriptor.Wrappers.Add("InputWrapper"));
        }

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

        private static void WriteLabel(HtmlHelper Html, TextWriter Output, string Id, dynamic DisplayName, string Name, string cssClass) {
            // note that there's an odd behavior where the ?? operator doesn't work along with this dynamic
            var displayName = DisplayName == null ? IdentifierToFriendlyCasingAndSpacing(Name) : DisplayName;
            if (displayName != null) {
                var label = new TagBuilder("label");
                label.MergeAttribute("for", Id);
                if (cssClass != null) {
                    label.AddCssClass(cssClass);
                }
                label.InnerHtml = Html.Encode(displayName);
                Output.WriteLine(label.ToString());
            }
        }

        [Shape]
        public void InputWrapper(
            HtmlHelper Html,
            TextWriter Output,
            bool? EnableWrapper,
            dynamic Display,
            dynamic Shape,
            string Type,
            string Id,
            string Name,
            // note: workaround for bug where having an object parameter fails, causing a nullreference exception during binding
            // so we get these parameters off of Shape.foo instead.
            //object ErrorMessage,
            bool? IsValid, // whether the model is valid
            //object DisplayName,
            //object ActionLink,
            //object ActionLinkValue,
            //object Description,
            string EnabledBy) {

            /* 
             * #id #type #name #title #title_display #field_prefix #field_suffix #description #required
             * #title_display=={before,after,invisible,attribute,none}
             * 
             *  {form_element}
             *      <div attributes id="#id" class="form-type-#type form-item-#name">
             *          {form_element_label}
             *              <label class="option?#title_display==after element-invisible?#title_display==invisible" for="#id">
             *                  #title 
             *                  {form_required_marker}
             *                      <span class='form-required'>This field is required.</span>
             *                  {/form_required_marker}?#required
             *              </label>
             *          {/form_element_label}?#title
             *          <span class="field-prefix">#field_prefix</span>?#field_prefix
             *          #child_content
             *          <span class="field-suffix">#field_suffix</span>?#field_suffix
             *          {form_element_label/}?#title_display==after
             *          <div class="description">#description</div>?
             *      </div>
             *  {/form_element}
             */


            // Wraps an input with metadata as follows (most of it is optional and won't render if not present):
            /*
            <div data-controllerid="{EnabledByInputId}">
                <label for="inputfieldname">{DisplayName}</label>
                {ChildContent}
                {ValidationMessage}
                <span class="hint">{Description}</span>
                <p><a href="{ActionLinkRouteUrl}">{ActionLinkText}</a></p>
            </div>
             * Note: For a checkbox, the label comes after
             * Note: Localizable strings should already be localized by this point.
             * When using this via an html.editor() call, the content has already been localized
             * by the generic MVC template.
             */

            if (!EnableWrapper.GetValueOrDefault()) {
                Output.WriteLine(Shape.Metadata.ChildContent);
                return;
            }

            // surrounding div
            var div = new TagBuilder("div");
            if (!string.IsNullOrEmpty(EnabledBy)) {
                // note: the value should be the html ID of the input.
                // When using this via html.editor() the id is automatically converted from the property name
                // to the field id before it gets to the shape.
                div.MergeAttribute("data-controllerid", EnabledBy);
            }
            Output.WriteLine(div.ToString(TagRenderMode.StartTag));

            if ("checkbox".Equals(Type, StringComparison.OrdinalIgnoreCase)) {
                // <input>
                Output.WriteLine(Shape.Metadata.ChildContent);
                // <label>
                WriteLabel(Html, Output, Id, Shape.DisplayName, Name, "forcheckbox");
            }
            else {
                // <label>
                WriteLabel(Html, Output, Id, Shape.DisplayName, Name, null);
                // <input>
                Output.WriteLine(Shape.Metadata.ChildContent);
            }

            // validation message
            if (!IsValid.GetValueOrDefault(true) && Shape.ErrorMessage != null) {
                var span = new TagBuilder("span");
                span.AddCssClass(HtmlHelper.ValidationMessageCssClassName);
                span.InnerHtml = Html.Encode(Shape.ErrorMessage);
                Output.WriteLine(span.ToString());
            }

            // <span class="hint">
            if (Shape.Description != null) {
                var span = new TagBuilder("span");
                span.AddCssClass("hint");
                var html = Html.Encode(Shape.Description);
                html = html.Replace("\n", "</span><span class=\"hint\">");
                span.InnerHtml = html;
                Output.WriteLine(span.ToString());
            }

            // action link
            if (Shape.ActionLinkValue != null) {
                if (Shape.ActionLink is string) {
                    Shape.ActionLink = new { Action = Shape.ActionLink };
                }
                Output.WriteLine("<p>" + (IHtmlString)Display.Link(RouteValues: Shape.ActionLink, Value: Shape.ActionLinkValue) + "</p>");
            }

            Output.WriteLine(div.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public void Form(Action<object> Output, dynamic Display, dynamic Shape) {
            OrchardTagBuilder tag = _tagBuilderFactory.Create(Shape, "form");
            Output(tag.ToString(TagRenderMode.StartTag));
            foreach(var item in Shape) {
                Output(Display(item));
            }
            Output(tag.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public void Fieldset(Action<object> Output, dynamic Display, dynamic Shape) {
            OrchardTagBuilder tag = _tagBuilderFactory.Create(Shape, "fieldset");
            Output(tag.ToString(TagRenderMode.StartTag));
            foreach(var item in Shape) {
                Output(Display(item));
            }
            Output(tag.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public IHtmlString Textbox(dynamic Display, dynamic Shape) {
            Shape.Metadata.Type = "Input";
            Shape.Type = "textbox";
            return Display(Shape);
        }

        [Shape]
        public IHtmlString Input(HtmlHelper Html, dynamic Shape, dynamic Display, string Type, string Name, dynamic Value) {
            var tag = (TagBuilder)_tagBuilderFactory.Create(Shape, "input");
            tag.MergeAttribute("type", Type, false);
            if (Name != null) {
                tag.MergeAttribute("name", Name, false);
            }
            if (Value != null) {
                Value = Value is string ? Value : Display(Value);
                tag.MergeAttribute("value", Convert.ToString(Value), false);
            }
            return new HtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        private static IHtmlString DisplayShapeAsInput(dynamic Display, dynamic Shape, string inputType) {
            Shape.Metadata.Alternates.Clear();
            Shape.Type = inputType;
            Shape.Metadata.Type = "Input";

            // HACK: This code shouldn't know about this, but cascading shapes doesn't work well with wrappers and other metadata yet
            // todo: alternates, etc?
            Shape.Metadata.Wrappers.Clear();
            Shape.Metadata.Wrappers.Add("InputWrapper");

            var display = Display(Shape);
            // hack: so that InputWrapper doesn't render a 2nd time on the master shape
            // todo: alternates, etc?
            Shape.Metadata.Wrappers.Clear();
            return display;
        }

        [Shape]
        public IHtmlString EditorString(dynamic Display, dynamic Shape) {
            return DisplayShapeAsInput(Display, Shape, "text");
        }

        private static string ListItemToOption(SelectListItem item) {
            var option = new TagBuilder("option");
            option.InnerHtml = HttpUtility.HtmlEncode(item.Text);

            if (item.Value != null) {
                option.Attributes["value"] = item.Value;
            }
            if (item.Selected) {
                option.Attributes["selected"] = "selected";
            }
            return option.ToString(TagRenderMode.Normal);
        }

        private static object GetSelectProperty(object obj, string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                return obj.ToString();
            }
            var pi = obj.GetType().GetProperty(propertyName);
            return pi.GetValue(obj, new object[] { });
        }

        [Shape]
        public void SelectList(
            TextWriter Output,
            dynamic Display,
            dynamic Shape,
            IEnumerable<dynamic> Items,
            string DataTextField,
            string DataValueField
            ) {
            var select = (TagBuilder)_tagBuilderFactory.Create(Shape, "select");
            Output.WriteLine(select.ToString(TagRenderMode.StartTag));

            foreach (var item in Items) {
                var selectItem = item as SelectListItem;
                if (selectItem == null) {
                    selectItem = new SelectListItem();
                    if (item is string) {
                        var itemStr = (string)item;
                        selectItem.Text = itemStr;
                        selectItem.Selected = (itemStr == Convert.ToString(Shape.Value));
                    }
                    else {
                        selectItem.Text = Convert.ToString(GetSelectProperty(item, DataTextField));
                        if (DataValueField != null) {
                            var value = GetSelectProperty(item, DataValueField);
                            selectItem.Value = Convert.ToString(value);
                            selectItem.Selected = (value == Shape.Value);
                        }
                    }
                }
                Output.WriteLine(ListItemToOption(selectItem));
            }

            Output.WriteLine(select.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public IHtmlString EditorBoolean(dynamic Display, dynamic Shape) {
            return DisplayShapeAsInput(Display, Shape, "checkbox");
        }

    }
}
