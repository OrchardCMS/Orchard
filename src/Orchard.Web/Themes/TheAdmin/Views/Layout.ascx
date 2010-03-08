<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%><%
Html.RegisterStyle("site.css");
Model.Zones.AddRenderPartial("header", "Header", Model);
Model.Zones.AddRenderPartial("header:after", "User", Model); // todo: (heskew) should be a user display or widget
Model.Zones.AddRenderPartial("menu", "Menu", Model);
%>
<!--[if lte IE 6]>
    <link rel="stylesheet" type="text/css" media="screen, projection" href="/Themes/TheAdmin/Styles/ie6.css" />
<![endif]-->
<div id="header" role="banner"><% Html.Zone("header"); %></div>
<div id="content">
    <div id="navshortcut"><a href="#menu"><%=_Encoded("Skip to navigation") %></a></div>
    <div id="main" role="main"><% Html.ZoneBody("content"); %></div>
    <div id="menu"><% Html.Zone("menu"); %></div>
</div>
<div id="footer" role="contentinfo"><% Html.Zone("footer"); %></div>