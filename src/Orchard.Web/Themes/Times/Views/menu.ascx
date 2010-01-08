<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<div id="menucontainer">
    <ul id="menu">
        <li><%= Html.ActionLink("Home", "Index", "Home", new {Area = ""}, new {})%></li>
        <li><%= Html.ActionLink("Blogs", "List", "Blog", new {Area = "Orchard.Blogs"}, new {})%></li>
        <li><%= Html.ActionLink("Admin", "List", new {Area = "Orchard.Blogs", Controller = "BlogAdmin"})%></li>
    </ul>
 </div>