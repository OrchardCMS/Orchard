<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.UI.Resources" %>
<%
    // todo: this is verbose because this view does not inherit from an Orchard base view and so does not
    // benefit from Style.Require().
    Html.Resolve<IResourceManager>().Require(new RequireSettings { Type = "stylesheet", Name = "Classic" });
    Html.Resolve<IResourceManager>().Require(new RequireSettings { Type = "stylesheet", Name = "Classic_Blog" });
%>

<%-- todo:(nheskew) this will need to be a generated menu --%>

<div id="wrapper">
<%--HTML.Include will render a div with an id="logindisplay" --%>
    <% Html.Include("User"); %>

<%--Top Navigation and branding--%>
<div id="headercontainer">
    <div id="header">
        <h1><%: Html.SiteName() %></h1>
        <div class="menucontainer">
            <% Html.Include("menu"); %>
        </div>
        <div class="clearBoth"></div>
    </div>
</div>

<div id="main">
        <div id="content">
        <%--Main Content--%>
        <%Html.ZoneBody("content");%>
        </div>
        <div id="sidebar">
            <% Html.Zone("search");
               Html.Zone("sidebar");%>
        </div>
        <%-- End Content --%>
        <% Html.Include("Footer"); %>
    </div>
</div>
