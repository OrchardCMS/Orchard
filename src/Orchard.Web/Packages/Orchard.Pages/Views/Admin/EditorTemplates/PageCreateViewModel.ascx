<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.Pages.Services.Templates" %>
<%@ Import Namespace="Orchard.Pages.ViewModels" %>
<%@ Import Namespace="Orchard.Utility" %>
<ul class="templates">
    <% foreach (var template in Model.Templates) {
          var t = template; %>
        <li>
            <%=Html.EditorFor(m => t) %>
        </li><%
       } %>
</ul>
<div><input class="button" type="submit" value="Create" /></div>