using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Forms.Services;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;

namespace Orchard.Projections.Drivers {

    public class QueryPartDriver : ContentPartDriver<QueryPart> {
        private readonly IProjectionManager _projectionManager;
        private readonly IFormManager _formManager;

        public QueryPartDriver(IProjectionManager projectionManager, IFormManager formManager) {
            _projectionManager = projectionManager;
            _formManager = formManager;
        }
        protected override string Prefix {
            get {
                return "Query_Part";
            }
        }
        protected override DriverResult Editor(QueryPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(QueryPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new QueryViewModel { VersionScope = part.VersionScope };
            if (updater != null) {
                if (updater.TryUpdateModel(model, Prefix, null, null)) {
                    part.VersionScope = model.VersionScope;
                }
            }
            return ContentShape("Parts_QueryPart_Edit",
                                () => {
                                    return shapeHelper.EditorTemplate(TemplateName: "Parts/QueryPart_Edit", Model: model, Prefix: Prefix);
                                });
        }

        protected override void Exporting(QueryPart part, ExportContentContext context) {

            var element = context.Element(part.PartDefinition.Name);

            element.SetAttributeValue("VersionScope", part.VersionScope);

            element.Add(
                new XElement("FilterGroups",
                    part.FilterGroups.Select(filterGroup =>
                        new XElement("FilterGroup",
                            filterGroup.Filters.Select(filter => {

                                var descriptor = _projectionManager.GetFilter(filter.Category, filter.Type);

                                var state = filter.State;
                                if (descriptor != null) {
                                    state = _formManager.Export(descriptor.Form, filter.State, context);
                                }

                                return new XElement("Filter",
                                             new XAttribute("Category", filter.Category ?? ""),
                                             new XAttribute("Description", filter.Description ?? ""),
                                             new XAttribute("Position", filter.Position),
                                             new XAttribute("State", state ?? ""),
                                             new XAttribute("Type", filter.Type ?? "")
                                    );
                            })
                        )
                    )
                ),
                new XElement("SortCriteria",
                    part.SortCriteria.Select(sortCriterion => {
                        var descriptor = _projectionManager.GetFilter(sortCriterion.Category, sortCriterion.Type);

                        var state = sortCriterion.State;
                        if (descriptor != null) {
                            state = _formManager.Export(descriptor.Form, sortCriterion.State, context);
                        }

                        return new XElement("SortCriterion",
                            new XAttribute("Category", sortCriterion.Category ?? ""),
                            new XAttribute("Description", sortCriterion.Description ?? ""),
                            new XAttribute("Position", sortCriterion.Position),
                            new XAttribute("State", state ?? ""),
                            new XAttribute("Type", sortCriterion.Type ?? "")
                        );
                    })
                ),
                new XElement("Layouts",
                    part.Layouts.Select(layout => {
                        var descriptor = _projectionManager.GetFilter(layout.Category, layout.Type);

                        var state = layout.State;
                        if (descriptor != null) {
                            state = _formManager.Export(descriptor.Form, layout.State, context);
                        }

                        return new XElement("Layout",
                            // Attributes
                            new XAttribute("Category", layout.Category ?? ""),
                            new XAttribute("Description", layout.Description ?? ""),
                            new XAttribute("State", state ?? ""),
                            new XAttribute("Display", layout.Display),
                            new XAttribute("DisplayType", layout.DisplayType ?? ""),
                            new XAttribute("Type", layout.Type ?? ""),

                            // Properties
                            new XElement("Properties", layout.Properties.Select(GetPropertyXml)),

                            // Group
                            new XElement("Group", GetPropertyXml(layout.GroupProperty))
                        );
                    })
                )
            );
        }

        protected override void Importing(QueryPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "VersionScope", scope => part.VersionScope = (QueryVersionScopeOptions)Enum.Parse(typeof(QueryVersionScopeOptions), scope));

            var queryElement = context.Data.Element(part.PartDefinition.Name);

            part.Record.FilterGroups.Clear();
            foreach (var item in queryElement.Element("FilterGroups").Elements("FilterGroup").Select(filterGroup =>
                new FilterGroupRecord {
                    Filters = filterGroup.Elements("Filter").Select(filter => {

                        var category = filter.Attribute("Category").Value;
                        var type = filter.Attribute("Type").Value;
                        var state = filter.Attribute("State").Value;

                        var descriptor = _projectionManager.GetFilter(category, type);
                        if (descriptor != null) {
                            state = _formManager.Import(descriptor.Form, state, context);
                        }

                        return new FilterRecord {
                            Category = category,
                            Description = filter.Attribute("Description").Value,
                            Position = Convert.ToInt32(filter.Attribute("Position").Value),
                            State = state,
                            Type = type
                        };
                    }).ToList()
                })) {
                part.Record.FilterGroups.Add(item);
            }

            part.Record.SortCriteria.Clear();
            foreach (var item in queryElement.Element("SortCriteria").Elements("SortCriterion").Select(sortCriterion => {
                var category = sortCriterion.Attribute("Category").Value;
                var type = sortCriterion.Attribute("Type").Value;
                var state = sortCriterion.Attribute("State").Value;

                var descriptor = _projectionManager.GetFilter(category, type);
                if (descriptor != null) {
                    state = _formManager.Import(descriptor.Form, state, context);
                }

                return new SortCriterionRecord {
                    Category = category,
                    Description = sortCriterion.Attribute("Description").Value,
                    Position = Convert.ToInt32(sortCriterion.Attribute("Position").Value),
                    State = state,
                    Type = type

                };
            })) {
                part.Record.SortCriteria.Add(item);
            }

            part.Record.Layouts.Clear();
            foreach (var item in queryElement.Element("Layouts").Elements("Layout").Select(layout => {
                var category = layout.Attribute("Category").Value;
                var type = layout.Attribute("Type").Value;
                var state = layout.Attribute("State").Value;

                var descriptor = _projectionManager.GetFilter(category, type);
                if (descriptor != null) {
                    state = _formManager.Import(descriptor.Form, state, context);
                }

                return new LayoutRecord {
                    Category = category,
                    Description = layout.Attribute("Description").Value,
                    Display = int.Parse(layout.Attribute("Display").Value),
                    DisplayType = layout.Attribute("DisplayType").Value,
                    State = state,
                    Type = type,
                    Properties = layout.Element("Properties").Elements("Property").Select(GetProperty).ToList(),
                    GroupProperty = GetProperty(layout.Element("Group").Element("Property"))
                };
            })) {
                part.Record.Layouts.Add(item);
            }
        }

