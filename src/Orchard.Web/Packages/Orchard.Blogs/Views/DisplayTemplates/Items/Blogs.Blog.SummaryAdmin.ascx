<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><%=Html.Link(Html.Encode(Model.Item.Name), Url.BlogForAdmin(Model.Item.Slug)) %></h2>
<div class="meta">
    <%=Html.Link(_Encoded("{0} post{1}", Model.Item.PostCount, Model.Item.PostCount == 1 ? "" : "s").ToString(), Url.BlogForAdmin(Model.Item.Slug))%>
    | <%=Html.Link(_Encoded("?? comments").ToString(), "") %>
</div>
<%--<p>[list of authors] [modify blog access]</p>--%>
<p><%=Html.Encode(Model.Item.Description) %></p>
<ul class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <li class="construct">
        <a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>" class="ibutton blog"><%=_Encoded("Manage Blog") %></a>
        <a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit"><%=_Encoded("Edit Blog")%></a>
        <a href="<%=Url.Blog(Model.Item.Slug) %>" class="ibutton view"><%=_Encoded("View Blog")%></a>
        <a href="<%=Url.BlogPostCreate(Model.Item.Slug) %>" class="ibutton add page"><%=_Encoded("New Post")%></a>
    </li>
    <li class="destruct">
        <% using (Html.BeginFormAntiForgeryPost(Url.BlogDelete(Model.Item.Slug))) { %>
            <fieldset>
                <input type="image" src="<%=Html.ThemePath("/styles/images/remove.png") %>" alt="<%=_Encoded("Remove Blog") %>" title="<%=_Encoded("Remove Blog") %>" class="ibutton image remove" />
            </fieldset><%
        } %>
    </li>
</ul>