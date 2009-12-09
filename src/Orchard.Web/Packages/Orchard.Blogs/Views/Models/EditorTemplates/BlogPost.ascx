<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Models.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%=Html.EditorZonesAny() %>