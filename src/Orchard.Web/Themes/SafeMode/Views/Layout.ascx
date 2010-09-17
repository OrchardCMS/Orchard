<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>
<%@ Import Namespace="Orchard.UI.Resources" %>
<%
    Script.Require("jQuery", "1.4.2");
    Script.Require("Theme_Base");
    Script.Require("Setup");
    Style.Require("SafeMode");
    RegisterLink(new LinkEntry { Condition = "lte IE 6", Rel = "stylesheet", Type="text/css", Href = ResolveUrl("../Styles/ie6.css")}.AddAttribute("media", "screen, projection"));
%>
<div id="header">
    <div id="branding">
        <h1>
            Welcome to Orchard</h1>
    </div>
</div>
<div id="main">
    <%: Display(Model.Content) %>
</div>
