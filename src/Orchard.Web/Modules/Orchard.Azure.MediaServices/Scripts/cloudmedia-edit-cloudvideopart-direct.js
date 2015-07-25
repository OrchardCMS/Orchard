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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtZWRpdC1jbG91ZHZpZGVvcGFydC1kaXJlY3QudHMiXSwibmFtZXMiOlsiT3JjaGFyZCIsIk9yY2hhcmQuQXp1cmUiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQudXBsb2FkQ29tcGxldGVkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmNsZWFudXAiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQucGFkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmNyZWF0ZUJsb2NrSWQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQuY29tbWl0QmxvY2tMaXN0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0Lmhhc0FjdGl2ZVVwbG9hZHMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQuaGFzQ29tcGxldGVkVXBsb2FkcyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdC5pc1N1Ym1pdHRpbmciLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQuaW5pdGlhbGl6ZVVwbG9hZCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdC5pbml0aWFsaXplVXBsb2FkRGlyZWN0Il0sIm1hcHBpbmdzIjoiQUFBQSw0Q0FBNEM7QUFDNUMsOENBQThDO0FBQzlDLDRDQUE0QztBQUU1QyxJQUFPLE9BQU8sQ0FpV2I7QUFqV0QsV0FBTyxPQUFPO0lBQUNBLElBQUFBLEtBQUtBLENBaVduQkE7SUFqV2NBLFdB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QUMsT0FBTyxDQUFDO3dCQUNuQixDQUFDO29CQUNMLENBQUMsQ0FBQ0EsQ0FBQ0E7Z0JBQ1BBLENBQUNBO2dCQXZDZVYscUNBQXNCQSx5QkF1Q3JDQSxDQUFBQTtZQUNMQSxDQUFDQSxFQWpXa0NELGNBQWNBLEdBQWRBLDRCQUFjQSxLQUFkQSw0QkFBY0EsUUFpV2hEQTtRQUFEQSxDQUFDQSxFQWpXb0JELGFBQWFBLEdBQWJBLG1CQUFhQSxLQUFiQSxtQkFBYUEsUUFpV2pDQTtJQUFEQSxDQUFDQSxFQWpXY0QsS0FBS0EsR0FBTEEsYUFBS0EsS0FBTEEsYUFBS0EsUUFpV25CQTtBQUFEQSxDQUFDQSxFQWpXTSxPQUFPLEtBQVAsT0FBTyxRQWlXYiIsImZpbGUiOiJjbG91ZG1lZGlhLWVkaXQtY2xvdWR2aWRlb3BhcnQtZGlyZWN0LmpzIiwic291cmNlc0NvbnRlbnQiOlsiLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5LmQudHNcIiAvPlxuLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5dWkuZC50c1wiIC8+XG4vLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9tb21lbnQuZC50c1wiIC8+XG5cbm1vZHVsZSBPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQge1xuICAgIHZhciByZXF1aXJlZFVwbG9hZHM6IEpRdWVyeTtcblxuICAgIGZ1bmN0aW9uIHVwbG9hZENvbXBsZXRlZChzZW5kZXIsIGUsIGRhdGEpIHtcbiAgICAgICAgdmFyIHNjb3BlID0gJChzZW5kZXIpLmNsb3Nlc3QoXCIuYXN5bmMtdXBsb2FkXCIpO1xuICAgICAgICB2YXIgc3RhdHVzID0gZGF0YS5lcnJvclRocm93biAmJiBkYXRhLmVycm9yVGhyb3duLmxlbmd0aCA+IDAgPyBkYXRhLmVycm9yVGhyb3duIDogZGF0YS50ZXh0U3RhdHVzO1xuICAgICAgICBzY29wZS5maW5kKFwiLnByb2dyZXNzLWJhclwiKS5oaWRlKCk7XG4gICAgICAgIHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtdGV4dFwiKS5oaWRlKCk7XG4gICAgICAgIHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtZGV0YWlsc1wiKS5oaWRlKCk7XG4gICAgICAgIHNjb3BlLmZpbmQoXCIuc3RhdHVzLnByZXBhcmluZ1wiKS5oaWRlKCk7XG4gICAgICAgIHNjb3BlLmZpbmQoXCIuc3RhdHVzLnVwbG9hZGluZ1wiKS5oaWRlKCk7XG5cbiAgICAgICAgc3dpdGNoIChzdGF0dXMpIHtcbiAgICAgICAgICAgIGNhc2UgXCJlcnJvclwiOlxuICAgICAgICAgICAgICAgIGNsZWFudXAoc2NvcGUsIGRhdGEpO1xuICAgICAgICAgICAgICAgIGFsZXJ0KFwiVGhlIHVwbG9hZCBvZiB0aGUgc2VsZWN0ZWQgZmlsZSBmYWlsZWQuIFlvdSBtYXkgdHJ5IGFnYWluIGFmdGVyIHRoZSBjbGVhbnVwIGhhcyBmaW5pc2hlZC5cIik7XG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgY2FzZSBcImFib3J0XCI6XG4gICAgICAgICAgICAgICAgY2xlYW51cChzY29wZSwgZGF0YSk7XG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICB9XG5cbiAgICAgICAgdmFyIGVkaXRlZEZpbGVOYW1lID0gc2NvcGUuZmluZChcImlucHV0W25hbWUkPScuRmlsZU5hbWUnXVwiKS52YWwoKTtcbiAgICAgICAgdmFyIHN0YXR1c1VwbG9hZGVkID0gc2NvcGUuZmluZChcIi5zdGF0dXMudXBsb2FkZWRcIikuc2hvdygpO1xuXG4gICAgICAgIHN0YXR1c1VwbG9hZGVkLnRleHQoc3RhdHVzVXBsb2FkZWQuZGF0YShcInRleHQtdGVtcGxhdGVcIikucmVwbGFjZShcIntmaWxlbmFtZX1cIiwgZWRpdGVkRmlsZU5hbWUpKTtcbiAgICAgICAgc2NvcGUuZGF0YShcInVwbG9hZC1pc2FjdGl2ZVwiLCBmYWxzZSk7XG4gICAgICAgIHNjb3BlLmRhdGEoXCJ1cGxvYWQtaXNjb21wbGV0ZWRcIiwgdHJ1ZSk7XG4gICAgICAgIHNjb3BlLmRhdGEoXCJ1cGxvYWQtc3RhcnQtdGltZVwiLCBudWxsKTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBjbGVhbnVwKHNjb3BlOiBKUXVlcnksIGRhdGEpIHtcbiAgICAgICAgdmFyIHdhbXNBc3NldElucHV0ID0gc2NvcGUuZmluZChcImlucHV0W25hbWUkPScuV2Ftc0Fzc2V0SWQnXVwiKTtcbiAgICAgICAgdmFyIGZpbGVOYW1lSW5wdXQgPSBzY29wZS5maW5kKFwiaW5wdXRbbmFtZSQ9Jy5GaWxlTmFtZSddXCIpO1xuICAgICAgICB2YXIgYXNzZXRJZCA9ICQudHJpbSh3YW1zQXNzZXRJbnB1dC52YWwoKSk7XG4gICAgICAgIHZhciBmaWxlVXBsb2FkV3JhcHBlciA9IGRhdGEuZmlsZUlucHV0LmNsb3Nlc3QoXCIuZmlsZS11cGxvYWQtd3JhcHBlclwiKTtcblxuICAgICAgICBpZiAoYXNzZXRJZC5sZW5ndGggPiAwKSB7XG4gICAgICAgICAgICB2YXIgdXJsID0gc2NvcGUuZGF0YShcImRlbGV0ZS1hc3NldC11cmxcIik7XG4gICAgICAgICAgICB2YXIgYW50aUZvcmdlcnlUb2tlbiA9IHNjb3BlLmNsb3Nlc3QoXCJmb3JtXCIpLmZpbmQoXCJbbmFtZT0nX19SZXF1ZXN0VmVyaWZpY2F0aW9uVG9rZW4nXVwiKS52YWwoKTtcbiAgICAgICAgICAgIHZhciBjbGVhbnVwTWVzc2FnZSA9IHNjb3BlLmZpbmQoXCIuc3RhdHVzLmNsZWFudXBcIik7XG5cbiAgICAgICAgICAgIHdhbXNBc3NldElucHV0LnZhbChcIlwiKTtcbiAgICAgICAgICAgIGZpbGVOYW1lSW5wdXQudmFsKFwiXCIpO1xuXG4gICAgICAgICAgICBjbGVhbnVwTWVzc2FnZS5zaG93KCk7XG5cbiAgICAgICAgICAgICQuYWpheCh7XG4gICAgICAgICAgICAgICAgdXJsOiB1cmwsXG4gICAgICAgICAgICAgICAgdHlwZTogXCJERUxFVEVcIixcbiAgICAgICAgICAgICAgICBkYXRhOiB7XG4gICAgICAgICAgICAgICAgICAgIGlkOiBhc3NldElkLFxuICAgICAgICAgICAgICAgICAgICBfX1JlcXVlc3RWZXJpZmljYXRpb25Ub2tlbjogYW50aUZvcmdlcnlUb2tlblxuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH0pLmRvbmUoZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJ1cGxvYWQtaXNhY3RpdmVcIiwgZmFsc2UpO1xuICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJ1cGxvYWQtc3RhcnQtdGltZVwiLCBudWxsKTtcbiAgICAgICAgICAgICAgICBzY29wZS5maW5kKFwiLmZpbGUtdXBsb2FkLXdyYXBwZXJcIikuc2hvdygpO1xuICAgICAgICAgICAgICAgIGNsZWFudXBNZXNzYWdlLmhpZGUoKTtcbiAgICAgICAgICAgIH0pLmZhaWwoZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIGFsZXJ0KFwiQW4gZXJyb3Igb2NjdXJyZWQgb24gdGhlIHNlcnZlciB3aGlsZSB0cnlpbmcgdG8gY2xlYW4gdXAuXCIpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgIH1cblxuICAgICAgICBmaWxlVXBsb2FkV3JhcHBlci5zaG93KCk7XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gcGFkKHZhbHVlOiBudW1iZXIsIGxlbmd0aDogbnVtYmVyKSB7XG4gICAgICAgIHZhciBzdHIgPSB2YWx1ZS50b1N0cmluZygpO1xuICAgICAgICB3aGlsZSAoc3RyLmxlbmd0aCA8IGxlbmd0aCkge1xuICAgICAgICAgICAgc3RyID0gXCIwXCIgKyBzdHI7XG4gICAgICAgIH1cbiAgICAgICAgcmV0dXJuIHN0cjtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBjcmVhdGVCbG9ja0lkKGJsb2NrSW5kZXg6IG51bWJlcikge1xuICAgICAgICB2YXIgYmxvY2tJZFByZWZpeCA9IFwiYmxvY2stXCI7XG4gICAgICAgIHJldHVybiBidG9hKGJsb2NrSWRQcmVmaXggKyBwYWQoYmxvY2tJbmRleCwgNikpO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIGNvbW1pdEJsb2NrTGlzdChzY29wZTogSlF1ZXJ5LCBkYXRhKSB7XG4gICAgICAgIHZhciBkZWZlcnJlZCA9ICQuRGVmZXJyZWQoKTtcbiAgICAgICAgdmFyIGJsb2NrSWRzID0gc2NvcGUuZGF0YShcImJsb2NrLWlkc1wiKTtcblxuICAgICAgICBpZiAoYmxvY2tJZHMubGVuZ3RoID09IDApIHtcbiAgICAgICAgICAgIC8vIFRoZSBmaWxlIHdhcyB1cGxvYWRlZCBhcyBhIHdob2xlLCBzbyBubyBtYW5pZmVzdCB0byBzdWJtaXQuXG4gICAgICAgICAgICBkZWZlcnJlZC5yZXNvbHZlKCk7XG4gICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICAvLyBUaGUgZmlsZSB3YXMgdXBsb2FkZWQgaW4gY2h1bmtzLlxuICAgICAgICAgICAgdmFyIHVybCA9IHNjb3BlLmRhdGEoXCJzYXMtbG9jYXRvclwiKSArIFwiJmNvbXA9YmxvY2tsaXN0XCI7XG4gICAgICAgICAgICB2YXIgcmVxdWVzdERhdGEgPSAnPD94bWwgdmVyc2lvbj1cIjEuMFwiIGVuY29kaW5nPVwidXRmLThcIj8+PEJsb2NrTGlzdD4nO1xuICAgICAgICAgICAgZm9yICh2YXIgaSA9IDA7IGkgPCBibG9ja0lkcy5sZW5ndGg7IGkrKykge1xuICAgICAgICAgICAgICAgIHJlcXVlc3REYXRhICs9ICc8TGF0ZXN0PicgKyBibG9ja0lkc1tpXSArICc8L0xhdGVzdD4nO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcmVxdWVzdERhdGEgKz0gJzwvQmxvY2tMaXN0Pic7XG5cbiAgICAgICAgICAgICQuYWpheCh7XG4gICAgICAgICAgICAgICAgdXJsOiB1cmwsXG4gICAgICAgICAgICAgICAgdHlwZTogXCJQVVRcIixcbiAgICAgICAgICAgICAgICBkYXRhOiByZXF1ZXN0RGF0YSxcbiAgICAgICAgICAgICAgICBjb250ZW50VHlwZTogXCJ0ZXh0L3BsYWluOyBjaGFyc2V0PVVURi04XCIsXG4gICAgICAgICAgICAgICAgY3Jvc3NEb21haW46IHRydWUsXG4gICAgICAgICAgICAgICAgY2FjaGU6IGZhbHNlLFxuICAgICAgICAgICAgICAgIGJlZm9yZVNlbmQ6IGZ1bmN0aW9uICh4aHIpIHtcbiAgICAgICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoJ3gtbXMtZGF0ZScsIG5ldyBEYXRlKCkudG9VVENTdHJpbmcoKSk7XG4gICAgICAgICAgICAgICAgICAgIHhoci5zZXRSZXF1ZXN0SGVhZGVyKCd4LW1zLWJsb2ItY29udGVudC10eXBlJywgZGF0YS5maWxlc1swXS50eXBlKTtcbiAgICAgICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoJ3gtbXMtdmVyc2lvbicsIFwiMjAxMi0wMi0xMlwiKTtcbiAgICAgICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoJ0NvbnRlbnQtTGVuZ3RoJywgcmVxdWVzdERhdGEubGVuZ3RoLnRvU3RyaW5nKCkpO1xuICAgICAgICAgICAgICAgIH0sXG4gICAgICAgICAgICAgICAgc3VjY2VzczogZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICBkZWZlcnJlZC5yZXNvbHZlKGRhdGEpO1xuICAgICAgICAgICAgICAgIH0sXG4gICAgICAgICAgICAgICAgZXJyb3I6IGZ1bmN0aW9uICh4aHIsIHN0YXR1cywgZXJyb3IpIHtcbiAgICAgICAgICAgICAgICAgICAgZGF0YS50ZXh0U3RhdHVzID0gc3RhdHVzO1xuICAgICAgICAgICAgICAgICAgICBkYXRhLmVycm9yVGhyb3duID0gZXJyb3I7XG4gICAgICAgICAgICAgICAgICAgIGRlZmVycmVkLmZhaWwoZGF0YSk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfSk7XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gZGVmZXJyZWQucHJvbWlzZSgpO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIGhhc0FjdGl2ZVVwbG9hZHMoKSB7XG4gICAgICAgIHZhciBzY29wZSA9ICQoXCIudXBsb2FkLWRpcmVjdFwiKTtcbiAgICAgICAgdmFyIGZsYWcgPSBmYWxzZTtcblxuICAgICAgICBzY29wZS5maW5kKFwiLmFzeW5jLXVwbG9hZFwiKS5lYWNoKGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICgkKHRoaXMpLmRhdGEoXCJ1cGxvYWQtaXNhY3RpdmVcIikgPT0gdHJ1ZSkge1xuICAgICAgICAgICAgICAgIGZsYWcgPSB0cnVlO1xuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfSk7XG5cbiAgICAgICAgcmV0dXJuIGZsYWc7XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gaGFzQ29tcGxldGVkVXBsb2FkcygpIHtcbiAgICAgICAgdmFyIHNjb3BlID0gJChcIi51cGxvYWQtZGlyZWN0XCIpO1xuICAgICAgICB2YXIgZmxhZyA9IGZhbHNlO1xuXG4gICAgICAgIHNjb3BlLmZpbmQoXCIuYXN5bmMtdXBsb2FkXCIpLmVhY2goZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCQodGhpcykuZGF0YShcInVwbG9hZC1pc2NvbXBsZXRlZFwiKSA9PSB0cnVlKSB7XG4gICAgICAgICAgICAgICAgZmxhZyA9IHRydWU7XG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgfVxuICAgICAgICB9KTtcblxuICAgICAgICByZXR1cm4gZmxhZztcbiAgICB9IFxuXG4gICAgZnVuY3Rpb24gaXNTdWJtaXR0aW5nKCkge1xuICAgICAgICB2YXIgc2NvcGUgPSAkKFwiLnVwbG9hZC1kaXJlY3RcIik7XG4gICAgICAgIHJldHVybiBzY29wZS5kYXRhKFwiaXMtc3VibWl0dGluZ1wiKSA9PSB0cnVlO1xuICAgIH07XG5cbiAgICBmdW5jdGlvbiBpbml0aWFsaXplVXBsb2FkKGZpbGVJbnB1dDogSlF1ZXJ5KSB7XG4gICAgICAgIHZhciBzY29wZSA9IGZpbGVJbnB1dC5jbG9zZXN0KFwiLmFzeW5jLXVwbG9hZFwiKTtcbiAgICAgICAgdmFyIGZpbGVVcGxvYWRXcmFwcGVyID0gc2NvcGUuZmluZChcIi5maWxlLXVwbG9hZC13cmFwcGVyXCIpO1xuICAgICAgICB2YXIgYWNjZXB0RmlsZVR5cGVzUmVnZXggPSBuZXcgUmVnRXhwKHNjb3BlLmRhdGEoXCJ1cGxvYWQtYWNjZXB0LWZpbGUtdHlwZXNcIikpO1xuICAgICAgICB2YXIgYW50aUZvcmdlcnlUb2tlbjogc3RyaW5nID0gc2NvcGUuY2xvc2VzdChcImZvcm1cIikuZmluZChcIltuYW1lPSdfX1JlcXVlc3RWZXJpZmljYXRpb25Ub2tlbiddXCIpLnZhbCgpO1xuXG4gICAgICAgIHZhciBzZWxlY3RlZEZpbGVXcmFwcGVyID0gc2NvcGUuZmluZChcIi5zZWxlY3RlZC1maWxlLXdyYXBwZXJcIik7XG4gICAgICAgIHZhciBmaWxlbmFtZUlucHV0ID0gc2NvcGUuZmluZChcIi5maWxlbmFtZS1pbnB1dFwiKTtcbiAgICAgICAgdmFyIHJlc2V0QnV0dG9uID0gc2NvcGUuZmluZChcIi5yZXNldC1idXR0b25cIik7XG4gICAgICAgIHZhciB1cGxvYWRCdXR0b24gPSBzY29wZS5maW5kKFwiLnVwbG9hZC1idXR0b25cIik7XG4gICAgICAgIHZhciBmaWxlbmFtZVRleHQgPSBzY29wZS5maW5kKFwiLmZpbGVuYW1lLXRleHRcIik7XG5cbiAgICAgICAgdmFyIHZhbGlkYXRpb25UZXh0ID0gc2NvcGUuZmluZChcIi52YWxpZGF0aW9uLXRleHRcIik7XG4gICAgICAgIHZhciBwcmVwYXJpbmdUZXh0ID0gc2NvcGUuZmluZChcIi5zdGF0dXMucHJlcGFyaW5nXCIpO1xuICAgICAgICB2YXIgdXBsb2FkaW5nQ29udGFpbmVyID0gc2NvcGUuZmluZChcIi5zdGF0dXMudXBsb2FkaW5nXCIpO1xuICAgICAgICB2YXIgcHJvZ3Jlc3NCYXIgPSBzY29wZS5maW5kKFwiLnByb2dyZXNzLWJhclwiKTtcbiAgICAgICAgdmFyIHByb2dyZXNzVGV4dCA9IHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtdGV4dFwiKTtcbiAgICAgICAgdmFyIHByb2dyZXNzRGV0YWlscyA9IHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtZGV0YWlsc1wiKTtcbiAgICAgICAgdmFyIGNhbmNlbExpbmsgPSBzY29wZS5maW5kKFwiLmNhbmNlbC1saW5rXCIpOyBcblxuICAgICAgICAoPGFueT5maWxlSW5wdXQpLmZpbGV1cGxvYWQoe1xuICAgICAgICAgICAgYXV0b1VwbG9hZDogZmFsc2UsXG4gICAgICAgICAgICBhY2NlcHRGaWxlVHlwZXM6IGFjY2VwdEZpbGVUeXBlc1JlZ2V4LFxuICAgICAgICAgICAgdHlwZTogXCJQVVRcIixcbiAgICAgICAgICAgIG1heENodW5rU2l6ZTogNCAqIDEwMjQgKiAxMDI0LCAvLyA0IE1CXG4gICAgICAgICAgICBiZWZvcmVTZW5kOiAoeGhyOiBKUXVlcnlYSFIsIGRhdGEpID0+IHtcbiAgICAgICAgICAgICAgICB4aHIuc2V0UmVxdWVzdEhlYWRlcihcIngtbXMtZGF0ZVwiLCBuZXcgRGF0ZSgpLnRvVVRDU3RyaW5nKCkpO1xuICAgICAgICAgICAgICAgIHhoci5zZXRSZXF1ZXN0SGVhZGVyKFwieC1tcy1ibG9iLXR5cGVcIiwgXCJCbG9ja0Jsb2JcIik7XG4gICAgICAgICAgICAgICAgeGhyLnNldFJlcXVlc3RIZWFkZXIoXCJjb250ZW50LWxlbmd0aFwiLCBkYXRhLmRhdGEuc2l6ZS50b1N0cmluZygpKTtcbiAgICAgICAgICAgIH0sXG4gICAgICAgICAgICBjaHVua3NlbmQ6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XG4gICAgICAgICAgICAgICAgdmFyIGJsb2NrSW5kZXggPSBzY29wZS5kYXRhKFwiYmxvY2staW5kZXhcIik7XG4gICAgICAgICAgICAgICAgdmFyIGJsb2NrSWRzID0gc2NvcGUuZGF0YShcImJsb2NrLWlkc1wiKTtcbiAgICAgICAgICAgICAgICB2YXIgYmxvY2tJZCA9IGNyZWF0ZUJsb2NrSWQoYmxvY2tJbmRleCk7XG4gICAgICAgICAgICAgICAgdmFyIHVybCA9IHNjb3BlLmRhdGEoXCJzYXMtbG9jYXRvclwiKSArIFwiJmNvbXA9YmxvY2smYmxvY2tpZD1cIiArIGJsb2NrSWQ7XG5cbiAgICAgICAgICAgICAgICBkYXRhLnVybCA9IHVybDtcbiAgICAgICAgICAgICAgICBibG9ja0lkcy5wdXNoKGJsb2NrSWQpO1xuICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJibG9jay1pbmRleFwiLCBibG9ja0luZGV4ICsgMSk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgcHJvZ3Jlc3NhbGw6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XG4gICAgICAgICAgICAgICAgdmFyIHBlcmNlbnRDb21wbGV0ZSA9IE1hdGguZmxvb3IoKGRhdGEubG9hZGVkIC8gZGF0YS50b3RhbCkgKiAxMDApO1xuICAgICAgICAgICAgICAgIHZhciBzdGFydFRpbWUgPSBuZXcgRGF0ZShzY29wZS5kYXRhKFwidXBsb2FkLXN0YXJ0LXRpbWVcIikpO1xuICAgICAgICAgICAgICAgIHZhciBlbGFwc2VkTWlsbGlzZWNvbmRzID0gbmV3IERhdGUoRGF0ZS5ub3coKSkuZ2V0VGltZSgpIC0gc3RhcnRUaW1lLmdldFRpbWUoKTtcbiAgICAgICAgICAgICAgICB2YXIgZWxhcHNlZCA9IG1vbWVudC5kdXJhdGlvbihlbGFwc2VkTWlsbGlzZWNvbmRzLCBcIm1zXCIpO1xuICAgICAgICAgICAgICAgIHZhciByZW1haW5pbmcgPSBtb21lbnQuZHVyYXRpb24oZWxhcHNlZE1pbGxpc2Vjb25kcyAvIE1hdGgubWF4KGRhdGEubG9hZGVkLCAxKSAqIChkYXRhLnRvdGFsIC0gZGF0YS5sb2FkZWQpLCBcIm1zXCIpO1xuICAgICAgICAgICAgICAgIHZhciBrYnBzID0gTWF0aC5mbG9vcihkYXRhLmJpdHJhdGUgLyA4IC8gMTAwMCk7XG4gICAgICAgICAgICAgICAgdmFyIHVwbG9hZGVkID0gTWF0aC5mbG9vcihkYXRhLmxvYWRlZCAvIDEwMDApO1xuICAgICAgICAgICAgICAgIHZhciB0b3RhbCA9IE1hdGguZmxvb3IoZGF0YS50b3RhbCAvIDEwMDApO1xuXG4gICAgICAgICAgICAgICAgcHJvZ3Jlc3NCYXIuc2hvdygpLmZpbmQoXCIucHJvZ3Jlc3NcIikuY3NzKFwid2lkdGhcIiwgcGVyY2VudENvbXBsZXRlICsgXCIlXCIpO1xuICAgICAgICAgICAgICAgIHByb2dyZXNzVGV4dC50ZXh0KHByb2dyZXNzVGV4dC5kYXRhKFwidGV4dC10ZW1wbGF0ZVwiKS5yZXBsYWNlKFwie3BlcmNlbnRhZ2V9XCIsIHBlcmNlbnRDb21wbGV0ZSkpLnNob3coKTtcbiAgICAgICAgICAgICAgICBwcm9ncmVzc0RldGFpbHMudGV4dChwcm9ncmVzc0RldGFpbHMuZGF0YShcInRleHQtdGVtcGxhdGVcIikucmVwbGFjZShcInt1cGxvYWRlZH1cIiwgdXBsb2FkZWQpLnJlcGxhY2UoXCJ7dG90YWx9XCIsIHRvdGFsKS5yZXBsYWNlKFwie2ticHN9XCIsIGticHMpLnJlcGxhY2UoXCJ7ZWxhcHNlZH1cIiwgZWxhcHNlZC5odW1hbml6ZSgpKS5yZXBsYWNlKFwie3JlbWFpbmluZ31cIiwgcmVtYWluaW5nLmh1bWFuaXplKCkpKS5zaG93KCk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgZG9uZTogZnVuY3Rpb24gKGUsIGRhdGEpIHtcbiAgICAgICAgICAgICAgICB2YXIgc2VsZiA9IHRoaXM7XG4gICAgICAgICAgICAgICAgY29tbWl0QmxvY2tMaXN0KHNjb3BlLCBkYXRhKS5hbHdheXMoZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICB1cGxvYWRDb21wbGV0ZWQoc2VsZiwgZSwgZGF0YSk7XG4gICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgZmFpbDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcbiAgICAgICAgICAgICAgICB1cGxvYWRDb21wbGV0ZWQodGhpcywgZSwgZGF0YSk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgcHJvY2Vzc2RvbmU6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XG4gICAgICAgICAgICAgICAgdmFyIHNlbGVjdGVkRmlsZW5hbWUgPSBkYXRhLmZpbGVzWzBdLm5hbWU7XG4gICAgICAgICAgICAgICAgc2NvcGUuZGF0YShcInNlbGVjdGVkLWZpbGVuYW1lXCIsIHNlbGVjdGVkRmlsZW5hbWUpO1xuICAgICAgICAgICAgICAgIHdpbmRvdy5zZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgZmlsZVVwbG9hZFdyYXBwZXIuaGlkZSgpO1xuICAgICAgICAgICAgICAgICAgICB2YWxpZGF0aW9uVGV4dC5oaWRlKCk7XG4gICAgICAgICAgICAgICAgICAgIHNlbGVjdGVkRmlsZVdyYXBwZXIuc2hvdygpO1xuICAgICAgICAgICAgICAgICAgICBmaWxlbmFtZVRleHQudGV4dChmaWxlbmFtZVRleHQuZGF0YShcInRleHQtdGVtcGxhdGVcIikucmVwbGFjZShcIntmaWxlbmFtZX1cIiwgc2VsZWN0ZWRGaWxlbmFtZSkpO1xuICAgICAgICAgICAgICAgIH0sIDEwKTsgXG5cbiAgICAgICAgICAgICAgICAoPGFueT5zY29wZVswXSkuZG9SZXNldCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgZmlsZVVwbG9hZFdyYXBwZXIuc2hvdygpO1xuICAgICAgICAgICAgICAgICAgICBmaWxlbmFtZUlucHV0LnZhbChcIlwiKTtcbiAgICAgICAgICAgICAgICAgICAgZmlsZW5hbWVUZXh0LnRleHQoXCJcIik7XG4gICAgICAgICAgICAgICAgICAgIHNlbGVjdGVkRmlsZVdyYXBwZXIuaGlkZSgpO1xuICAgICAgICAgICAgICAgICAgICB2YWxpZGF0aW9uVGV4dC5oaWRlKCk7XG4gICAgICAgICAgICAgICAgfTtcblxuICAgICAgICAgICAgICAgICg8YW55PnNjb3BlWzBdKS5kb1VwbG9hZCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIGVkaXRlZEZpbGVuYW1lID0gZmlsZW5hbWVJbnB1dC52YWwoKSB8fCBzZWxlY3RlZEZpbGVuYW1lO1xuICAgICAgICAgICAgICAgICAgICBpZiAoIWFjY2VwdEZpbGVUeXBlc1JlZ2V4LnRlc3QoZWRpdGVkRmlsZW5hbWUpKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICB2YWxpZGF0aW9uVGV4dC5zaG93KCk7XG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm47XG4gICAgICAgICAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLWlzYWN0aXZlXCIsIHRydWUpO1xuICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLXN0YXJ0LXRpbWVcIiwgRGF0ZS5ub3coKSk7XG4gICAgICAgICAgICAgICAgICAgIHZhciBnZW5lcmF0ZUFzc2V0VXJsID0gc2NvcGUuZGF0YShcImdlbmVyYXRlLWFzc2V0LXVybFwiKTtcbiAgICAgICAgICAgICAgICAgICAgc2NvcGUuZGF0YShcImJsb2NrLWluZGV4XCIsIDApO1xuICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwiYmxvY2staWRzXCIsIG5ldyBBcnJheSgpKTtcblxuICAgICAgICAgICAgICAgICAgICBwcmVwYXJpbmdUZXh0LnNob3coKTtcbiAgICAgICAgICAgICAgICAgICAgc2VsZWN0ZWRGaWxlV3JhcHBlci5oaWRlKCk7XG4gICAgICAgICAgICAgICAgICAgIHZhbGlkYXRpb25UZXh0LmhpZGUoKTtcblxuICAgICAgICAgICAgICAgICAgICAkLmFqYXgoe1xuICAgICAgICAgICAgICAgICAgICAgICAgdXJsOiBnZW5lcmF0ZUFzc2V0VXJsLFxuICAgICAgICAgICAgICAgICAgICAgICAgZGF0YToge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZpbGVuYW1lOiBlZGl0ZWRGaWxlbmFtZSxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBfX1JlcXVlc3RWZXJpZmljYXRpb25Ub2tlbjogYW50aUZvcmdlcnlUb2tlblxuICAgICAgICAgICAgICAgICAgICAgICAgfSxcbiAgICAgICAgICAgICAgICAgICAgICAgIHR5cGU6IFwiUE9TVFwiXG4gICAgICAgICAgICAgICAgICAgIH0pLmRvbmUoZnVuY3Rpb24gKGFzc2V0KSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZGF0YS51cmwgPSBhc3NldC5zYXNMb2NhdG9yO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRhdGEubXVsdGlwYXJ0ID0gZmFsc2U7XG5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwic2FzLWxvY2F0b3JcIiwgYXNzZXQuc2FzTG9jYXRvcik7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZmluZChcImlucHV0W25hbWUkPScuRmlsZU5hbWUnXVwiKS52YWwoZWRpdGVkRmlsZW5hbWUpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLmZpbmQoXCJpbnB1dFtuYW1lJD0nLldhbXNBc3NldElkJ11cIikudmFsKGFzc2V0LmFzc2V0SWQpO1xuXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcHJlcGFyaW5nVGV4dC5oaWRlKCk7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcHJvZ3Jlc3NUZXh0LnRleHQocHJvZ3Jlc3NUZXh0LmRhdGEoXCJ0ZXh0LXRlbXBsYXRlXCIpLnJlcGxhY2UoXCJ7cGVyY2VudGFnZX1cIiwgMCkpLnNob3coKTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB1cGxvYWRpbmdDb250YWluZXIuc2hvdygpO1xuXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHhociA9IGRhdGEuc3VibWl0KCk7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZGF0YShcInhoclwiLCB4aHIpO1xuICAgICAgICAgICAgICAgICAgICAgICAgfSkuZmFpbChmdW5jdGlvbiAoeGhyLCBzdGF0dXMsIGVycm9yKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZmlsZVVwbG9hZFdyYXBwZXIuc2hvdygpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdGVkRmlsZVdyYXBwZXIuc2hvdygpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHByZXBhcmluZ1RleHQuaGlkZSgpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHVwbG9hZGluZ0NvbnRhaW5lci5oaWRlKCk7XG5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLWlzYWN0aXZlXCIsIGZhbHNlKTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLXN0YXJ0LXRpbWVcIiwgbnVsbCk7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgYWxlcnQoXCJBbiBlcnJvciBvY2N1cnJlZC4gRXJyb3I6IFwiICsgZXJyb3IpO1xuICAgICAgICAgICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgfTtcbiAgICAgICAgICAgIH0sXG4gICAgICAgICAgICBwcm9jZXNzZmFpbDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcbiAgICAgICAgICAgICAgICB2YWxpZGF0aW9uVGV4dC5zaG93KCk7XG4gICAgICAgICAgICAgICAgZmlsZW5hbWVJbnB1dC52YWwoXCJcIik7XG4gICAgICAgICAgICAgICAgZmlsZW5hbWVUZXh0LnRleHQoXCJcIik7XG4gICAgICAgICAgICAgICAgc2VsZWN0ZWRGaWxlV3JhcHBlci5oaWRlKCk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgY2hhbmdlOiBmdW5jdGlvbiAoZSwgZGF0YSkge1xuICAgICAgICAgICAgICAgIHZhciBwcm9tcHQgPSBmaWxlSW5wdXQuZGF0YShcInByb21wdFwiKTtcbiAgICAgICAgICAgICAgICBpZiAocHJvbXB0ICYmIHByb21wdC5sZW5ndGggPiAwKSB7XG4gICAgICAgICAgICAgICAgICAgIGlmICghY29uZmlybShwcm9tcHQpKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgIH0pO1xuXG4gICAgICAgIGNhbmNlbExpbmsub24oXCJjbGlja1wiLCBmdW5jdGlvbiAoZSkge1xuICAgICAgICAgICAgZS5wcmV2ZW50RGVmYXVsdCgpO1xuXG4gICAgICAgICAgICBpZiAoY29uZmlybSgkKHRoaXMpLmRhdGEoXCJjYW5jZWwtcHJvbXB0XCIpKSkge1xuICAgICAgICAgICAgICAgIHZhciB4aHIgPSBzY29wZS5kYXRhKFwieGhyXCIpO1xuICAgICAgICAgICAgICAgIHhoci5hYm9ydCgpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9KTtcbiAgICB9XG5cbiAgICBleHBvcnQgZnVuY3Rpb24gaW5pdGlhbGl6ZVVwbG9hZERpcmVjdCgpIHtcblxuICAgICAgICB2YXIgc2NvcGUgPSAkKFwiLnVwbG9hZC1kaXJlY3RcIikuc2hvdygpO1xuXG4gICAgICAgIHNjb3BlLmZpbmQoXCIucmVzZXQtYnV0dG9uXCIpLm9uKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpIHtcbiAgICAgICAgICAgIHZhciBkb1Jlc2V0ID0gKDxhbnk+JCh0aGlzKS5jbG9zZXN0KFwiLmFzeW5jLXVwbG9hZFwiKVswXSkuZG9SZXNldDtcbiAgICAgICAgICAgIGlmICghIWRvUmVzZXQpXG4gICAgICAgICAgICAgICAgZG9SZXNldCgpO1xuICAgICAgICB9KTtcblxuICAgICAgICBzY29wZS5maW5kKFwiLnVwbG9hZC1idXR0b25cIikub24oXCJjbGlja1wiLCBmdW5jdGlvbiAoZSkge1xuICAgICAgICAgICAgdmFyIGRvVXBsb2FkID0gKDxhbnk+JCh0aGlzKS5jbG9zZXN0KFwiLmFzeW5jLXVwbG9hZFwiKVswXSkuZG9VcGxvYWQ7XG4gICAgICAgICAgICBpZiAoISFkb1VwbG9hZClcbiAgICAgICAgICAgICAgICBkb1VwbG9hZCgpO1xuICAgICAgICB9KTsgXG4gICAgICAgICAgXG4gICAgICAgIHJlcXVpcmVkVXBsb2FkcyA9IHNjb3BlLmZpbmQoXCIucmVxdWlyZWQtdXBsb2FkXCIpO1xuXG4gICAgICAgIHNjb3BlLmZpbmQoXCIuYXN5bmMtdXBsb2FkLWZpbGUtaW5wdXRcIikuZWFjaChmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpbml0aWFsaXplVXBsb2FkKCQodGhpcykpO1xuICAgICAgICB9KTtcblxuICAgICAgICBzY29wZS5jbG9zZXN0KFwiZm9ybVwiKS5vbihcInN1Ym1pdFwiLCBmdW5jdGlvbiAoZSkge1xuICAgICAgICAgICAgaWYgKGhhc0FjdGl2ZVVwbG9hZHMoKSkge1xuICAgICAgICAgICAgICAgIGFsZXJ0KHNjb3BlLmRhdGEoXCJibG9jay1zdWJtaXQtcHJvbXB0XCIpKTtcbiAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICBzY29wZS5kYXRhKFwiaXMtc3VibWl0dGluZ1wiLCB0cnVlKTtcbiAgICAgICAgfSk7XG4gICAgICAgICBcbiAgICAgICAgJCh3aW5kb3cpLm9uKFwiYmVmb3JldW5sb2FkXCIsIGZ1bmN0aW9uIChlKSB7XG4gICAgICAgICAgICBpZiAoKGhhc0FjdGl2ZVVwbG9hZHMoKSB8fCBoYXNDb21wbGV0ZWRVcGxvYWRzKCkpICYmICFpc1N1Ym1pdHRpbmcoKSkge1xuICAgICAgICAgICAgICAgIHZhciBtZXNzYWdlID0gc2NvcGUuZGF0YShcIm5hdmlnYXRlLWF3YXktcHJvbXB0XCIpO1xuICAgICAgICAgICAgICAgIGUucmVzdWx0ID0gbWVzc2FnZTtcbiAgICAgICAgICAgICAgICByZXR1cm4gbWVzc2FnZTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfSk7XG4gICAgfVxufSJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==