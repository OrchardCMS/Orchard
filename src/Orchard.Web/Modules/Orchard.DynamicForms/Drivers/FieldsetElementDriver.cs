using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Drivers {
    public class FieldsetElementDriver : ElementDriver<Fieldset> {
        private readonly ITokenizer _tokenizer;

        public FieldsetElementDriver(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        protected override void OnDisplaying(Fieldset element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedLegend = _tokenizer.Replace(element.Legend, context.GetTokenData(), new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
        }
    }
}