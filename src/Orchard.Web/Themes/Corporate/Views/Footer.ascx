<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
    <div id="footer">
        <div id="footer-content">
            <div id="footernav">
                <div>
                    &copy;2010 <%= Html.Encode(Html.SiteName()) %>. All rights reserved.</div>
            </div>
            <div id="disclaimer">
                *Actual results will vary based on individual situations and negotiations. Success
                in our program is highly dependent on your ability to save a specified amount consistently
                each month. At program completion if your total debt reduction is less than 3 times
                the Service Fees you have paid to us, we will refund a portion of those Service
                fees. The amount of the refund will be calculated so that the amount of Service
                Fees we retain is equal to only 1/3 of your total Debt Reduction. Please keep in
                mind that Retainer Fees are generally non-refundable.
            </div>
        </div>
    </div>