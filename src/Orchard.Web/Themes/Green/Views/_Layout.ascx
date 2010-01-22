<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<%
    Html.RegisterStyle("yui.css");
    Html.RegisterStyle("site.css");
    Html.RegisterStyle("blog.css");
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
 <h3>Sidebar</h3>
	<ul>
		<li><h4>Item 1</h4></li>
		<li><p class="small">Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor vel nunc molestie laoreet. Curabitur vitae elit et massa consequat interdum. Curabitur blandit leo nec magna dictum vitae mollis tellus gravida. Morbi non condimentum neque. Suspendisse commodo condimentum feugiat. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.</p></li>
		
				<li><h4>Item 2</h4></li>
		<li><p class="small">Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor vel nunc molestie laoreet. Curabitur vitae elit et massa consequat interdum. Curabitur blandit leo nec magna dictum vitae mollis tellus gravida. Morbi non condimentum neque. Suspendisse commodo condimentum feugiat. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.</p></li>
	</ul>

    </div>
  </div>
  <%-- End Content --%>
  <%Html.Zone("footer");%>
  <% Html.Include("Footer"); %>
</div>
