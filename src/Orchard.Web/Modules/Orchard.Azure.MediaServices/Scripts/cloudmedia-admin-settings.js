/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/knockout.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var Admin;
            (function (Admin) {
                var Settings;
                (function (Settings) {
                    var StringItem = (function () {
                        function StringItem(value) {
                            this.value = ko.observable(value);
                        }
                        return StringItem;
                    })();
                    Settings.StringItem = StringItem;
                    var EncodingPreset = (function () {
                        function EncodingPreset(name, customXml) {
                            this.name = ko.observable(name);
                            this.customXml = ko.observable(customXml);
                            this.isExpanded = ko.observable(false);
                            this.type = ko.computed(function () {
                                var customXml = this.customXml();
                                if (!!customXml && customXml.length > 0)
                                    return "Custom preset";
                                return "Standard preset";
                            }, this);
                        }
                        EncodingPreset.prototype.toggle = function () {
                            this.isExpanded(!this.isExpanded());
                        };
                        return EncodingPreset;
                    })();
                    Settings.EncodingPreset = EncodingPreset;
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
                        Settings.clientViewModel.wamsEncodingPresets.push(new EncodingPreset("Unnamed", null));
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
                        $.each(initWamsEncodingPresets, function (presetIndex, preset) {
                            Settings.clientViewModel.wamsEncodingPresets.push(new EncodingPreset(preset.name, preset.customXml));
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
                })(Settings = Admin.Settings || (Admin.Settings = {}));
            })(Admin = MediaServices.Admin || (MediaServices.Admin = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
