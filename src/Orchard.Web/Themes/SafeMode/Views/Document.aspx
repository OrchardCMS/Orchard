<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage" %>
<%@ Import Namespace="Orchard.Mvc.Html"
%><!DOCTYPE html>
<html lang="en" class="static">
<head>
    <title><%: Html.Title() %></title>
    <link rel="shortcut icon" type="image/x-icon" href="<%=ResolveUrl("../Content/orchard.ico") %>" />
    <%--<%
     //todo: (heskew) have resource modules that can be leaned on (like a jQuery module that knows about various CDNs and jQuery's version and min naming schemes)
     //todo: (heskew) this is an interim solution to inlude jQuery in every page and still allow that to be overriden in some theme by it containing a headScripts partial
     Html.Zone("head", ":metas :styles :scripts"); %>--%>
     <%:Display(Model.Head) %>
    <script type="text/javascript">document.documentElement.className="dyn";</script>
</head>
<body class="<%: Html.ClassForPage() %>">
<%: Display(Model.Body) %>
</body>
</html>
