<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%: Html.TitleForPage(T("Access Denied").ToString()) %></h1>
<p><%: T("You do not have permission to complete your request.")%></p>
