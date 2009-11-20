<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.Wikis.ViewModels"%>
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
<h3>Wiki Pages</h3>
<ul>
<li><%=Html.ActionLink("Create new page", "create") %></li>
<%foreach(var item in Model.Pages.OrderBy(x=>x.Record.Name)){%>
<li><%=Html.ActionLink(item.Record.Name??"(no name)", "show", new { item.ContentItem.Id }, new{})%></li>
<%}%>
</ul>

</div>

<div id="footer">
     <% Html.Include("footer"); %>
</div>
</div>
</body>
</html>


