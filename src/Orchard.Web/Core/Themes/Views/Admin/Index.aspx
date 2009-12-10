<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ThemesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Themes.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("AdminHead"); %>
    <div class="yui-u">
        <h2 class="separator">
            Themes</h2>
    </div>
    <div class="yui-u">
    List of Orchard Themes
    </div>
<% Html.Include("AdminFoot"); %>