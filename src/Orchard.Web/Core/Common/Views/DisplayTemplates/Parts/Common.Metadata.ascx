<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.ViewModels.CommonMetadataViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.Extensions" %><%
if (Model.Creator != null) { %>
<div class="metadata">
    <div class="posted"><%: T("Published by {0} {1}", Model.Creator.UserName, Html.PublishedWhen(Model, T))%></div>
</div><%
} %>