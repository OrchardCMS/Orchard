<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<%
    Html.RegisterStyle("site.css");
   
    Model.Zones.AddRenderPartial("header", "Header", Model);
    Model.Zones.AddRenderPartial("menu", "Menu", Model);
    Model.Zones.AddRenderPartial("footer", "Footer", Model);
%>

<div class="admin-bar group">
    <% Html.Include("User"); %>
</div>

<div class="header group">
    <%-- Main Menu --%>
    <div id="nav">
        <div class="brand group">
            <div class="title">
                <%=Html.Encode(Html.SiteName()) %></div>
        </div>
        <% Html.Zone("menu"); %>
    </div>
</div>

<div id="shell">  
    <div id="container">
        <div id="content">

            <%-- Home Hero --%>
	        <div class="main-box">
                <div class="top">
                    <img src="<%= Url.Content("~/Themes/Corporate/Content/Images/content-top.png") %>" /></div>
	                <div class="content group">

                        <% Html.Zone("home-hero-gallery"); %>

		                <% Html.Zone("home-hero"); %>

		            </div>
	            <div class="bottom">
                    <img src="<%= Url.Content("~/Themes/Corporate/Content/Images/content-bottom.png") %>" /></div>
	        </div>

	        <%-- Main Content Area --%>
	        <div class="content-container">
	            <div class="content">
	                <div class="content-items group">
		                <div class="item">
			                <% Html.ZoneBody("content"); %>
		                </div>
		                <div class="item note">
			                <% Html.Zone("home-headline"); %>
		                </div>
		            </div>
	            </div>
	        </div>

        </div>
    </div>
</div>
	
	<%-- Footer --%>
	<% Html.Zone("footer"); %>
