<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h1 class="withActions">
    <a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>"><%=Html.TitleForPage(Model.Item.Name) %></a>
</h1>

<%--<form>
<div class="actions bulk">
<fieldset>
    <label for="filterResults"><%=_Encoded("Filter:")%></label>
        <select id="filterResults" name="">
            <option value="">All Posts</option>
            <option value="">Published Posts</option>
        </select>
    <input class="button" type="submit" name="submit.Filter" value="<%=_Encoded("Apply") %>"/>
</fieldset>
</div>
</form>--%>

<div class="actions"><a href="<%=Url.BlogPostCreate(Model.Item) %>" class="add button primaryAction"><%=_Encoded("New Post")%></a></div>


<% Html.Zone("primary");
   Html.ZonesAny(); %>