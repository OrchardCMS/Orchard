<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Themes"%>
<%@ Import Namespace="Orchard.Extensions"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><%=Html.Link(Html.Encode(Model.Item.Title), Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Id)) %></h2>
<div class="meta"><%=Html.PublishedState(Model.Item) %> | <%Html.Zone("meta");%></div>
<div class="content"><%=Model.Item.As<BodyAspect>().Text ?? string.Format("<p><em>{0}</em></p>", _Encoded("there's no content for this blog post"))%></div>
<ul class="actions">
    <li class="construct">
        <a href="<%=Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Id) %>" class="ibutton edit" title="<%=_Encoded("Edit Post")%>"><%=_Encoded("Edit Post")%></a>
        <a href="<%=Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton view" title="<%=_Encoded("View Post")%>"><%=_Encoded("View Post")%></a><%
        if (Model.Item.ContentItem.VersionRecord.Published == false) { // todo: (heskew) be smart about this and maybe have other contextual actions - including view/preview for view up there ^^
            using (Html.BeginFormAntiForgeryPost(Url.BlogPostPublish(Model.Item.Blog.Slug, Model.Item.Id), FormMethod.Post, new { @class = "inline" })) { %>
        <fieldset>
            <button type="submit" class="ibutton publish" title="<%=_Encoded("Publish Post Now") %>"><%=_Encoded("Publish Post Now")%></button>
        </fieldset><%
            }
        } %>
    </li>
    <li class="destruct">
        <% using (Html.BeginFormAntiForgeryPost(Url.BlogPostDelete(Model.Item.Blog.Slug, Model.Item.Id), FormMethod.Post, new { @class = "inline" })) { %>
            <fieldset>
                <button type="submit" class="ibutton remove" title="<%=_Encoded("Remove Post") %>"><%=_Encoded("Remove Post") %></button>
            </fieldset><%
        } %>
    </li>
</ul>