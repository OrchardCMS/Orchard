<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.ViewModels"%>
<%@ Import Namespace="Orchard.Utility"%>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <%using (Html.BeginForm()) { %>
    <div class="yui-u">
        <h2>Publish later</h2>
        <p class="bottomSpacer">Enter the scheduled publication date:</p>
        <%=Html.EditorFor(m => m.Options.BulkPublishLaterDate)%>
    </div>
    
    <div class="yui-u">
        <%= Html.ValidationSummary() %>

            <div class="yui-u first">
            <ol>
                <li class="clearLayout">
                    <input type="hidden" name="<%=Html.NameOf(m => m.Options.BulkAction)%>" value="<%=PageIndexBulkAction.PublishLater%>" />
                    <input class="button" type="submit" name="submit.BulkEdit" value="Publish later" />
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
    </div>
    <%}/*EndForm*/%>
<% Html.Include("Foot"); %>