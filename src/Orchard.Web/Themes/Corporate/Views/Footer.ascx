<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
    <div id="footer">
        <div id="footer-content">
            <div id="footernav">
                <div>
                    &copy;2010 <%: Html.SiteName() %>. All rights reserved.</div>
            </div>
            <div id="disclaimer">
                *This is perfect for disclaimer content. You can use this content to let people know something important. It shows up on every page. You can even put some legal terms here. 
            </div>
        </div>
    </div>