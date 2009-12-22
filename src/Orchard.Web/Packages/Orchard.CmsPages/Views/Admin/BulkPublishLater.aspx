<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.ViewModels"%>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <h2>Publish later</h2>
    <p>Enter the scheduled publication date:</p>
    <%=Html.ValidationSummary() %>
    <fieldset>
        <%=Html.EditorFor(m => m.Options.BulkPublishLaterDate)%>
        <input class="button" type="submit" name="submit.BulkEdit" value="Publish later" />
        <input type="hidden" name="<%=Html.NameOf(m => m.Options.BulkAction)%>" value="<%=PageIndexBulkAction.PublishLater%>" />
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