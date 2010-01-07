<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PagesViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%>
<%-- todo: (heskew) localize --%>
<h2><%=Html.TitleForPage("Manage Pages") %></h2>
<p>Possible text about setting up a page goes here. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla erat turpis, blandit eget feugiat nec, tempus vel quam. Mauris et neque eget justo suscipit blandit.</p>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
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
            <% foreach (var page in Model.Pages) { %>
            <tr>
                <td>
                </td>
                <td>
                  <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/online.gif") %>" alt="Online" title="The page is currently online" />
                </td>
                <td><%=page.Title ?? "(no title)" %></td>
                <td><%=Html.ActionLink(page.Slug ?? "(no slug)", "Item", new { controller = "Page", slug = page.Slug }) %></td>
                <td>By <%= page.Creator.UserName %></td>
                <td></td>
                <td>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/draft.gif") %>" alt="Draft" title="The page has a draft" />
                </td>
                <td></td>
                <td><%=Html.ActionLink("Edit", "Edit", new { pageSlug = page.Slug }) %></td>
            </tr>
            <% }%>
        </table>
    </fieldset>
    <div class="manage"><%=Html.ActionLink("Add a page", "Create", new {}, new { @class = "button"}) %></div>
<% } %>