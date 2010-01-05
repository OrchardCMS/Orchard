using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentManagement.Drivers {
    public class ItemTemplateResult<TContent> : DriverResult where TContent : class, IContent {
        public ItemTemplateResult(string templateName) {
            TemplateName = templateName;
        }

        public string TemplateName { get; set; }

        public override void Apply(BuildDisplayModelContext context) {
            context.ViewModel.TemplateName = TemplateName;
            if (context.ViewModel.GetType() != typeof(ItemViewModel<TContent>)) {
                context.ViewModel.Adaptor = (html, viewModel) => {
                    return new HtmlHelper<ItemViewModel<TContent>>(
                        html.ViewContext,
                        new ViewDataContainer { ViewData = new ViewDataDictionary(new ItemViewModel<TContent>(viewModel)) },
                        html.RouteCollection);
                };
            }
        }

        public override void Apply(BuildEditorModelContext context) {
            context.ViewModel.TemplateName = TemplateName;
            if (context.ViewModel.GetType() != typeof(ItemViewModel<TContent>)) {
                context.ViewModel.Adaptor = (html, viewModel) => {
                    return new HtmlHelper<ItemViewModel<TContent>>(
                        html.ViewContext,
                        new ViewDataContainer { ViewData = new ViewDataDictionary(new ItemViewModel<TContent>(viewModel)) },
                        html.RouteCollection);
                };
            }
        }


        class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }
        }

        public ItemTemplateResult<TContent> LongestMatch(string displayType, params string[] knownDisplayTypes) {

            if (string.IsNullOrEmpty(displayType))
                return this;

            var longest = knownDisplayTypes.Aggregate("", (best, x) => {
                if (displayType.StartsWith(x) && x.Length > best.Length) return x;
                return best;
            });

            if (string.IsNullOrEmpty(longest))
                return this;

            TemplateName += "." + longest;
            return this;
        }
    }
}