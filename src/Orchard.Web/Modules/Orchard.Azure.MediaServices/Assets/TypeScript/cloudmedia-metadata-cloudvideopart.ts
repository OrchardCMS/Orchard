/// <reference path="Typings/jquery.d.ts" />

($=> {
    var hasFocus = (videoId: number)=> {
        var focusedVideoId: number = $("#media-library-main-list li.has-focus .media-thumbnail-cloud-video").data("id");
        return focusedVideoId == videoId;
    };

    var updateUploadProgressLabel = ()=> {
        var containers: JQuery = $("#media-library-main-editor-focus .properties");

        containers.each(function () {
            var container = $(this);
            var statusWrapper = container.find(".upload-status-wrapper");
            var publicationStatusLabel = container.find(".publication-status span");
            var uploadStatusLabel = container.find(".upload-status-text");
            var progressLabel = container.find(".upload-progress-value");
            var uploadProgressContainer = $(".upload-progress-status");
            var statusUrl: string = statusWrapper.data("status-url");
            var status: string = statusWrapper.data("upload-status");
            var published: string = statusWrapper.data("published");
            var videoId: number = statusWrapper.data("video-id");

            if (status == "Uploaded" && published) {
                return;
            }

            var update = () => {
                if (!hasFocus(videoId)) {
                    return;
                }
                $.ajax({
                    url: statusUrl,
                    cache: false
                }).done(data=> {
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