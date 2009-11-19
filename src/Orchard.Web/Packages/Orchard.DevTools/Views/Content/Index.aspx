<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.DevTools.ViewModels.ContentIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>TwoColumns</title>
    <link href="<%=ResolveUrl("~/Content/Site2.css") %>" rel="stylesheet" type="text/css" />
</head>
<body>

<div id="header">
<div id="innerheader">
<% Html.Include("header"); %>
</div>
</div>

<div id="page">
<div id="sideBar">
<% Html.Include("Navigation"); %>
</div>

<div id="main">
<h3>Content Types</h3>
<ul>
<%foreach(var item in Model.Types.OrderBy(x=>x.Name)){%>
<li><%=Html.Encode(item.Name) %></li>
<%}%>
</ul>

<h3>Content Items</h3>
<ul>
<%foreach(var item in Model.Items.OrderBy(x=>x.Id)){%>
<li><%=Html.ActionLink(item.Id+": "+item.ContentType.Name, "details", "content", new{item.Id},new{}) %></li>
<%}%>
</ul>

</div>

<div id="footer">
     <% Html.Include("footer"); %>
</div>
</div>
</body>
</html>


