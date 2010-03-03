<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="summary">
    <div class="properties">
        <h3><%=Html.Link(Html.Encode(Model.Item.Name), Url.BlogForAdmin(Model.Item.Slug)) %></h3>
        <p><% Html.Zone("meta");%></p>
        <%--<p>[list of authors] [modify blog access]</p>--%>
        <p><%=Html.Encode(Model.Item.Description) %></p>              
    </div>
    <div class="related">
        <a href="<%=Url.Blog(Model.Item.Slug) %>" title="<%=_Encoded("View") %>"><%=_Encoded("View") %></a><%=_Encoded(" | ")%>
        <a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>" title="<%=_Encoded("Edit Posts") %>"><%=_Encoded("Edit Posts")%></a><%=_Encoded(" | ")%>
        <a href="<%=Url.BlogPostCreate(Model.Item) %>" title="<%=_Encoded("New Post") %>"><%=_Encoded("New Post") %></a><%=_Encoded(" | ")%>
        <a href="<%=Url.BlogEdit(Model.Item.Slug) %>" title="<%=_Encoded("Settings") %>"><%=_Encoded("Settings") %></a><%=_Encoded(" | ")%>
        <%-- todo: (heskew) this is waaaaa too verbose. need template helpers for all ibuttons --%>
        <% using (Html.BeginFormAntiForgeryPost(Url.BlogDelete(Model.Item.Slug), FormMethod.Post, new { @class = "inline" })) { %>
        <button type="submit" class="linkButton" title="<%=_Encoded("Delete") %>"><%=_Encoded("Delete") %></button><%
        } %>
    </div>
    <div style="clear:both;"></div>
</div>