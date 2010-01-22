<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<PagesViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Pages").ToString())%></h1>
<%-- todo: Add helper text here when ready. <p><%=_Encoded("Possible text about setting up a page goes here.")%></p>--%>
<div class="manage"><%=Html.ActionLink(T("Add a page").ToString(), "Create", new { }, new { @class = "button" })%></div>
<% using (Html.BeginFormAntiForgeryPost())
   { %>
    <%=Html.ValidationSummary()%>
    <fieldset class="actions bulk">
        <label for="publishActions"><%=_Encoded("Actions:")%></label>
        <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction) %>">
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.None, _Encoded("Choose action...").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.PublishNow, _Encoded("Publish Now").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Unpublish, _Encoded("Unpublish").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Delete, _Encoded("Delete").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="<%=_Encoded("Apply") %>" />
    </fieldset>
    <fieldset class="actions bulk">
        <label for="filterResults"><%=_Encoded("Filter:")%></label>
        <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter) %>">
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.All, _Encoded("All Pages").ToString())%>
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.Published, _Encoded("Published Pages").ToString())%>
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.Offline, _Encoded("Offline Pages").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.Filter" value="<%=_Encoded("Apply") %>"/>
    </fieldset>
    <fieldset>
        <table class="items" summary="<%=_Encoded("This is a table of the PageEntries currently available for use in your application.") %>">
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
                    <th scope="col"><%=_Encoded("Status")%></th>
                    <th scope="col"><%=_Encoded("Title")%></th>
                    <th scope="col"><%=_Encoded("Slug")%></th>
                    <th scope="col"><%=_Encoded("Author")%></th>
                    <th scope="col"><%=_Encoded("Draft")%></th>
                    <th scope="col"><%=_Encoded("Scheduled")%></th>
                    <th scope="col"></th>
                </tr>
            </thead>
            <%
int pageIndex = 0;
foreach (var pageEntry in Model.PageEntries)
{
    var pi = pageIndex; %>
            <tr>
                <td>
                    <input type="hidden" value="<%=Model.PageEntries[pageIndex].PageId %>" name="<%=Html.NameOf(m => m.PageEntries[pi].PageId) %>"/>
                    <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.PageEntries[pi].IsChecked) %>"/>
                </td>
                <td>
                  <% if (pageEntry.Page.HasPublished)
                     { %>
                  <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/online.gif") %>" alt="<%=_Encoded("Online") %>" title="<%=_Encoded("The page is currently online") %>" />
                  <% }
                     else
                     { %>
                  <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/offline.gif") %>" alt="<%=_Encoded("Offline") %>" title="<%=_Encoded("The page is currently offline") %>" />
                  <% } %>
                </td>
                <td><%=Html.Encode(pageEntry.Page.Title ?? T("(no title)").ToString())%></td>
                <td><% if (pageEntry.Page.HasPublished)
                       { %>
                        <%=Html.ActionLink(pageEntry.Page.Slug ?? T("(no slug)").ToString(), "Item", new { controller = "Page", slug = pageEntry.Page.PublishedSlug })%>
                    <% }
                       else
                       {%>
                        <%=Html.Encode(pageEntry.Page.Slug ?? T("(no slug)").ToString())%>
                    <% } %>   
                 </td>
                <td><%=_Encoded("By {0}", pageEntry.Page.Creator.UserName)%></td>
                <td>
                    <% if (pageEntry.Page.HasDraft)
                       { %>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.Pages/Content/Admin/images/draft.gif") %>" alt="<%=_Encoded("Draft") %>" title="<%=_Encoded("The page has a draft") %>" />
                    <% } %>
                </td>
                <td>
                    <% if (!pageEntry.Page.IsPublished)
                       { %>
                        <%=pageEntry.Page.Published != null
                          ? string.Format("{0:d}<br />{0:t}", pageEntry.Page.Published.Value)
                          : ""%>
                    <% } %>    
                </td>
                <td><%=Html.ActionLink(T("Edit").ToString(), "Edit", new { id = pageEntry.PageId })%></td>
            </tr>
            <%
pageIndex++;
} %>
        </table>
    </fieldset>
<% } %>
<div class="manage"><%=Html.ActionLink(T("Add a page").ToString(), "Create", new { }, new { @class = "button" })%></div>