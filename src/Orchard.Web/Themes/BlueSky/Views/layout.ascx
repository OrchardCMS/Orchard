<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<% Html.RegisterStyle(ResolveUrl("~/Themes/BlueSky/Styles/site.css")); %>

<div class="page">
    <div id="header">
        <div id="title"><h1>My MVC Application</h1></div>
        <%Html.Zone("header"); Html.Zone("menu"); %>
    </div>
    <div id="main">
        <%Html.Zone("content", () => Html.RenderBody() );%>
        <div id="footer"><%Html.Zone("footer");%></div>
    </div>
</div>