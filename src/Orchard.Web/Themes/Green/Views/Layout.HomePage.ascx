<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.UI.Resources" %>

<%
    Html.Resolve<IResourceManager>().Require(new RequireSettings { Type = "stylesheet", Name = "Green_YUI" });
    Html.Resolve<IResourceManager>().Require(new RequireSettings { Type = "stylesheet", Name = "Green" });
    Html.Resolve<IResourceManager>().Require(new RequireSettings { Type = "stylesheet", Name = "Green_Blog" });
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
            <h1><a href="/" title="Go to Home"><%: Html.SiteName() %></a></h1>
        </div>
        <div class="yui-u">
            <div id="logIn">
                <%-- todo:(nheskew) this will need to all go in the header zone (user widget) --%>
                <% Html.Include("User"); %>
                <%Html.Zone("search");%>
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
      <div id="maincolumn">
        <div class="yui-g">
          <%--Main Content--%>
          <%Html.ZoneBody("content");%>
           <div class="yui-u first subZone">
               <%Html.Zone("sideBarZone1"); %>
	    </div>
        <div class="yui-u subZone">
                <%Html.Zone("sideBarZone2"); %>
	    </div>

        </div>
      </div>
    </div>
  </div>
  <%-- End Content --%>
  <% Html.Include("Footer"); %>
</div>
