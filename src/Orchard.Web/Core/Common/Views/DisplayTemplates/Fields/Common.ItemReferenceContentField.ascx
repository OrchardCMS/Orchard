<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemReferenceContentFieldDisplayViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<%= Html.ItemDisplayLink(Model.Item) %>