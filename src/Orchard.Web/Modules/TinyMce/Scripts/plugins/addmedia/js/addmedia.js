tinyMCEPopup.requireLangPack("addmedia");

var AddMediaDialog = {
    init: function() {
        var form = document.forms[0];
        form.action = tinyMCE.activeEditor.getParam('addmedia_action');
        form.MediaPath.value = tinyMCE.activeEditor.getParam('addmedia_path');
        form.__RequestVerificationToken.value = tinyMCE.activeEditor.getParam('request_verification_token');
    }
};

tinyMCEPopup.onInit.add(AddMediaDialog.init, AddMediaDialog);