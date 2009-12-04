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
    <%-- todo: (heskew) same as scripts [below]
    --%><link href="<%=Page.ResolveClientUrl("~/Content/Admin/css/base.css") %>" rel="stylesheet" type="text/css" />
    <%-- todo: (heskew) this should come from the admin "page" (partial) 
         todo: (heskew) should have at the minimum something like, say, includeScript(scriptName[, releaseScriptName], scriptPath[, releaseScriptPath])
    --%><script src="<%=Page.ResolveClientUrl("~/Scripts/jquery-1.3.2.js") %>" type="text/javascript"></script>
    <%-- todo: (heskew) this should come from the admin "page" (partial)
         todo: (heskew) use the TinyMCE jQuery package instead?
    --%><script type="text/javascript" src="<%=ResolveUrl("~/Packages/TinyMce/Scripts/tiny_mce.js") %>"></script>
    <script type="text/javascript">
        tinyMCE.init({
            theme: "advanced",
            mode: "specific_textareas",
            editor_selector: "html",
            plugins: "fullscreen,autoresize,searchreplace",
            theme_advanced_toolbar_location: "top",
            theme_advanced_toolbar_align: "left",
            theme_advanced_buttons1: "search,replace,|,cut,copy,paste,|,undo,redo,|,image,|,link,unlink,charmap,emoticon,codeblock,|,bold,italic,|,numlist,bullist,formatselect,|,code,fullscreen",
            theme_advanced_buttons2: "",
            theme_advanced_buttons3: ""
            });
     </script>
</head>
<body>
    <div id="header" role="banner">
        <h1><%=Html.ActionLink(T("Project Orchard").ToString(), "Index", new { Area = "", Controller = "Home" })%></h1>
        <div id="site"><%=Html.ActionLink(T("Your Site").ToString(), "Index", new { Area = "", Controller = "Home" })%></div>
        <% if (Model.CurrentUser != null) { //todo: (heskew) localize the string format "User: <username> | a:logoff"
            %><div id="login"><%=T("User")%>: <%=H(Model.CurrentUser.UserName)%> | <%=Html.ActionLink(T("Logout").ToString(), "LogOff", new { Area = "", Controller = "Account" }) %></div><%
            } %>
    </div>
    <div id="content">
        <div id="navshortcut"><a href="#navigation"><%=T("Skip to navigation") %></a></div>
        <div id="main" role="main">
            <div class="wrapper">
                <% Html.RenderPartial("Messages", Model.Messages); %>