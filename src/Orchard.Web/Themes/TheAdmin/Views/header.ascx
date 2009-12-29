<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%=Html.ActionLink("Project Orchard", "Index", new { Area = "", Controller = "Home" })%></h1>
<div id="site"><%=Html.ActionLink("Your Site", "Index", new { Area = "", Controller = "Home" })%></div>