<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<% Html.AddPageClassNames("home"); %>
<h1><%=Html.Encode(Html.SiteName()) %></h1>
<h2><%=Html.Encode(ViewData["Message"]) %></h2>
<p>To learn more about Orchard visit <a href="http://orchardproject.net" title="Orchard Project">http://orchardproject.net</a>.</p>