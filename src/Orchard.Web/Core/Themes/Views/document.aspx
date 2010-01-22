<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"
%><!DOCTYPE html>
<html lang="en" class="static">
<head>
    <title><%=Html.Title() %></title><%
     //todo: (heskew) have resource modules that can be leaned on (like a jQuery module that knows about various CDNs and jQuery's version and min naming schemes)
     //todo: (heskew) this is an interim solution to inlude jQuery in every page and still allow that to be overriden in some theme by it containing a headScripts partial
     Model.Zones.AddRenderPartial("head:before", "HeadPreload", Model);
     Html.Zone("head", ":metas :styles :scripts"); %>
    <script type="text/javascript">document.documentElement.className="dyn";</script>
</head>
<body class="<%=Html.ClassForPage() %>"><%
    Html.ZoneBody("body"); %>
</body>
</html>
