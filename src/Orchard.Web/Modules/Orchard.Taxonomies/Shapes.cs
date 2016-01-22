using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Taxonomies.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility.Extensions;

namespace Orchard.Taxonomies {
    public class Shapes : IShapeTableProvider {

        public void Discover(ShapeTableBuilder builder) {

            builder.Describe("Taxonomy")
                .OnDisplaying(displaying => {
                    var shape = displaying.Shape;
                    var metadata = displaying.ShapeMetadata;
                    TaxonomyPart taxonomy = shape.ContentPart;
                    shape.Classes.Add("taxonomy-" + taxonomy.Slug.HtmlClassify());
                    shape.Classes.Add("taxonomy");
                    metadata.Alternates.Add("Taxonomy__" + FormatAlternate(taxonomy.Slug));
                });

            builder.Describe("TaxonomyItem")
                .OnDisplaying(displaying => {
                    var shape = displaying.Shape;
                    var metadata = displaying.ShapeMetadata;
                    IContent content = shape.Taxonomy;
                    var taxonomy = content.As<TaxonomyPart>();
                    metadata.Alternates.Add("TaxonomyItem__" + FormatAlternate(taxonomy.Slug));

                    TermPart term = shape.ContentPart;
                    foreach (var alternate in GetHierarchyAlternates(term).Reverse()) {
                        metadata.Alternates.Add("TaxonomyItem__" + FormatAlternate(alternate));
                    }
                });

            builder.Describe("TaxonomyItemLink")
                .OnDisplaying(displaying => {
                    var shape = displaying.Shape;
                    var metadata = displaying.ShapeMetadata;
                    IContent content = shape.Taxonomy;
                    var taxonomy = content.As<TaxonomyPart>();
                    metadata.Alternates.Add("TaxonomyItemLink__" + FormatAlternate(taxonomy.Slug));

                    TermPart term = shape.ContentPart;
                    foreach (var alternate in GetHierarchyAlternates(term).Reverse()) {
                        metadata.Alternates.Add("TaxonomyItemLink__" + FormatAlternate(alternate));
                    }
                });

            builder.Describe("Content")
                .OnDisplaying(displaying => {

                    // add specific alternates for customizing a Content item when
                    // it is associated to a term or taxonomy

                    var shape = displaying.Shape;
                    var metadata = displaying.ShapeMetadata;

                    // use TermsPart to detect if the content item has a TermPart attached
                    // in conjunction with its field name
                    ContentItem contentItem = shape.ContentItem;
                    var termsPart = contentItem.As<TermsPart>();

                    if (termsPart == null) {
                        return;
                    }

                    var taxonomy = displaying.Shape.Taxonomy as TaxonomyPart;

                    // Content__[ContentType]__[Field]__[Slug]
                    // Content-Image-MainColor-Blue.cshtml
                    // Content-Image-MainColor-Blue-Light-Blue.cshtml

                    // Content_[DisplayType]__[ContentType]__[Field]__[Slug]
                    // Content-Image-MainColor-Blue.Summary.cshtml
                    // Content-Image-MainColor-Blue-Light-Blue.Summary.cshtml

                    foreach (var termContentItem in termsPart.TermParts) {
                        var field = termContentItem.Field;
                        var termPart = termContentItem.TermPart;

                        foreach (var parent in GetHierarchyAlternates(termPart).Reverse()) {
                            var formatted = FormatAlternate(parent);
                            
                            metadata.Alternates.Add(String.Concat("Content__", contentItem.ContentType, "__", field, "__", formatted));
                            metadata.Alternates.Add(String.Concat("Content_", metadata.DisplayType, "__", contentItem.ContentType, "__", field, "__", formatted));
                        }
                    }
                });

        }

        public IEnumerable<string> GetHierarchyAlternates(TermPart part) {
            var parent = part;

            do {
                yield return parent.Slug;
            } while (null != (parent = parent.Container.As<TermPart>()));
        }

        public string FormatAlternate(string path) {
            return path.Replace("-", "__").Replace("/", "__");
        }
    }
}