$(document).ready(function () {
    // Pass anti-forgery token through in the header of all ajax requests
    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
        var verificationTokenField = $("input[name='__RequestVerificationToken']").first();
        if (verificationTokenField) {
            jqXHR.setRequestHeader("X-Request-Verification-Token", verificationTokenField.val());
        }
    });
});
