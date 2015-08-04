/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />
/// <reference path="Typings/moment.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var CloudVideoEdit;
            (function (CloudVideoEdit) {
                var requiredUploads;
                function uploadCompleted(sender, e, data) {
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
                function cleanup(scope, data) {
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
                function pad(value, length) {
                    var str = value.toString();
                    while (str.length < length) {
                        str = "0" + str;
                    }
                    return str;
                }
                function createBlockId(blockIndex) {
                    var blockIdPrefix = "block-";
                    return btoa(blockIdPrefix + pad(blockIndex, 6));
                }
                function commitBlockList(scope, data) {
                    var deferred = $.Deferred();
                    var blockIds = scope.data("block-ids");
                    if (blockIds.length == 0) {
                        // The file was uploaded as a whole, so no manifest to submit.
                        deferred.resolve();
                    }
                    else {
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
                }
                ;
                function initializeUpload(fileInput) {
                    var scope = fileInput.closest(".async-upload");
                    var fileUploadWrapper = scope.find(".file-upload-wrapper");
                    var acceptFileTypesRegex = new RegExp(scope.data("upload-accept-file-types"));
                    var antiForgeryToken = scope.closest("form").find("[name='__RequestVerificationToken']").val();
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
                    fileInput.fileupload({
                        autoUpload: false,
                        acceptFileTypes: acceptFileTypesRegex,
                        type: "PUT",
                        maxChunkSize: 4 * 1024 * 1024,
                        beforeSend: function (xhr, data) {
                            xhr.setRequestHeader("x-ms-date", new Date().toUTCString());
                            xhr.setRequestHeader("x-ms-blob-type", "BlockBlob");
                            xhr.setRequestHeader("content-length", data.data.size.toString());
                        },
                        chunksend: function (e, data) {
                            var blockIndex = scope.data("block-index");
                            var blockIds = scope.data("block-ids");
                            var blockId = createBlockId(blockIndex);
                            var url = scope.data("sas-locator") + "&comp=block&blockid=" + blockId;
                            data.url = url;
                            blockIds.push(blockId);
                            scope.data("block-index", blockIndex + 1);
                        },
                        progressall: function (e, data) {
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
                        done: function (e, data) {
                            var self = this;
                            commitBlockList(scope, data).always(function () {
                                uploadCompleted(self, e, data);
                            });
                        },
                        fail: function (e, data) {
                            uploadCompleted(this, e, data);
                        },
                        processdone: function (e, data) {
                            var selectedFilename = data.files[0].name;
                            scope.data("selected-filename", selectedFilename);
                            window.setTimeout(function () {
                                fileUploadWrapper.hide();
                                validationText.hide();
                                selectedFileWrapper.show();
                                filenameText.text(filenameText.data("text-template").replace("{filename}", selectedFilename));
                            }, 10);
                            scope[0].doReset = function () {
                                fileUploadWrapper.show();
                                filenameInput.val("");
                                filenameText.text("");
                                selectedFileWrapper.hide();
                                validationText.hide();
                            };
                            scope[0].doUpload = function () {
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
                                }).done(function (asset) {
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
                                }).fail(function (xhr, status, error) {
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
                        processfail: function (e, data) {
                            validationText.show();
                            filenameInput.val("");
                            filenameText.text("");
                            selectedFileWrapper.hide();
                        },
                        change: function (e, data) {
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
                function initializeUploadDirect() {
                    var scope = $(".upload-direct").show();
                    scope.find(".reset-button").on("click", function (e) {
                        var doReset = $(this).closest(".async-upload")[0].doReset;
                        if (!!doReset)
                            doReset();
                    });
                    scope.find(".upload-button").on("click", function (e) {
                        var doUpload = $(this).closest(".async-upload")[0].doUpload;
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
                CloudVideoEdit.initializeUploadDirect = initializeUploadDirect;
            })(CloudVideoEdit = MediaServices.CloudVideoEdit || (MediaServices.CloudVideoEdit = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtZWRpdC1jbG91ZHZpZGVvcGFydC1kaXJlY3QudHMiXSwibmFtZXMiOlsiT3JjaGFyZCIsIk9yY2hhcmQuQXp1cmUiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQudXBsb2FkQ29tcGxldGVkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmNsZWFudXAiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQucGFkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmNyZWF0ZUJsb2NrSWQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQuY29tbWl0QmxvY2tMaXN0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0Lmhhc0FjdGl2ZVVwbG9hZHMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQuaGFzQ29tcGxldGVkVXBsb2FkcyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdC5pc1N1Ym1pdHRpbmciLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQuaW5pdGlhbGl6ZVVwbG9hZCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdC5pbml0aWFsaXplVXBsb2FkRGlyZWN0Il0sIm1hcHBpbmdzIjoiQUFBQSw0Q0FBNEM7QUFDNUMsOENBQThDO0FBQzlDLDRDQUE0QztBQUU1QyxJQUFPLE9BQU8sQ0FpV2I7QUFqV0QsV0FBTyxPQUFPO0lBQUNBLElBQUFBLEtBQUtBLENBaVduQkE7SUFqV2NBLFdB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QUMsT0FBTyxDQUFDO3dCQUNuQixDQUFDO29CQUNMLENBQUMsQ0FBQ0EsQ0FBQ0E7Z0JBQ1BBLENBQUNBO2dCQXZDZVYscUNBQXNCQSx5QkF1Q3JDQSxDQUFBQTtZQUNMQSxDQUFDQSxFQWpXa0NELGNBQWNBLEdBQWRBLDRCQUFjQSxLQUFkQSw0QkFBY0EsUUFpV2hEQTtRQUFEQSxDQUFDQSxFQWpXb0JELGFBQWFBLEdBQWJBLG1CQUFhQSxLQUFiQSxtQkFBYUEsUUFpV2pDQTtJQUFEQSxDQUFDQSxFQWpXY0QsS0FBS0EsR0FBTEEsYUFBS0EsS0FBTEEsYUFBS0EsUUFpV25CQTtBQUFEQSxDQUFDQSxFQWpXTSxPQUFPLEtBQVAsT0FBTyxRQWlXYiIsImZpbGUiOiJjbG91ZG1lZGlhLWVkaXQtY2xvdWR2aWRlb3BhcnQtZGlyZWN0LmpzIiwic291cmNlc0NvbnRlbnQiOlsiLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5LmQudHNcIiAvPlxyXG4vLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnl1aS5kLnRzXCIgLz5cclxuLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvbW9tZW50LmQudHNcIiAvPlxyXG5cclxubW9kdWxlIE9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdCB7XHJcbiAgICB2YXIgcmVxdWlyZWRVcGxvYWRzOiBKUXVlcnk7XHJcblxyXG4gICAgZnVuY3Rpb24gdXBsb2FkQ29tcGxldGVkKHNlbmRlciwgZSwgZGF0YSkge1xyXG4gICAgICAgIHZhciBzY29wZSA9ICQoc2VuZGVyKS5jbG9zZXN0KFwiLmFzeW5jLXVwbG9hZFwiKTtcclxuICAgICAgICB2YXIgc3RhdHVzID0gZGF0YS5lcnJvclRocm93biAmJiBkYXRhLmVycm9yVGhyb3duLmxlbmd0aCA+IDAgPyBkYXRhLmVycm9yVGhyb3duIDogZGF0YS50ZXh0U3RhdHVzO1xyXG4gICAgICAgIHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtYmFyXCIpLmhpZGUoKTtcclxuICAgICAgICBzY29wZS5maW5kKFwiLnByb2dyZXNzLXRleHRcIikuaGlkZSgpO1xyXG4gICAgICAgIHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtZGV0YWlsc1wiKS5oaWRlKCk7XHJcbiAgICAgICAgc2NvcGUuZmluZChcIi5zdGF0dXMucHJlcGFyaW5nXCIpLmhpZGUoKTtcclxuICAgICAgICBzY29wZS5maW5kKFwiLnN0YXR1cy51cGxvYWRpbmdcIikuaGlkZSgpO1xyXG5cclxuICAgICAgICBzd2l0Y2ggKHN0YXR1cykge1xyXG4gICAgICAgICAgICBjYXNlIFwiZXJyb3JcIjpcclxuICAgICAgICAgICAgICAgIGNsZWFudXAoc2NvcGUsIGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgYWxlcnQoXCJUaGUgdXBsb2FkIG9mIHRoZSBzZWxlY3RlZCBmaWxlIGZhaWxlZC4gWW91IG1heSB0cnkgYWdhaW4gYWZ0ZXIgdGhlIGNsZWFudXAgaGFzIGZpbmlzaGVkLlwiKTtcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgY2FzZSBcImFib3J0XCI6XHJcbiAgICAgICAgICAgICAgICBjbGVhbnVwKHNjb3BlLCBkYXRhKTtcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBlZGl0ZWRGaWxlTmFtZSA9IHNjb3BlLmZpbmQoXCJpbnB1dFtuYW1lJD0nLkZpbGVOYW1lJ11cIikudmFsKCk7XHJcbiAgICAgICAgdmFyIHN0YXR1c1VwbG9hZGVkID0gc2NvcGUuZmluZChcIi5zdGF0dXMudXBsb2FkZWRcIikuc2hvdygpO1xyXG5cclxuICAgICAgICBzdGF0dXNVcGxvYWRlZC50ZXh0KHN0YXR1c1VwbG9hZGVkLmRhdGEoXCJ0ZXh0LXRlbXBsYXRlXCIpLnJlcGxhY2UoXCJ7ZmlsZW5hbWV9XCIsIGVkaXRlZEZpbGVOYW1lKSk7XHJcbiAgICAgICAgc2NvcGUuZGF0YShcInVwbG9hZC1pc2FjdGl2ZVwiLCBmYWxzZSk7XHJcbiAgICAgICAgc2NvcGUuZGF0YShcInVwbG9hZC1pc2NvbXBsZXRlZFwiLCB0cnVlKTtcclxuICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLXN0YXJ0LXRpbWVcIiwgbnVsbCk7XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gY2xlYW51cChzY29wZTogSlF1ZXJ5LCBkYXRhKSB7XHJcbiAgICAgICAgdmFyIHdhbXNBc3NldElucHV0ID0gc2NvcGUuZmluZChcImlucHV0W25hbWUkPScuV2Ftc0Fzc2V0SWQnXVwiKTtcclxuICAgICAgICB2YXIgZmlsZU5hbWVJbnB1dCA9IHNjb3BlLmZpbmQoXCJpbnB1dFtuYW1lJD0nLkZpbGVOYW1lJ11cIik7XHJcbiAgICAgICAgdmFyIGFzc2V0SWQgPSAkLnRyaW0od2Ftc0Fzc2V0SW5wdXQudmFsKCkpO1xyXG4gICAgICAgIHZhciBmaWxlVXBsb2FkV3JhcHBlciA9IGRhdGEuZmlsZUlucHV0LmNsb3Nlc3QoXCIuZmlsZS11cGxvYWQtd3JhcHBlclwiKTtcclxuXHJcbiAgICAgICAgaWYgKGFzc2V0SWQubGVuZ3RoID4gMCkge1xyXG4gICAgICAgICAgICB2YXIgdXJsID0gc2NvcGUuZGF0YShcImRlbGV0ZS1hc3NldC11cmxcIik7XHJcbiAgICAgICAgICAgIHZhciBhbnRpRm9yZ2VyeVRva2VuID0gc2NvcGUuY2xvc2VzdChcImZvcm1cIikuZmluZChcIltuYW1lPSdfX1JlcXVlc3RWZXJpZmljYXRpb25Ub2tlbiddXCIpLnZhbCgpO1xyXG4gICAgICAgICAgICB2YXIgY2xlYW51cE1lc3NhZ2UgPSBzY29wZS5maW5kKFwiLnN0YXR1cy5jbGVhbnVwXCIpO1xyXG5cclxuICAgICAgICAgICAgd2Ftc0Fzc2V0SW5wdXQudmFsKFwiXCIpO1xyXG4gICAgICAgICAgICBmaWxlTmFtZUlucHV0LnZhbChcIlwiKTtcclxuXHJcbiAgICAgICAgICAgIGNsZWFudXBNZXNzYWdlLnNob3coKTtcclxuXHJcbiAgICAgICAgICAgICQuYWpheCh7XHJcbiAgICAgICAgICAgICAgICB1cmw6IHVybCxcclxuICAgICAgICAgICAgICAgIHR5cGU6IFwiREVMRVRFXCIsXHJcbiAgICAgICAgICAgICAgICBkYXRhOiB7XHJcbiAgICAgICAgICAgICAgICAgICAgaWQ6IGFzc2V0SWQsXHJcbiAgICAgICAgICAgICAgICAgICAgX19SZXF1ZXN0VmVyaWZpY2F0aW9uVG9rZW46IGFudGlGb3JnZXJ5VG9rZW5cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSkuZG9uZShmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLWlzYWN0aXZlXCIsIGZhbHNlKTtcclxuICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJ1cGxvYWQtc3RhcnQtdGltZVwiLCBudWxsKTtcclxuICAgICAgICAgICAgICAgIHNjb3BlLmZpbmQoXCIuZmlsZS11cGxvYWQtd3JhcHBlclwiKS5zaG93KCk7XHJcbiAgICAgICAgICAgICAgICBjbGVhbnVwTWVzc2FnZS5oaWRlKCk7XHJcbiAgICAgICAgICAgIH0pLmZhaWwoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgYWxlcnQoXCJBbiBlcnJvciBvY2N1cnJlZCBvbiB0aGUgc2VydmVyIHdoaWxlIHRyeWluZyB0byBjbGVhbiB1cC5cIik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZmlsZVVwbG9hZFdyYXBwZXIuc2hvdygpO1xyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIHBhZCh2YWx1ZTogbnVtYmVyLCBsZW5ndGg6IG51bWJlcikge1xyXG4gICAgICAgIHZhciBzdHIgPSB2YWx1ZS50b1N0cmluZygpO1xyXG4gICAgICAgIHdoaWxlIChzdHIubGVuZ3RoIDwgbGVuZ3RoKSB7XHJcbiAgICAgICAgICAgIHN0ciA9IFwiMFwiICsgc3RyO1xyXG4gICAgICAgIH1cclxuICAgICAgICByZXR1cm4gc3RyO1xyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIGNyZWF0ZUJsb2NrSWQoYmxvY2tJbmRleDogbnVtYmVyKSB7XHJcbiAgICAgICAgdmFyIGJsb2NrSWRQcmVmaXggPSBcImJsb2NrLVwiO1xyXG4gICAgICAgIHJldHVybiBidG9hKGJsb2NrSWRQcmVmaXggKyBwYWQoYmxvY2tJbmRleCwgNikpO1xyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIGNvbW1pdEJsb2NrTGlzdChzY29wZTogSlF1ZXJ5LCBkYXRhKSB7XHJcbiAgICAgICAgdmFyIGRlZmVycmVkID0gJC5EZWZlcnJlZCgpO1xyXG4gICAgICAgIHZhciBibG9ja0lkcyA9IHNjb3BlLmRhdGEoXCJibG9jay1pZHNcIik7XHJcblxyXG4gICAgICAgIGlmIChibG9ja0lkcy5sZW5ndGggPT0gMCkge1xyXG4gICAgICAgICAgICAvLyBUaGUgZmlsZSB3YXMgdXBsb2FkZWQgYXMgYSB3aG9sZSwgc28gbm8gbWFuaWZlc3QgdG8gc3VibWl0LlxyXG4gICAgICAgICAgICBkZWZlcnJlZC5yZXNvbHZlKCk7XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgLy8gVGhlIGZpbGUgd2FzIHVwbG9hZGVkIGluIGNodW5rcy5cclxuICAgICAgICAgICAgdmFyIHVybCA9IHNjb3BlLmRhdGEoXCJzYXMtbG9jYXRvclwiKSArIFwiJmNvbXA9YmxvY2tsaXN0XCI7XHJcbiAgICAgICAgICAgIHZhciByZXF1ZXN0RGF0YSA9ICc8P3htbCB2ZXJzaW9uPVwiMS4wXCIgZW5jb2Rpbmc9XCJ1dGYtOFwiPz48QmxvY2tMaXN0Pic7XHJcbiAgICAgICAgICAgIGZvciAodmFyIGkgPSAwOyBpIDwgYmxvY2tJZHMubGVuZ3RoOyBpKyspIHtcclxuICAgICAgICAgICAgICAgIHJlcXVlc3REYXRhICs9ICc8TGF0ZXN0PicgKyBibG9ja0lkc1tpXSArICc8L0xhdGVzdD4nO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJlcXVlc3REYXRhICs9ICc8L0Jsb2NrTGlzdD4nO1xyXG5cclxuICAgICAgICAgICAgJC5hamF4KHtcclxuICAgICAgICAgICAgICAgIHVybDogdXJsLFxyXG4gICAgICAgICAgICAgICAgdHlwZTogXCJQVVRcIixcclxuICAgICAgICAgICAgICAgIGRhdGE6IHJlcXVlc3REYXRhLFxyXG4gICAgICAgICAgICAgICAgY29udGVudFR5cGU6IFwidGV4dC9wbGFpbjsgY2hhcnNldD1VVEYtOFwiLFxyXG4gICAgICAgICAgICAgICAgY3Jvc3NEb21haW46IHRydWUsXHJcbiAgICAgICAgICAgICAgICBjYWNoZTogZmFsc2UsXHJcbiAgICAgICAgICAgICAgICBiZWZvcmVTZW5kOiBmdW5jdGlvbiAoeGhyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoJ3gtbXMtZGF0ZScsIG5ldyBEYXRlKCkudG9VVENTdHJpbmcoKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoJ3gtbXMtYmxvYi1jb250ZW50LXR5cGUnLCBkYXRhLmZpbGVzWzBdLnR5cGUpO1xyXG4gICAgICAgICAgICAgICAgICAgIHhoci5zZXRSZXF1ZXN0SGVhZGVyKCd4LW1zLXZlcnNpb24nLCBcIjIwMTItMDItMTJcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoJ0NvbnRlbnQtTGVuZ3RoJywgcmVxdWVzdERhdGEubGVuZ3RoLnRvU3RyaW5nKCkpO1xyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIHN1Y2Nlc3M6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICBkZWZlcnJlZC5yZXNvbHZlKGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIGVycm9yOiBmdW5jdGlvbiAoeGhyLCBzdGF0dXMsIGVycm9yKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZGF0YS50ZXh0U3RhdHVzID0gc3RhdHVzO1xyXG4gICAgICAgICAgICAgICAgICAgIGRhdGEuZXJyb3JUaHJvd24gPSBlcnJvcjtcclxuICAgICAgICAgICAgICAgICAgICBkZWZlcnJlZC5mYWlsKGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiBkZWZlcnJlZC5wcm9taXNlKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gaGFzQWN0aXZlVXBsb2FkcygpIHtcclxuICAgICAgICB2YXIgc2NvcGUgPSAkKFwiLnVwbG9hZC1kaXJlY3RcIik7XHJcbiAgICAgICAgdmFyIGZsYWcgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgc2NvcGUuZmluZChcIi5hc3luYy11cGxvYWRcIikuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICgkKHRoaXMpLmRhdGEoXCJ1cGxvYWQtaXNhY3RpdmVcIikgPT0gdHJ1ZSkge1xyXG4gICAgICAgICAgICAgICAgZmxhZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgcmV0dXJuIGZsYWc7XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gaGFzQ29tcGxldGVkVXBsb2FkcygpIHtcclxuICAgICAgICB2YXIgc2NvcGUgPSAkKFwiLnVwbG9hZC1kaXJlY3RcIik7XHJcbiAgICAgICAgdmFyIGZsYWcgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgc2NvcGUuZmluZChcIi5hc3luYy11cGxvYWRcIikuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICgkKHRoaXMpLmRhdGEoXCJ1cGxvYWQtaXNjb21wbGV0ZWRcIikgPT0gdHJ1ZSkge1xyXG4gICAgICAgICAgICAgICAgZmxhZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgcmV0dXJuIGZsYWc7XHJcbiAgICB9IFxyXG5cclxuICAgIGZ1bmN0aW9uIGlzU3VibWl0dGluZygpIHtcclxuICAgICAgICB2YXIgc2NvcGUgPSAkKFwiLnVwbG9hZC1kaXJlY3RcIik7XHJcbiAgICAgICAgcmV0dXJuIHNjb3BlLmRhdGEoXCJpcy1zdWJtaXR0aW5nXCIpID09IHRydWU7XHJcbiAgICB9O1xyXG5cclxuICAgIGZ1bmN0aW9uIGluaXRpYWxpemVVcGxvYWQoZmlsZUlucHV0OiBKUXVlcnkpIHtcclxuICAgICAgICB2YXIgc2NvcGUgPSBmaWxlSW5wdXQuY2xvc2VzdChcIi5hc3luYy11cGxvYWRcIik7XHJcbiAgICAgICAgdmFyIGZpbGVVcGxvYWRXcmFwcGVyID0gc2NvcGUuZmluZChcIi5maWxlLXVwbG9hZC13cmFwcGVyXCIpO1xyXG4gICAgICAgIHZhciBhY2NlcHRGaWxlVHlwZXNSZWdleCA9IG5ldyBSZWdFeHAoc2NvcGUuZGF0YShcInVwbG9hZC1hY2NlcHQtZmlsZS10eXBlc1wiKSk7XHJcbiAgICAgICAgdmFyIGFudGlGb3JnZXJ5VG9rZW46IHN0cmluZyA9IHNjb3BlLmNsb3Nlc3QoXCJmb3JtXCIpLmZpbmQoXCJbbmFtZT0nX19SZXF1ZXN0VmVyaWZpY2F0aW9uVG9rZW4nXVwiKS52YWwoKTtcclxuXHJcbiAgICAgICAgdmFyIHNlbGVjdGVkRmlsZVdyYXBwZXIgPSBzY29wZS5maW5kKFwiLnNlbGVjdGVkLWZpbGUtd3JhcHBlclwiKTtcclxuICAgICAgICB2YXIgZmlsZW5hbWVJbnB1dCA9IHNjb3BlLmZpbmQoXCIuZmlsZW5hbWUtaW5wdXRcIik7XHJcbiAgICAgICAgdmFyIHJlc2V0QnV0dG9uID0gc2NvcGUuZmluZChcIi5yZXNldC1idXR0b25cIik7XHJcbiAgICAgICAgdmFyIHVwbG9hZEJ1dHRvbiA9IHNjb3BlLmZpbmQoXCIudXBsb2FkLWJ1dHRvblwiKTtcclxuICAgICAgICB2YXIgZmlsZW5hbWVUZXh0ID0gc2NvcGUuZmluZChcIi5maWxlbmFtZS10ZXh0XCIpO1xyXG5cclxuICAgICAgICB2YXIgdmFsaWRhdGlvblRleHQgPSBzY29wZS5maW5kKFwiLnZhbGlkYXRpb24tdGV4dFwiKTtcclxuICAgICAgICB2YXIgcHJlcGFyaW5nVGV4dCA9IHNjb3BlLmZpbmQoXCIuc3RhdHVzLnByZXBhcmluZ1wiKTtcclxuICAgICAgICB2YXIgdXBsb2FkaW5nQ29udGFpbmVyID0gc2NvcGUuZmluZChcIi5zdGF0dXMudXBsb2FkaW5nXCIpO1xyXG4gICAgICAgIHZhciBwcm9ncmVzc0JhciA9IHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtYmFyXCIpO1xyXG4gICAgICAgIHZhciBwcm9ncmVzc1RleHQgPSBzY29wZS5maW5kKFwiLnByb2dyZXNzLXRleHRcIik7XHJcbiAgICAgICAgdmFyIHByb2dyZXNzRGV0YWlscyA9IHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtZGV0YWlsc1wiKTtcclxuICAgICAgICB2YXIgY2FuY2VsTGluayA9IHNjb3BlLmZpbmQoXCIuY2FuY2VsLWxpbmtcIik7IFxyXG5cclxuICAgICAgICAoPGFueT5maWxlSW5wdXQpLmZpbGV1cGxvYWQoe1xyXG4gICAgICAgICAgICBhdXRvVXBsb2FkOiBmYWxzZSxcclxuICAgICAgICAgICAgYWNjZXB0RmlsZVR5cGVzOiBhY2NlcHRGaWxlVHlwZXNSZWdleCxcclxuICAgICAgICAgICAgdHlwZTogXCJQVVRcIixcclxuICAgICAgICAgICAgbWF4Q2h1bmtTaXplOiA0ICogMTAyNCAqIDEwMjQsIC8vIDQgTUJcclxuICAgICAgICAgICAgYmVmb3JlU2VuZDogKHhocjogSlF1ZXJ5WEhSLCBkYXRhKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB4aHIuc2V0UmVxdWVzdEhlYWRlcihcIngtbXMtZGF0ZVwiLCBuZXcgRGF0ZSgpLnRvVVRDU3RyaW5nKCkpO1xyXG4gICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoXCJ4LW1zLWJsb2ItdHlwZVwiLCBcIkJsb2NrQmxvYlwiKTtcclxuICAgICAgICAgICAgICAgIHhoci5zZXRSZXF1ZXN0SGVhZGVyKFwiY29udGVudC1sZW5ndGhcIiwgZGF0YS5kYXRhLnNpemUudG9TdHJpbmcoKSk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGNodW5rc2VuZDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuICAgICAgICAgICAgICAgIHZhciBibG9ja0luZGV4ID0gc2NvcGUuZGF0YShcImJsb2NrLWluZGV4XCIpO1xyXG4gICAgICAgICAgICAgICAgdmFyIGJsb2NrSWRzID0gc2NvcGUuZGF0YShcImJsb2NrLWlkc1wiKTtcclxuICAgICAgICAgICAgICAgIHZhciBibG9ja0lkID0gY3JlYXRlQmxvY2tJZChibG9ja0luZGV4KTtcclxuICAgICAgICAgICAgICAgIHZhciB1cmwgPSBzY29wZS5kYXRhKFwic2FzLWxvY2F0b3JcIikgKyBcIiZjb21wPWJsb2NrJmJsb2NraWQ9XCIgKyBibG9ja0lkO1xyXG5cclxuICAgICAgICAgICAgICAgIGRhdGEudXJsID0gdXJsO1xyXG4gICAgICAgICAgICAgICAgYmxvY2tJZHMucHVzaChibG9ja0lkKTtcclxuICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJibG9jay1pbmRleFwiLCBibG9ja0luZGV4ICsgMSk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHByb2dyZXNzYWxsOiBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHBlcmNlbnRDb21wbGV0ZSA9IE1hdGguZmxvb3IoKGRhdGEubG9hZGVkIC8gZGF0YS50b3RhbCkgKiAxMDApO1xyXG4gICAgICAgICAgICAgICAgdmFyIHN0YXJ0VGltZSA9IG5ldyBEYXRlKHNjb3BlLmRhdGEoXCJ1cGxvYWQtc3RhcnQtdGltZVwiKSk7XHJcbiAgICAgICAgICAgICAgICB2YXIgZWxhcHNlZE1pbGxpc2Vjb25kcyA9IG5ldyBEYXRlKERhdGUubm93KCkpLmdldFRpbWUoKSAtIHN0YXJ0VGltZS5nZXRUaW1lKCk7XHJcbiAgICAgICAgICAgICAgICB2YXIgZWxhcHNlZCA9IG1vbWVudC5kdXJhdGlvbihlbGFwc2VkTWlsbGlzZWNvbmRzLCBcIm1zXCIpO1xyXG4gICAgICAgICAgICAgICAgdmFyIHJlbWFpbmluZyA9IG1vbWVudC5kdXJhdGlvbihlbGFwc2VkTWlsbGlzZWNvbmRzIC8gTWF0aC5tYXgoZGF0YS5sb2FkZWQsIDEpICogKGRhdGEudG90YWwgLSBkYXRhLmxvYWRlZCksIFwibXNcIik7XHJcbiAgICAgICAgICAgICAgICB2YXIga2JwcyA9IE1hdGguZmxvb3IoZGF0YS5iaXRyYXRlIC8gOCAvIDEwMDApO1xyXG4gICAgICAgICAgICAgICAgdmFyIHVwbG9hZGVkID0gTWF0aC5mbG9vcihkYXRhLmxvYWRlZCAvIDEwMDApO1xyXG4gICAgICAgICAgICAgICAgdmFyIHRvdGFsID0gTWF0aC5mbG9vcihkYXRhLnRvdGFsIC8gMTAwMCk7XHJcblxyXG4gICAgICAgICAgICAgICAgcHJvZ3Jlc3NCYXIuc2hvdygpLmZpbmQoXCIucHJvZ3Jlc3NcIikuY3NzKFwid2lkdGhcIiwgcGVyY2VudENvbXBsZXRlICsgXCIlXCIpO1xyXG4gICAgICAgICAgICAgICAgcHJvZ3Jlc3NUZXh0LnRleHQocHJvZ3Jlc3NUZXh0LmRhdGEoXCJ0ZXh0LXRlbXBsYXRlXCIpLnJlcGxhY2UoXCJ7cGVyY2VudGFnZX1cIiwgcGVyY2VudENvbXBsZXRlKSkuc2hvdygpO1xyXG4gICAgICAgICAgICAgICAgcHJvZ3Jlc3NEZXRhaWxzLnRleHQocHJvZ3Jlc3NEZXRhaWxzLmRhdGEoXCJ0ZXh0LXRlbXBsYXRlXCIpLnJlcGxhY2UoXCJ7dXBsb2FkZWR9XCIsIHVwbG9hZGVkKS5yZXBsYWNlKFwie3RvdGFsfVwiLCB0b3RhbCkucmVwbGFjZShcIntrYnBzfVwiLCBrYnBzKS5yZXBsYWNlKFwie2VsYXBzZWR9XCIsIGVsYXBzZWQuaHVtYW5pemUoKSkucmVwbGFjZShcIntyZW1haW5pbmd9XCIsIHJlbWFpbmluZy5odW1hbml6ZSgpKSkuc2hvdygpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBkb25lOiBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICAgICAgY29tbWl0QmxvY2tMaXN0KHNjb3BlLCBkYXRhKS5hbHdheXMoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHVwbG9hZENvbXBsZXRlZChzZWxmLCBlLCBkYXRhKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBmYWlsOiBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG4gICAgICAgICAgICAgICAgdXBsb2FkQ29tcGxldGVkKHRoaXMsIGUsIGRhdGEpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBwcm9jZXNzZG9uZTogZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuICAgICAgICAgICAgICAgIHZhciBzZWxlY3RlZEZpbGVuYW1lID0gZGF0YS5maWxlc1swXS5uYW1lO1xyXG4gICAgICAgICAgICAgICAgc2NvcGUuZGF0YShcInNlbGVjdGVkLWZpbGVuYW1lXCIsIHNlbGVjdGVkRmlsZW5hbWUpO1xyXG4gICAgICAgICAgICAgICAgd2luZG93LnNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGZpbGVVcGxvYWRXcmFwcGVyLmhpZGUoKTtcclxuICAgICAgICAgICAgICAgICAgICB2YWxpZGF0aW9uVGV4dC5oaWRlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZWN0ZWRGaWxlV3JhcHBlci5zaG93KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZW5hbWVUZXh0LnRleHQoZmlsZW5hbWVUZXh0LmRhdGEoXCJ0ZXh0LXRlbXBsYXRlXCIpLnJlcGxhY2UoXCJ7ZmlsZW5hbWV9XCIsIHNlbGVjdGVkRmlsZW5hbWUpKTtcclxuICAgICAgICAgICAgICAgIH0sIDEwKTsgXHJcblxyXG4gICAgICAgICAgICAgICAgKDxhbnk+c2NvcGVbMF0pLmRvUmVzZXQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZVVwbG9hZFdyYXBwZXIuc2hvdygpO1xyXG4gICAgICAgICAgICAgICAgICAgIGZpbGVuYW1lSW5wdXQudmFsKFwiXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIGZpbGVuYW1lVGV4dC50ZXh0KFwiXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHNlbGVjdGVkRmlsZVdyYXBwZXIuaGlkZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIHZhbGlkYXRpb25UZXh0LmhpZGUoKTtcclxuICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAgICAgKDxhbnk+c2NvcGVbMF0pLmRvVXBsb2FkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBlZGl0ZWRGaWxlbmFtZSA9IGZpbGVuYW1lSW5wdXQudmFsKCkgfHwgc2VsZWN0ZWRGaWxlbmFtZTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoIWFjY2VwdEZpbGVUeXBlc1JlZ2V4LnRlc3QoZWRpdGVkRmlsZW5hbWUpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhbGlkYXRpb25UZXh0LnNob3coKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgc2NvcGUuZGF0YShcInVwbG9hZC1pc2FjdGl2ZVwiLCB0cnVlKTtcclxuICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLXN0YXJ0LXRpbWVcIiwgRGF0ZS5ub3coKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGdlbmVyYXRlQXNzZXRVcmwgPSBzY29wZS5kYXRhKFwiZ2VuZXJhdGUtYXNzZXQtdXJsXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJibG9jay1pbmRleFwiLCAwKTtcclxuICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwiYmxvY2staWRzXCIsIG5ldyBBcnJheSgpKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgcHJlcGFyaW5nVGV4dC5zaG93KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZWN0ZWRGaWxlV3JhcHBlci5oaWRlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFsaWRhdGlvblRleHQuaGlkZSgpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAkLmFqYXgoe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB1cmw6IGdlbmVyYXRlQXNzZXRVcmwsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRhdGE6IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZpbGVuYW1lOiBlZGl0ZWRGaWxlbmFtZSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIF9fUmVxdWVzdFZlcmlmaWNhdGlvblRva2VuOiBhbnRpRm9yZ2VyeVRva2VuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHR5cGU6IFwiUE9TVFwiXHJcbiAgICAgICAgICAgICAgICAgICAgfSkuZG9uZShmdW5jdGlvbiAoYXNzZXQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRhdGEudXJsID0gYXNzZXQuc2FzTG9jYXRvcjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRhdGEubXVsdGlwYXJ0ID0gZmFsc2U7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZGF0YShcInNhcy1sb2NhdG9yXCIsIGFzc2V0LnNhc0xvY2F0b3IpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZmluZChcImlucHV0W25hbWUkPScuRmlsZU5hbWUnXVwiKS52YWwoZWRpdGVkRmlsZW5hbWUpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZmluZChcImlucHV0W25hbWUkPScuV2Ftc0Fzc2V0SWQnXVwiKS52YWwoYXNzZXQuYXNzZXRJZCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcHJlcGFyaW5nVGV4dC5oaWRlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBwcm9ncmVzc1RleHQudGV4dChwcm9ncmVzc1RleHQuZGF0YShcInRleHQtdGVtcGxhdGVcIikucmVwbGFjZShcIntwZXJjZW50YWdlfVwiLCAwKSkuc2hvdygpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdXBsb2FkaW5nQ29udGFpbmVyLnNob3coKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgeGhyID0gZGF0YS5zdWJtaXQoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJ4aHJcIiwgeGhyKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSkuZmFpbChmdW5jdGlvbiAoeGhyLCBzdGF0dXMsIGVycm9yKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBmaWxlVXBsb2FkV3JhcHBlci5zaG93KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3RlZEZpbGVXcmFwcGVyLnNob3coKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHByZXBhcmluZ1RleHQuaGlkZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdXBsb2FkaW5nQ29udGFpbmVyLmhpZGUoKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLWlzYWN0aXZlXCIsIGZhbHNlKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJ1cGxvYWQtc3RhcnQtdGltZVwiLCBudWxsKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGFsZXJ0KFwiQW4gZXJyb3Igb2NjdXJyZWQuIEVycm9yOiBcIiArIGVycm9yKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBwcm9jZXNzZmFpbDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuICAgICAgICAgICAgICAgIHZhbGlkYXRpb25UZXh0LnNob3coKTtcclxuICAgICAgICAgICAgICAgIGZpbGVuYW1lSW5wdXQudmFsKFwiXCIpO1xyXG4gICAgICAgICAgICAgICAgZmlsZW5hbWVUZXh0LnRleHQoXCJcIik7XHJcbiAgICAgICAgICAgICAgICBzZWxlY3RlZEZpbGVXcmFwcGVyLmhpZGUoKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgY2hhbmdlOiBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHByb21wdCA9IGZpbGVJbnB1dC5kYXRhKFwicHJvbXB0XCIpO1xyXG4gICAgICAgICAgICAgICAgaWYgKHByb21wdCAmJiBwcm9tcHQubGVuZ3RoID4gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghY29uZmlybShwcm9tcHQpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGUucHJldmVudERlZmF1bHQoKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgY2FuY2VsTGluay5vbihcImNsaWNrXCIsIGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgIGUucHJldmVudERlZmF1bHQoKTtcclxuXHJcbiAgICAgICAgICAgIGlmIChjb25maXJtKCQodGhpcykuZGF0YShcImNhbmNlbC1wcm9tcHRcIikpKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgeGhyID0gc2NvcGUuZGF0YShcInhoclwiKTtcclxuICAgICAgICAgICAgICAgIHhoci5hYm9ydCgpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcbiAgICB9XHJcblxyXG4gICAgZXhwb3J0IGZ1bmN0aW9uIGluaXRpYWxpemVVcGxvYWREaXJlY3QoKSB7XHJcblxyXG4gICAgICAgIHZhciBzY29wZSA9ICQoXCIudXBsb2FkLWRpcmVjdFwiKS5zaG93KCk7XHJcblxyXG4gICAgICAgIHNjb3BlLmZpbmQoXCIucmVzZXQtYnV0dG9uXCIpLm9uKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgdmFyIGRvUmVzZXQgPSAoPGFueT4kKHRoaXMpLmNsb3Nlc3QoXCIuYXN5bmMtdXBsb2FkXCIpWzBdKS5kb1Jlc2V0O1xyXG4gICAgICAgICAgICBpZiAoISFkb1Jlc2V0KVxyXG4gICAgICAgICAgICAgICAgZG9SZXNldCgpO1xyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBzY29wZS5maW5kKFwiLnVwbG9hZC1idXR0b25cIikub24oXCJjbGlja1wiLCBmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICB2YXIgZG9VcGxvYWQgPSAoPGFueT4kKHRoaXMpLmNsb3Nlc3QoXCIuYXN5bmMtdXBsb2FkXCIpWzBdKS5kb1VwbG9hZDtcclxuICAgICAgICAgICAgaWYgKCEhZG9VcGxvYWQpXHJcbiAgICAgICAgICAgICAgICBkb1VwbG9hZCgpO1xyXG4gICAgICAgIH0pOyBcclxuICAgICAgICAgIFxyXG4gICAgICAgIHJlcXVpcmVkVXBsb2FkcyA9IHNjb3BlLmZpbmQoXCIucmVxdWlyZWQtdXBsb2FkXCIpO1xyXG5cclxuICAgICAgICBzY29wZS5maW5kKFwiLmFzeW5jLXVwbG9hZC1maWxlLWlucHV0XCIpLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpbml0aWFsaXplVXBsb2FkKCQodGhpcykpO1xyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBzY29wZS5jbG9zZXN0KFwiZm9ybVwiKS5vbihcInN1Ym1pdFwiLCBmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICBpZiAoaGFzQWN0aXZlVXBsb2FkcygpKSB7XHJcbiAgICAgICAgICAgICAgICBhbGVydChzY29wZS5kYXRhKFwiYmxvY2stc3VibWl0LXByb21wdFwiKSk7XHJcbiAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJpcy1zdWJtaXR0aW5nXCIsIHRydWUpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgICBcclxuICAgICAgICAkKHdpbmRvdykub24oXCJiZWZvcmV1bmxvYWRcIiwgZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgaWYgKChoYXNBY3RpdmVVcGxvYWRzKCkgfHwgaGFzQ29tcGxldGVkVXBsb2FkcygpKSAmJiAhaXNTdWJtaXR0aW5nKCkpIHtcclxuICAgICAgICAgICAgICAgIHZhciBtZXNzYWdlID0gc2NvcGUuZGF0YShcIm5hdmlnYXRlLWF3YXktcHJvbXB0XCIpO1xyXG4gICAgICAgICAgICAgICAgZS5yZXN1bHQgPSBtZXNzYWdlO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIG1lc3NhZ2U7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuICAgIH1cclxufSJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==