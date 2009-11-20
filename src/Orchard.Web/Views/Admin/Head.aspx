<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html"
%><%--
todo: (heskew) rework how/what pages are assembled when we get into theming --%><!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
    <%-- todo: (heskew) get this from the menu system
    --%><title>Admin :: Orchard</title>
    <link href="<%=Page.ResolveClientUrl("~/Content/Admin/css/yui.reset-3.0.0.css") %>" rel="stylesheet" type="text/css" />
    <%-- todo: (heskew) same as scripts [below]
    --%><link href="<%=Page.ResolveClientUrl("~/Content/Admin/css/base.css") %>" rel="stylesheet" type="text/css" />
    <%-- todo: (heskew) this should come from the admin "page" (partial) 
         todo: (heskew) should have at the minimum something like, say, includeScript(scriptName[, releaseScriptName], scriptPath[, releaseScriptPath])
    --%><script src="<%=Page.ResolveClientUrl("~/Scripts/jquery-1.3.2.js") %>" type="text/javascript"></script>
    <%-- todo: (heskew) this should come from the admin "page" (partial)
    --%><script type="text/javascript" src="<%=ResolveUrl("~/Packages/TinyMce/Scripts/tiny_mce.js") %>"></script>
    <script type="text/javascript">tinyMCE.init({ theme: "advanced", mode: "textareas", plugins: "fullscreen,autoresize", theme_advanced_toolbar_location: "top", theme_advanced_toolbar_align: "left", theme_advanced_buttons3_add: "fullscreen" });</script>
</head>
<body>
    <div id="banner" role="banner">
        <h1><%=Html.ActionLink(T("Project Orchard").ToString(), "Index", new { Area = "", Controller = "Home" })%></h1>
        <div><%=Html.ActionLink(T("Your Site").ToString(), "Index", new { Area = "", Controller = "Home" })%></div>
        <% if (Model.CurrentUser != null) {
            %><div id="login"><%=T("User")%><%=H(Model.CurrentUser.UserName)%> | <%=Html.ActionLink(T("Logout").ToString(), "LogOff", new { Area = "", Controller = "Account" }) %></div><%
            } %>
    </div>
    <div id="main" role="main">
        <div id="content">
            <% Html.RenderPartial("Messages", Model.Messages); %>