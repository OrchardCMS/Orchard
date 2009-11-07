<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.CmsPages.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<script runat="server">
public string DefaultText(string valueText, string defaultText)
{
    if (string.IsNullOrEmpty(valueText))
        return defaultText;
    return valueText;
}
string SplitDateTime(DateTime dt) 
{
    return string.Format("{0:d}", dt) + "<br />" +
           string.Format("{0:t}", dt);
}
</script>

<!DOCTYPE html>
<html>
<head id="Head1" runat="server">
    <title>Index2</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
        <h2>
            Manage Pages</h2>
        <p class="bottomSpacer">
            Possible text about setting up a page goes here. Lorem ipsum dolor sit amet, consectetur
            adipiscing elit. Nulla erat turpis, blandit eget feugiat nec, tempus vel quam. Mauris
            et neque eget justo suscipit blandit.</p>
        <%=Html.ValidationSummary() %>
        <ol class="horizontal actions floatLeft">
            <li>
                <label class="floatLeft" for="publishActions">
                    Actions:</label>
                <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction)%>">
                    <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.None, "Choose action...")%>
                    <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.PublishNow, "Publish Now")%>
                    <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.PublishLater, "Publish Later")%>
                    <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.Unpublish, "Unpublish")%>
                    <%=Html.SelectOption(Model.Options.BulkAction, PageIndexBulkAction.Delete, "Delete")%>
                </select>
            </li>
            <li>
                <input class="button roundCorners" type="submit" name="submit.BulkEdit" value="Apply" />
            </li>
        </ol>
        <ol class="horizontal actions">
            <li>
                <label class="floatLeft" for="filterResults">
                </label>
                <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter)%>">
                    <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.All, "All Pages")%>
                    <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.Published, "Published Pages")%>
                    <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.Offline, "Offline Pages")%>
                    <%=Html.SelectOption(Model.Options.Filter, PageIndexFilter.Scheduled, "Publish Pending")%>
                </select>
            </li>
            <li>
                <input class="button roundCorners" type="submit" name="submit.Filter" value="Filter"/>
            </li>
        </ol>
        <%=Html.ActionLink("Add a page", "Create", new {}, new {@class="floatRight topSpacer"}) %>
        <table id="pluginListTable" cellspacing="0" class="clearLayout" summary="This is a table of the PageEntries currently available for use in your application.">
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
                    <th scope="col"><%--<input type="checkbox" value="1" name="<%=Html.NameOf(m => m.Options.BulkChecked)%>"/>--%></th>
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
                foreach (var pageEntry in Model.PageEntries) {
                  var revision = pageEntry.Page.Revisions.LastOrDefault();
                  if (revision == null)
                      continue;
            %>
            <tr>
                <td>
                    <%--TODO: Use "NameOf" when it supports these expressions--%>
                    <input type="hidden" value="<%=Model.PageEntries[pageIndex].PageId %>" name="<%=string.Format("PageEntries[{0}].PageId", pageIndex)%>"/>
                    <input type="checkbox" value="true" name="<%=string.Format("PageEntries[{0}].IsChecked", pageIndex)%>"/>
                </td>
                <td>
                <% if (pageEntry.IsPublished) { %>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/online.gif")%>" alt="Online" title="The page is currently online" />
                <% } else { %>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/offline.gif")%>" alt="Offline" title="The page is currently offline" />
                <% } %>
                </td>
                <td><%=Html.ActionLink(DefaultText(revision.Title, "(no title)"), "Show", new { Controller = "Templates", revision.Slug })%></td>
                <td><%=Html.ActionLink(DefaultText(revision.Slug, "(no slug)"), "Show", new { Controller = "Templates", revision.Slug })%></td>
                <td>By Unk</td>
                <td><%=string.Format("{0:d}", revision.ModifiedDate) %><br /><%=string.Format("{0:t}", revision.ModifiedDate) %></td>
                <td>
                <% if (pageEntry.HasDraft) { %>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/draft.gif")%>" alt="Draft" title="The page has a draft" />
                <% } %>
                <% if (revision.Page.Scheduled.Any()) { %>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.CmsPages/Content/Admin/images/scheduled.gif")%>" alt="Scheduled" title="The draft is scheduled for publishing" />
                <% } %>
                </td>
                <td>
                <%=revision.Page.Scheduled.Any() ? SplitDateTime(revision.Page.Scheduled.First().ScheduledDate.Value) : ""%>
                </td>
                <td><%=Html.ActionLink("Edit", "Edit", new { revision.Page.Id })%></td>
            </tr>
            <% pageIndex++;
            }%>
        </table>
        <%=Html.ActionLink("Add a page", "Create", new {}, new {@class="floatRight bottomSpacer"}) %>
    </div>
    <% Html.EndForm(); %>
    <% Html.Include("Footer"); %>
</body>
</html>
