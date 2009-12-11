<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ThemesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Themes.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%
    Html.Include("AdminHead");%>
    <div class="yui-u">
        <h2 class="separator">
            Manage Themes</h2>
    </div>
    <div class="yui-u">
    <ul class="templates">
        <li>
        <div>
    <h3>Current Theme</h3>
    <% if (Model.CurrentTheme == null) { %>
           <p>There is no current theme in the application. The built-in theme will be used.<br />
           <%=Html.ActionLink("Install a new Theme", "Install") %></p>
    <% } else { %>
        <h4><%= Model.CurrentTheme.DisplayName %> </h4>
         <p><img src="<%= ResolveUrl("~/Themes/" + Model.CurrentTheme.ThemeName + "/Theme.gif")%>" alt="<%= Model.CurrentTheme.DisplayName %>" /><br />
            By <%= Model.CurrentTheme.Author %><br />
            <%= Model.CurrentTheme.Version %><br />
            <%= Model.CurrentTheme.Description %><br />
            <%= Model.CurrentTheme.HomePage %><br />
            <%=Html.ActionLink("Install a new Theme", "Install") %>
         </p>
    <% } %>
    </div>
        </li>
        <li>
            
    <div>
    <h3>Available Themes</h3>
    <% foreach (var theme in Model.Themes) {
        if (Model.CurrentTheme == null || theme.ThemeName != Model.CurrentTheme.ThemeName) {%>
            <h4><%= theme.DisplayName %> </h4>
            <p><img src="<%= ResolveUrl("~/Themes/" + theme.ThemeName + "/Theme.gif") %>" alt="<%= theme.DisplayName %>" /><br />
            By <%= theme.Author %><br />
            <%= theme.Version %><br />
            <%= theme.Description %><br />
            <%= theme.HomePage %><br />
            <%=Html.ActionLink("Activate", "Activate", new {themeName = theme.ThemeName}) %>
            </p>
        <% }
    } %>
</div>
        </li>
</ul>
    </div>
<% Html.Include("AdminFoot"); %>