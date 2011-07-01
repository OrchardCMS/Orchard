using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClaySharp;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Mvc;
using Orchard.UI;
using Orchard.UI.Resources;

// ReSharper disable InconsistentNaming

namespace Orchard.Core.Shapes
{
    public class EditorShapes : IShapeTableProvider
    {
        private readonly ITagBuilderFactory _tagBuilderFactory;

        public EditorShapes(ITagBuilderFactory tagBuilderFactory)
        {
            _tagBuilderFactory = tagBuilderFactory;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            // hack: This is important when using the Input shape directly, but it doesn't come into play
            // when using a 'master' shape yet.
            builder.Describe("Input").Configure(descriptor => descriptor.Wrappers.Add("InputWrapper"));
            builder.Describe("SelectList").Configure(descriptor => descriptor.Wrappers.Add("InputWrapper"));
            builder.Describe("Form").OnCreating(ctx => ctx.Behaviors.Add(new PropertiesAreItems()));
            builder.Describe("Fieldset").OnCreating(ctx => ctx.Behaviors.Add(new PropertiesAreItems()));
        }

        class PropertiesAreItems : ClayBehavior
        {
            public override object SetMember(Func<object> proceed, dynamic self, string name, object value)
            {
                Patch(self, name, value);
                return proceed();
            }

            public override object SetIndex(Func<object> proceed, dynamic self, IEnumerable<object> keys, object value)
            {
                if (keys.Count() == 1 && keys.All(k => k is string))
                    Patch(self, System.Convert.ToString(keys.Single()), value);
                return proceed();
            }

            public override object InvokeMember(Func<object> proceed, dynamic self, string name, INamedEnumerable<object> args)
            {
                if (args.Count() == 1 && args.Named.Count() == 0)
                    Patch(self, name, args.Single());
                return proceed();
            }

            readonly IDictionary<string, object> _assigned = new Dictionary<string, object>();
            private void Patch(dynamic self, string name, object value)
            {
                if (!name.StartsWith("_"))
                    return;

                object priorValue;
                if (_assigned.TryGetValue(name, out priorValue) && priorValue != null)
                {
                    // it's a no-op to reassign same value to a prop
                    if (priorValue == value)
                        return;

                    self.Items.Remove(priorValue);
                }
                if (value is IShape)
                {
                    self.Items.Add(value);
                }
                _assigned[name] = value;
            }
        }

        [Shape]
        public void InputLabel(HtmlHelper Html, TextWriter Output, dynamic Shape, string For, object Title)
        {
            if (Title != null)
            {
                var label = _tagBuilderFactory.Create(Shape, "label");
                label.MergeAttribute("for", For);
                label.InnerHtml = Html.Encode(Title);
                Output.WriteLine(label.ToString());
            }
        }

        [Shape]
        public void InputHint(dynamic Display, dynamic Shape, HtmlHelper Html, TextWriter Output, object Description/*, object ActionLinkValue, object ActionLink*/) {
            // <span class="hint">
            if (Description != null) {
                var span = _tagBuilderFactory.Create(Shape, "span");
                span.AddCssClass("hint");
                var html = Html.Encode(Description);
                html = html.Replace("\n", "</span><span class=\"hint\">");
                span.InnerHtml = html;
                Output.WriteLine(span.ToString(TagRenderMode.Normal));
            }
            // action link
            if (Shape.ActionLinkValue != null) {
                if (Shape.ActionLink is string) {
                    Shape.ActionLink = new { Action = Shape.ActionLink };
                }
                Output.WriteLine("<p>" + Display.Link(RouteValues: Shape.ActionLink, Value: Shape.ActionLinkValue) + "</p>");
            }
        }

        [Shape]
        public void InputRequiredMarker(TextWriter Output, HtmlHelper Html, dynamic Shape, object ErrorMessage, bool? IsValid) {
            if (!IsValid.GetValueOrDefault(true) && ErrorMessage != null) {
                var span = _tagBuilderFactory.Create(Shape, "span");
                span.AddCssClass(HtmlHelper.ValidationMessageCssClassName);
                span.InnerHtml = Html.Encode(Shape.ErrorMessage);
                Output.WriteLine(span.ToString());
            }
        }

