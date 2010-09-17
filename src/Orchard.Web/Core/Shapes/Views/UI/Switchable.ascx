<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="Orchard.UI.Resources" %>
<%
    // todo: use Style.Require and Script.Require when this is converted to use a base Orchard view type.
    var rm = Html.Resolve<IResourceManager>();
    rm.Require("stylesheet", "Switchable");
    rm.Require("script", "Switchable");
    var cssClass = string.Format("{0} switchable", Model);
    
 %>
 <%:cssClass %>