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
</head>
<body><%
    Html.ZoneBody("body"); %>
</body>
</html>