<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>

<%
    Html.RegisterStyle("site.css");
   
    Model.Zones.AddRenderPartial("header", "Header", Model);
    Model.Zones.AddRenderPartial("menu", "Menu", Model);
    Model.Zones.AddRenderPartial("footer", "Footer", Model);
%>

<div id="container">
    <%-- Header --%>
    <% Html.Zone("header"); %>
	
	<%-- Main Menu --%>
	<div id="nav">
        <% Html.Zone("menu"); %>
    </div>
    
    <%-- Home Hero --%>
	<div class="home-hero-container">
	    <div class="home-hero">
	        <%-- Init jQuery Slider --%>
            <script src="<%: Url.Content("~/Themes/Contoso/Scripts/easySlider.js") %>" type="text/javascript"></script>
            <script type="text/javascript">
                $(document).ready(function() {
                    $("#slider").easySlider({
                        prevText: '',
		                nextText: '',
		                speed: '1600'
                    });
            });
            </script>
           
            <div class="hero-gallery">
                <% Html.Zone("home-hero-gallery"); %>
            </div>
            
		    <div class="hero-content">
		        <% Html.Zone("home-hero"); %>
		    </div>
		</div>
	</div>
	
	<%-- Main Content Area --%>
	<div class="content-container">
	    <div class="content">
	        <div class="content-items group">
		        <div class="item">
			        <% Html.ZoneBody("content"); %>
		        </div>
		        <div class="item">
			        <% Html.Zone("home-headline"); %>
		        </div>
		    </div>
	    </div>
	</div>
	
	<%-- (Optional) Highlights Area --%>
    <% Html.Zone("highlights"); %>    
	
	<%-- Footer --%>
	<% Html.Zone("footer"); %>
</div>