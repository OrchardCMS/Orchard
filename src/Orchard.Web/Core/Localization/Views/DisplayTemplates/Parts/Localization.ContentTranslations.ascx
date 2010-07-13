<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Localization.ViewModels.ContentLocalizationsViewModel>" %>
<%@ Import Namespace="Orchard.Core.Localization.Models" %>
<%@ Import Namespace="Orchard.ContentManagement" %>
<%
    Html.RegisterStyle("base.css"); %>
<% if (Model.Localizations.Count() > 0 || Model.CanLocalize) { %>
<div class="content-localization"><%
    if (Model.Localizations.Count() > 0) { %>
    <%--//todo: need this info in the view model--%>
    <div class="content-localizations"><%:Html.UnorderedList(Model.Localizations, (c, i) => Html.ItemDisplayLink(c.ContentItem.As<Localized>().Culture != null ? c.ContentItem.As<Localized>().Culture.Culture : "[site's default culture]", c), "localizations") %></div><%
    }
    if (Model.CanLocalize) { %>
    <div class="add-localization"><%:Html.ActionLink(T("+ New translation").Text, "translate", "admin", new { area = "Localization", id = Model.Id }, null)%></div><%
    } %>
</div><%
} %>