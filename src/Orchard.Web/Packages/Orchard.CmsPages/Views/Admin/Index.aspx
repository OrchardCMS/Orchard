<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.CmsPages.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%-- todo: (heskew) localize --%>
<% Html.Include("AdminHead"); %>
    <h2>Manage Pages</h2>
    <p>Possible text about setting up a page goes here. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla erat turpis, blandit eget feugiat nec, tempus vel quam. Mauris et neque eget justo suscipit blandit.</p>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <fieldset class="actions bulk">
            <label for="publishActions">Actions: </label>
            <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction) %>">
                <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.None, "Choose action...") %>
                <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.PublishNow, "Publish Now") %>
                <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.PublishLater, "Publish Later") %>
                <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.Unpublish, "Unpublish") %>
                <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.Delete, "Delete") %>
            </select>
            <input class="button" type="submit" name="submit.BulkEdit" value="Apply" />
        </fieldset>
        <fieldset class="actions bulk">
            <label for="filterResults">Filter: </label>
            <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter) %>">
                <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.All, "All Pages") %>
                <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.Published, "Published Pages") %>
                <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.Offline, "Offline Pages") %>
                <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.Scheduled, "Publish Pending") %>
            </select>
            <input class="button" type="submit" name="submit.Filter" value="Apply"/>
        </fieldset>
        <div class="manage"><%=Html.ActionLink("Add a page", "Create", new {}, new { @class = "button" }) %></div>
        <fieldset>
            <table class="items" summary="This is a table of the PageEntries currently available for use in your application.">
                <colgroup>
                    <col id="Actions" />
                    <col id="Status" />
                    <col id="Title" />
                    <col id="Author" />
                    <col id="LastUpdated" />
                    <col id="Draft" />
                    <col id="Timer" />
                    <col id="Edit" />
                </colgroup>
                <thead>
                    <tr>
                        <th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
                        <th scope="col">Status</th>
                        <th scope="col">Title</th>
                        <th scope="col">Slug</th>
                        <th scope="col">Author</th>
                        <th scope="col">Last Updated</th>
                        <th scope="col">Draft</th>
                        <th scope="col">Scheduled</th>
                        <th scope="col"></th>
                    </tr>
                </thead>
                <%
            int pageIndex = 0;
            foreach (var pageEntry in Model.PageEntries)
            {
                var revision = pageEntry.Page.Revisions.LastOrDefault();
                
                if (revision == null) continue;
                
                %><tr>
                    <td>
                        <input type="hidden" value="<%=Model.PageEntries[pageIndex].PageId %>" name="<%=Html.NameOf(m => m.PageEntries[pageIndex].PageId) %>"/>
                        <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.PageEntries[pageIndex].IsChecked) %>"/>
                    </td>
                    <td>
                        <% if (pageEntry.IsPublished) {
                            %><img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/online.gif") %>" alt="Online" title="The page is currently online" />
                        <% } else {
                            %><img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/offline.gif") %>" alt="Offline" title="The page is currently offline" />
                        <% } %>
                    </td>
                    <td><%=Html.ActionLink(revision.Title ?? "(no title)", "Show", new { Controller = "Templates", revision.Slug }) %></td>
                    <td><%=Html.ActionLink(revision.Slug ?? "(no slug)", "Show", new { Controller = "Templates", revision.Slug }) %></td>
                    <td>By Unk</td>
                    <td><%=string.Format("{0:d}<br />{0:t}", revision.ModifiedDate) %></td>
                    <td>
                        <% if (pageEntry.HasDraft) {
                            %><img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/draft.gif") %>" alt="Draft" title="The page has a draft" />
                        <% } 
                           if (revision.Page.Scheduled.Any()) {
                            %><img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/scheduled.gif") %>" alt="Scheduled" title="The draft is scheduled for publishing" />
                        <% } %>
                    </td>
                    <td><%=revision.Page.Scheduled.Any() ? string.Format("{0:d}<br />{0:t}", revision.Page.Scheduled.First().ScheduledDate.Value) : "" %></td>
                    <td><%=Html.ActionLink("Edit", "Edit", new { revision.Page.Id }) %></td>
                </tr>
                <%
                pageIndex++;
            }%>
            </table>
        </fieldset>
        <div class="manage"><%=Html.ActionLink("Add a page", "Create", new {}, new { @class = "button"}) %></div>
    <% } %>
<% Html.Include("AdminFoot"); %>