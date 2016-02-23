using System;
using System.Web;
using System.Web.Mvc;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Models;
using Orchard.Localization;

namespace Orchard.Layouts.Helpers {
    public static class SnippetHtmlExtensions {
        
        public static SnippetFieldDescriptorBuilder SnippetField(this HtmlHelper htmlHelper, string name, string type = null) {
            var shape = (dynamic) htmlHelper.ViewData.Model;

            return new SnippetFieldDescriptorBuilder(shape)
                .Named(name)
                .WithType(type);
        }

        public class SnippetFieldDescriptorBuilder : IHtmlString {
            private readonly dynamic _shape;

            public SnippetFieldDescriptorBuilder(dynamic shape) {
                _shape = shape;
                Descriptor = new SnippetFieldDescriptor();
            }

            public SnippetFieldDescriptor Descriptor { get; private set; }

            public SnippetFieldDescriptorBuilder Named(string value) {
                Descriptor.Name = value;
                return this;
            }

            public SnippetFieldDescriptorBuilder WithType(string value) {
                Descriptor.Type = value;
                return this;
            }

            public SnippetFieldDescriptorBuilder DisplayedAs(LocalizedString value) {
                Descriptor.DisplayName = value;
                return this;
            }

            public SnippetFieldDescriptorBuilder WithDescription(LocalizedString value) {
                Descriptor.Description = value;
                return this;
            }

            public override string ToString() {
                var registratorCallback = (Action<SnippetFieldDescriptor>)_shape.DescriptorRegistrationCallback;

                if (registratorCallback != null)
                    registratorCallback(Descriptor);

                var element = (Snippet)_shape.Element;

                if(element != null)
                    return element.Data.Get(Descriptor.Name);

                return null;
            }

            public string ToHtmlString() {
                return ToString();
            }
        }
    }
}