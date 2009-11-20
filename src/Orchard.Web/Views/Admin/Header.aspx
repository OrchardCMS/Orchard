<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"
%><!DOCTYPE html>
<html>
<head>
    <title>Admin :: Orchard</title>
    <link href="http://yui.yahooapis.com/2.7.0/build/reset-fonts-grids/reset-fonts-grids.css" rel="stylesheet" type="text/css" />
    <%-- todo: (heskew) same as scripts [below] --%>
    <link href="<%=Page.ResolveClientUrl("~/Content/Admin/css/Site.css") %>" rel="stylesheet" type="text/css" />
    <%-- todo: (heskew) this should come from the admin "page" (partial) --%>
    <%-- todo: (heskew) should have at the minimum something like, say, includeScript(scriptName[, releaseScriptName], scriptPath[, releaseScriptPath]) --%>
    <script src="<%=Page.ResolveClientUrl("~/Scripts/jquery-1.3.2.js") %>" type="text/javascript"></script>
    <%-- todo: (heskew) this should come from the admin "page" (partial) --%>
    <script type="text/javascript" src="<%=ResolveUrl("~/Packages/TinyMce/Scripts/tiny_mce.js") %>"></script>
    <script type="text/javascript">tinyMCE.init({ theme: "advanced", mode: "textareas", plugins: "fullscreen,autoresize", theme_advanced_toolbar_location: "top", theme_advanced_toolbar_align: "left", theme_advanced_buttons3_add: "fullscreen" });</script>
</head>
<body>
    <div id="doc3" class="yui-t2">
        <div id="hd" role="banner">
            <div class="yui-g">
                <div class="yui-u first">
                    <h1>
                        <a href="<%=Url.Action("Index", new{Area="",Controller="Home"}) %>"><span class="displayText">
                            Project Orchard</span></a></h1>
                    <%= Html.ActionLink("Your Site", "Index", new { Area = "", Controller = "Home" })%>
                </div>
                <div class="yui-u">
                <% if (Model.CurrentUser != null) { %>
                    <div id="login">
                        User:
                        <%=Model.CurrentUser.UserName%>
                        | <%=Html.ActionLink("Logout", "LogOff", "Account", new {}, new{area=""})%></div>
                        <%} %>
                </div>
            </div>
        </div>
        <div id="bd" role="main">
            <div id="yui-main">
                <div class="yui-b">
                    <% Html.RenderPartial("Messages", Model.Messages); %>