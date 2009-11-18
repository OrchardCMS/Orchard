<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<!DOCTYPE html>
<html>
<head>
    <title>Index2</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
    <div class="yui-g">
        <h2>Blogs Admin</h2>
    </div>
    <% Html.Include("Footer"); %>
</body>
</html>
