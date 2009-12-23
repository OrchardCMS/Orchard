<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%><%
Html.RegisterStyle("site.css"); %>
<div class="page">
    <div id="header">
        <div id="title"><%=Html.TitleForPage(Html.SiteName()) %></div><%
        Model.Zones.AddRenderPartial("header", "user", Model);
        Html.Zone("header");
        Model.Zones.AddRenderPartial("menu", "menu", Model);
        Html.Zone("menu"); %>
    </div>
    <div id="main"><%
        Html.ZoneBody("content");
%>        <div id="footer"><%
            Html.Zone("footer");
        %></div>
    </div>
</div>