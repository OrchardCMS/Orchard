<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>

<div class="bloginfo">
    <h1><%=Html.TitleForPage(Model.Item.Name) %></h1>
    <div class="manage">
        <a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit"><%=_Encoded("Edit") %></a>
    </div>
    <div class="blogdescription">
        <p><%=Html.Encode(Model.Item.Description) %></p>
    </div>
</div>


<% Html.Zone("primary");
   Html.ZonesAny(); %>
   
   
 