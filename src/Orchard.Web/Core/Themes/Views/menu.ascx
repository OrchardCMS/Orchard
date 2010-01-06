<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<div id="menucontainer">
    <ul id="menu">
        <li><%= Html.ActionLink(T("Home").ToString(), "Index", "Home", new {Area = ""}, new {})%></li>
        <li><%= Html.ActionLink(T("About").ToString(), "About", "Home", new { Area = "" }, new { })%></li>
        <li><%= Html.ActionLink(T("Blogs").ToString(), "List", "Blog", new { Area = "Orchard.Blogs" }, new { })%></li>
        <li><%= Html.ActionLink(T("Admin").ToString(), "List", new { Area = "Orchard.Blogs", Controller = "BlogAdmin" })%></li>
    </ul>
</div>