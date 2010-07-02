<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentManagement.MetaData.Models.ContentTypeDefinition>" %>
<div class="summary">
    <div class="properties">
        <h3><%:Model.DisplayName%></h3>
        <p><%:Model.Name %> - <%:Html.ActionLink("[new content]", "Create", new {area = "Contents", id = Model.Name}) %></p>
    </div>
    <div class="related">
        <%:Html.ActionLink(T("List Items").ToString(), "List", new {area = "Contents", id = Model.Name})%><%:T(" | ")%>
        <%:Html.ActionLink(T("Edit").ToString(), "Edit", new {area = "Orchard.ContentTypes", id = Model.Name})%>
    </div>
</div>