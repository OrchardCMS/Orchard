<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"
%><!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
    <title><%=Html.Title() %></title><%
     Html.Zone("head", ":metas :styles :scripts"); %>
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
<body><%
    Html.ZoneBody("body"); %>
</body>
</html>