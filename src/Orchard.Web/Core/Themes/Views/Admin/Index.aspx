<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ThemesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Themes.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("AdminHead"); %>
    <div class="yui-u">
        <h2 class="separator">
            Themes</h2>
    </div>
    <div class="yui-u">
    <% foreach (var theme in Model.Themes) { %>
       <p>Name: <%= theme.ThemeName %></p>
       <p>DisplayName: <%= theme.DisplayName %></p>
       <p>Description: <%= theme.Description %></p>
       <p>Author: <%= theme.Author %></p>
       <p>Version: <%= theme.Version %></p>
       <p>HomePage: <%= theme.HomePage %></p>
    <% } %>
    Current Theme: <%= Model.CurrentTheme != null ? Model.CurrentTheme.ThemeName : "None" %>
    </div>
<% Html.Include("AdminFoot"); %>