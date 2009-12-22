<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.Models.PageRevision>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%--
name: Three column layout
description: This has a main content area and two sidebars.
zones: Content, Left sidebar, Right sidebar
author: Jon
--%>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>ThreeColumns</title>
    <link href="<%=ResolveUrl("~/Content/Site.css") %>" rel="stylesheet" type="text/css" />
    <link href="<%=ResolveUrl("~/Content/Site3.css") %>" rel="stylesheet" type="text/css" />
</head>
<body>
    <div id="page">
        <div id="header">
<%--            <% Html.Include("header"); %>
--%><%--            <% Html.Include("Navigation"); %>--%>
        </div>

        <ul id="main">
        <li id="sideBar1">
            <%= Html.IncludeZone("Left sidebar") %>
            </li>
        <li id="contentMain">
            <%= Html.IncludeZone("Content") %>
        </li>
            <li id="sideBar2">
            <%= Html.IncludeZone("Right sidebar") %>
            </li>
            <li id="footer" class="clear">
<%--                <% Html.Include("footer"); %>
--%>            </li>
        </ul>

    </div>
</body>
</html>
