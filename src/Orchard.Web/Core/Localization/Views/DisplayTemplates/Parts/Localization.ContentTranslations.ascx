<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Localization.ViewModels.ContentLocalizationsViewModel>" %>
<%
Html.RegisterStyle("base.css");
if (Model.Localizations.Count() > 0) { %>
<div class="content-localization">
    <%--//todo: need this info in the view model--%>
    <div class="content-localizations"><%:Html.UnorderedList(Model.Localizations, (c, i) => Html.ItemDisplayLink(c.Culture.Culture, c), "localizations") %></div>
</div><%
} %>