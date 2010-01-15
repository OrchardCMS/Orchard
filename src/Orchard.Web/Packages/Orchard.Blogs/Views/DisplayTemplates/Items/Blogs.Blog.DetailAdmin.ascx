<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h1 class="withActions">
    <a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>"><%=Html.TitleForPage(Model.Item.Name) %></a>
</h1>
<ul class="actions">
    <li class="construct">
        <a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit" title="<%=_Encoded("Edit Blog") %>"></a>
    </li>
    <li class="destruct">
        <% using (Html.BeginFormAntiForgeryPost(Url.BlogDelete(Model.Item.Slug), FormMethod.Post, new { @class = "inline" })) { %>
            <fieldset>
                <button type="submit" class="ibutton remove" title="<%=_Encoded("Remove Blog") %>"><%=_Encoded("Remove Blog") %></button>
            </fieldset><%
        } %>
    </li>
</ul>
<p><%=Html.Encode(Model.Item.Description) %></p>
<div class="actions"><a href="<%=Url.BlogPostCreate(Model.Item.Slug) %>" class="add button"><%=_Encoded("New Post")%></a></div>
<% Html.Zone("primary");
   Html.ZonesAny(); %>