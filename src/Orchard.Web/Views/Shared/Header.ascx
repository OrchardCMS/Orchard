<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<div id="title">
    <h1>
        My MVC Application</h1>
</div>
<div id="logindisplay">
    <% Html.RenderPartial("LogOnUserControl"); %>
    <% Html.RenderPartial("ExtraUserControl"); %>
</div>
