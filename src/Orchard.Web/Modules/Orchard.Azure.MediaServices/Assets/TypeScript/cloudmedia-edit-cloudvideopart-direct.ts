/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />
/// <reference path="Typings/moment.d.ts" />

module Orchard.Azure.MediaServices.CloudVideoEdit {
    var requiredUploads: JQuery;

    function uploadCompleted(sender: any, e: any, data: any) {
        var scope = $(sender).closest(".async-upload");
        var status = data.errorThrown && data.errorThrown.length > 0 ? data.errorThrown : data.textStatus;
        scope.find(".progress-bar").hide();
        scope.find(".progress-text").hide();
        scope.find(".progress-details").hide();
        scope.find(".status.preparing").hide();
        scope.find(".status.uploading").hide();

        switch (status) {
            case "error":
                cleanup(scope, data);
                alert("The upload of the selected file failed. You may try again after the cleanup has finished.");
                return;
            case "abort":
                cleanup(scope, data);
                return;
        }

        var editedFileName = scope.find("input[name$='.FileName']").val();
        var statusUploaded = scope.find(".status.uploaded").show();

        statusUploaded.text(statusUploaded.data("text-template").replace("{filename}", editedFileName));
        scope.data("upload-isactive", false);
        scope.data("upload-iscompleted", true);
        scope.data("upload-start-time", null);
    }

    function cleanup(scope: JQuery, data: any) {
        var wamsAssetInput = scope.find("input[name$='.WamsAssetId']");
        var fileNameInput = scope.find("input[name$='.FileName']");
        var assetId = $.trim(wamsAssetInput.val());
        var fileUploadWrapper = data.fileInput.closest(".file-upload-wrapper");

        if (assetId.length > 0) {
            var url = scope.data("delete-asset-url");
            var antiForgeryToken = scope.closest("form").find("[name='__RequestVerificationToken']").val();
            var cleanupMessage = scope.find(".status.cleanup");

            wamsAssetInput.val("");
            fileNameInput.val("");

            cleanupMessage.show();

            $.ajax({
                url: url,
                type: "DELETE",
                data: {
                    id: assetId,
                    __RequestVerificationToken: antiForgeryToken
                }
            }).done(function () {
                scope.data("upload-isactive", false);
                scope.data("upload-start-time", null);
                scope.find(".file-upload-wrapper").show();
                cleanupMessage.hide();
            }).fail(function () {
                alert("An error occurred on the server while trying to clean up.");
            });
        }

        fileUploadWrapper.show();
    }

    function pad(value: number, length: number) {
        var str = value.toString();
        while (str.length < length) {
            str = "0" + str;
        }
        return str;
    }

    function createBlockId(blockIndex: number) {
        var blockIdPrefix = "block-";
        return btoa(blockIdPrefix + pad(blockIndex, 6));
    }

    function commitBlockList(scope: JQuery, data: any) {
        var deferred = $.Deferred();
        var blockIds = scope.data("block-ids");

        if (blockIds.length == 0) {
            // The file was uploaded as a whole, so no manifest to submit.
            deferred.resolve();
        } else {
            // The file was uploaded in chunks.
            var url = scope.data("sas-locator") + "&comp=blocklist";
            var requestData = '<?xml version="1.0" encoding="utf-8"?><BlockList>';
            for (var i = 0; i < blockIds.length; i++) {
                requestData += '<Latest>' + blockIds[i] + '</Latest>';
            }
            requestData += '</BlockList>';

            $.ajax({
                url: url,
                type: "PUT",
                data: requestData,
                contentType: "text/plain; charset=UTF-8",
                crossDomain: true,
                cache: false,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader('x-ms-date', new Date().toUTCString());
                    xhr.setRequestHeader('x-ms-blob-content-type', data.files[0].type);
                    xhr.setRequestHeader('x-ms-version', "2012-02-12");
                    xhr.setRequestHeader('Content-Length', requestData.length.toString());
                },
                success: function () {
                    deferred.resolve(data);
                },
                error: function (xhr, status, error) {
                    data.textStatus = status;
                    data.errorThrown = error;
                    deferred.fail(data);
                }
            });
        }

