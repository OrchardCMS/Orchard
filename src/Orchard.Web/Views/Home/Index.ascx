<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.AddTitleParts("Home Page"); %>
<h2><%= Html.Encode(ViewData["Message"]) %></h2>
<p>To learn more about ASP.NET MVC visit <a href="http://asp.net/mvc" title="ASP.NET MVC Website">http://asp.net/mvc</a>.</p>