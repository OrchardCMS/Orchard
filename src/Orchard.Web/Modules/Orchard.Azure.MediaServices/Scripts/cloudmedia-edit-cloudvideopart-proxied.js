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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtZWRpdC1jbG91ZHZpZGVvcGFydC1wcm94aWVkLnRzIl0sIm5hbWVzIjpbIk9yY2hhcmQiLCJPcmNoYXJkLkF6dXJlIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmdldEFsbEZpbGVzQ29tcGxldGVkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LnVuYmxvY2tJZkNvbXBsZXRlIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LnVwbG9hZENvbXBsZXRlZCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdC5pbml0aWFsaXplVXBsb2FkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkNsb3VkVmlkZW9FZGl0LmluaXRpYWxpemVVcGxvYWRQcm94aWVkIl0sIm1hcHBpbmdzIjoiQUFBQSw0Q0FBNEM7QUFDNUMsOENBQThDO0FBRTlDLElBQU8sT0FBTyxDQTJKYjtBQTNKRCxXQUFPLE9BQU87SUFBQ0EsSUFBQUEsS0FBS0EsQ0EySm5CQTtJQTNKY0EsV0FBQUEsS0FBS0E7UUFBQ0MsSUFBQUEsYUFBYUEsQ0EySmpDQTtRQTNKb0JBLFdBQUFBLGFBQWFBO1lBQUNDLElBQUFBLGNBQWNBLENBMkpoREE7WUEzSmtDQSxXQUFBQSxjQUFjQSxFQUFDQSxDQUFDQTtnQkFFL0NDLElBQUlBLGVBQXVCQSxDQUFDQTtnQkFDNUJBLElBQUlBLE9BQWVBLENBQUNBO2dCQUNwQkEsSUFBSUEsbUJBQTRCQSxDQUFDQTtnQkFFakNBO29CQUNJQyxJQUFJQSxpQkFBaUJBLEdBQVlBLElBQUlBLENBQUNBO29CQUV0Q0EsZUFBZUEsQ0FBQ0EsSUFBSUEsQ0FBQ0EsMkRBQTJEQSxDQUFDQSxD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QSxJQUFJQSxDQUFDQSxRQUFRQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQTs0QkFDbENBLE1BQU1BLENBQUNBLEdBQUdBLENBQUNBLEVBQUVBLENBQUNBLENBQUNBO3dCQUNuQkEsQ0FBQ0E7b0JBQ0xBLENBQUNBLENBQUNBLENBQUNBO2dCQUNQQSxDQUFDQTtnQkF2RGVMLHNDQUF1QkEsMEJBdUR0Q0EsQ0FBQUE7WUFDTEEsQ0FBQ0EsRUEzSmtDRCxjQUFjQSxHQUFkQSw0QkFBY0EsS0FBZEEsNEJBQWNBLFFBMkpoREE7UUFBREEsQ0FBQ0EsRUEzSm9CRCxhQUFhQSxHQUFiQSxtQkFBYUEsS0FBYkEsbUJBQWFBLFFBMkpqQ0E7SUFBREEsQ0FBQ0EsRUEzSmNELEtBQUtBLEdBQUxBLGFBQUtBLEtBQUxBLGFBQUtBLFFBMkpuQkE7QUFBREEsQ0FBQ0EsRUEzSk0sT0FBTyxLQUFQLE9BQU8sUUEySmIiLCJmaWxlIjoiY2xvdWRtZWRpYS1lZGl0LWNsb3VkdmlkZW9wYXJ0LXByb3hpZWQuanMiLCJzb3VyY2VzQ29udGVudCI6WyIvLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnkuZC50c1wiIC8+XHJcbi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2pxdWVyeXVpLmQudHNcIiAvPlxyXG5cclxubW9kdWxlIE9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdCB7XHJcblxyXG4gICAgdmFyIHJlcXVpcmVkVXBsb2FkczogSlF1ZXJ5O1xyXG4gICAgdmFyIGJsb2NrZWQ6IEpRdWVyeTtcclxuICAgIHZhciBoYXNSZXF1aXJlZFVwbG9hZHNwOiBib29sZWFuO1xyXG5cclxuICAgIGZ1bmN0aW9uIGdldEFsbEZpbGVzQ29tcGxldGVkKCk6IGJvb2xlYW4ge1xyXG4gICAgICAgIHZhciBhbGxGaWxlc0NvbXBsZXRlZDogYm9vbGVhbiA9IHRydWU7XHJcblxyXG4gICAgICAgIHJlcXVpcmVkVXBsb2Fkcy5maW5kKFwiaW5wdXRbbmFtZSQ9Jy5PcmlnaW5hbEZpbGVOYW1lJ10sIGlucHV0LnN5bmMtdXBsb2FkLWlucHV0XCIpLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoJCh0aGlzKS52YWwoKSA9PSBcIlwiKSB7XHJcbiAgICAgICAgICAgICAgICBhbGxGaWxlc0NvbXBsZXRlZCA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgIHJldHVybiBhbGxGaWxlc0NvbXBsZXRlZDtcclxuICAgIH07XHJcblxyXG4gICAgZnVuY3Rpb24gdW5ibG9ja0lmQ29tcGxldGUoKSB7XHJcbiAgICAgICAgaWYgKGdldEFsbEZpbGVzQ29tcGxldGVkKCkpXHJcbiAgICAgICAgICAgICg8YW55PmJsb2NrZWQpLnVuYmxvY2soKTtcclxuICAgIH1cclxuXHJcbiAgICBmdW5jdGlvbiB1cGxvYWRDb21wbGV0ZWQoc2VuZGVyLCBkYXRhKSB7XHJcbiAgICAgICAgdmFyIHNjb3BlID0gJChzZW5kZXIpLmNsb3Nlc3QoXCJbZGF0YS11cGxvYWQtYWNjZXB0LWZpbGUtdHlwZXNdXCIpO1xyXG4gICAgICAgIHZhciBzdGF0dXMgPSBkYXRhLmVycm9yVGhyb3duICYmIGRhdGEuZXJyb3JUaHJvd24ubGVuZ3RoID4gMCA/IGRhdGEuZXJyb3JUaHJvd24gOiBkYXRhLnRleHRTdGF0dXM7XHJcbiAgICAgICAgc2NvcGUuZmluZChcIi5wcm9ncmVzcy1iYXJcIikuaGlkZSgpO1xyXG4gICAgICAgIHNjb3BlLmZpbmQoXCIucHJvZ3Jlc3MtdGV4dFwiKS5oaWRlKCk7XHJcbiAgICAgICAgc2NvcGUuZmluZChcIi5jYW5jZWwtdXBsb2FkXCIpLmhpZGUoKTtcclxuICAgICAgICBzY29wZS5kYXRhKFwidXBsb2FkLWlzYWN0aXZlXCIsIGZhbHNlKTtcclxuXHJcbiAgICAgICAgc3dpdGNoIChzdGF0dXMpIHtcclxuICAgICAgICAgICAgY2FzZSBcImVycm9yXCI6XHJcbiAgICAgICAgICAgICAgICBhbGVydChcIlRoZSB1cGxvYWQgb2YgdGhlIHNlbGVjdGVkIGZpbGUgZmFpbGVkLiBPbmUgcG9zc2libGUgY2F1c2UgaXMgdGhhdCB0aGUgZmlsZSBzaXplIGV4Y2VlZHMgdGhlIGNvbmZpZ3VyZWQgbWF4UmVxdWVzdExlbmd0aCBzZXR0aW5nIChzZWU6IGh0dHA6Ly9tc2RuLm1pY3Jvc29mdC5jb20vZW4tdXMvbGlicmFyeS9zeXN0ZW0ud2ViLmNvbmZpZ3VyYXRpb24uaHR0cHJ1bnRpbWVzZWN0aW9uLm1heHJlcXVlc3RsZW5ndGgodj12cy4xMTApLmFzcHgpLiBBbHNvIG1ha2Ugc3VyZSB0aGUgZXhlY3V0aW9uVGltZU91dCBpcyBzZXQgdG8gYWxsb3cgZm9yIGVub3VnaCB0aW1lIGZvciB0aGUgcmVxdWVzdCB0byBleGVjdXRlIHdoZW4gZGVidWc9XFxcImZhbHNlXFxcIi5cIik7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIGNhc2UgXCJhYm9ydFwiOlxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIHRlbXBvcmFyeUZpbGVOYW1lID0gZGF0YS5yZXN1bHQudGVtcG9yYXJ5RmlsZU5hbWU7XHJcbiAgICAgICAgdmFyIG9yaWdpbmFsRmlsZU5hbWUgPSBkYXRhLnJlc3VsdC5vcmlnaW5hbEZpbGVOYW1lO1xyXG4gICAgICAgIHZhciBmaWxlU2l6ZSA9IGRhdGEucmVzdWx0LmZpbGVTaXplO1xyXG5cclxuICAgICAgICBzY29wZS5maW5kKFwiaW5wdXRbbmFtZSQ9Jy5PcmlnaW5hbEZpbGVOYW1lJ11cIikudmFsKG9yaWdpbmFsRmlsZU5hbWUpO1xyXG4gICAgICAgIHNjb3BlLmZpbmQoXCJpbnB1dFtuYW1lJD0nLlRlbXBvcmFyeUZpbGVOYW1lJ11cIikudmFsKHRlbXBvcmFyeUZpbGVOYW1lKTtcclxuICAgICAgICBzY29wZS5maW5kKFwiaW5wdXRbbmFtZSQ9Jy5GaWxlU2l6ZSddXCIpLnZhbChmaWxlU2l6ZSk7XHJcblxyXG4gICAgICAgIHVuYmxvY2tJZkNvbXBsZXRlKCk7XHJcbiAgICAgICAgJChzZW5kZXIpLnJlcGxhY2VXaXRoKFwiPHNwYW4+U3VjY2Vzc2Z1bGx5IHVwbG9hZGVkIHZpZGVvIGZpbGUgJ1wiICsgb3JpZ2luYWxGaWxlTmFtZSArIFwiJy48L3NwYW4+XCIpO1xyXG4gICAgfVxyXG4gICAgIFxyXG4gICAgZnVuY3Rpb24gaW5pdGlhbGl6ZVVwbG9hZChmaWxlSW5wdXQpIHtcclxuICAgICAgICB2YXIgc2NvcGUgPSAkKGZpbGVJbnB1dCkuY2xvc2VzdChcIltkYXRhLXVwbG9hZC1hY2NlcHQtZmlsZS10eXBlc11cIik7XHJcbiAgICAgICAgdmFyIGFjY2VwdEZpbGVUeXBlczogc3RyaW5nID0gc2NvcGUuZGF0YShcInVwbG9hZC1hY2NlcHQtZmlsZS10eXBlc1wiKTtcclxuICAgICAgICB2YXIgYW50aUZvcmdlcnlUb2tlbiA9IHJlcXVpcmVkVXBsb2Fkcy5jbG9zZXN0KFwiZm9ybVwiKS5maW5kKFwiW25hbWU9J19fUmVxdWVzdFZlcmlmaWNhdGlvblRva2VuJ11cIikudmFsKCk7XHJcbiAgICAgICAgdmFyIGNhbmNlbFVwbG9hZCA9IHNjb3BlLmZpbmQoXCIuY2FuY2VsLXVwbG9hZFwiKTtcclxuXHJcbiAgICAgICAgZmlsZUlucHV0LmZpbGV1cGxvYWQoe1xyXG4gICAgICAgICAgICBhdXRvVXBsb2FkOiBmYWxzZSxcclxuICAgICAgICAgICAgYWNjZXB0RmlsZVR5cGVzOiBuZXcgUmVnRXhwKGFjY2VwdEZpbGVUeXBlcywgXCJpXCIpLFxyXG4gICAgICAgICAgICB0eXBlOiBcIlBPU1RcIixcclxuICAgICAgICAgICAgdXJsOiBzY29wZS5kYXRhKFwidXBsb2FkLWZhbGxiYWNrLXVybFwiKSxcclxuICAgICAgICAgICAgZm9ybURhdGE6IHtcclxuICAgICAgICAgICAgICAgIF9fUmVxdWVzdFZlcmlmaWNhdGlvblRva2VuOiBhbnRpRm9yZ2VyeVRva2VuXHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHByb2dyZXNzYWxsOiAoZSwgZGF0YSkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdmFyIHBlcmNlbnRDb21wbGV0ZSA9IE1hdGguZmxvb3IoKGRhdGEubG9hZGVkIC8gZGF0YS50b3RhbCkgKiAxMDApO1xyXG4gICAgICAgICAgICAgICAgc2NvcGUuZmluZChcIi5wcm9ncmVzcy1iYXJcIikuc2hvdygpLmZpbmQoJy5wcm9ncmVzcycpLmNzcygnd2lkdGgnLCBwZXJjZW50Q29tcGxldGUgKyAnJScpO1xyXG4gICAgICAgICAgICAgICAgc2NvcGUuZmluZChcIi5wcm9ncmVzcy10ZXh0XCIpLnNob3coKS50ZXh0KFwiVXBsb2FkaW5nIChcIiArIHBlcmNlbnRDb21wbGV0ZSArIFwiJSkuLi5cIik7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGRvbmU6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcbiAgICAgICAgICAgICAgICB1cGxvYWRDb21wbGV0ZWQodGhpcywgZGF0YSk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGZhaWw6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcbiAgICAgICAgICAgICAgICB1cGxvYWRDb21wbGV0ZWQodGhpcywgZGF0YSk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHByb2Nlc3Nkb25lOiAoZSwgZGF0YSkgPT4ge1xyXG4gICAgICAgICAgICAgICAgc2NvcGUuZmluZChcIi52YWxpZGF0aW9uLXRleHRcIikuaGlkZSgpO1xyXG4gICAgICAgICAgICAgICAgc2NvcGUuZGF0YShcInVwbG9hZC1pc2FjdGl2ZVwiLCB0cnVlKTtcclxuICAgICAgICAgICAgICAgIGNhbmNlbFVwbG9hZC5zaG93KCk7XHJcbiAgICAgICAgICAgICAgICB2YXIgeGhyID0gZGF0YS5zdWJtaXQoKTtcclxuICAgICAgICAgICAgICAgIHNjb3BlLmRhdGEoXCJ4aHJcIiwgeGhyKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgcHJvY2Vzc2ZhaWw6IChlLCBkYXRhKSA9PiB7XHJcbiAgICAgICAgICAgICAgICBzY29wZS5maW5kKFwiLnZhbGlkYXRpb24tdGV4dFwiKS5zaG93KCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgY2FuY2VsVXBsb2FkLm9uKFwiY2xpY2tcIiwgZT0+IHtcclxuICAgICAgICAgICAgZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cclxuICAgICAgICAgICAgaWYgKGNvbmZpcm0oXCJBcmUgeW91IHN1cmUgeW91IHdhbnQgdG8gY2FuY2VsIHRoaXMgdXBsb2FkP1wiKSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHhociA9IHNjb3BlLmRhdGEoXCJ4aHJcIik7XHJcbiAgICAgICAgICAgICAgICB4aHIuYWJvcnQoKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0pO1xyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCBmdW5jdGlvbiBpbml0aWFsaXplVXBsb2FkUHJveGllZCgpIHtcclxuICAgICAgICB2YXIgc2NvcGVQcm94aWVkID0gJChcIi51cGxvYWQtcHJveGllZFwiKS5zaG93KCk7XHJcbiAgICAgICAgcmVxdWlyZWRVcGxvYWRzID0gc2NvcGVQcm94aWVkLmZpbmQoXCIucmVxdWlyZWQtdXBsb2Fkcy1ncm91cFwiKTtcclxuICAgICAgICBibG9ja2VkID0gc2NvcGVQcm94aWVkLmZpbmQoXCIuZWRpdC1pdGVtLXNpZGViYXJcIik7XHJcbiAgICAgICAgaGFzUmVxdWlyZWRVcGxvYWRzcCA9IHJlcXVpcmVkVXBsb2Fkcy5sZW5ndGggPiAwO1xyXG5cclxuICAgICAgICBpZiAoaGFzUmVxdWlyZWRVcGxvYWRzcCkge1xyXG4gICAgICAgICAgICAoPGFueT5ibG9ja2VkKS5ibG9jayh7XHJcbiAgICAgICAgICAgICAgICBtZXNzYWdlOiByZXF1aXJlZFVwbG9hZHMuZGF0YShcImJsb2NrLWRlc2NyaXB0aW9uXCIpLFxyXG4gICAgICAgICAgICAgICAgb3ZlcmxheUNTUzoge1xyXG4gICAgICAgICAgICAgICAgICAgIGJhY2tncm91bmRDb2xvcjogXCIjZmZmXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgY3Vyc29yOiBcImRlZmF1bHRcIlxyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIGNzczoge1xyXG4gICAgICAgICAgICAgICAgICAgIGN1cnNvcjogXCJkZWZhdWx0XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgYm9yZGVyOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgICAgIHdpZHRoOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgICAgIGxlZnQ6IDAsXHJcbiAgICAgICAgICAgICAgICAgICAgbWFyZ2luOiBcIjMwcHggMCAwIDBcIixcclxuICAgICAgICAgICAgICAgICAgICBiYWNrZ3JvdW5kQ29sb3I6IG51bGxcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICBzY29wZVByb3hpZWQuZmluZChcIi5hc3luYy11cGxvYWQtZmlsZS1pbnB1dFwiKS5lYWNoKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIGluaXRpYWxpemVVcGxvYWQoJCh0aGlzKSk7XHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgd2luZG93Lm9uYmVmb3JldW5sb2FkID0gZSA9PiB7XHJcbiAgICAgICAgICAgICAgICB2YXIgaGFzQWN0aXZlVXBsb2FkcyA9IGZhbHNlO1xyXG5cclxuICAgICAgICAgICAgICAgIHNjb3BlUHJveGllZC5maW5kKFwiW2RhdGEtdXBsb2FkLWFjY2VwdC1maWxlLXR5cGVzXVwiKS5lYWNoKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoJCh0aGlzKS5kYXRhKFwidXBsb2FkLWlzYWN0aXZlXCIpID09IHRydWUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaGFzQWN0aXZlVXBsb2FkcyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoaGFzQWN0aXZlVXBsb2FkcylcclxuICAgICAgICAgICAgICAgICAgICBlLnJldHVyblZhbHVlID0gXCJUaGVyZSBhcmUgdXBsb2FkcyBpbiBwcm9ncmVzcy4gVGhlc2Ugd2lsbCBiZSBhYm9ydGVkIGlmIHlvdSBuYXZpZ2F0ZSBhd2F5LlwiO1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgc2NvcGVQcm94aWVkLmZpbmQoXCIuc3luYy11cGxvYWQtaW5wdXRcIikub24oXCJjaGFuZ2VcIiwgZT0+IHtcclxuICAgICAgICAgICAgICAgIHVuYmxvY2tJZkNvbXBsZXRlKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgdW5ibG9ja0lmQ29tcGxldGUoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHNjb3BlUHJveGllZC5maW5kKFwiW2RhdGEtcHJvbXB0XVwiKS5vbihcImNoYW5nZVwiLCBlID0+IHtcclxuICAgICAgICAgICAgdmFyIHNlbmRlciA9ICQoZS5jdXJyZW50VGFyZ2V0KTtcclxuICAgICAgICAgICAgXHJcbiAgICAgICAgICAgIGlmICghY29uZmlybShzZW5kZXIuZGF0YShcInByb21wdFwiKSkpIHtcclxuICAgICAgICAgICAgICAgIHNlbmRlci52YWwoXCJcIik7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuICAgIH1cclxufSJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==