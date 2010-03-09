<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BodyDisplayViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<div class="manage">
    <%=Html.ItemEditLinkWithReturnUrl(_Encoded("Edit").ToString(), Model.BodyAspect.ContentItem) %>
</div>