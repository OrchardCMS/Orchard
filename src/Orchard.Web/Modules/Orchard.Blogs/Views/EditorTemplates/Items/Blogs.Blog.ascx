<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<BlogPart>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.AddTitleParts(Model.Item.Name); %>
<% Html.Zone("primary"); %>
<% Html.ZonesAny(); %>