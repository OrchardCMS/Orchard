<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Pages.Models.Page>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<div class="metadata">
    <div class="posted"><%: T("Published by {0}", Model.Creator != null ? Model.Creator.UserName : T("nobody(?)").ToString())%></div>
</div>