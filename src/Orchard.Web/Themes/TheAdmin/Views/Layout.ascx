<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>
<%
Model.Content.Add(Model.Metadata.ChildContent, "5");

Html.RegisterStyle("site.css", "1");
Html.RegisterStyle("ie.css", "1").WithCondition("if (lte IE 8)").ForMedia("screen, projection");
Html.RegisterStyle("ie6.css", "1").WithCondition("if (lte IE 6)").ForMedia("screen, projection");
Html.RegisterFootScript("admin.js", "1");
//Model.Zones.AddRenderPartial("header", "Header", Model);
//Model.Zones.AddRenderPartial("header:after", "User", Model); // todo: (heskew) should be a user display or widget
//Model.Zones.AddRenderPartial("menu", "Menu", Model);
%>
<div id="header" role="banner"><%: Display(Model.Header) %></div>
<div id="content">
    <div id="navshortcut"><a href="#Menu-admin"><%: T("Skip to navigation") %></a></div>
    <div id="main" role="main"><%: Display(Model.Content) %></div>
    <div id="menu"><%: Display(Model.Navigation) %></div>
</div>
<div id="footer" role="contentinfo"><%: Display(Model.Footer) %></div>
