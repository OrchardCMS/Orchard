<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>

<p><%=Html.ActionLink("Contents", "Index", "Content") %></p>
