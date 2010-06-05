<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<ThemesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Themes.ViewModels"%><%
 Html.RegisterStyle("admin.css"); %>
<h1><%=Html.TitleForPage(T("Manage Themes").ToString()) %></h1>
<% if (Model.CurrentTheme == null) {
    %><p><%=_Encoded("There is no current theme in the application. The built-in theme will be used.")
             %><br /><%=Html.ActionLink(T("Install a new Theme").ToString(), "Install") %></p><%
   } else {
    %><h3><%=_Encoded("Current Theme")%> - <%=Html.Encode(Model.CurrentTheme.DisplayName) %></h3>

        <%=Html.Image(Html.ThemePath(Model.CurrentTheme, "/Theme.png"), Html.Encode(Model.CurrentTheme.DisplayName), new { @class = "themePreviewImage" })%>
        <h5><%=_Encoded("By") %> <%=Html.Encode(Model.CurrentTheme.Author) %></h5>
        
        <p>
        <%=_Encoded("Version:") %> <%=Html.Encode(Model.CurrentTheme.Version) %><br />
        <%=Html.Encode(Model.CurrentTheme.Description) %><br />
        <%=Html.Encode(Model.CurrentTheme.HomePage) %>
        </p>
        <%=Html.ActionLink(T("Install a new Theme").ToString(), "Install", null, new { @class = "button primaryAction" })%>
     
<% } %>
<h2><%=_Encoded("Available Themes")%></h2>
<ul class="templates">
<% foreach (var theme in Model.Themes) {
    if (Model.CurrentTheme == null || theme.ThemeName != Model.CurrentTheme.ThemeName) {
        %> <li>
      <div>
        <h3><%=Html.Encode(theme.DisplayName) %></h3>
        <%=Html.Image(Html.ThemePath(theme, "/Theme.png"), Html.Encode(theme.DisplayName), null)%>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("Activate"), FormMethod.Post, new { @class = "inline" })) { %>
            <%=Html.Hidden("themeName", theme.ThemeName)%>
            <button type="submit" title="<%=_Encoded("Activate") %>"><%=_Encoded("Activate") %></button>
        <% } %>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("Preview"), FormMethod.Post, new { @class = "inline" })) { %>
            <%=Html.Hidden("themeName", theme.ThemeName)%>
            <button type="submit" title="<%=_Encoded("Preview") %>"><%=_Encoded("Preview") %></button>
        <% } %>
        <h5><%=_Encoded("By") %> <%=Html.Encode(theme.Author) %></h5>
        <p>
            <%=_Encoded("Version:") %> <%=Html.Encode(theme.Version) %><br />
            <%=Html.Encode(theme.Description) %><br />
            <%=Html.Encode(theme.HomePage) %>
        </p>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("Uninstall"), FormMethod.Post, new { @class = "inline link" })) { %>
            <%=Html.Hidden("themeName", theme.ThemeName)%>
           <button type="submit" class="uninstall" title="<%=_Encoded("Uninstall") %>"><%=_Encoded("Uninstall")%></button>
        <% } %>
     </div>   
    </li>
    <% }
} %>
</ul>