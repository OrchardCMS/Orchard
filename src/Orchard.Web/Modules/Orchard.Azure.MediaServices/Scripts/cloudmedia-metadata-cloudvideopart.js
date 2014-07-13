/// <reference path="typings/jquery.d.ts" />
(function ($) {
    var hasFocus = function (videoId) {
        var focusedVideoId = $("#media-library-main-list li.has-focus .media-thumbnail-cloud-video").data("id");
        return focusedVideoId == videoId;
    };

    var updateUploadProgressLabel = function () {
        var containers = $("#media-library-main-editor-focus .properties");

        containers.each(function () {
            var container = $(this);
            var statusWrapper = container.find(".upload-status-wrapper");
            var publicationStatusLabel = container.find(".publication-status span");
            var uploadStatusLabel = container.find(".upload-status-text");
            var progressLabel = container.find(".upload-progress-value");
            var uploadProgressContainer = $(".upload-progress-status");
            var statusUrl = statusWrapper.data("status-url");
            var status = statusWrapper.data("upload-status");
            var published = statusWrapper.data("published");
            var videoId = statusWrapper.data("video-id");

            if (status == "Uploaded" && published) {
                return;
            }

            var update = function () {
                if (!hasFocus(videoId)) {
                    return;
                }
                $.ajax({
                    url: statusUrl,
                    cache: false
                }).done(function (data) {
                    progressLabel.text(data.uploadState.percentComplete + "%");
                    uploadStatusLabel.text(data.uploadState.status);
                    publicationStatusLabel.text(data.published ? statusWrapper.data("published-text") : statusWrapper.data("draft-text"));

                    if (data.published) {
                        var mediathumbnail = $(".media-thumbnail-cloud-video[data-id=" + videoId + "]");
                        mediathumbnail.parents("li:first").find(".publication-status").hide();
                    }

                    if (data.uploadState.status == "Uploaded") {
                        uploadProgressContainer.hide();
                        return;
                    } else if (data.uploadState.status == "Uploading") {
                        uploadProgressContainer.show();
                    }

                    window.setTimeout(update, 5000);
                });
            };
            update();
        });
    };

    updateUploadProgressLabel();
})(jQuery);
//# sourceMappingURL=cloudmedia-metadata-cloudvideopart.js.map
