<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%
  Html.RegisterStyle("site.css");
  Html.RegisterStyle("blog.css");
%>
<%--Top Navigation--%>
<%-- todo:(nheskew) this will need to be a generated menu --%>
<% Html.Include("menu"); %>
<div id="wrapper">
    <div id="header">
            <h1><%=Html.Encode(Html.SiteName()) %></h1>
        <%-- todo:(nheskew) this will need to all go in the header zone (user widget) --%>
        <% Html.Include("user"); %>
    </div>
    <div id="main">
        <div id="contentMain">

                    <%--Main Content--%>
                    <%Html.ZoneBody("content");%>

        </div>
        <div id="sideBar1">
            <ul>
                <li>
                    <h3>
                        Heading</h3>
                </li>
                <li>
                    <p class="small">
                        Paragraph - Small</p>
                </li>
            </ul>
        </div>
        <%-- End Content --%>
        <%Html.Zone("footer");%>
        <% Html.Include("footer"); %>
    </div>
</div>
