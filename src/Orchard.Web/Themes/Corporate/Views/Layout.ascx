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
                <%: Html.SiteName() %></div>
        </div>
        <% Html.Zone("menu"); %>
    </div>
</div>

<div id="shell">  
    <div id="container">
        <div id="content">

            <%-- Content Hero --%>
	        <div class="main-box">
                <div class="top">
                   <img src="<%: Url.Content("~/Themes/Corporate/Content/Images/content-top.png") %>" /></div>
	                <div class="content">
                        <div class="subpage group">
                            <div class="sub-content">
                                <% Html.ZoneBody("content"); %>
                            </div>
                            <div class="sidebar">
                                <%-- START Blog Sidebars --%>
		                        <%Html.Zone("sidebar");%>
		                        <%-- END Blog Sidebars --%>

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
	            <div class="bottom">
                   <img src="<%: Url.Content("~/Themes/Corporate/Content/Images/content-bottom.png") %>" /></div>
	        </div>

        </div>
    </div>
</div>
	
	<%-- Footer --%>
	<% Html.Zone("footer"); %>