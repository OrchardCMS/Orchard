<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TenantsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.MultiTenancy.ViewModels"%>
<h1><%=Html.TitleForPage(T("List of Site's Tenants").ToString())%></h1>
<table class="items">
    <colgroup>
        <col id="Name" />
        <col id="Data Provider" />
        <col id="ConnectionString" />
        <col id="TablePrefix" />
        <col id="RequestUrlHost" />
        <col id="RequestUrlPrefix" />
        <col id="TenantState" />
    </colgroup>
    <thead>
        <tr>
            <td scope="col"><%=_Encoded("Name") %></td>
            <td scope="col"><%=_Encoded("Data Provider") %></td>
            <td scope="col"><%=_Encoded("ConnectionString") %></td>
            <td scope="col"><%=_Encoded("Table Prefix") %></td>
            <td scope="col"><%=_Encoded("Request Url Host") %></td>
            <td scope="col"><%=_Encoded("Request Url Prefix") %></td>
            <td scope="col"><%=_Encoded("State") %></td>
        </tr>
    </thead>
    <tbody><%
    foreach (var tenant in Model.TenantSettings) { %>
        <tr>
            <td><%= tenant.Name %></td>
            <td><%= tenant.DataProvider %></td>
            <td><%= tenant.DataConnectionString %></td>
            <td><%= tenant.DataTablePrefix %></td>
            <td><%= tenant.RequestUrlHost %></td>
            <td><%= tenant.RequestUrlPrefix %></td>
            <td><%= tenant.State %></td>
       </tr><%
    } %>
    </tbody>
</table>
