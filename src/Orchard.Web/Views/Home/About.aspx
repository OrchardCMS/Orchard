<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<HomeAboutViewModel>" %>

<%@ Import Namespace="Orchard.Web.Controllers" %>
<asp:Content ID="aboutTitle" ContentPlaceHolderID="TitleContent" runat="server">
    About Us
</asp:Content>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        About</h2>
    <p>
        Put content here.
    </p>
    <%using (Html.BeginForm()) { %>
    <%
        foreach (var foo in Model.Foos) {%>
    <%=Html.TextArea("Foos[" + foo.Name + "].Content", foo.Content)%>
    <%
        }%>
        <input type="submit" />
        <%
      }%>
</asp:Content>
