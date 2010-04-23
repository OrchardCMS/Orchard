<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ModulesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Modules.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Modules").ToString()) %></h1>
<h2><%=T("Installed Modules") %></h2>
<% if (Model.Modules.Count() > 0) { %>
<ul><%
    foreach (var module in Model.Modules.OrderBy(m => m.DisplayName)) { %>
    <li>
        <h3><%=Html.Encode(module.DisplayName) %></h3>
        <p><%=module.Description != null ? Html.Encode(module.Description) : T("<em>no description</em>") %></p>
    </li><%
    } %>
</ul><%
 } %>