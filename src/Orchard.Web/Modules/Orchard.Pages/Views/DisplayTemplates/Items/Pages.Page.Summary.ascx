<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h3><a href="<%:Url.Action("Item", "Page", new { area = "Orchard.Pages", slug = Model.Item.Slug }) %>"><%: Model.Item.Title %></a></h3>
<div class="content">
<% Html.Zone("primary", ":manage :metadata"); %>
</div>