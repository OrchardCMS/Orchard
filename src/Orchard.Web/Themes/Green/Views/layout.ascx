<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<%
    Html.RegisterStyle("yui.css");
    Html.RegisterStyle("site.css");
    %>

<div id="doc4" class="yui-t6">

<% Html.Zone("header"); Html.Zone("menu"); %>

<div id="hd" role="banner">
    <div class="yui-g" id="branding">
        <div class="yui-u first">
            <h1><a href="/" title="Go to Home"><span class="displayText">AdventureWorks</span></a></h1>
        </div>
        <div class="yui-u">
            <div id="logIn">
                <%-- todo:(nheskew) this will need to all go in the header zone (user widget) --%>
                <% Html.Include("user"); %>
            </div>
        </div>
    </div>
    <%--Top Navigation--%>
    <%-- todo:(nheskew) this will need to be a generated menu --%>
    <% Html.Include("menu"); %>
</div>

<%-- Begin Page Content --%>
  <div id="bd" role="main">
    <div id="yui-main">
      <div id="mainColumn" class="yui-b">
        <div class="yui-g coreWidget">
          <%--Main Content--%>
          <%Html.ZoneBody("content");%>
        </div>
      </div>
    </div>
    <div id="subColumn1" class="yui-b">
      <%--Start widgets--%>
      This was our old widget zone
    </div>
  </div>
  <%-- End Content --%>
  <%Html.Zone("footer");%>
  <% Html.Include("footer"); %>
</div>
