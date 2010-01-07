<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<div class="manage"><a href="<%=Url.Action("Edit", "Admin", new {pageSlug = Model.Item.Slug}) %>" class="ibutton edit">edit</a></div>
<h1><%=Html.TitleForPage(Model.Item.Title)%></h1>
<div class="metadata">
    <% if (Model.Item.Creator != null)
       { 
       %><div class="posted">Published by <%=Html.Encode(Model.Item.Creator.UserName)%> </div><%
       } %>
</div>
<% Html.Zone("primary"); %>
<% Html.ZonesAny(); %>