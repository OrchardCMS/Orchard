<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%><%
Html.RegisterStyle("site.css");
Model.Zones.AddRenderPartial("header", "Header", Model);
Model.Zones.AddRenderPartial("header:after", "User", Model);
Model.Zones.AddRenderPartial("menu", "Menu", Model);
%>
<div class="page">
    <div id="header"><%
        Html.Zone("header");
        Html.Zone("menu"); %>
    </div>
    <div id="main"><%
        Html.ZoneBody("content"); %>
        <div id="footer"><%
            Html.Zone("footer");
        %></div>
    </div>
</div>