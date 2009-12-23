<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%=Html.TitleForPage("Dev Tools") %></h1>
<p><%=Html.ActionLink("Contents", "Index", "Content") %></p>
