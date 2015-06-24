using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IDeliverable.Slides.Services;
using IDeliverable.Tutorials.Slides.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace IDeliverable.Tutorials.Slides.SlidesProviders
{
    public class LinesSlidesProvider :  SlidesProvider
    {
        private const string TextKey = "LinesSlidesProvider.Text";
        public override LocalizedString DisplayName => T("Lines");

        public override dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context)
        {
            return UpdateEditor(shapeFactory, context: context, updater: null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater)
        {
            var viewModel = new LinesSlidesProviderViewModel
            {
                Text = context.Storage.Retrieve<string>(TextKey)
            };

            if (updater != null)
            {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null))
                {
                    context.Storage.Store(TextKey, viewModel.Text?.Trim());
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "SlidesProviders.Lines", Model: viewModel, Prefix: Prefix);
        }

        public override IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context)
        {
            var text = context.Storage.Retrieve<string>(TextKey);
            var lines = ParseLines(text);

            return lines.Select(x => shapeFactory.SlidesProviders_LineSlide(Text: x));
        }

        public override void Exporting(SlidesProviderExportContext context)
        {
            var text = context.Storage.Retrieve<string>(TextKey);
            context.Element.SetElementValue("Text", text);
        }

        public override void Importing(SlidesProviderImportContext context)
        {
            var text = context.Element.El("Text");
            
            context.Storage.Store(TextKey, text);
        }

        private IList<string> ParseLines(string text)
        {
            return !String.IsNullOrWhiteSpace(text) ? Regex.Split(text, "\n", RegexOptions.Multiline) : new string[0];
        }
    }
}