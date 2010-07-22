<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPostPart>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Core.Common.Extensions" %>
<%@ Import Namespace="Orchard.Core.Common.Models" %>
<%@ Import Namespace="Orchard.ContentManagement" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<h2><%: Html.Link(Model.Item.Title, Url.BlogPost(Model.Item)) %></h2>
<div class="meta"><%: Html.PublishedState(new CommonMetadataViewModel(Model.Item.As<CommonPart>()), T) %> | <%Html.Zone("meta");%></div>
<div class="content"><% Html.Zone("primary", ":manage :metadata");%></div>
