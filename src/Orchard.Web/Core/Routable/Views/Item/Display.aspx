<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<Orchard.Core.Routable.ViewModels.RoutableDisplayViewModel>" %>
<% Html.AddTitleParts(Model.Routable.Item.Title); %>
<%=Html.DisplayForItem(m=>m.Routable) %>