        private XElement GetPropertyXml(PropertyRecord property) {
            if (property == null) {
                return null;
            }

            return new XElement("Property",
                new XAttribute("Category", property.Category ?? ""),
                new XAttribute("Description", property.Description ?? ""),
                new XAttribute("Position", property.Position),
                new XAttribute("State", property.State ?? ""),
                new XAttribute("Type", property.Type ?? ""),

                new XAttribute("AddEllipsis", property.AddEllipsis),
                new XAttribute("CreateLabel", property.CreateLabel),
                new XAttribute("CustomLabelCss", property.CustomLabelCss ?? ""),
                new XAttribute("CustomLabelTag", property.CustomLabelTag ?? ""),
                new XAttribute("CustomPropertyCss", property.CustomPropertyCss ?? ""),
                new XAttribute("CustomPropertyTag", property.CustomPropertyTag ?? ""),
                new XAttribute("CustomWrapperCss", property.CustomWrapperCss ?? ""),
                new XAttribute("CustomWrapperTag", property.CustomWrapperTag ?? ""),
                new XAttribute("CustomizeLabelHtml", property.CustomizeLabelHtml),
                new XAttribute("CustomizePropertyHtml", property.CustomizePropertyHtml),
                new XAttribute("CustomizeWrapperHtml", property.CustomizeWrapperHtml),
                new XAttribute("ExcludeFromDisplay", property.ExcludeFromDisplay),
                new XAttribute("HideEmpty", property.HideEmpty),
                new XAttribute("Label", property.Label ?? ""),
                new XAttribute("LinkToContent", property.LinkToContent),
                new XAttribute("MaxLength", property.MaxLength),
                new XAttribute("NoResultText", property.NoResultText ?? ""),
                new XAttribute("PreserveLines", property.PreserveLines),
                new XAttribute("RewriteOutput", property.RewriteOutput),
                new XAttribute("RewriteText", property.RewriteText ?? ""),
                new XAttribute("StripHtmlTags", property.StripHtmlTags),
                new XAttribute("TrimLength", property.TrimLength),
                new XAttribute("TrimOnWordBoundary", property.TrimOnWordBoundary),
                new XAttribute("TrimWhiteSpace", property.TrimWhiteSpace),
                new XAttribute("ZeroIsEmpty", property.ZeroIsEmpty)
            );
        }

