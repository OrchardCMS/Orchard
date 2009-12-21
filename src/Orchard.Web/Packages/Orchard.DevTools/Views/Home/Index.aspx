<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>

<p><%=Html.ActionLink("Contents", "Index", "Content") %></p>
