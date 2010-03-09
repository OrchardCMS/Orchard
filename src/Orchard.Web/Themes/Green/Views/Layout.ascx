<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<%
    Html.RegisterStyle("yui.css");
    Html.RegisterStyle("site.css");
    Html.RegisterStyle("blog.css");
    %>

<script type="text/javascript"> 
$(document).ready(function(){
$(".collapsible").click(function() {
$(this).next().slideToggle(600);
return false;
 });
}); 
</script>

<div id="doc4" class="yui-t6">

<% Html.Zone("header"); Html.Zone("menu"); %>

<div id="hd" role="banner">
    <div class="yui-g" id="branding">
        <div class="yui-u first">
            <h1><a href="/" title="Go to Home"><%=Html.Encode(Html.SiteName()) %></a></h1>
        </div>
        <div class="yui-u">
            <div id="logIn">
                <%-- todo:(nheskew) this will need to all go in the header zone (user widget) --%>
                <% Html.Include("User"); %>
            </div>
        </div>
    </div>
    <%--Top Navigation--%>
    <%-- todo:(nheskew) this will need to be a generated menu --%>
    <% Html.Include("Menu"); %>
</div>

<%-- Begin Page Content --%>
  <div id="bd" role="main">
    <div id="yui-main">
      <div id="maincolumn" class="yui-b">
        <div class="yui-g">
          <%--Main Content--%>
          <%Html.ZoneBody("content");%>
        </div>
      </div>
    </div>
    <div id="subcolumn" class="yui-b">
    
<div>
<ul>
<li>
    <%Html.Zone("Widget"); %>
</li>
<li>
    <%Html.Zone("Widget1"); %>
</li>
</ul>
</div>

    </div>
  </div>
  <%-- End Content --%>
  <%Html.Zone("footer");%>
  <% Html.Include("Footer"); %>
</div>