        private PropertyRecord GetProperty(XElement property) {
            if (property == null) {
                return null;
            }

            return new PropertyRecord {
                AddEllipsis = Convert.ToBoolean(property.Attribute("AddEllipsis").Value),
                Category = property.Attribute("Category").Value,
                CreateLabel = Convert.ToBoolean(property.Attribute("CreateLabel").Value),
                CustomLabelCss = property.Attribute("CustomLabelCss").Value,
                Description = property.Attribute("Description").Value,
                Type = property.Attribute("Type").Value,
                CustomLabelTag = property.Attribute("CustomLabelTag").Value,
                CustomPropertyCss = property.Attribute("CustomPropertyCss").Value,
                CustomPropertyTag = property.Attribute("CustomPropertyTag").Value,
                CustomWrapperCss = property.Attribute("CustomWrapperCss").Value,
                CustomWrapperTag = property.Attribute("CustomWrapperTag").Value,
                CustomizeLabelHtml = Convert.ToBoolean(property.Attribute("CustomizeLabelHtml").Value),
                CustomizePropertyHtml = Convert.ToBoolean(property.Attribute("CustomizePropertyHtml").Value),
                CustomizeWrapperHtml = Convert.ToBoolean(property.Attribute("CustomizeWrapperHtml").Value),
                ExcludeFromDisplay = Convert.ToBoolean(property.Attribute("ExcludeFromDisplay").Value),
                HideEmpty = Convert.ToBoolean(property.Attribute("HideEmpty").Value),
                Label = property.Attribute("Label").Value,
                LinkToContent = Convert.ToBoolean(property.Attribute("LinkToContent").Value),
                MaxLength = Convert.ToInt32(property.Attribute("MaxLength").Value),
                NoResultText = property.Attribute("NoResultText").Value,
                Position = Convert.ToInt32(property.Attribute("Position").Value),
                PreserveLines = Convert.ToBoolean(property.Attribute("PreserveLines").Value),
                RewriteOutput = Convert.ToBoolean(property.Attribute("RewriteOutput").Value),
                RewriteText = property.Attribute("RewriteText").Value,
                State = property.Attribute("State").Value,
                StripHtmlTags = Convert.ToBoolean(property.Attribute("StripHtmlTags").Value),
                TrimLength = Convert.ToBoolean(property.Attribute("TrimLength").Value),
                TrimOnWordBoundary = Convert.ToBoolean(property.Attribute("TrimOnWordBoundary").Value),
                TrimWhiteSpace = Convert.ToBoolean(property.Attribute("TrimWhiteSpace").Value),
                ZeroIsEmpty = Convert.ToBoolean(property.Attribute("ZeroIsEmpty").Value),
            };
        }
    }
}