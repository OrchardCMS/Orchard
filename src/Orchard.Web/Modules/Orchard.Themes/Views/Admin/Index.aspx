<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<ThemesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Themes.ViewModels"%>
<h1><%: Html.TitleForPage(T("Manage Themes").ToString()) %></h1>
<% if (Model.CurrentTheme == null) {
    %><p><%: T("There is no current theme in the application. The built-in theme will be used.")
             %><br /><%: Html.ActionLink(T("Install a new Theme").ToString(), "Install") %></p><%
   } else {
    %><h3><%: T("Current Theme")%> - <%: Model.CurrentTheme.DisplayName %></h3>

        <%=Html.Image(Html.ThemePath(Model.CurrentTheme, "/Theme.png"), Html.Encode(Model.CurrentTheme.DisplayName), new { @class = "themePreviewImage" })%>
        <h5><%: T("By") %> <%: Model.CurrentTheme.Author %></h5>
        
        <p>
        <%: T("Version:") %> <%: Model.CurrentTheme.Version %><br />
        <%: Model.CurrentTheme.Description %><br />
        <%: Model.CurrentTheme.HomePage %>
        </p>
        <%: Html.ActionLink(T("Install a new Theme").ToString(), "Install", null, new { @class = "button primaryAction" })%>
     
<% } %>
<h2><%: T("Available Themes")%></h2>
<ul class="templates">
<% foreach (var theme in Model.Themes) {
    if (Model.CurrentTheme == null || theme.ThemeName != Model.CurrentTheme.ThemeName) {
        %> <li>
      <div>
        <h3><%: theme.DisplayName %></h3>
        <%=Html.Image(Html.ThemePath(theme, "/Theme.png"), Html.Encode(theme.DisplayName), null)%>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("Activate"), FormMethod.Post, new { @class = "inline" })) { %>
            <%: Html.Hidden("themeName", theme.ThemeName)%>
            <button type="submit" title="<%: T("Activate") %>"><%: T("Activate") %></button>
        <% } %>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("Preview"), FormMethod.Post, new { @class = "inline" })) { %>
            <%: Html.Hidden("themeName", theme.ThemeName)%>
            <button type="submit" title="<%: T("Preview") %>"><%: T("Preview") %></button>
        <% } %>
        <h5><%: T("By") %> <%: theme.Author %></h5>
        <p>
            <%: T("Version:") %> <%: theme.Version %><br />
            <%: theme.Description %><br />
            <%: theme.HomePage %>
        </p>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("Uninstall"), FormMethod.Post, new { @class = "inline link" })) { %>
            <%: Html.Hidden("themeName", theme.ThemeName)%>
           <button type="submit" class="uninstall" title="<%: T("Uninstall") %>"><%: T("Uninstall")%></button>
        <% } %>
     </div>   
    </li>
    <% }
} %>
</ul>