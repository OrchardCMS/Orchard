<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.EditTypeViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.Settings" %>
<div class="summary">
    <div class="properties">
        <h3><%:Model.DisplayName%></h3><%
        var creatable = Model.Settings.GetModel<ContentTypeSettings>().Creatable;
        if (creatable) { %>
        <p class="pageStatus"><%:Html.ActionLink(T("Create New {0}", Model.DisplayName).Text, "Create", new {area = "Contents", id = Model.Name}) %></p><%
        } %>
    </div>
    <div class="related"><%
        if (creatable) { %>
        <%:Html.ActionLink(T("List Items").ToString(), "List", new {area = "Contents", id = Model.Name})%><%:T(" | ")%><%
        } %>
        <%:Html.ActionLink(T("Edit").ToString(), "Edit", new {area = "Orchard.ContentTypes", id = Model.Name})%>
    </div>
</div>