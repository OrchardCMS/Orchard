(function ($) {
    $(document).ready(function () {
        $('.comment-reply-form-settings').map(function () {
            var $self = $(this);
            var contentItemId = $self.data("contentitem-id");
            var activeCommentId = $self.data('active-comment-id');

            InitializeCommentReplyUI(contentItemId, activeCommentId);
        });

        $('.comment-reply-button').click(function () {
            var $self = $(this);
            var contentItemId = $self.data("contentitem-id");
            var commentId = $self.data('id');

            MoveFormToReplyFormBeacon(contentItemId, commentId);

            $('.comment-reply-button[data-contentitem-id="' + contentItemId + '"]').show();
            $('.comment-reply-button[data-contentitem-id="' + contentItemId + '"][data-id="' + commentId + '"]').fadeOut("fast");
            return false;
        });
    });

    function InitializeCommentReplyUI(contentItemId, activeCommentId) {
        MoveFormToReplyFormBeacon(contentItemId, activeCommentId);

        if (activeCommentId === "root") {
            $('.comment-reply-button[data-contentitem-id="' + contentItemId + '"][data-id="root"]').hide();
        }
    }

    function MoveFormToReplyFormBeacon(contentItemId, commentId) {
        // update the repliedOn with the current comment id
        if (commentId !== "root") {
            var $reply = $('.comments-repliedon[data-contentitem-id="' + contentItemId + '"]');
            $reply.val(commentId);
        }

        // move the form
        var $replyFormBeacon = $('.comment-reply-form-beacon[data-contentitem-id="' + contentItemId + '"][data-id="' + commentId + '"]');
        $('.comment-form[data-contentitem-id="' + contentItemId + '"]')
            .slideUp("fast", function () {
                $(this)
                    .appendTo($replyFormBeacon)
                    .slideDown();
            });
    }
})(jQuery)