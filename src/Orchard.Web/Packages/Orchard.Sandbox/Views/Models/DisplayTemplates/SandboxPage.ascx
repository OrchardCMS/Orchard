<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.Models.ViewModels" %>
<%@ Import Namespace="Orchard.Models" %>
<%=Html.DisplayZone("before")%>
<div class="item">
    <%=Html.DisplayZone("first")%>
    <h1>
        <%=Html.Encode(Model.Item.Record.Name) %></h1>
    <%=Html.DisplayZone("metatop")%>
    <div class="body">
        <%=Html.DisplayZone("body")%></div>
    <%=Html.DisplayZone("metabottom")%>
    <div class="actions">
        <%=Html.ItemEditLink("Edit this page", Model.Item) %>,
        <%=Html.ActionLink("Return to list", "index") %>
        <%=Html.DisplayZone("actions") %></div>
    <%=Html.DisplayZonesExcept("last","after") %>
    <%=Html.DisplayZone("last")%>
</div>
<%=Html.DisplayZone("after")%>
