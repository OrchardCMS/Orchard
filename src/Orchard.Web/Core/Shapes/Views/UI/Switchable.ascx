<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="Orchard.UI.Resources" %>
<%
    // todo: use Style.Require and Script.Require when this is converted to use a base Orchard view type.
    var rm = Html.Resolve<IResourceManager>();
    rm.Require(new RequireSettings { ResourceType = "stylesheet", ResourceName = "Switchable" });
    rm.Require(new RequireSettings { ResourceType = "script", ResourceName = "Switchable", Location = ResourceLocation.Foot });
    var cssClass = string.Format("{0} switchable", Model);
    
 %>
 <%:cssClass %>