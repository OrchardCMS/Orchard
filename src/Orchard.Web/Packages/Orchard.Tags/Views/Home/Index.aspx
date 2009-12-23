<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TagsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h2>Tags</h2>
    <%=Html.UnorderedList(
        Model.Tags,
        (t, i) => Html.ActionLink(
            t.TagName,
            "Search",
            new { tagName = t.TagName },
            new { @class = "" /* todo: (heskew) classify according to tag use */ }
            ).ToHtmlString(),
        "tagCloud")
         %>
</asp:Content>