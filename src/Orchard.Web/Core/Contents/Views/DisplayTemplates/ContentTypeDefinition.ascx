<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentManagement.MetaData.Models.ContentTypeDefinition>" %>
<div class="summary">
    <div class="properties">
        <h3><%:Model.DisplayName%></h3>
        <p><%:Model.Name %> - <%:Html.ActionLink("[new content]", "Create", new {area = "Contents", id = Model.Name}) %></p>
    </div>
    <div class="related">
        <%:Html.ActionLink(T("List Items").ToString(), "List", new {area = "Contents", id = Model.Name})%><%:T(" | ")%>
        <%:Html.ActionLink(T("[Edit]").ToString(), "EditType", new {area = "Contents", id = Model.Name})%><%:T(" | ") %>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("RemoveType", new {area = "Contents", id = Model.Name}), FormMethod.Post, new { @class = "inline link" })) { %>
            <button type="submit" class="linkButton" title="<%:T("Delete") %>"><%:T("[Delete]")%></button><%
        } %>
    </div>
</div>