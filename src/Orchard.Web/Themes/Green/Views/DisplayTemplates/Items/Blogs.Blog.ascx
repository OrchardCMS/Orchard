<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="bloginfo">
    <h1><%=Html.TitleForPage(Model.Item.Name) %></h1>
</div>
<% Html.Zone("primary", ":manage :metadata");
   Html.ZonesAny(); %>