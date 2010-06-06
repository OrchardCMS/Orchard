<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%=Html.TitleForPage(T("Dev Tools").ToString()) %></h1>
<p><%=Html.ActionLink(T("Contents").ToString(), "Index", "Content") %></p>
<p><%=Html.ActionLink(T("Metadata").ToString(), "Index", "Metadata") %></p>
<p><%=Html.ActionLink(T("Test Unauthorized Request").ToString(), "NotAuthorized", "Home")%></p>
