<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels" %>
<div class="item">
    <%=Html.DisplayZone("first")%>
    <div class="title">
        <%=Html.DisplayZone("title")%>
    </div>
    <%=Html.DisplayZone("metatop")%>
    <div class="actions">
        <%=Html.ItemEditLink("Edit this page", Model.Item) %>
        <%=Html.ActionLink("Return to list", "index") %>
        <%=Html.DisplayZone("actions") %>
    </div>
    <div class="body">
        <%=Html.DisplayZone("body")%>
    </div>
    <%=Html.DisplayZone("metabottom")%>
    <div class="footer">
        <%=Html.DisplayZonesExcept("last") %>
        <%=Html.DisplayZone("last")%>
    </div>
</div>
