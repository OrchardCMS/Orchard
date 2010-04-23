<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%=Html.TitleForPage(T("Not found").ToString()) %></h1>
<p><%=_Encoded("The page you are looking for does not exist.")%></p>