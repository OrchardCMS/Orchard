<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<AdminViewModel>" %>

<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<div id="doc3" class="yui-t2">
    <div id="hd" role="banner">
        <div class="yui-g">
            <div class="yui-u first">
                <h1>
                    <a href="<%=Url.Action("Index", new{Area="",Controller="Home"}) %>"><span class="displayText">
                        Project Orchard</span></a></h1>
                <%= Html.ActionLink("Your Site", "Index", new { Area = "", Controller = "Home" })%>
            </div>
            <div class="yui-u">
            <% if (Model.CurrentUser != null) { %>
                <div id="login">
                    User:
                    <%=Model.CurrentUser.UserName%>
                    | <%=Html.ActionLink("Logout", "LogOff", "Account", new {}, new{area=""})%></div>
                    <%} %>
            </div>
        </div>
    </div>
    <div id="bd" role="main">
        <div id="yui-main">
            <div class="yui-b">
                <% Html.RenderPartial("Messages", Model.Messages); %>
