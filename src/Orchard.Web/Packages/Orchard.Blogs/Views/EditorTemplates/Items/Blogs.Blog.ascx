<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorModel<Blog>>" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.Title(Model.Item.Name); %>
<%=Html.EditorZone("primary") %>
<%=Html.EditorZonesAny() %>