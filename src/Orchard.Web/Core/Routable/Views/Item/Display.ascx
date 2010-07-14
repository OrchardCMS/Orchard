<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Routable.ViewModels.RoutableDisplayViewModel>" %>
<% Html.AddTitleParts(Model.Routable.Item.Title); %>
<%: Html.DisplayForItem(m=>m.Routable) %>
