<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<%
    Html.RegisterStyle("site.css");
    
    Model.Zones.AddRenderPartial("header", "Header", Model);
    Model.Zones.AddRenderPartial("menu", "Menu", Model);
    Model.Zones.AddRenderPartial("footer", "Footer", Model);
%>

<div id="container">
    <!-- Header -->
    <% Html.Zone("header"); %>
	
	<!-- Main Menu -->
	<div id="nav">
        <% Html.Zone("menu"); %>
    </div>
	
	<!-- Main Content Area -->
	
	<div class="content-container">
	    <div class="content sub">
	        <div class="content-items group">
		        <div class="main">
			        <% Html.ZoneBody("content"); %>
		        </div>
		        <div class="sidebar">
                    <div class="side-block">
                        <% Html.Zone("sidebar-w1"); %>
                    </div>
                    <div class="side-block">
                        <% Html.Zone("sidebar-w2"); %>
                    </div>
                    <div class="side-block">
                        <% Html.Zone("sidebar-w3"); %>
                    </div>
		        </div>
		    </div>
	    </div>
	</div>
	
	<!-- Footer -->
	<% Html.Zone("footer"); %>
