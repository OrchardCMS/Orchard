<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("site.css");
Html.RegisterStyle("site.css"); %>
<div class="page">
    <div id="header"><%
        Html.Zone("header");
        Html.Zone("menu");
    %></div>
    <div id="main"><%
        Html.ZoneBody("content");
%>        <div id="footer"><%
            Html.Zone("footer");
        %></div>
    </div>
</div>