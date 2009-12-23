<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.ViewModels"%>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<h2><%=Html.TitleForPage("Delete pages") %></h2>
<p>Are you sure you want to delete the pages?</p>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <input type="hidden" name="<%=Html.NameOf(m => m.Options.BulkAction)%>" value="<%=PageIndexBulkAction.Delete%>" />
        <input type="hidden" name="<%=Html.NameOf(m => m.Options.BulkDeleteConfirmed)%>" value="true" />
        <input class="button" type="submit" name="submit.BulkEdit" value="Delete" />
        <%
        int pageIndex = 0;
        foreach (var pageEntry in Model.PageEntries.Where(e => e.IsChecked)) {
            var pi = pageIndex;
            %><input type="hidden" value="<%=pageEntry.PageId %>" name="<%=Html.NameOf(m => m.PageEntries[pi].PageId)%>"/>
        <input type="hidden" value="<%=pageEntry.IsChecked %>" name="<%=Html.NameOf(m => m.PageEntries[pi].IsChecked)%>"/><%
            pageIndex++;
        } %>
    </fieldset>
<% } %>