        return deferred.promise();
    }

    function hasActiveUploads() {
        var scope = $(".upload-direct");
        var flag = false;

        scope.find(".async-upload").each(function () {
            if ($(this).data("upload-isactive") == true) {
                flag = true;
                return false;
            }
        });

        return flag;
    }

    function hasCompletedUploads() {
        var scope = $(".upload-direct");
        var flag = false;

        scope.find(".async-upload").each(function () {
            if ($(this).data("upload-iscompleted") == true) {
                flag = true;
                return false;
            }
        });

        return flag;
    } 

    function isSubmitting() {
        var scope = $(".upload-direct");
        return scope.data("is-submitting") == true;
    };

    function initializeUpload(fileInput: JQuery) {
        var scope = fileInput.closest(".async-upload");
        var fileUploadWrapper = scope.find(".file-upload-wrapper");
        var acceptFileTypesRegex = new RegExp(scope.data("upload-accept-file-types"));
        var antiForgeryToken: string = scope.closest("form").find("[name='__RequestVerificationToken']").val();

        var selectedFileWrapper = scope.find(".selected-file-wrapper");
        var filenameInput = scope.find(".filename-input");
        var resetButton = scope.find(".reset-button");
        var uploadButton = scope.find(".upload-button");
        var filenameText = scope.find(".filename-text");

        var validationText = scope.find(".validation-text");
        var preparingText = scope.find(".status.preparing");
        var uploadingContainer = scope.find(".status.uploading");
        var progressBar = scope.find(".progress-bar");
        var progressText = scope.find(".progress-text");
        var progressDetails = scope.find(".progress-details");
        var cancelLink = scope.find(".cancel-link"); 

        (<any>fileInput).fileupload({
            autoUpload: false,
            acceptFileTypes: acceptFileTypesRegex,
            type: "PUT",
            maxChunkSize: 4 * 1024 * 1024, // 4 MB
            beforeSend: (xhr: JQueryXHR, data: any) => {
                xhr.setRequestHeader("x-ms-date", new Date().toUTCString());
                xhr.setRequestHeader("x-ms-blob-type", "BlockBlob");
                xhr.setRequestHeader("content-length", data.data.size.toString());
            },
            chunksend: function (e: any, data: any) {
                var blockIndex = scope.data("block-index");
                var blockIds = scope.data("block-ids");
                var blockId = createBlockId(blockIndex);
                var url = scope.data("sas-locator") + "&comp=block&blockid=" + blockId;

                data.url = url;
                blockIds.push(blockId);
                scope.data("block-index", blockIndex + 1);
            },
            progressall: function (e: any, data: any) {
                var percentComplete = Math.floor((data.loaded / data.total) * 100);
                var startTime = new Date(scope.data("upload-start-time"));
                var elapsedMilliseconds = new Date(Date.now()).getTime() - startTime.getTime();
                var elapsed = moment.duration(elapsedMilliseconds, "ms");
                var remaining = moment.duration(elapsedMilliseconds / Math.max(data.loaded, 1) * (data.total - data.loaded), "ms");
                var kbps = Math.floor(data.bitrate / 8 / 1000);
                var uploaded = Math.floor(data.loaded / 1000);
                var total = Math.floor(data.total / 1000);

                progressBar.show().find(".progress").css("width", percentComplete + "%");
                progressText.text(progressText.data("text-template").replace("{percentage}", percentComplete)).show();
                progressDetails.text(progressDetails.data("text-template").replace("{uploaded}", uploaded).replace("{total}", total).replace("{kbps}", kbps).replace("{elapsed}", elapsed.humanize()).replace("{remaining}", remaining.humanize())).show();
            },
            done: function (e: any, data: any) {
                var self = this;
                commitBlockList(scope, data).always(function () {
                    uploadCompleted(self, e, data);
                });
            },
            fail: function (e: any, data: any) {
                uploadCompleted(this, e, data);
            },
            processdone: function (e: any, data: any) {
                var selectedFilename = data.files[0].name;
                scope.data("selected-filename", selectedFilename);
                window.setTimeout(function () {
                    fileUploadWrapper.hide();
                    validationText.hide();
                    selectedFileWrapper.show();
                    filenameText.text(filenameText.data("text-template").replace("{filename}", selectedFilename));
                }, 10); 

                (<any>scope[0]).doReset = function () {
                    fileUploadWrapper.show();
                    filenameInput.val("");
                    filenameText.text("");
                    selectedFileWrapper.hide();
                    validationText.hide();
                };

                (<any>scope[0]).doUpload = function () {
                    var editedFilename = filenameInput.val() || selectedFilename;
                    if (!acceptFileTypesRegex.test(editedFilename)) {
                        validationText.show();
                        return;
                    }

                    scope.data("upload-isactive", true);
                    scope.data("upload-start-time", Date.now());
                    var generateAssetUrl = scope.data("generate-asset-url");
                    scope.data("block-index", 0);
                    scope.data("block-ids", new Array());

                    preparingText.show();
                    selectedFileWrapper.hide();
                    validationText.hide();

                    $.ajax({
                        url: generateAssetUrl,
                        data: {
                            filename: editedFilename,
                            __RequestVerificationToken: antiForgeryToken
                        },
                        type: "POST"
                    }).done(function (asset: any) {
                            data.url = asset.sasLocator;
                            data.multipart = false;

                            scope.data("sas-locator", asset.sasLocator);
                            scope.find("input[name$='.FileName']").val(editedFilename);
                            scope.find("input[name$='.WamsAssetId']").val(asset.assetId);

                            preparingText.hide();
                            progressText.text(progressText.data("text-template").replace("{percentage}", 0)).show();
                            uploadingContainer.show();

                            var xhr = data.submit();
                            scope.data("xhr", xhr);
                        }).fail(function (xhr: any, status: any, error: any) {
                            fileUploadWrapper.show();
                            selectedFileWrapper.show();
                            preparingText.hide();
                            uploadingContainer.hide();

                            scope.data("upload-isactive", false);
                            scope.data("upload-start-time", null);
                            alert("An error occurred. Error: " + error);
                        });
                };
            },
            processfail: function (e: any, data: any) {
                validationText.show();
                filenameInput.val("");
                filenameText.text("");
                selectedFileWrapper.hide();
            },
            change: function (e: any, data: any) {
                var prompt = fileInput.data("prompt");
                if (prompt && prompt.length > 0) {
                    if (!confirm(prompt)) {
                        e.preventDefault();
                    }
                }
            }
        });

        cancelLink.on("click", function (e) {
            e.preventDefault();

            if (confirm($(this).data("cancel-prompt"))) {
                var xhr = scope.data("xhr");
                xhr.abort();
            }
        });
    }

    export function initializeUploadDirect() {

        var scope = $(".upload-direct").show();

        scope.find(".reset-button").on("click", function (e) {
            var doReset = (<any>$(this).closest(".async-upload")[0]).doReset;
            if (!!doReset)
                doReset();
        });

        scope.find(".upload-button").on("click", function (e) {
            var doUpload = (<any>$(this).closest(".async-upload")[0]).doUpload;
            if (!!doUpload)
                doUpload();
        }); 
          
        requiredUploads = scope.find(".required-upload");

        scope.find(".async-upload-file-input").each(function () {
            initializeUpload($(this));
        });

        scope.closest("form").on("submit", function (e) {
            if (hasActiveUploads()) {
                alert(scope.data("block-submit-prompt"));
                e.preventDefault();
                return false;
            }

            scope.data("is-submitting", true);
        });
         
        $(window).on("beforeunload", function (e) {
            if ((hasActiveUploads() || hasCompletedUploads()) && !isSubmitting()) {
                var message = scope.data("navigate-away-prompt");
                e.result = message;
                return message;
            }
        });
    }
}