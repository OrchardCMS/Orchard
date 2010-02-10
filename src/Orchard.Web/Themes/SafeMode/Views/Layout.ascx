<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%  //todo: (heskew) this should really be using the IResourceManager if it's to be a theme
    var siteCss = ResolveUrl("../Styles/site.css");
    Model.Zones.AddAction("head:styles", html =>
      html.ViewContext.Writer.Write(@"<link rel=""stylesheet"" type=""text/css"" href="""+siteCss+@"""/>")); %>
<div id="header">
	<div id="branding"><h1>Welcome to Orchard</h1></div>
</div>
<div id="main">
    <%Html.ZoneBody("content"); %>
</div>