        [Shape]
        public void InputWrapper(
            HtmlHelper Html,
            TextWriter Output,
            dynamic Display,
            dynamic Shape,
            string Type,
            string Id,
            string Name,
            object Title,
            string TitleDisplay,
            string EnabledBy,
            bool? EnableWrapper)
        {

            /* 
             * #id #type #name #title #title_display #field_prefix #field_suffix #description #required
             * #title_display=={before,after,invisible,attribute,none}
             * 
             *  {form_element} ==InputWrapper
             *      <div attributes id="#id" class="form-type-#type form-item-#name">
             *          {form_element_label} ==InputLabel
             *              <label class="option?#title_display==after element-invisible?#title_display==invisible" for="#id">
             *                  #title 
             *                  {form_required_marker}  ==InputRequiredMarker
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

            if (!EnableWrapper.GetValueOrDefault(true)) {
                Output.WriteLine(Shape.Metadata.ChildContent);
                return;
            }

            // surrounding div
            var div = new TagBuilder("div");
            if (TitleDisplay == "attribute")
            {
                if (Title != null)
                {
                    div.MergeAttribute("title", Display(Title).ToString());
                }
            }

            if (!string.IsNullOrEmpty(EnabledBy))
            {
                // note: the value should be the html ID of the input.
                // When using this via html.editor() the id is automatically converted from the property name
                // to the field id before it gets to the shape.
                div.MergeAttribute("data-controllerid", EnabledBy);
            }
            Output.WriteLine(div.ToString(TagRenderMode.StartTag));

            var isCheckbox = "checkbox".Equals(Type, StringComparison.OrdinalIgnoreCase);

            IHtmlString labelHtml = null;
            if (Title != null)
            {
                labelHtml = Display.InputLabel(For: Id, Title: Title, Input: Shape, Classes: isCheckbox ? new [] { "forcheckbox" } : null);

                if (TitleDisplay == "before" || (string.IsNullOrEmpty(TitleDisplay) && !isCheckbox))
                {
                    Output.Write(labelHtml);
                }
            }
            Output.WriteLine(Shape.Metadata.ChildContent);
            if (Title != null && (TitleDisplay == "after" || (string.IsNullOrEmpty(TitleDisplay) && isCheckbox)))
            {
                Output.Write(labelHtml);
            }

            Output.WriteLine(Display.InputRequiredMarker(Input: Shape));
            Output.WriteLine(Display.InputHint(Input: Shape, Description: Shape.Description, ActionLink: Shape.ActionLink, ActionLinkValue: Shape.ActionLinkValue));

            Output.WriteLine(div.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public void Form(Action<object> Output, dynamic Display, dynamic Shape)
        {   
            // (todo) design markup
            OrchardTagBuilder tag = _tagBuilderFactory.Create(Shape, "form");
            Output(tag.ToString(TagRenderMode.StartTag));
            foreach (var item in Ordered(Shape.Items))
            {
                Output(Display(item));
            }
            Output(tag.ToString(TagRenderMode.EndTag));
        }


        [Shape]
        public void Fieldset(Action<object> Output, dynamic Display, dynamic Shape, object Title)
        {
            // (todo) design markup
            OrchardTagBuilder tag = _tagBuilderFactory.Create(Shape, "fieldset");
            Output(tag.ToString(TagRenderMode.StartTag));
            if (Title != null)
            {
                Output("<legend>");
                Output(Display(Title));
                Output("</legend>");
            }
            foreach (var item in Ordered(Shape.Items))
            {
                Output(Display(item));
            }
            Output(tag.ToString(TagRenderMode.EndTag));
        }

        private IEnumerable<dynamic> Ordered(IEnumerable<dynamic> items) {
            return items.OfType<IShape>().OrderBy(item => item.Metadata.Position, new FlatPositionComparer());
        }

        [Shape]
        public IHtmlString Textbox(dynamic Display, dynamic Shape)
        {
            return DisplayShapeAsInput(Display, Shape, "textbox");
        }

        [Shape]
        public IHtmlString Submit(dynamic Display, dynamic Shape)
        {
            // (todo) this might not need full wrapper in hindsight?
            return DisplayShapeAsInput(Display, Shape, "submit");
        }

        [Shape]
        public IHtmlString Button(dynamic Display, dynamic Shape)
        {
            // (todo) this might not need full wrapper in hindsight?
            return DisplayShapeAsInput(Display, Shape, "button");
        }

        [Shape]
        public IHtmlString Input(HtmlHelper Html, dynamic Shape, dynamic Display, string Type, string Name, dynamic Value)
        {
            var tag = (TagBuilder)_tagBuilderFactory.Create(Shape, "input");
            tag.MergeAttribute("type", Type, false);
            if (Name != null)
            {
                tag.MergeAttribute("name", Name, false);
            }
            if (Value != null)
            {
                Value = Value is string ? Value : Display(Value);
                tag.MergeAttribute("value", Convert.ToString(Value), false);
            }
            return new HtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        private static IHtmlString DisplayShapeAsInput(dynamic Display, dynamic Shape, string inputType)
        {
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
        public IHtmlString EditorString(dynamic Display, dynamic Shape)
        {
            return DisplayShapeAsInput(Display, Shape, "text");
        }

        private static string ListItemToOption(SelectListItem item)
        {
            var option = new TagBuilder("option");
            option.InnerHtml = HttpUtility.HtmlEncode(item.Text);

            if (item.Value != null)
            {
                option.Attributes["value"] = item.Value;
            }
            if (item.Selected)
            {
                option.Attributes["selected"] = "selected";
            }
            return option.ToString(TagRenderMode.Normal);
        }

        private static object GetSelectProperty(object obj, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
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
            )
        {
            var select = (TagBuilder)_tagBuilderFactory.Create(Shape, "select");
            Output.WriteLine(select.ToString(TagRenderMode.StartTag));

            foreach (var item in Items)
            {
                var selectItem = item as SelectListItem;
                if (selectItem == null)
                {
                    selectItem = new SelectListItem();
                    if (item is string)
                    {
                        var itemStr = (string)item;
                        selectItem.Text = itemStr;
                        selectItem.Selected = (itemStr == Convert.ToString(Shape.Value));
                    }
                    else
                    {
                        selectItem.Text = Convert.ToString(GetSelectProperty(item, DataTextField));
                        if (DataValueField != null)
                        {
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
        public IHtmlString EditorBoolean(dynamic Display, dynamic Shape)
        {
            return DisplayShapeAsInput(Display, Shape, "checkbox");
        }

    }
}
