<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Modules.Packaging.ViewModels.PackagingIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<h1><%: Html.TitleForPage(T("Packages").ToString()) %></h1>
<p><%:Html.ActionLink("Update List", "Update") %> &bull; <%:Html.ActionLink("Edit Sources", "Sources") %></p>
<ul><%foreach (var item in Model.Modules) {%><li><%:item.AtomEntry.Title %></li><%
      }%></ul>

