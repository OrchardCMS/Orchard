<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<WidgetEditViewModel>" %>
<%@ Import Namespace="Futures.Widgets.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h1><%=Html.TitleForPage(T("Edit Widget").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <%= Html.EditorForItem(m => m.Widget) %>
    <fieldset>
        <%= Html.HiddenFor(m => m.ReturnUrl) %>
        <input class="button primaryAction" type="submit" name="submit.Save" value="Save"/>
    </fieldset>
<%} %>
