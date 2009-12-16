<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("site.css"); %>
<div class="page">
    <div id="header"><%
        Html.RenderZone("header");
        Html.RenderZone("menu");
    %></div>
    <div id="main"><%
        Html.RenderBody();
%>        <div id="footer"><%
            Html.RenderZone("footer");
        %></div>
    </div>
</div>