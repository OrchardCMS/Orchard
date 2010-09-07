<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%
    Html.RegisterStyle("jquery.switchable.css");
    Html.RegisterFootScript("jquery.switchable.js");
    var cssClass = string.Format("{0} switchable", Model);
    
 %>
 <%:cssClass %>