<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<div id="menucontainer">
    <ul id="menu">
        <li><%= Html.ActionLink("Home", "Index", "Home", new {Area = ""}, new {})%></li>
        <li><%= Html.ActionLink("About", "About", "Home", new {Area = ""}, new {})%></li>
        <li><%= Html.ActionLink("Admin", "Index", new {Area = "Orchard.Media", Controller = "Admin"})%></li>
    </ul>
</div>