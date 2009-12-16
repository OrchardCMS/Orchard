<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
    <title><%=Html.Title() %></title>
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
