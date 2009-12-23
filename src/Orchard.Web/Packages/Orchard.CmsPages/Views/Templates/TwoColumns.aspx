<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.Models.PageRevision>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%--
name: Two column layout
description: This has a main content area and a sidebar on the right.
zones: Content, Right sidebar
author: Jon
--%>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TwoColumns</title>
    <link href="<%=ResolveUrl("~/Content/Site2.css") %>" rel="stylesheet" type="text/css" />
</head>
<%--<body>
    <div class="page">
        <div id="header">
            <% Html.Include("header"); %>
        </div>
        <div id="main">
            <%= Html.IncludeZone("Content") %>
            <%= Html.IncludeZone("Right sidebar") %>
            <div id="footer">
                <% Html.Include("footer"); %>
            </div>
        </div>
    </div>
</body>--%>


<body>

<div id="header">
<div id="innerheader">
<%--<% Html.Include("header"); %>
--%></div>
</div>

<div id="page">
<div id="sideBar">
<%--<% Html.Include("Navigation"); %>
--%><%= Html.IncludeZone("Right sidebar") %>
</div>

<div id="main">
<%= Html.IncludeZone("Content") %>
</div>

<div id="footer">
<%--     <% Html.Include("footer"); %>
--%></div>
</div>
</body>
</html>


