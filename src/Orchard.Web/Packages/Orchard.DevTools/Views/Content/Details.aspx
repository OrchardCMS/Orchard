<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.DevTools.ViewModels.ContentDetailsViewModel>" %>

<%@ Import Namespace="System.Reflection" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>TwoColumns</title>
    <link href="<%=ResolveUrl("~/Content/Site2.css") %>" rel="stylesheet" type="text/css" />
</head>
<body>
    <div id="header">
        <div id="innerheader">
            <% Html.Include("header"); %>
        </div>
    </div>
    <div id="page">
        <div id="sideBar">
            <% Html.Include("Navigation"); %>
        </div>
        <div id="main">
            <h3>
                Content Item</h3>
            <p>
                Id:
                <%=Model.Item.Id %></p>
            <p>
                ContentType:
                <%=Model.Item.ContentType%></p>
            <h3>
                Content Item Parts</h3>
            <ul>
                <%foreach (var partType in Model.PartTypes.OrderBy(x => x.Name)) {%>
                <li><span style="font-weight: bold;">
                    <%if (partType.IsGenericType) {%><%=Html.Encode(partType.Name +" "+partType.GetGenericArguments().First().Name)%></span>
                    <%=Html.Encode(" (" + partType.GetGenericArguments().First().Namespace + ")")%><%}
                      else {%><%=Html.Encode(partType.Name)%></span>
                    <%=Html.Encode( " (" + partType.Namespace + ")")%><%
                                                                        }%>
                    <ul style="margin-left: 20px">
                        <%foreach (var prop in partType.GetProperties().Where(x => x.DeclaringType == partType)) {
                              var value = prop.GetValue(Model.Locate(partType), null);%>
                        <li style="font-weight: normal;">
                            <%=Html.Encode(prop.Name) %>:
                            <%=Html.Encode(value) %>
                            <ul style="margin-left: 20px">
                                <%if (value == null || prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string)) { }
                                  else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)) {
                                      foreach (var item in value as IEnumerable) {
                                %>
                                <li><%=Html.Encode(item.GetType().Name) %>:<%=Html.Encode(item) %></li>
                                <%
                                    }

                                  }
                                  else {%>
                                <%foreach (var prop2 in value.GetType().GetProperties().Where(x => x.GetIndexParameters().Count() == 0)) {%>
                                <li>
                                    <%=Html.Encode(prop2.Name)%>
                                    <%=Html.Encode(prop2.GetValue(value, null))%></li>
                                <%} %>
                                <%} %>
                            </ul>
                        </li>
                        <%} %>
                    </ul>
                </li>
                <%}%>
            </ul>
            <h3>
                Editors</h3>
            <ul>
                <%foreach (var editor in Model.Editors) {%>
                <li><span style="font-weight: bold">
                    <%=Html.Encode(editor.Prefix) %></span>
                    <%=Html.Encode(editor.Model.GetType().Name) %>
                    (<%=Html.Encode(editor.Model.GetType().Namespace) %>)
                    <div style="margin-left: 20px; border: solid 1px black;">
                        <%=Html.EditorFor(x=>editor.Model, editor.TemplateName, editor.Prefix) %>
                    </div>
                </li>
                <%                      
                    }%>
            </ul>
        </div>
        <div id="footer">
            <% Html.Include("footer"); %>
        </div>
    </div>
</body>
</html>
