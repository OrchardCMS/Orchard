<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ModulesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Modules.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Modules").ToString()) %></h1>
<div class="manage"><%=Html.ActionLink(T("Install a module").ToString(), "Features", new { }, new { @class = "button primaryAction" })%></div>
<h2><%=T("Installed Modules") %></h2>
<% if (Model.Modules.Count() > 0) { %>
<fieldset class="pageList">
<ul class="contentItems blogs"><%
    foreach (var module in Model.Modules.OrderBy(m => m.DisplayName)) { %>
    <li>
        <div class="summary">
            <div class="properties">
                <h3><%=Html.Encode(module.DisplayName) %></h3>
                <div class="related">
                    <%=Html.ActionLink(T("Edit").ToString(), "edit", new {moduleName = module.ModuleName, area = "Orchard.Modules"}) %><%=_Encoded(" | ")%>
                    <a href="#">Delete</a>
                </div>
            </div>
        </div><%
        if (!string.IsNullOrEmpty(module.Description)) { %>
        <p><%=Html.Encode(module.Description) %></p><%
        } %>
    </li><%
    } %>
</ul>
</fieldset><%
 } %>