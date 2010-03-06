<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%  //todo: (heskew) this should really be using the IResourceManager if it's to be a theme especially for the jquery dep (w/out needing to copy into this theme...)
    var jquery = ResolveUrl("~/Core/Themes/Scripts/jquery-1.4.1.js");
    Model.Zones.AddAction("head:scripts", html =>
      html.ViewContext.Writer.Write(@"<script type=""text/javascript"" src=""" + jquery + @"""></script>"));
    var basejs = ResolveUrl("~/Core/Themes/Scripts/base.js");
    Model.Zones.AddAction("content:after", html =>
      html.ViewContext.Writer.Write(@"<script type=""text/javascript"" src=""" + basejs + @"""></script>"));
    var siteCss = ResolveUrl("../Styles/site.css");
    Model.Zones.AddAction("head:styles", html =>
      html.ViewContext.Writer.Write(@"<link rel=""stylesheet"" type=""text/css"" href=""" + siteCss + @"""/>")); %>
<div id="header">
	<div id="branding"><h1>Welcome to Orchard</h1></div>
</div>
<div id="main">
    <%Html.ZoneBody("content"); %>
</div>