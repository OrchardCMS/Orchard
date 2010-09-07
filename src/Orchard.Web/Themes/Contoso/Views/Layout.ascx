<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.DisplayManagement.Shapes" %>

<%
    Html.RegisterStyle("site.css", "1");

    Model.Header.Add(New.Header());
    Model.Header.Add(New.Footer());
    Model.Content.Add(Model.Metadata.ChildContent, "5");    
%>

<div id="container">
    <%-- Header --%>
    <%: Display(Model.Header)) %>
	
	<%-- Main Menu --%>
	<div id="nav">
        <%: Display(Model.Navigation) %>
        <%: Display(Model.Search) %>
    </div>
	
	<%-- Main Content Area --%>
	
	<div class="content-container">
	    <div class="content sub">
	        <div class="content-items group">
		        <div class="main">
			        <%: Display(Model.Content) %>
		        </div>
		        <div class="sidebar">
                    
                    <%-- START Blog Sidebars --%>
		            <%: Display(Model.Sidebar) %>
		            <%-- END Blog Sidebars --%>
                    
                    <%--these would be widgets--%>
                    <%--
                    <div class="side-block">
                        <% Html.Zone("sidebar-w1"); %>
                    </div>
                    <div class="side-block">
                        <% Html.Zone("sidebar-w2"); %>
                    </div>
                    <div class="side-block">
                        <% Html.Zone("sidebar-w3"); %>
                    </div>--%>
		        </div>
		    </div>
	    </div>
	</div>
	
	<%-- Footer --%>
	<%: Display(Model.Footer) %>
