<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<div class="footer group">
    <ul class="group">
        <li>&copy;2010 <%= Html.Encode(Html.SiteName()) %>. All rights reserved.</li>
        <li><a href="http://www.orchardproject.net" title="Orchard Project">Powered by Orchard</a></li>
    </ul>
</div>
