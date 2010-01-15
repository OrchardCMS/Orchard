<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%=Html.TitleForPage(Model.Item.Title)%></h1>
<div class="manage"><a href="<%=Url.Action(T("Edit").ToString(), "Admin", new {pageSlug = Model.Item.Slug}) %>" class="ibutton edit"><%=_Encoded("edit")%></a></div>
<div class="metadata">
    <div class="posted">Published by <%=Model.Item.Creator != null ? Html.Encode(Model.Item.Creator.UserName) : _Encoded("nobody(?)").ToString()%></div>
</div>
<% Html.Zone("primary");
   Html.ZonesAny(); %>

   
