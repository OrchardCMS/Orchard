<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>

<%
    Html.RegisterStyle("yui.css", "1");
    Html.RegisterStyle("site.css", "1");
    Html.RegisterStyle("blog.css", "1");
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

<div id="hd" role="banner">
    <div class="yui-g" id="branding">
        <div class="yui-u first">
            <h1><a href="/" title="Go to Home"><%: Html.SiteName() %></a></h1>
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
    <div class="menucontainer">
            <% Html.Include("menu"); %>
        </div>
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
<%Html.Zone("sidebar");%>
<ul>
<li>
    <%Html.Zone("search"); %>
</li>
<li>
    <%Html.Zone("sideBarZone1"); %>
</li>
<li>
    <%Html.Zone("sideBarZone2"); %>
</li>
</ul>
</div>

    </div>
  </div>
  <%-- End Content --%>
  <%Html.Zone("footer");%>
  <% Html.Include("Footer"); %>
</div>
