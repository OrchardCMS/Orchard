<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="summary">
    <div class="related">
        <a href="<%: Url.Blog(Model.Item.Slug) %>" title="<%: T("View") %>"><%: T("View") %></a><%: T(" | ")%>
        <a href="<%: Url.BlogForAdmin(Model.Item.Slug) %>" title="<%: T("Edit Posts") %>"><%: T("Edit Posts")%></a><%: T(" | ")%>
        <a href="<%: Url.BlogPostCreate(Model.Item) %>" title="<%: T("New Post") %>"><%: T("New Post") %></a><%: T(" | ")%>
        <a href="<%: Url.BlogEdit(Model.Item.Slug) %>" title="<%: T("Settings") %>"><%: T("Settings") %></a><%: T(" | ")%>
        <%-- todo: (heskew) this is waaaaa too verbose. need template helpers for all ibuttons --%>
        <% using (Html.BeginFormAntiForgeryPost(Url.BlogRemove(Model.Item.Slug), FormMethod.Post, new { @class = "inline link" })) { %>
        <button type="submit" class="linkButton" title="<%: T("Remove") %>"><%: T("Remove") %></button><%
        } %>
    </div>
    <div class="properties">
        <h3><%: Html.Link(Model.Item.Name, Url.BlogForAdmin(Model.Item.Slug)) %></h3>
        <p><% Html.Zone("meta");%></p>
        <%--<p>[list of authors] [modify blog access]</p>--%>
        <p><%: Model.Item.Description %></p>              
    </div>
    <div style="clear:both;"></div>
</div>