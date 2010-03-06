<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<div id="ft" role="contentinfo">
    <div id="innerft" class="yui-g">
        <div class="yui-u first">
            <%Html.Zone("User1"); %>
        </div>
        <div class="yui-g">
            <div class="yui-u first">
                <%Html.Zone("User2"); %>
            </div>
            <div class="yui-u">
                <%Html.Zone("User3"); %>
            </div>
        </div>
    </div>
</div>
