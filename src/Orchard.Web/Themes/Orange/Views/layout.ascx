<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%><%
Html.RegisterStyle("site.css");
 %>
<div class="page">
    <div id="header">
        <div id="title"><%=Html.TitleForPage(Html.SiteName()) %></div><%
        Html.Zone("header");
        Html.Zone("menu"); %>
        <%-- todo:(nheskew) this will need to all go in the header zone (user widget) --%>
        <% Html.Include("user"); %>
        <%-- todo:(nheskew) this will need to be a generated menu --%>
        <% Html.Include("menu"); %>
    </div>
    <div id="main"><%
        Html.ZoneBody("content");
%>        <div id="footer"><%
            Html.Zone("footer");
        %></div>
    </div>
</div>