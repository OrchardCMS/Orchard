/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />

module Orchard.Azure.MediaServices.CloudVideoEdit {

    var requiredUploads: JQuery;
    var blocked: JQuery;
    var hasRequiredUploadsp: boolean;

    function getAllFilesCompleted(): boolean {
        var allFilesCompleted: boolean = true;

        requiredUploads.find("input[name$='.OriginalFileName'], input.sync-upload-input").each(function () {
            if ($(this).val() == "") {
                allFilesCompleted = false;
                return false;
            }
        });

        return allFilesCompleted;
    };

    function unblockIfComplete() {
        if (getAllFilesCompleted())
            (<any>blocked).unblock();
    }

    function uploadCompleted(sender, data) {
        var scope = $(sender).closest("[data-upload-accept-file-types]");
        var status = data.errorThrown && data.errorThrown.length > 0 ? data.errorThrown : data.textStatus;
        scope.find(".progress-bar").hide();
        scope.find(".progress-text").hide();
        scope.find(".cancel-upload").hide();
        scope.data("upload-isactive", false);

        switch (status) {
            case "error":
                alert("The upload of the selected file failed. One possible cause is that the file size exceeds the configured maxRequestLength setting (see: http://msdn.microsoft.com/en-us/library/system.web.configuration.httpruntimesection.maxrequestlength(v=vs.110).aspx). Also make sure the executionTimeOut is set to allow for enough time for the request to execute when debug=\"false\".");
                return;
            case "abort":
                return;
        }

        var temporaryFileName = data.result.temporaryFileName;
        var originalFileName = data.result.originalFileName;
        var fileSize = data.result.fileSize;

        scope.find("input[name$='.OriginalFileName']").val(originalFileName);
        scope.find("input[name$='.TemporaryFileName']").val(temporaryFileName);
        scope.find("input[name$='.FileSize']").val(fileSize);

        unblockIfComplete();
        $(sender).replaceWith("<span>Successfully uploaded video file '" + originalFileName + "'.</span>");
    }
     
    function initializeUpload(fileInput) {
        var scope = $(fileInput).closest("[data-upload-accept-file-types]");
        var acceptFileTypes: string = scope.data("upload-accept-file-types");
        var antiForgeryToken = requiredUploads.closest("form").find("[name='__RequestVerificationToken']").val();
        var cancelUpload = scope.find(".cancel-upload");

        fileInput.fileupload({
            autoUpload: false,
            acceptFileTypes: new RegExp(acceptFileTypes, "i"),
            type: "POST",
            url: scope.data("upload-fallback-url"),
            formData: {
                __RequestVerificationToken: antiForgeryToken
            },
            progressall: (e, data) => {
                var percentComplete = Math.floor((data.loaded / data.total) * 100);
                scope.find(".progress-bar").show().find('.progress').css('width', percentComplete + '%');
                scope.find(".progress-text").show().text("Uploading (" + percentComplete + "%)...");
            },
            done: function (e, data) {
                uploadCompleted(this, data);
            },
            fail: function (e, data) {
                uploadCompleted(this, data);
            },
            processdone: (e, data) => {
                scope.find(".validation-text").hide();
                scope.data("upload-isactive", true);
                cancelUpload.show();
                var xhr = data.submit();
                scope.data("xhr", xhr);
            },
            processfail: (e, data) => {
                scope.find(".validation-text").show();
            }
        });

        cancelUpload.on("click", e=> {
            e.preventDefault();

            if (confirm("Are you sure you want to cancel this upload?")) {
                var xhr = scope.data("xhr");
                xhr.abort();
            }
        });
    }

    export function initializeUploadProxied() {
        var scopeProxied = $(".upload-proxied").show();
        requiredUploads = scopeProxied.find(".required-uploads-group");
        blocked = scopeProxied.find(".edit-item-sidebar");
        hasRequiredUploadsp = requiredUploads.length > 0;

        if (hasRequiredUploadsp) {
            (<any>blocked).block({
                message: requiredUploads.data("block-description"),
                overlayCSS: {
                    backgroundColor: "#fff",
                    cursor: "default"
                },
                css: {
                    cursor: "default",
                    border: null,
                    width: null,
                    left: 0,
                    margin: "30px 0 0 0",
                    backgroundColor: null
                }
            });

            scopeProxied.find(".async-upload-file-input").each(function () {
                initializeUpload($(this));
            });

            window.onbeforeunload = e => {
                var hasActiveUploads = false;

                scopeProxied.find("[data-upload-accept-file-types]").each(function () {
                    if ($(this).data("upload-isactive") == true) {
                        hasActiveUploads = true;
                        return false;
                    }
                });

                if (hasActiveUploads)
                    e.returnValue = "There are uploads in progress. These will be aborted if you navigate away.";
            };

            scopeProxied.find(".sync-upload-input").on("change", e=> {
                unblockIfComplete();
            });

            unblockIfComplete();
        }

        scopeProxied.find("[data-prompt]").on("change", e => {
            var sender = $(e.currentTarget);
            
            if (!confirm(sender.data("prompt"))) {
                sender.val("");
            }
        });
    }
}