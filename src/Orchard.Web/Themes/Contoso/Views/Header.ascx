<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<div class="header">
    <div class="brand group">
        <div class="title"><%=Html.Encode(Html.SiteName()) %></div>
        <% Html.Include("User"); %>
    </div>
</div>
