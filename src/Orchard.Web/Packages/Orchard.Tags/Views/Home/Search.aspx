<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TagsSearchViewModel>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>List of contents tagged with <%= Model.TagName %></h2>
    <!-- TODO: (erikpo) The class being used for the lists "posts" should be made into something more generic like "contentItems" for the front end -->
    <%=Html.UnorderedList(Model.Contents, (c, i) => Html.ItemDisplayTemplate(c, "Summary").ToHtmlString(), "posts")%>
</asp:Content>