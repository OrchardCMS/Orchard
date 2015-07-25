/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var CloudVideoEdit;
            (function (CloudVideoEdit) {
                var requiredUploads;
                var blocked;
                var hasRequiredUploadsp;
                function getAllFilesCompleted() {
                    var allFilesCompleted = true;
                    requiredUploads.find("input[name$='.OriginalFileName'], input.sync-upload-input").each(function () {
                        if ($(this).val() == "") {
                            allFilesCompleted = false;
                            return false;
                        }
                    });
                    return allFilesCompleted;
                }
                ;
                function unblockIfComplete() {
                    if (getAllFilesCompleted())
                        blocked.unblock();
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
                    var acceptFileTypes = scope.data("upload-accept-file-types");
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
                        progressall: function (e, data) {
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
                        processdone: function (e, data) {
                            scope.find(".validation-text").hide();
                            scope.data("upload-isactive", true);
                            cancelUpload.show();
                            var xhr = data.submit();
                            scope.data("xhr", xhr);
                        },
                        processfail: function (e, data) {
                            scope.find(".validation-text").show();
                        }
                    });
                    cancelUpload.on("click", function (e) {
                        e.preventDefault();
                        if (confirm("Are you sure you want to cancel this upload?")) {
                            var xhr = scope.data("xhr");
                            xhr.abort();
                        }
                    });
                }
                function initializeUploadProxied() {
                    var scopeProxied = $(".upload-proxied").show();
                    requiredUploads = scopeProxied.find(".required-uploads-group");
                    blocked = scopeProxied.find(".edit-item-sidebar");
                    hasRequiredUploadsp = requiredUploads.length > 0;
                    if (hasRequiredUploadsp) {
                        blocked.block({
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
                        window.onbeforeunload = function (e) {
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
                        scopeProxied.find(".sync-upload-input").on("change", function (e) {
                            unblockIfComplete();
                        });
                        unblockIfComplete();
                    }
                    scopeProxied.find("[data-prompt]").on("change", function (e) {
                        var sender = $(e.currentTarget);
                        if (!confirm(sender.data("prompt"))) {
                            sender.val("");
                        }
                    });
                }
                CloudVideoEdit.initializeUploadProxied = initializeUploadProxied;
            })(CloudVideoEdit = MediaServices.CloudVideoEdit || (MediaServices.CloudVideoEdit = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtZWRpdC1jbG91ZHZpZGVvcGFydC1wcm94aWVkLnRzIl0sIm5hbWVzIjpbIk9yY2hhcmQiLCJPcmNoYXJkLkF6dXJlIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmdldEFsbEZpbGVzQ29tcGxldGVkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LnVuYmxvY2tJZkNvbXBsZXRlIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LnVwbG9hZENvbXBsZXRlZCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdC5pbml0aWFsaXplVXBsb2FkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmluaXRpYWxpemVVcGxvYWRQcm94aWVkIl0sIm1hcHBpbmdzIjoiQUFBQSw0Q0FBNEM7QUFDNUMsOENBQThDO0FBRTlDLElBQU8sT0FBTyxDQTJKYjtBQTNKRCxXQUFPLE9BQU87SUFBQ0EsSUFBQUEsS0FBS0EsQ0EySm5CQTtJQTNKY0EsV0FBQUEsS0FBS0E7UUFBQ0MsSUFBQUEsYUFBYUEsQ0EySmpDQTtRQTNKb0JBLFdBQUFBLGFBQWFBO1lBQUNDLElBQUFBLGNBQWNBLENBMkpoREE7WUEzSmtDQSxXQUFBQSxjQUFjQSxFQUFDQSxDQUFDQTtnQkFFL0NDLElBQUlBLGVBQXVCQSxDQUFDQTtnQkFDNUJBLElBQUlBLE9BQWVBLENBQUNBO2dCQUNwQkEsSUFBSUEsbUJBQTRCQSxDQUFDQTtnQkFFakNBO29CQUNJQyxJQUFJQSxpQkFBaUJBLEdBQVlBLElBQUlBLENBQUNBO29CQUV0Q0EsZUFBZUEsQ0FBQ0EsSUFBSUEsQ0FBQ0EsMkRBQTJEQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQTt3QkFDbkYsRUFBRSxDQUFDLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxDQUFDLEdBQUcsRUFBRSxJQUFJLEVBQUUsQ0FBQyxDQUFDLENBQUM7NEJBQ3RCLGlCQUFpQixHQUFHLEtBQUssQ0FBQzs0QkFDMUIsTUFBTSxDQUFDLEtBQUssQ0FBQzt3QkFDakIsQ0FBQztvQkFDTCxDQUFDLENBQUNBLENBQUNBO29CQUVIQSxNQUFNQSxDQUFDQSxpQkFBaUJBLENBQUNBO2dCQUM3QkEsQ0FBQ0E7Z0JBQUFELENBQUNBO2dCQUVGQTtvQkFDSUUsRUFBRUEsQ0FBQ0EsQ0FBQ0Esb0JBQW9CQSxFQUFFQSxDQUFDQTt3QkFDakJBLE9BQVFBLENBQUNBLE9BQU9BLEVBQUVBLENBQUNBO2dCQUNqQ0EsQ0FBQ0E7Z0JBRURGLHlCQUF5QkEsTUFBTUEsRUFBRUEsSUFBSUE7b0JBQ2pDRyxJQUFJQSxLQUFLQSxHQUFHQSxDQUFDQSxDQUFDQSxNQUFNQSxDQUFDQSxDQUFDQSxPQUFPQSxDQUFDQSxpQ0FBaUNBLENBQUNBLENBQUNBO29CQUNqRUEsSUFBSUEsTUFBTUEsR0FBR0EsSUFBSUEsQ0FBQ0EsV0FBV0EsSUFBSUEsSUFBSUEsQ0FBQ0EsV0FBV0EsQ0FBQ0EsTUFBTUEsR0FBR0EsQ0FBQ0EsR0FBR0EsSUFBSUEsQ0FBQ0EsV0FBV0EsR0FBR0EsSUFBSUEsQ0FBQ0EsVUFBVUEsQ0FBQ0E7b0JBQ2xHQSxLQUFLQSxDQUFDQSxJQUFJQSxDQUFDQSxlQUFlQSxDQUFDQSxDQUFDQSxJQUFJQSxFQUFFQSxDQUFDQTtvQkFDbkNBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLGdCQUFnQkEsQ0FBQ0EsQ0FBQ0EsSUFBSUEsRUFBRUEsQ0FBQ0E7b0JBQ3BDQSxLQUFLQSxDQUFDQSxJQUFJQSxDQUFDQSxnQkFBZ0JBLENBQUNBLENBQUNBLElBQUlBLEVBQUVBLENBQUNBO29CQUNwQ0EsS0FBS0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EsaUJBQWlCQSxFQUFFQSxLQUFLQSxDQUFDQSxDQUFDQTtvQkFFckNBLE1BQU1BLENBQUNBLENBQUNBLE1BQU1BLENBQUNBLENBQUNBLENBQUNBO3dCQUNiQSxLQUFLQSxPQUFPQTs0QkFDUkEsS0FBS0EsQ0FBQ0EsbVhBQW1YQSxDQUFDQSxDQUFDQTs0QkFDM1hBLE1BQU1BLENBQUNBO3dCQUNYQSxLQUFLQSxPQUFPQTs0QkFDUkEsTUFBTUEsQ0FBQ0E7b0JBQ2ZBLENBQUNBO29CQUVEQSxJQUFJQSxpQkFBaUJBLEdBQUdBLElBQUlBLENBQUNBLE1BQU1BLENBQUNBLGlCQUFpQkEsQ0FBQ0E7b0JBQ3REQSxJQUFJQSxnQkFBZ0JBLEdBQUdBLElBQUlBLENBQUNBLE1BQU1BLENBQUNBLGdCQUFnQkEsQ0FBQ0E7b0JBQ3BEQSxJQUFJQSxRQUFRQSxHQUFHQSxJQUFJQSxDQUFDQSxNQUFNQSxDQUFDQSxRQUFRQSxDQUFDQTtvQkFFcENBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLGtDQUFrQ0EsQ0FBQ0EsQ0FBQ0EsR0FBR0EsQ0FBQ0EsZ0JBQWdCQSxDQUFDQSxDQUFDQTtvQkFDckVBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLG1DQUFtQ0EsQ0FBQ0EsQ0FBQ0EsR0FBR0EsQ0FBQ0EsaUJBQWlCQSxDQUFDQSxDQUFDQTtvQkFDdkVBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLDBCQUEwQkEsQ0FBQ0EsQ0FBQ0EsR0FBR0EsQ0FBQ0EsUUFBUUEsQ0FBQ0EsQ0FBQ0E7b0JBRXJEQSxpQkFBaUJBLEVBQUVBLENBQUNBO29CQUNwQkEsQ0FBQ0EsQ0FBQ0EsTUFBTUEsQ0FBQ0EsQ0FBQ0EsV0FBV0EsQ0FBQ0EsMENBQTBDQSxHQUFHQSxnQkFBZ0JBLEdBQUdBLFdBQVdBLENBQUNBLENBQUNBO2dCQUN2R0EsQ0FBQ0E7Z0JBRURILDBCQUEwQkEsU0FBU0E7b0JBQy9CSSxJQUFJQSxLQUFLQSxHQUFHQSxDQUFDQSxDQUFDQSxTQUFTQSxDQUFDQSxDQUFDQSxPQUFPQSxDQUFDQSxpQ0FBaUNBLENBQUNBLENBQUNBO29CQUNwRUEsSUFBSUEsZUFBZUEsR0FBV0EsS0FBS0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EsMEJBQTBCQSxDQUFDQSxDQUFDQTtvQkFDckVBLElBQUlBLGdCQUFnQkEsR0FBR0EsZUFBZUEsQ0FBQ0EsT0FBT0EsQ0FBQ0EsTUFBTUEsQ0FBQ0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EscUNBQXFDQSxDQUFDQSxDQUFDQSxHQUFHQSxFQUFFQSxDQUFDQTtvQkFDekdBLElBQUlBLFlBQVlBLEdBQUdBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLGdCQUFnQkEsQ0FBQ0EsQ0FBQ0E7b0JBRWhEQSxTQUFTQSxDQUFDQSxVQUFVQSxDQUFDQTt3QkFDakJBLFVBQVVBLEVBQUVBLEtBQUtBO3dCQUNqQkEsZUFBZUEsRUFBRUEsSUFBSUEsTUFBTUEsQ0FBQ0EsZUFBZUEsRUFBRUEsR0FBR0EsQ0FBQ0E7d0JBQ2pEQSxJQUFJQSxFQUFFQSxNQUFNQTt3QkFDWkEsR0FBR0EsRUFBRUEsS0FBS0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EscUJBQXFCQSxDQUFDQTt3QkFDdENBLFFBQVFBLEVBQUVBOzRCQUNOQSwwQkFBMEJBLEVBQUVBLGdCQUFnQkE7eUJBQy9DQTt3QkFDREEsV0FBV0EsRUFBRUEsVUFBQ0EsQ0FBQ0EsRUFBRUEsSUFBSUE7NEJBQ2pCQSxJQUFJQSxlQUFlQSxHQUFHQSxJQUFJQSxDQUFDQSxLQUFLQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxNQUFNQSxHQUFHQSxJQUFJQSxDQUFDQSxLQUFLQSxDQUFDQSxHQUFHQSxHQUFHQSxDQUFDQSxDQUFDQTs0QkFDbkVBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLGVBQWVBLENBQUNBLENBQUNBLElBQUlBLEVBQUVBLENBQUNBLElBQUlBLENBQUNBLFdBQVdBLENBQUNBLENBQUNBLEdBQUdBLENBQUNBLE9BQU9BLEVBQUVBLGVBQWVBLEdBQUdBLEdBQUdBLENBQUNBLENBQUNBOzRCQUN6RkEsS0FBS0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EsZ0JBQWdCQSxDQUFDQSxDQUFDQSxJQUFJQSxFQUFFQSxDQUFDQSxJQUFJQSxDQUFDQSxhQUFhQSxHQUFHQSxlQUFlQSxHQUFHQSxPQUFPQSxDQUFDQSxDQUFDQTt3QkFDeEZBLENBQUNBO3dCQUNEQSxJQUFJQSxFQUFFQSxVQUFVQSxDQUFDQSxFQUFFQSxJQUFJQTs0QkFDbkIsZUFBZSxDQUFDLElBQUksRUFBRSxJQUFJLENBQUMsQ0FBQzt3QkFDaEMsQ0FBQzt3QkFDREEsSUFBSUEsRUFBRUEsVUFBVUEsQ0FBQ0EsRUFBRUEsSUFBSUE7NEJBQ25CLGVBQWUsQ0FBQyxJQUFJLEVBQUUsSUFBSSxDQUFDLENBQUM7d0JBQ2hDLENBQUM7d0JBQ0RBLFdBQVdBLEVBQUVBLFVBQUNBLENBQUNBLEVBQUVBLElBQUlBOzRCQUNqQkEsS0FBS0EsQ0FBQ0EsSUFBSUEsQ0FBQ0Esa0JBQWtCQSxDQUFDQSxDQUFDQSxJQUFJQSxFQUFFQSxDQUFDQTs0QkFDdENBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLGlCQUFpQkEsRUFBRUEsSUFBSUEsQ0FBQ0EsQ0FBQ0E7NEJBQ3BDQSxZQUFZQSxDQUFDQSxJQUFJQSxFQUFFQSxDQUFDQTs0QkFDcEJBLElBQUlBLEdBQUdBLEdBQUdBLElBQUlBLENBQUNBLE1BQU1BLEVBQUVBLENBQUNBOzRCQUN4QkEsS0FBS0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EsS0FBS0EsRUFBRUEsR0FBR0EsQ0FBQ0EsQ0FBQ0E7d0JBQzNCQSxDQUFDQTt3QkFDREEsV0FBV0EsRUFBRUEsVUFBQ0EsQ0FBQ0EsRUFBRUEsSUFBSUE7NEJBQ2pCQSxLQUFLQSxDQUFDQSxJQUFJQSxDQUFDQSxrQkFBa0JBLENBQUNBLENBQUNBLElBQUlBLEVBQUVBLENBQUNBO3dCQUMxQ0EsQ0FBQ0E7cUJBQ0pBLENBQUNBLENBQUNBO29CQUVIQSxZQUFZQSxDQUFDQSxFQUFFQSxDQUFDQSxPQUFPQSxFQUFFQSxVQUFBQSxDQUFDQTt3QkFDdEJBLENBQUNBLENBQUNBLGNBQWNBLEVBQUVBLENBQUNBO3dCQUVuQkEsRUFBRUEsQ0FBQ0EsQ0FBQ0EsT0FBT0EsQ0FBQ0EsOENBQThDQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQTs0QkFDMURBLElBQUlBLEdBQUdBLEdBQUdBLEtBQUtBLENBQUNBLElBQUlBLENBQUNBLEtBQUtBLENBQUNBLENBQUNBOzRCQUM1QkEsR0FBR0EsQ0FBQ0EsS0FBS0EsRUFBRUEsQ0FBQ0E7d0JBQ2hCQSxDQUFDQTtvQkFDTEEsQ0FBQ0EsQ0FBQ0EsQ0FBQ0E7Z0JBQ1BBLENBQUNBO2dCQUVESjtvQkFDSUssSUFBSUEsWUFBWUEsR0FBR0EsQ0FBQ0EsQ0FBQ0EsaUJBQWlCQSxDQUFDQSxDQUFDQSxJQUFJQSxFQUFFQSxDQUFDQTtvQkFDL0NBLGVBQWVBLEdBQUdBLFlBQVlBLENBQUNBLElBQUlBLENBQUNBLHlCQUF5QkEsQ0FBQ0EsQ0FBQ0E7b0JBQy9EQSxPQUFPQSxHQUFHQSxZQUFZQSxDQUFDQSxJQUFJQSxDQUFDQSxvQkFBb0JBLENBQUNBLENBQUNBO29CQUNsREEsbUJBQW1CQSxHQUFHQSxlQUFlQSxDQUFDQSxNQUFNQSxHQUFHQSxDQUFDQSxDQUFDQTtvQkFFakRBLEVBQUVBLENBQUNBLENBQUNBLG1CQUFtQkEsQ0FBQ0EsQ0FBQ0EsQ0FBQ0E7d0JBQ2hCQSxPQUFRQSxDQUFDQSxLQUFLQSxDQUFDQTs0QkFDakJBLE9BQU9BLEVBQUVBLGVBQWVBLENBQUNBLElBQUlBLENBQUNBLG1CQUFtQkEsQ0FBQ0E7NEJBQ2xEQSxVQUFVQSxFQUFFQTtnQ0FDUkEsZUFBZUEsRUFBRUEsTUFBTUE7Z0NBQ3ZCQSxNQUFNQSxFQUFFQSxTQUFTQTs2QkFDcEJBOzRCQUNEQSxHQUFHQSxFQUFFQTtnQ0FDREEsTUFBTUEsRUFBRUEsU0FBU0E7Z0NBQ2pCQSxNQUFNQSxFQUFFQSxJQUFJQTtnQ0FDWkEsS0FBS0EsRUFBRUEsSUFBSUE7Z0NBQ1hBLElBQUlBLEVBQUVBLENBQUNBO2dDQUNQQSxNQUFNQSxFQUFFQSxZQUFZQTtnQ0FDcEJBLGVBQWVBLEVBQUVBLElBQUlBOzZCQUN4QkE7eUJBQ0pBLENBQUNBLENBQUNBO3dCQUVIQSxZQUFZQSxDQUFDQSxJQUFJQSxDQUFDQSwwQkFBMEJBLENBQUNBLENBQUNBLElBQUlBLENBQUNBOzRCQUMvQyxnQkFBZ0IsQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FBQzt3QkFDOUIsQ0FBQyxDQUFDQSxDQUFDQTt3QkFFSEEsTUFBTUEsQ0FBQ0EsY0FBY0EsR0FBR0EsVUFBQUEsQ0FBQ0E7NEJBQ3JCQSxJQUFJQSxnQkFBZ0JBLEdBQUdBLEtBQUtBLENBQUNBOzRCQUU3QkEsWUFBWUEsQ0FBQ0EsSUFBSUEsQ0FBQ0EsaUNBQWlDQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQTtnQ0FDdEQsRUFBRSxDQUFDLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxDQUFDLElBQUksQ0FBQyxpQkFBaUIsQ0FBQyxJQUFJLElBQUksQ0FBQyxDQUFDLENBQUM7b0NBQzFDLGdCQUFnQixHQUFHLElBQUksQ0FBQztvQ0FDeEIsTUFBTSxDQUFDLEtBQUssQ0FBQztnQ0FDakIsQ0FBQzs0QkFDTCxDQUFDLENBQUNBLENBQUNBOzRCQUVIQSxFQUFFQSxDQUFDQSxDQUFDQSxnQkFBZ0JBLENBQUNBO2dDQUNqQkEsQ0FBQ0EsQ0FBQ0EsV0FBV0EsR0FBR0EsNEVBQTRFQSxDQUFDQTt3QkFDckdBLENBQUNBLENBQUNBO3dCQUVGQSxZQUFZQSxDQUFDQSxJQUFJQSxDQUFDQSxvQkFBb0JBLENBQUNBLENBQUNBLEVBQUVBLENBQUNBLFFBQVFBLEVBQUVBLFVBQUFBLENBQUNBOzRCQUNsREEsaUJBQWlCQSxFQUFFQSxDQUFDQTt3QkFDeEJBLENBQUNBLENBQUNBLENBQUNBO3dCQUVIQSxpQkFBaUJBLEVBQUVBLENBQUNBO29CQUN4QkEsQ0FBQ0E7b0JBRURBLFlBQVlBLENBQUNBLElBQUlBLENBQUNBLGVBQWVBLENBQUNBLENBQUNBLEVBQUVBLENBQUNBLFFBQVFBLEVBQUVBLFVBQUFBLENBQUNBO3dCQUM3Q0EsSUFBSUEsTUFBTUEsR0FBR0EsQ0FBQ0EsQ0FBQ0EsQ0FBQ0EsQ0FBQ0EsYUFBYUEsQ0FBQ0EsQ0FBQ0E7d0JBRWhDQSxFQUFFQSxDQUFDQSxDQUFDQSxDQUFDQSxPQUFPQSxDQUFDQSxNQUFNQSxDQUFDQSxJQUFJQSxDQUFDQSxRQUFRQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQTs0QkFDbENBLE1BQU1BLENBQUNBLEdBQUdBLENBQUNBLEVBQUVBLENBQUNBLENBQUNBO3dCQUNuQkEsQ0FBQ0E7b0JBQ0xBLENBQUNBLENBQUNBLENBQUNBO2dCQUNQQSxDQUFDQTtnQkF2RGVMLHNDQUF1QkEsMEJBdUR0Q0EsQ0FBQUE7WUFDTEEsQ0FBQ0EsRUEzSmtDRCxjQUFjQSxHQUFkQSw0QkFBY0EsS0FBZEEsNEJBQWNBLFFBMkpoREE7UUFBREEsQ0FBQ0EsRUEzSm9CRCxhQUFhQSxHQUFiQSxtQkFBYUEsS0FBYkEsbUJBQWFBLFFBMkpqQ0E7SUFBREEsQ0FBQ0EsRUEzSmNELEtBQUtBLEdBQUxBLGFBQUtBLEtBQUxBLGFBQUtBLFFBMkpuQkE7QUFBREEsQ0FBQ0EsRUEzSk0sT0FBTyxLQUFQLE9BQU8sUUEySmIiLCJmaWxlIjoiY2xvdWRtZWRpYS1lZGl0LWNsb3VkdmlkZW9wYXJ0LXByb3hpZWQuanMiLCJzb3VyY2VzQ29udGVudCI6WyIvLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnkuZC50c1wiIC8+XG4vLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnl1aS5kLnRzXCIgLz5cblxubW9kdWxlIE9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdCB7XG5cbiAgICB2YXIgcmVxdWlyZWRVcGxvYWRzOiBKUXVlcnk7XG4gICAgdmFyIGJsb2NrZWQ6IEpRdWVyeTtcbiAgICB2YXIgaGFzUmVxdWlyZWRVcGxvYWRzcDogYm9vbGVhbjtcblxuICAgIGZ1bmN0aW9uIGdldEFsbEZpbGVzQ29tcGxldGVkKCk6IGJvb2xlYW4ge1xuICAgICAgICB2YXIgYWxsRmlsZXNDb21wbGV0ZWQ6IGJvb2xlYW4gPSB0cnVlO1xuXG4gICAgICAgIHJlcXVpcmVkVXBsb2Fkcy5maW5kKFwiaW5wdXRbbmFtZSQ9Jy5PcmlnaW5hbEZpbGVOYW1lJ10sIGlucHV0LnN5bmMtdXBsb2FkLWlucHV0XCIpLmVhY2goZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCQodGhpcykudmFsKCkgPT0gXCJcIikge1xuICAgICAgICAgICAgICAgIGFsbEZpbGVzQ29tcGxldGVkID0gZmFsc2U7XG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgfVxuICAgICAgICB9KTtcblxuICAgICAgICByZXR1cm4gYWxsRmlsZXNDb21wbGV0ZWQ7XG4gICAgfTtcblxuICAgIGZ1bmN0aW9uIHVuYmxvY2tJZkNvbXBsZXRlKCkge1xuICAgICAgICBpZiAoZ2V0QWxsRmlsZXNDb21wbGV0ZWQoKSlcbiAgICAgICAgICAgICg8YW55PmJsb2NrZWQpLnVuYmxvY2soKTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiB1cGxvYWRDb21wbGV0ZWQoc2VuZGVyLCBkYXRhKSB7XG4gICAgICAgIHZhciBzY29wZSA9ICQoc2VuZGVyKS5jbG9zZXN0KFwiW2RhdGEtdXBsb2FkLWFjY2VwdC1maWxlLXR5cGVzXVwiKTtcbiAgICAgICAgdmFyIHN0YXR1cyA9IGRhdGEuZXJyb3JUaHJvd24gJiYgZGF0YS5lcnJvclRocm93bi5sZW5ndGggPiAwID8gZGF0YS5lcnJvclRocm93biA6IGRhdGEudGV4dFN0YXR1cztcbiAgICAgICAgc2NvcGUuZmluZChcIi5wcm9ncmVzcy1iYXJcIikuaGlkZSgpO1xuICAgICAgICBzY29wZS5maW5kKFwiLnByb2dyZXNzLXRleHRcIikuaGlkZSgpO1xuICAgICAgICBzY29wZS5maW5kKFwiLmNhbmNlbC11cGxvYWRcIikuaGlkZSgpO1xuICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLWlzYWN0aXZlXCIsIGZhbHNlKTtcblxuICAgICAgICBzd2l0Y2ggKHN0YXR1cykge1xuICAgICAgICAgICAgY2FzZSBcImVycm9yXCI6XG4gICAgICAgICAgICAgICAgYWxlcnQoXCJUaGUgdXBsb2FkIG9mIHRoZSBzZWxlY3RlZCBmaWxlIGZhaWxlZC4gT25lIHBvc3NpYmxlIGNhdXNlIGlzIHRoYXQgdGhlIGZpbGUgc2l6ZSBleGNlZWRzIHRoZSBjb25maWd1cmVkIG1heFJlcXVlc3RMZW5ndGggc2V0dGluZyAoc2VlOiBodHRwOi8vbXNkbi5taWNyb3NvZnQuY29tL2VuLXVzL2xpYnJhcnkvc3lzdGVtLndlYi5jb25maWd1cmF0aW9uLmh0dHBydW50aW1lc2VjdGlvbi5tYXhyZXF1ZXN0bGVuZ3RoKHY9dnMuMTEwKS5hc3B4KS4gQWxzbyBtYWtlIHN1cmUgdGhlIGV4ZWN1dGlvblRpbWVPdXQgaXMgc2V0IHRvIGFsbG93IGZvciBlbm91Z2ggdGltZSBmb3IgdGhlIHJlcXVlc3QgdG8gZXhlY3V0ZSB3aGVuIGRlYnVnPVxcXCJmYWxzZVxcXCIuXCIpO1xuICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgICAgIGNhc2UgXCJhYm9ydFwiOlxuICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgfVxuXG4gICAgICAgIHZhciB0ZW1wb3JhcnlGaWxlTmFtZSA9IGRhdGEucmVzdWx0LnRlbXBvcmFyeUZpbGVOYW1lO1xuICAgICAgICB2YXIgb3JpZ2luYWxGaWxlTmFtZSA9IGRhdGEucmVzdWx0Lm9yaWdpbmFsRmlsZU5hbWU7XG4gICAgICAgIHZhciBmaWxlU2l6ZSA9IGRhdGEucmVzdWx0LmZpbGVTaXplO1xuXG4gICAgICAgIHNjb3BlLmZpbmQoXCJpbnB1dFtuYW1lJD0nLk9yaWdpbmFsRmlsZU5hbWUnXVwiKS52YWwob3JpZ2luYWxGaWxlTmFtZSk7XG4gICAgICAgIHNjb3BlLmZpbmQoXCJpbnB1dFtuYW1lJD0nLlRlbXBvcmFyeUZpbGVOYW1lJ11cIikudmFsKHRlbXBvcmFyeUZpbGVOYW1lKTtcbiAgICAgICAgc2NvcGUuZmluZChcImlucHV0W25hbWUkPScuRmlsZVNpemUnXVwiKS52YWwoZmlsZVNpemUpO1xuXG4gICAgICAgIHVuYmxvY2tJZkNvbXBsZXRlKCk7XG4gICAgICAgICQoc2VuZGVyKS5yZXBsYWNlV2l0aChcIjxzcGFuPlN1Y2Nlc3NmdWxseSB1cGxvYWRlZCB2aWRlbyBmaWxlICdcIiArIG9yaWdpbmFsRmlsZU5hbWUgKyBcIicuPC9zcGFuPlwiKTtcbiAgICB9XG4gICAgIFxuICAgIGZ1bmN0aW9uIGluaXRpYWxpemVVcGxvYWQoZmlsZUlucHV0KSB7XG4gICAgICAgIHZhciBzY29wZSA9ICQoZmlsZUlucHV0KS5jbG9zZXN0KFwiW2RhdGEtdXBsb2FkLWFjY2VwdC1maWxlLXR5cGVzXVwiKTtcbiAgICAgICAgdmFyIGFjY2VwdEZpbGVUeXBlczogc3RyaW5nID0gc2NvcGUuZGF0YShcInVwbG9hZC1hY2NlcHQtZmlsZS10eXBlc1wiKTtcbiAgICAgICAgdmFyIGFudGlGb3JnZXJ5VG9rZW4gPSByZXF1aXJlZFVwbG9hZHMuY2xvc2VzdChcImZvcm1cIikuZmluZChcIltuYW1lPSdfX1JlcXVlc3RWZXJpZmljYXRpb25Ub2tlbiddXCIpLnZhbCgpO1xuICAgICAgICB2YXIgY2FuY2VsVXBsb2FkID0gc2NvcGUuZmluZChcIi5jYW5jZWwtdXBsb2FkXCIpO1xuXG4gICAgICAgIGZpbGVJbnB1dC5maWxldXBsb2FkKHtcbiAgICAgICAgICAgIGF1dG9VcGxvYWQ6IGZhbHNlLFxuICAgICAgICAgICAgYWNjZXB0RmlsZVR5cGVzOiBuZXcgUmVnRXhwKGFjY2VwdEZpbGVUeXBlcywgXCJpXCIpLFxuICAgICAgICAgICAgdHlwZTogXCJQT1NUXCIsXG4gICAgICAgICAgICB1cmw6IHNjb3BlLmRhdGEoXCJ1cGxvYWQtZmFsbGJhY2stdXJsXCIpLFxuICAgICAgICAgICAgZm9ybURhdGE6IHtcbiAgICAgICAgICAgICAgICBfX1JlcXVlc3RWZXJpZmljYXRpb25Ub2tlbjogYW50aUZvcmdlcnlUb2tlblxuICAgICAgICAgICAgfSxcbiAgICAgICAgICAgIHByb2dyZXNzYWxsOiAoZSwgZGF0YSkgPT4ge1xuICAgICAgICAgICAgICAgIHZhciBwZXJjZW50Q29tcGxldGUgPSBNYXRoLmZsb29yKChkYXRhLmxvYWRlZCAvIGRhdGEudG90YWwpICogMTAwKTtcbiAgICAgICAgICAgICAgICBzY29wZS5maW5kKFwiLnByb2dyZXNzLWJhclwiKS5zaG93KCkuZmluZCgnLnByb2dyZXNzJykuY3NzKCd3aWR0aCcsIHBlcmNlbnRDb21wbGV0ZSArICclJyk7XG4gICAgICAgICAgICAgICAgc2NvcGUuZmluZChcIi5wcm9ncmVzcy10ZXh0XCIpLnNob3coKS50ZXh0KFwiVXBsb2FkaW5nIChcIiArIHBlcmNlbnRDb21wbGV0ZSArIFwiJSkuLi5cIik7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgZG9uZTogZnVuY3Rpb24gKGUsIGRhdGEpIHtcbiAgICAgICAgICAgICAgICB1cGxvYWRDb21wbGV0ZWQodGhpcywgZGF0YSk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgZmFpbDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcbiAgICAgICAgICAgICAgICB1cGxvYWRDb21wbGV0ZWQodGhpcywgZGF0YSk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgcHJvY2Vzc2RvbmU6IChlLCBkYXRhKSA9PiB7XG4gICAgICAgICAgICAgICAgc2NvcGUuZmluZChcIi52YWxpZGF0aW9uLXRleHRcIikuaGlkZSgpO1xuICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJ1cGxvYWQtaXNhY3RpdmVcIiwgdHJ1ZSk7XG4gICAgICAgICAgICAgICAgY2FuY2VsVXBsb2FkLnNob3coKTtcbiAgICAgICAgICAgICAgICB2YXIgeGhyID0gZGF0YS5zdWJtaXQoKTtcbiAgICAgICAgICAgICAgICBzY29wZS5kYXRhKFwieGhyXCIsIHhocik7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgcHJvY2Vzc2ZhaWw6IChlLCBkYXRhKSA9PiB7XG4gICAgICAgICAgICAgICAgc2NvcGUuZmluZChcIi52YWxpZGF0aW9uLXRleHRcIikuc2hvdygpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9KTtcblxuICAgICAgICBjYW5jZWxVcGxvYWQub24oXCJjbGlja1wiLCBlPT4ge1xuICAgICAgICAgICAgZS5wcmV2ZW50RGVmYXVsdCgpO1xuXG4gICAgICAgICAgICBpZiAoY29uZmlybShcIkFyZSB5b3Ugc3VyZSB5b3Ugd2FudCB0byBjYW5jZWwgdGhpcyB1cGxvYWQ/XCIpKSB7XG4gICAgICAgICAgICAgICAgdmFyIHhociA9IHNjb3BlLmRhdGEoXCJ4aHJcIik7XG4gICAgICAgICAgICAgICAgeGhyLmFib3J0KCk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH0pO1xuICAgIH1cblxuICAgIGV4cG9ydCBmdW5jdGlvbiBpbml0aWFsaXplVXBsb2FkUHJveGllZCgpIHtcbiAgICAgICAgdmFyIHNjb3BlUHJveGllZCA9ICQoXCIudXBsb2FkLXByb3hpZWRcIikuc2hvdygpO1xuICAgICAgICByZXF1aXJlZFVwbG9hZHMgPSBzY29wZVByb3hpZWQuZmluZChcIi5yZXF1aXJlZC11cGxvYWRzLWdyb3VwXCIpO1xuICAgICAgICBibG9ja2VkID0gc2NvcGVQcm94aWVkLmZpbmQoXCIuZWRpdC1pdGVtLXNpZGViYXJcIik7XG4gICAgICAgIGhhc1JlcXVpcmVkVXBsb2Fkc3AgPSByZXF1aXJlZFVwbG9hZHMubGVuZ3RoID4gMDtcblxuICAgICAgICBpZiAoaGFzUmVxdWlyZWRVcGxvYWRzcCkge1xuICAgICAgICAgICAgKDxhbnk+YmxvY2tlZCkuYmxvY2soe1xuICAgICAgICAgICAgICAgIG1lc3NhZ2U6IHJlcXVpcmVkVXBsb2Fkcy5kYXRhKFwiYmxvY2stZGVzY3JpcHRpb25cIiksXG4gICAgICAgICAgICAgICAgb3ZlcmxheUNTUzoge1xuICAgICAgICAgICAgICAgICAgICBiYWNrZ3JvdW5kQ29sb3I6IFwiI2ZmZlwiLFxuICAgICAgICAgICAgICAgICAgICBjdXJzb3I6IFwiZGVmYXVsdFwiXG4gICAgICAgICAgICAgICAgfSxcbiAgICAgICAgICAgICAgICBjc3M6IHtcbiAgICAgICAgICAgICAgICAgICAgY3Vyc29yOiBcImRlZmF1bHRcIixcbiAgICAgICAgICAgICAgICAgICAgYm9yZGVyOiBudWxsLFxuICAgICAgICAgICAgICAgICAgICB3aWR0aDogbnVsbCxcbiAgICAgICAgICAgICAgICAgICAgbGVmdDogMCxcbiAgICAgICAgICAgICAgICAgICAgbWFyZ2luOiBcIjMwcHggMCAwIDBcIixcbiAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZENvbG9yOiBudWxsXG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfSk7XG5cbiAgICAgICAgICAgIHNjb3BlUHJveGllZC5maW5kKFwiLmFzeW5jLXVwbG9hZC1maWxlLWlucHV0XCIpLmVhY2goZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIGluaXRpYWxpemVVcGxvYWQoJCh0aGlzKSk7XG4gICAgICAgICAgICB9KTtcblxuICAgICAgICAgICAgd2luZG93Lm9uYmVmb3JldW5sb2FkID0gZSA9PiB7XG4gICAgICAgICAgICAgICAgdmFyIGhhc0FjdGl2ZVVwbG9hZHMgPSBmYWxzZTtcblxuICAgICAgICAgICAgICAgIHNjb3BlUHJveGllZC5maW5kKFwiW2RhdGEtdXBsb2FkLWFjY2VwdC1maWxlLXR5cGVzXVwiKS5lYWNoKGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgaWYgKCQodGhpcykuZGF0YShcInVwbG9hZC1pc2FjdGl2ZVwiKSA9PSB0cnVlKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBoYXNBY3RpdmVVcGxvYWRzID0gdHJ1ZTtcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIH0pO1xuXG4gICAgICAgICAgICAgICAgaWYgKGhhc0FjdGl2ZVVwbG9hZHMpXG4gICAgICAgICAgICAgICAgICAgIGUucmV0dXJuVmFsdWUgPSBcIlRoZXJlIGFyZSB1cGxvYWRzIGluIHByb2dyZXNzLiBUaGVzZSB3aWxsIGJlIGFib3J0ZWQgaWYgeW91IG5hdmlnYXRlIGF3YXkuXCI7XG4gICAgICAgICAgICB9O1xuXG4gICAgICAgICAgICBzY29wZVByb3hpZWQuZmluZChcIi5zeW5jLXVwbG9hZC1pbnB1dFwiKS5vbihcImNoYW5nZVwiLCBlPT4ge1xuICAgICAgICAgICAgICAgIHVuYmxvY2tJZkNvbXBsZXRlKCk7XG4gICAgICAgICAgICB9KTtcblxuICAgICAgICAgICAgdW5ibG9ja0lmQ29tcGxldGUoKTtcbiAgICAgICAgfVxuXG4gICAgICAgIHNjb3BlUHJveGllZC5maW5kKFwiW2RhdGEtcHJvbXB0XVwiKS5vbihcImNoYW5nZVwiLCBlID0+IHtcbiAgICAgICAgICAgIHZhciBzZW5kZXIgPSAkKGUuY3VycmVudFRhcmdldCk7XG4gICAgICAgICAgICBcbiAgICAgICAgICAgIGlmICghY29uZmlybShzZW5kZXIuZGF0YShcInByb21wdFwiKSkpIHtcbiAgICAgICAgICAgICAgICBzZW5kZXIudmFsKFwiXCIpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9KTtcbiAgICB9XG59Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9