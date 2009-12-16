<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
    <title><%=Html.Title() %> - Safe Mode!</title>
    <% Html.RenderZone("metas"); %>
    <% Html.RenderZone("styles"); %>
    <% Html.RenderZone("scripts"); %>
</head>
<body>
    <% Html.RenderZone("document-first"); %>
    <% Html.RenderBody(); %>
    <% Html.RenderZone("document-last"); %>
</body>
</html>
