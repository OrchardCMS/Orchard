using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Settings;
using Orchard.Tokens;
using System;
using System.Collections.Generic;

namespace Orchard.Fields.Drivers {
    // The original driver of the TextField is in Orchard.Core, where tokenization can not be used.
    // This driver was added so the default value of the TextField can be tokenized.
    public class TextFieldDriver : ContentFieldDriver<TextField> {
        private readonly ITokenizer _tokenizer;

        public TextFieldDriver(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        protected override DriverResult Editor(ContentPart part, TextField field, IUpdateModel updater, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>();

            if (!String.IsNullOrEmpty(settings.DefaultValue) && (String.IsNullOrEmpty(field.Value) || field.Value.Equals(settings.DefaultValue))) {
                field.Value = _tokenizer.Replace(settings.DefaultValue, new Dictionary<string, object> { { "Content", part.ContentItem } });
            }

            return null;
        }
    }
}
