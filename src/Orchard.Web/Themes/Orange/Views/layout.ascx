<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%><%
Html.RegisterStyle("site.css");
Model.Zones.AddRenderPartial("header", "header", Model);
Model.Zones.AddRenderPartial("header:after", "user", Model);
Model.Zones.AddRenderPartial("menu", "menu", Model);
Model.Zones.AddRenderPartial("content:before", "messages", Model.Messages);
%>
<div class="page">
    <div id="header"><%Html.Zone("header");%>.ToHtmlString();
        Html.Zone("header").Render();
        Html.Zone("menu"); %>
    </div>
    <div id="main"><%
        Html.Zone("content"); %>
        <div id="footer"><%
            Html.Zone("footer", ()=>Html.RenderPartial("footer", Model));
        %></div>
    </div>
</div>