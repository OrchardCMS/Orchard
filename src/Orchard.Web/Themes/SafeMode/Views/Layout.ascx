<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.UI.Resources"%>
<%@ Import Namespace="System.IO"%>
<%@ Import Namespace="Orchard.Mvc.ViewEngines"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%  //todo: (heskew) this should really be using the IResourceManager if it's to be a theme especially for the jquery dep (w/out needing to copy into this theme...)
    var jquery = ResolveUrl("~/Modules/Orchard.Themes/Scripts/jquery-1.4.2.js");
    Model.Zones.AddAction("head:scripts", html =>
      html.ViewContext.Writer.Write(@"<script type=""text/javascript"" src=""" + jquery + @"""></script>"));
    var basejs = ResolveUrl("~/Modules/Orchard.Themes/Scripts/base.js");
    Model.Zones.AddAction("content:after", html =>
      html.ViewContext.Writer.Write(@"<script type=""text/javascript"" src=""" + basejs + @"""></script>"));
    var setupjs = ResolveUrl("~/Modules/Orchard.Setup/Scripts/setup.js");
    Model.Zones.AddAction("content:after", html =>
      html.ViewContext.Writer.Write(@"<script type=""text/javascript"" src=""" + setupjs + @"""></script>"));
    var siteCss = ResolveUrl("../Styles/site.css");
    Model.Zones.AddAction("head:styles", html =>
      html.ViewContext.Writer.Write(@"<link rel=""stylesheet"" type=""text/css"" href=""" + siteCss + @"""/>"));
    var ie6Css = ResolveUrl("../Styles/ie6.css");
    Model.Zones.AddAction("head:styles", html =>
      html.ViewContext.Writer.Write(@"<!--[if lte IE 6]><link rel=""stylesheet"" type=""text/css"" media=""screen, projection"" href=""" + ie6Css + @"""/><![endif]-->")); %>
<div id="header">
	<div id="branding"><h1>Welcome to Orchard</h1></div>
</div>
<div id="main">
    <%Html.ZoneBody("content"); %>
</div>