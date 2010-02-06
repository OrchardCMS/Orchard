<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% Html.AddTitleParts(Model.Item.Title); %>
<div class="sections">
    <div class="primary"><%
        Html.Zone("primary");
        Html.ZonesExcept("secondary"); %>
    </div>
    <div class="secondary">
        <% Html.Zone("secondary");%>
        <fieldset>
            <input class="button" type="submit" name="submit.Save" value="<%=_Encoded("Save") %>"/>
            <% if (Model.IsDraft) { %>
            <%=Html.ActionLink(T("Discard Draft").ToString(), "DiscardDraft", new { Area = "Orchard.Pages", Controller = "Admin", Model.Item.Id }, new { @class = "button" })%>
            <% } %>
        </fieldset>
    </div>
</div>