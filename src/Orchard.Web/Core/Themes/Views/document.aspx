<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"
%><!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
    <title><%=Html.Title("site name") %></title><%
     Html.Zone("head", ":metas :styles :scripts"); %>
</head>
<body><%
    Html.ZoneBody("body"); %>
</body>
</html>
