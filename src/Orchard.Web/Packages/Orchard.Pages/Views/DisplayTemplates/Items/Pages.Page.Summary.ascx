<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<h3><a href="<%=Url.Action("Item", "Page", new { pageSlug = Model.Item.Slug }) %>"><%=Html.Encode(Model.Item.Title) %></a></h3>
<div class="content"><%=Model.Item.As<BodyAspect>().Text ?? "<p><em>there's no content for this blog post</em></p>" %></div>