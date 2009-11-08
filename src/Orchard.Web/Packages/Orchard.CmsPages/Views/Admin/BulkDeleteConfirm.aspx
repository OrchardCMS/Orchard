<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.ViewModels"%>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<!DOCTYPE html>
<html>
<head id="Head1" runat="server">
    <title>Index2</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
    <div class="yui-u">
        <h2 class="separator">Delete pages</h2>
        <p class="bottomSpacer">
            Are you sure you want to delete the pages?</p>
    </div>
    
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <%= Html.ValidationSummary() %>

            <div class="yui-u first">
            <ol>
                <li class="clearLayout">
                    <input type="hidden" name="<%=Html.NameOf(m => m.Options.BulkAction)%>" value="<%=PageIndexBulkAction.Delete%>" />
                    <input type="hidden" name="<%=Html.NameOf(m => m.Options.BulkDeleteConfirmed)%>" value="true" />
                    <input class="button" type="submit" name="submit.BulkEdit" value="Delete" />
                </li>
            </ol>
            </div>

            <%
                int pageIndex = 0;
                foreach (var pageEntry in Model.PageEntries.Where(e => e.IsChecked)) {
            %>
                    <input type="hidden" value="<%=pageEntry.PageId %>" name="<%=Html.NameOf(m => m.PageEntries[pageIndex].PageId)%>"/>
                    <input type="hidden" value="<%=pageEntry.IsChecked %>" name="<%=Html.NameOf(m => m.PageEntries[pageIndex].IsChecked)%>"/>
            <%  pageIndex++;
            }%>
        <%}/*EndForm*/%>
    </div>
    <% Html.Include("Footer"); %>
</body>
</html>
