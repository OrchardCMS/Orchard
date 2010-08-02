<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ListContentTypesViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<h1><%:Html.TitleForPage(T("Create New Content").ToString())%></h1>
<%:Html.UnorderedList(
    Model.Types,
    (ctd, i) => MvcHtmlString.Create(string.Format("<p>{0}</p>", Html.ActionLink(ctd.DisplayName, "Create", new { Area = "Contents", Id = ctd.Name }))),
    "contentTypes")%>