<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Pages.Models.Page>" %>
<%@ Import Namespace="Orchard.Pages"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
if (AuthorizedFor(Permissions.EditOthersPages)) { %>
<div class="manage">
    <a href="<%=Url.Action("Edit", "Admin", new {id = Model.Id, area = "Orchard.Pages"}) %>" class="edit"><%=_Encoded("Edit")%></a>
</div><%
} %>