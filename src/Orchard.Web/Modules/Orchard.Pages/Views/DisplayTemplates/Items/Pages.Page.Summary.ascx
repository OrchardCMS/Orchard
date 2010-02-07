<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h3><a href="<%=Url.Action(T("Item").ToString(), "Page", new { slug = Model.Item.Slug }) %>"><%=Html.Encode(Model.Item.Title) %></a></h3>
<div class="content"><%=Model.Item.As<BodyAspect>().Text ?? T("<p><em>there's no content for this blog post</em></p>").ToString() %></div>