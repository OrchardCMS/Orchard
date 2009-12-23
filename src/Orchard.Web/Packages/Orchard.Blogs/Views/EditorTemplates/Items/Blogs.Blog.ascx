<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorModel<Blog>>" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.AddTitleParts(Model.Item.Name); %>
<%=Html.EditorZone("primary") %>
<%=Html.EditorZonesAny() %>