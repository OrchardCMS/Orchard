using System.Collections.Generic;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.DisplayManagement.Descriptors;

namespace Contrib.Taxonomies {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Content")
                .OnDisplaying(displaying => {
                    var term = displaying.Shape.Term as TermPart;
                    var taxonomy = displaying.Shape.Taxonomy as TaxonomyPart;

                    if (taxonomy == null) {
                        return;
                    }

                    if (term != null) {

                        // Taxonomy.Content.Categories.cshtml
                        // Taxonomy.Content.BlogPost-Categories.cshtml
                        // Taxonomy.Content.Categories-World.cshtml
                        // Taxonomy.Content.BlogPost-Categories-World.cshtml
                        // Taxonomy.Content.Categories-World-France.cshtml
                        // Taxonomy.Content.BlogPost-Categories-World-France.cshtml
                        // Taxonomy.Content.23.cshtml
                        // Taxonomy.Content.BlogPost-23.cshtml

                        var metadata = displaying.ShapeMetadata;
                        ContentItem contentItem = displaying.Shape.ContentItem;

                        metadata.Alternates.Add("Taxonomy_Content_" + FormatAlternate(taxonomy.Slug));
                        metadata.Alternates.Add("Taxonomy_Content_" + contentItem.ContentType + "__" + FormatAlternate(taxonomy.Slug));

                        var curTerm = term;
                        var alternates = new List<string>();
                        while (curTerm != null) {
                            var alternate = FormatAlternate(term.Slug);
                            alternates.Add("Taxonomy_Content_" + alternate);
                            alternates.Add("Taxonomy_Content_" + contentItem.ContentType + "__" + alternate);
                            curTerm = curTerm.Container.As<TermPart>();
                        }

                        alternates.Reverse();
                        foreach (var alternate in alternates) {
                            metadata.Alternates.Add(alternate);
                        }

                        metadata.Alternates.Add("Taxonomy_Content_" + term.Id);
                        metadata.Alternates.Add("Taxonomy_Content_" + contentItem.ContentType + "__" + term.Id);
                    }
                });

            builder.Describe("Taxonomies_TermContentItems_List")
                .OnDisplaying(displaying => {

                    // Taxonomy.TermContentItems.List.Categories.cshtml
                    // Taxonomy.TermContentItems.List.Categories-World.cshtml
                    // Taxonomy.TermContentItems.List.Categories-World-France.cshtml
                    // Taxonomy.TermContentItems.List.12.cshtml

                    var shape = displaying.Shape;
                    var metadata = displaying.ShapeMetadata;
                    var term = shape.Term as TermPart;
                    var taxonomy = shape.Taxonomy as TaxonomyPart;

                    if (taxonomy == null || term == null){
                        return;
                    }

                    metadata.Alternates.Add("Taxonomies_TermContentItems_List_" + FormatAlternate(taxonomy.Slug));

                    var curTerm = term;
                    var alternates = new List<string>();
                    while (curTerm != null) {
                        alternates.Add("Taxonomies_TermContentItems_List_" + FormatAlternate(term.Slug));
                        curTerm = curTerm.Container.As<TermPart>();
                    }

                    alternates.Reverse();
                    foreach(var alternate in alternates) {
                        metadata.Alternates.Add(alternate);
                    }
                    
                    metadata.Alternates.Add("Taxonomy_List_" + term.Id);
                });

            builder.Describe("Pager")
                .OnDisplaying(displaying => {

                    // Taxonomy.Pager.Categories.cshtml
                    // Taxonomy.Pager.Categories-World.cshtml
                    // Taxonomy.Pager.Categories-World-France.cshtml
                    // Taxonomy.Pager.12.cshtml

                    var shape = displaying.Shape;
                    var metadata = displaying.ShapeMetadata;
                    var term = shape.Term as TermPart;
                    var taxonomy = shape.Taxonomy as TaxonomyPart;

                    if(taxonomy == null || term == null) {
                        return;
                    }

                    metadata.Alternates.Add("Taxonomy_Pager_" + FormatAlternate(taxonomy.Slug));

                    var curTerm = term;
                    var alternates = new List<string>();
                    while (curTerm != null) {
                        alternates.Add("Taxonomy_Pager_" + FormatAlternate(term.Slug));
                        curTerm = curTerm.Container.As<TermPart>();
                    }

                    alternates.Reverse();
                    foreach (var alternate in alternates) {
                        metadata.Alternates.Add(alternate);
                    }

                    metadata.Alternates.Add("Taxonomy_Pager_" + term.Id);
                });

        }

        public string FormatAlternate(string path) {
            return path.Replace("-", "__").Replace("/", "__");
        }
    }
}