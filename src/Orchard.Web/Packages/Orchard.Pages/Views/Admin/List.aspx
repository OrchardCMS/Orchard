<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PagesViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%>
<%-- todo: (heskew) localize --%>
<h2><%=Html.TitleForPage("Manage Pages") %></h2>
<p>Possible text about setting up a page goes here. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla erat turpis, blandit eget feugiat nec, tempus vel quam. Mauris et neque eget justo suscipit blandit.</p>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions">Actions: </label>
        <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction) %>">
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.None, "Choose action...") %>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.PublishNow, "Publish Now") %>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Unpublish, "Unpublish") %>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Delete, "Delete") %>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="Apply" />
    </fieldset>
    <fieldset class="actions bulk">
        <label for="filterResults">Filter: </label>
        <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter) %>">
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.All, "All Pages") %>
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.Published, "Published Pages") %>
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.Offline, "Offline Pages") %>
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
                foreach (var pageEntry in Model.PageEntries) { %>
            <tr>
                <td>
                    <input type="hidden" value="<%=Model.PageEntries[pageIndex].PageId %>" name="<%=Html.NameOf(m => m.PageEntries[pageIndex].PageId) %>"/>
                    <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.PageEntries[pageIndex].IsChecked) %>"/>
                </td>
                <td>
                  <% if (pageEntry.Page.HasPublished) {%>
                  <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/online.gif")%>" alt="Online" title="The page is currently online" />
                  <% } else { %>
                  <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/offline.gif")%>" alt="Offline" title="The page is currently offline" />
                  <% } %>
                </td>
                <td><%=pageEntry.Page.Title ?? "(no title)" %></td>
                <td><% if (pageEntry.Page.HasPublished) {%>
                        <%=Html.ActionLink(pageEntry.Page.Slug ?? "(no slug)", "Item", new {controller = "Page", slug = pageEntry.Page.PublishedSlug})%>
                    <% } else {%>
                        <%= pageEntry.Page.Slug ?? "(no slug)" %>
                    <% } %>   
                 </td>
                <td>By <%= pageEntry.Page.Creator.UserName %></td>
                <td></td>
                <td>
                    <% if (pageEntry.Page.HasDraft) { %>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/draft.gif") %>" alt="Draft" title="The page has a draft" />
                    <% } %>
                </td>
                <td></td>
                <td><%=Html.ActionLink("Edit", "Edit", new { pageSlug = pageEntry.Page.Slug }) %></td>
            </tr>
            <%
                pageIndex++; 
            } %>
        </table>
    </fieldset>
    <div class="manage"><%=Html.ActionLink("Add a page", "Create", new {}, new { @class = "button"}) %></div>
<% } %>