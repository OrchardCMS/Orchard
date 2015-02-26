using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Helpers;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Tokens;
using Orchard.Utility.Extensions;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class EnumerationElementDriver : FormsElementDriver<Enumeration> {
        private readonly ITokenizer _tokenizer;
        public EnumerationElementDriver(IFormManager formManager, ITokenizer tokenizer)
            : base(formManager) {
            _tokenizer = tokenizer;
        }

        protected override IEnumerable<string> FormNames {
            get {
                yield return "AutoLabel";
                yield return "Enumeration";
            }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("Enumeration", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "Enumeration",
                    _Options: shape.Textarea(
                        Id: "Options",
                        Name: "Options",
                        Title: "Options",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("Enter one option per line. To differentiate between an option's text and value, separate the two by a colon. For example: &quot;Option 1:1&quot;")),
                    _InputType: shape.SelectList(
                        Id: "InputType",
                        Name: "InputType",
                        Title: "Input Type",
                        Description: T("The control to render when presenting the list of options.")));

                form._InputType.Items.Add(new SelectListItem { Text = T("Select List").Text, Value = "SelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Multi Select List").Text, Value = "MultiSelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Radio List").Text, Value = "RadioList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Check List").Text, Value = "CheckList" });

                return form;
            });
        }

        protected override void OnDisplaying(Enumeration element, ElementDisplayContext context) {
            var tokenizedOptions = _tokenizer.Replace(element.Options).ToArray();
            var typeName = element.GetType().Name;
            var displayType = context.DisplayType;

            context.ElementShape.TokenizedOptions = tokenizedOptions;
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}", typeName, element.InputType));
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}__{2}", displayType, typeName, element.InputType));
        }
    }
}