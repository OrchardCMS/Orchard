<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Mvc.ViewModels.ContentItemViewModel>" %>
<h3><%:Html.ItemDisplayLink(Model.Item) %></h3>
<div class="content">
<% Html.Zone("primary", ":manage :metadata"); %>
</div>