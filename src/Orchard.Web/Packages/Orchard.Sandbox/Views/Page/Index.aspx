<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<h1><%=Html.TitleForPage(T("Sandbox Pages").ToString()) %></h1>
<p><%=Html.ActionLink(T("Create new page").ToString(), "create") %></p>
<%=Html.UnorderedList(Model.Pages, (sp, i) => Html.DisplayForItem(sp).ToHtmlString(), "pages contentItems") %>