<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<div class="manage"><a href="<%=Url.Action("Edit", "Page", new {pageSlug = Model.Item.Slug}) %>" class="ibutton edit">edit</a></div>
<h1><%=Html.TitleForPage(Model.Item.Title)%></h1>
<div class="metadata">
    <% if (Model.Item.Creator != null)
       { 
       %><div class="posted">Posted by <%=Html.Encode(Model.Item.Creator.UserName)%> at <%=Model.Item.Published%></div><%
       } %>
</div>
<% Html.Zone("primary"); %>
<% Html.ZonesAny(); %>