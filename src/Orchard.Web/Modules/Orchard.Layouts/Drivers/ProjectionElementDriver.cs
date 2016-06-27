using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;
using Orchard.Tokens;
using Orchard.UI.Navigation;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.Layouts.Drivers {
    [OrchardFeature("Orchard.Layouts.Projections")]
    public class ProjectionElementDriver : FormsElementDriver<Projection> {
        private readonly IProjectionManager _projectionManager;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _services;
        private readonly IRepository<LayoutRecord> _layoutRepository;
        private readonly ITokenizer _tokenizer;
        private readonly IDisplayHelperFactory _displayHelperFactory;

        public ProjectionElementDriver(
            IFormsBasedElementServices formsServices,
            IProjectionManager projectionManager,
            IOrchardServices services,
            IRepository<LayoutRecord> layoutRepository,
            ITokenizer tokenizer,
            IDisplayHelperFactory displayHelperFactory)
            : base(formsServices) {

            _projectionManager = projectionManager;
            _contentManager = services.ContentManager;
            _services = services;
            _layoutRepository = layoutRepository;
            _tokenizer = tokenizer;
            _displayHelperFactory = displayHelperFactory;
        }

        protected override IEnumerable<string> FormNames {
            get {
                yield return "ProjectionForm";
            }
        }

        protected override void OnDisplaying(Projection element, ElementDisplayingContext context) {
            var queryId = element.QueryId;
            var layoutId = element.LayoutId;
            var query = queryId != null ? _contentManager.Get<QueryPart>(queryId.Value) : default(QueryPart);
            var emptyContentItemsList = Enumerable.Empty<ContentManagement.ContentItem>();

            context.ElementShape.ContentItems = emptyContentItemsList;
            context.ElementShape.BuildShapes = (Func<string, IEnumerable<dynamic>>)(displayType => emptyContentItemsList.Select(x => _contentManager.BuildDisplay(x, displayType)));

            if (query == null || layoutId == null) {
                return;
            }

            // Retrieving paging parameters.
            var queryString = _services.WorkContext.HttpContext.Request.QueryString;
            var pageKey = String.IsNullOrWhiteSpace(element.PagerSuffix) ? "page" : "page-" + element.PagerSuffix;
            var page = 0;

            // Default page size.
            var pageSize = element.ItemsToDisplay;

            // Don't try to page if not necessary.
            if (element.DisplayPager && queryString.AllKeys.Contains(pageKey)) {
                Int32.TryParse(queryString[pageKey], out page);
            }

            // If 0, then assume "All", limit to 128 by default.
            if (pageSize == 128) {
                pageSize = Int32.MaxValue;
            }

            // If pageSize is provided on the query string, ensure it is compatible within allowed limits.
            var pageSizeKey = "pageSize" + element.PagerSuffix;

            if (queryString.AllKeys.Contains(pageSizeKey)) {
                int qsPageSize;

                if (Int32.TryParse(queryString[pageSizeKey], out qsPageSize)) {
                    if (element.MaxItems == 0 || qsPageSize <= element.MaxItems) {
                        pageSize = qsPageSize;
                    }
                }
            }

            var pager = new Pager(_services.WorkContext.CurrentSite, page, pageSize);

            // TODO: Investigate this further and see if it makes sense to implement for a Projection Element.
            //// Generates a link to the RSS feed for this term.
            //var metaData = _services.ContentManager.GetItemMetadata(part.ContentItem);
            //_feedManager.Register(metaData.DisplayText, "rss", new RouteValueDictionary { { "projection", part.Id } });

            // Execute the query.
            var contentItems = _projectionManager.GetContentItems(query.Id, pager.GetStartIndex() + element.Skip, pager.PageSize).ToList();

            context.ElementShape.ContentItems = contentItems;
            context.ElementShape.BuildShapes = (Func<string, IEnumerable<dynamic>>)(displayType => contentItems.Select(x => _contentManager.BuildDisplay(x, displayType)));

            // TODO: Figure out if we need this for a Projection Element, and if so, how.
            //// Sanity check so that content items with ProjectionPart can't be added here, or it will result in an infinite loop.
            //contentItems = contentItems.Where(x => !x.Has<ProjectionPart>()).ToList();

            // Applying layout.
            var layout = layoutId != null ? _layoutRepository.Get(layoutId.Value) : default(LayoutRecord);
            var layoutDescriptor = layout == null ? null : _projectionManager.DescribeLayouts().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == layout.Category && x.Type == layout.Type);

            // Create pager shape.
            if (element.DisplayPager) {
                var contentItemsCount = Math.Max(0, _projectionManager.GetCount(query.Id) - element.Skip);
                var pagerShape = _services.New.Pager(pager)
                    .Element(element)
                    .PagerId(pageKey)
                    .TotalItemCount(contentItemsCount);

                context.ElementShape.Pager = pagerShape;
            }

            // Renders in a standard List shape if no specific layout could be found.
            if (layoutDescriptor == null) {
                var contentShapes = contentItems.Select(item => _contentManager.BuildDisplay(item, "Summary"));

                var list = context.ElementShape.List = _services.New.List();
                list.AddRange(contentShapes);

                return;
            }

            var allFielDescriptors = _projectionManager.DescribeProperties().ToList();
            var fieldDescriptors = layout.Properties.OrderBy(p => p.Position).Select(p => allFielDescriptors.SelectMany(x => x.Descriptors).Select(d => new { Descriptor = d, Property = p }).FirstOrDefault(x => x.Descriptor.Category == p.Category && x.Descriptor.Type == p.Type)).ToList();

            var layoutComponents = contentItems.Select(contentItem => {
                var contentItemMetadata = _contentManager.GetItemMetadata(contentItem);
                var propertyDescriptors = fieldDescriptors.Select(d => {
                    var fieldContext = new PropertyContext {
                        State = FormParametersHelper.ToDynamic(d.Property.State),
                        Tokens = new Dictionary<string, object> { { "Content", contentItem } }
                    };

                    return new { d.Property, Shape = d.Descriptor.Property(fieldContext, contentItem) };
                });

                // Apply all settings to the field content, wrapping it in a FieldWrapper shape.
                var properties = _services.New.Properties(
                    Items: propertyDescriptors.Select(
                        pd => _services.New.PropertyWrapper(
                            Item: pd.Shape,
                            Property: pd.Property,
                            ContentItem: contentItem,
                            ContentItemMetadata: contentItemMetadata
                        )
                    )
                );

                return new LayoutComponentResult {
                    ContentItem = contentItem,
                    ContentItemMetadata = contentItemMetadata,
                    Properties = properties
                };

            }).ToList();

            var tokenizedState = _tokenizer.Replace(layout.State, new Dictionary<string, object> { { "Content", context.Content.ContentItem } });

            var renderLayoutContext = new LayoutContext {
                State = FormParametersHelper.ToDynamic(tokenizedState),
                LayoutRecord = layout,
            };

            if (layout.GroupProperty != null) {
                var groupPropertyId = layout.GroupProperty.Id;
                var display = _displayHelperFactory.CreateHelper(new ViewContext { HttpContext = _services.WorkContext.HttpContext }, new ViewDataContainer());

                // Index by PropertyWrapper shape.
                var groups = layoutComponents.GroupBy(x => {
                    var propertyShape = ((IEnumerable<dynamic>)x.Properties.Items).First(p => ((PropertyRecord)p.Property).Id == groupPropertyId);

                    // clear the wrappers, as they shouldn't be needed to generate the grouping key itself
                    // otherwise the DisplayContext.View is null, and throws an exception if a wrapper is rendered (#18558)
                    ((IShape)propertyShape).Metadata.Wrappers.Clear();

                    string key = Convert.ToString(display(propertyShape));
                    return key;
                }).Select(x => new { Key = x.Key, Components = x });

                var list = context.ElementShape.List = _services.New.List();
                foreach (var group in groups) {
                    var localResult = layoutDescriptor.Render(renderLayoutContext, group.Components);

                    // Add the Context to the shape.
                    localResult.Context(renderLayoutContext);
                    list.Add(_services.New.LayoutGroup(Key: new MvcHtmlString(group.Key), List: localResult));
                }
                return;
            }

            var layoutResult = layoutDescriptor.Render(renderLayoutContext, layoutComponents);

            // Add the Context to the shape.
            layoutResult.Context(renderLayoutContext);

            // Set the List shape to be the layout result.
            context.ElementShape.List = layoutResult;
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("ProjectionForm", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "ProjectionForm",
                    _QueryLayoutId: shape.SelectList(
                        Id: "QueryLayoutId",
                        Name: "QueryLayoutId",
                        Title: T("For Query"),
                        Description: T("The query to display.")),
                    _ItemsToDisplay: shape.Textbox(
                        Id: "ItemsToDisplay",
                        Name: "ItemsToDisplay",
                        Title: T("Items to display"),
                        Value: "0",
                        Description: T("The number of items to display. Enter 0 for no limit. When using pagination, this is the number of items per page."),
                        Classes: new[] { "text", "medium" }),
                    _Skip: shape.Textbox(
                        Id: "Skip",
                        Name: "Skip",
                        Title: T("Offset"),
                        Value: "0",
                        Description: T("The number of items to skip (e.g., if 2 is entered, the first 2 items won't be diplayed)."),
                        Classes: new[] { "text", "medium" }),
                    _MaxItems: shape.Textbox(
                        Id: "MaxItems",
                        Name: "MaxItems",
                        Title: T("MaxItems items"),
                        Value: "20",
                        Description: T("Maximum number of items which can be queried at once. Use 0 for unlimited. This is only used as a failsafe when the number of items comes from a user-provided source such as the query string."),
                        Classes: new[] { "text", "medium" }),
                    _PagerSuffix: shape.Textbox(
                        Id: "PagerSuffix",
                        Name: "PagerSuffix",
                        Title: T("Suffix"),
                        Description: T("Optional. Provide a suffix to use when multiple pagers are displayed on the same page, e.g., when using multiple Projection Widgets, or to define alternates."),
                        Classes: new[] { "text", "medium" }),
                    _DisplayPager: shape.Checkbox(
                        Id: "DisplayPager",
                        Name: "DisplayPager",
                        Title: T("Show Pager"),
                        Value: "true",
                        Description: T("Check to add a pager to the list.")));

                // Populate the list of queries and layouts.
                var layouts = _projectionManager.DescribeLayouts().SelectMany(x => x.Descriptors).ToList();
                var queries = _contentManager.Query<QueryPart, QueryPartRecord>().Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                    .Select(x => new QueryRecordEntry {
                        Id = x.Id,
                        Name = x.Name,
                        LayoutRecordEntries = x.Layouts.Select(l => new LayoutRecordEntry {
                            Id = l.Id,
                            Description = GetLayoutDescription(layouts, l)
                        })
                    }).ToArray();

                foreach (var queryRecord in queries.OrderBy(x => x.Name)) {
                    form._QueryLayoutId.Add(new SelectListGroup { Name = queryRecord.Name });
                    form._QueryLayoutId.Add(new SelectListItem { Text = queryRecord.Name + " " + T("(Default Layout)").Text, Value = queryRecord.Id + ";-1" });

                    foreach (var layoutRecord in queryRecord.LayoutRecordEntries.OrderBy(x => x.Description)) {
                        form._QueryLayoutId.Add(new SelectListItem { Text = queryRecord.Name + " " + T("({0})", layoutRecord.Description).Text, Value = queryRecord.Id + ";" + layoutRecord.Id });
                    }
                }

                return form;
            });
        }

        protected override void OnExporting(Projection element, ExportElementContext context) {
            var query = element.QueryId != null ? _contentManager.Get<QueryPart>(element.QueryId.Value) : default(QueryPart);
            var layout = query != null && element.LayoutId != null ? _layoutRepository.Get(element.LayoutId.Value) : default(LayoutRecord);
            var queryIdentity = query != null ? _contentManager.GetItemMetadata(query).Identity.ToString() : default(string);
            var layoutIndex = layout != null ? query.Layouts.IndexOf(layout) : -1; // -1 is the Default Layout.

            if (queryIdentity != null) {
                context.ExportableData["QueryId"] = queryIdentity;
                context.ExportableData["LayoutIndex"] = layoutIndex.ToString();
            }
        }

        protected override void OnImportCompleted(Projection element, ImportElementContext context) {
            var queryIdentity = context.ExportableData.Get("QueryId");
            var query = queryIdentity != null ? context.Session.GetItemFromSession(queryIdentity) : default(ContentManagement.ContentItem);

            if (query == null)
                return;

            var queryPart = query.As<QueryPart>();
            var layoutIndex = XmlHelper.Parse<int>(context.ExportableData.Get("LayoutIndex"));

            element.QueryId = queryPart.Id;
            element.LayoutId = layoutIndex != -1 ? queryPart.Layouts[layoutIndex].Id : -1;
        }

        private static string GetLayoutDescription(IEnumerable<LayoutDescriptor> layouts, LayoutRecord l) {
            var descriptor = layouts.First(x => l.Category == x.Category && l.Type == x.Type);
            return String.IsNullOrWhiteSpace(l.Description) ? descriptor.Display(new LayoutContext { State = FormParametersHelper.ToDynamic(l.State) }).Text : l.Description;
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }
        }
    }
}