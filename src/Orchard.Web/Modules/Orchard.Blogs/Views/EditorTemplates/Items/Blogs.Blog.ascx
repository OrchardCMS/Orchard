<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPart>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.AddTitleParts(Model.Item.Name); %>
<% Html.Zone("primary"); %>
<% Html.ZonesAny(); %>
<fieldset><input class="button primaryAction" type="submit" value="<%: T("Add") %>" /></fieldset>