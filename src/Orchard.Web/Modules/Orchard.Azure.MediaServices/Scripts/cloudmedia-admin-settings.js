/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/knockout.d.ts" />

var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (Admin) {
                (function (Settings) {
                    var StringItem = (function () {
                        function StringItem(value) {
                            this.value = ko.observable(value);
                        }
                        return StringItem;
                    })();
                    Settings.StringItem = StringItem;

                    Settings.clientViewModel = {
                        wamsEncodingPresets: ko.observableArray(),
                        defaultWamsEncodingPresetIndex: ko.observable(),
                        subtitleLanguages: ko.observableArray()
                    };

                    function deleteWamsEncodingPreset(preset) {
                        var removedIndex = Settings.clientViewModel.wamsEncodingPresets.indexOf(preset);
                        Settings.clientViewModel.wamsEncodingPresets.remove(preset);
                        if (removedIndex === Settings.clientViewModel.defaultWamsEncodingPresetIndex())
                            Settings.clientViewModel.defaultWamsEncodingPresetIndex(0);
                        else if (removedIndex < Settings.clientViewModel.defaultWamsEncodingPresetIndex())
                            Settings.clientViewModel.defaultWamsEncodingPresetIndex(Settings.clientViewModel.defaultWamsEncodingPresetIndex() - 1);
                    }
                    Settings.deleteWamsEncodingPreset = deleteWamsEncodingPreset;

                    function addNewWamsEncodingPreset() {
                        Settings.clientViewModel.wamsEncodingPresets.push(new StringItem("Unnamed"));
                        $("#presets-table tbody:first-of-type tr:last-of-type td:nth-child(2) input").focus().select();
                    }
                    Settings.addNewWamsEncodingPreset = addNewWamsEncodingPreset;

                    function deleteSubtitleLanguage(languageCultureCode) {
                        Settings.clientViewModel.subtitleLanguages.remove(languageCultureCode);
                    }
                    Settings.deleteSubtitleLanguage = deleteSubtitleLanguage;

                    function addNewSubtitleLanguage() {
                        Settings.clientViewModel.subtitleLanguages.push(new StringItem("Unnamed"));
                        $("#languages-table tbody:first-of-type tr:last-of-type td:nth-child(1) input").focus().select();
                    }
                    Settings.addNewSubtitleLanguage = addNewSubtitleLanguage;

                    $(function () {
                        $.each(initWamsEncodingPresets, function (presetIndex, presetName) {
                            Settings.clientViewModel.wamsEncodingPresets.push(new StringItem(presetName));
                        });

                        Settings.clientViewModel.defaultWamsEncodingPresetIndex(initDefaultWamsEncodingPresetIndex);

                        $.each(initSubtitleLanguages, function (languageIndex, languageCultureCode) {
                            Settings.clientViewModel.subtitleLanguages.push(new StringItem(languageCultureCode));
                        });

                        ko.applyBindings(Settings.clientViewModel);

                        var localStorage = window["localStorage"];
                        $("#tabs").tabs({
                            activate: function () {
                                if (localStorage && localStorage.setItem)
                                    localStorage.setItem("selectedCloudMediaSettingsTab", $("#tabs").tabs("option", "active"));
                            },
                            active: localStorage && localStorage.getItem ? localStorage.getItem("selectedCloudMediaSettingsTab") : null
                        }).show();
                    });
                })(Admin.Settings || (Admin.Settings = {}));
                var Settings = Admin.Settings;
            })(MediaServices.Admin || (MediaServices.Admin = {}));
            var Admin = MediaServices.Admin;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-admin-settings.js.map
