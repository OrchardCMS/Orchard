<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>
<div id="app"><%: Html.ActionLink(T("Project Orchard").ToString(), "Index", new { Area = "", Controller = "Home" })%></div>
<div id="site"><%: Html.ActionLink(T("Your Site").ToString(), "Index", new { Area = "", Controller = "Home" })%></div>
