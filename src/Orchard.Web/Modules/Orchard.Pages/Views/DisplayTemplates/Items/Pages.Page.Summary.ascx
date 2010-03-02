<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h3><a href="<%=Url.Action(T("Item").ToString(), "Page", new { slug = Model.Item.Slug }) %>"><%=Html.Encode(Model.Item.Title) %></a></h3>
<div class="content">
<% Html.Zone("primary", ":manage :metadata"); %>
</div>