<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.ContentManagement.Aspects" %>
<%@ Import Namespace="Orchard.ContentManagement" %>
    <div class="summary">
        <div class="properties">
            <h3><%:Html.ActionLink(Model.Item.Is<IRoutableAspect>() ? Model.Item.As<IRoutableAspect>().Title : string.Format("[title for this {0}]", Model.Item.TypeDefinition.DisplayName), "Edit", new { id = Model.Item.Id }) %></h3><%
            Html.Zone("metadata"); %>
        </div>
        <div class="related"><%
            Html.Zone("secondary"); %>
            <%:Html.ActionLink(T("Edit").ToString(), "Edit", new { id = Model.Item.Id }, new { title = T("Edit Page").ToString() })%><%:T(" | ") %>
            <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Delete", new {id = Model.Item.Id})) %>" title="<%:T("Remove Page") %>"><%:T("Remove") %></a>
        </div>
        <div style="clear:both;"></div>
        <% Html.Zone("primary"); %>
    </div>