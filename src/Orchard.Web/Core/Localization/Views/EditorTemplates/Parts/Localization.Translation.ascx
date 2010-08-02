<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Localization.ViewModels.EditLocalizationViewModel>" %>
<%
    Html.RegisterStyle("admin.css");
    if (Model.ContentItem.ContentItem.Id > 0 && Model.SelectedCulture != null && Model.ContentLocalizations.Localizations.Count() > 0) {
        Html.RegisterStyle("base.css");%>
<fieldset class="localization culture-selection">
    <fieldset class="culture-selected">
        <label for="SelectedCulture"><%:T("Content Localization")%></label>
        <div><%=T("This is the <em>{0}</em> variation of {1}.",
                            Html.Encode(Model.SelectedCulture),
                            Html.ItemEditLink(Model.MasterContentItem ?? Model.ContentItem))%></div>
           <%:Html.Hidden("SelectedCulture", Model.SelectedCulture)%>
    </fieldset><%
        if (Model.ContentLocalizations.Localizations.Count() > 0) {%>
    <dl class="content-localization">
        <dt><%:T("Other translations:")%></dt>
        <dd class="content-localizations">
            <%:Html.UnorderedList(Model.ContentLocalizations.Localizations, (c, i) => Html.ItemEditLink(c.Culture.Culture, c), "localizations")%>
        </dd>
    </dl><%
        }%>
</fieldset><%
    } %>