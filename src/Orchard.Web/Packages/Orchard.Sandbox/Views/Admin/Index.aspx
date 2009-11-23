<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
        <h2>Wiki Admin</h2>
        <p><%=Html.ActionLink("Create", "Create") %></p>
    </div>
    <% Html.EndForm(); %>
<% Html.Include("Footer"); %>