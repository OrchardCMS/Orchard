<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TenantsListViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.MultiTenancy.ViewModels"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<h1><%=Html.TitleForPage(T("List of Site's Tenants").ToString())%></h1>
<table class="items">
    <colgroup>
        <col id="Name" />
        <col id="Provider" />
        <col id="ConnectionString" />
        <col id="Prefix" />
    </colgroup>
    <thead>
        <tr>
            <td scope="col"><%=_Encoded("Name") %></td>
            <td scope="col"><%=_Encoded("Provider") %></td>
            <td scope="col"><%=_Encoded("ConnectionString") %></td>
            <td scope="col"><%=_Encoded("Prefix") %></td>
        </tr>
    </thead>
    <tbody><%
    foreach (var tenant in Model.TenantSettings) { %>
        <tr>
            <td><%= tenant.Name %></td>
            <td><%= tenant.DataProvider %></td>
            <td><%= tenant.DataConnectionString %></td>
            <td><%= tenant.DataTablePrefix %></td>
       </tr><%
    } %>
    </tbody>
</table>