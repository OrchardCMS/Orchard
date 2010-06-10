<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<string>" %>
<div><%:Model %></div>
<% using (Html.BeginFormAntiForgeryPost(Url.Action("DeleteCulture", "Admin", new { area = "Settings" }), FormMethod.Post, new {@class = "inline link"})) { %>
    <%=Html.Hidden("cultureName", Model, new { id = "" }) %>
    <button type="submit" title="<%:T("Delete") %>">x</button>
<% } %>