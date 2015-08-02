/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />
/// <reference path="Typings/knockout.d.ts" />

declare var initWamsEncodingPresets: any[];
declare var initDefaultWamsEncodingPresetIndex: number;
declare var initSubtitleLanguages: string[];

module Orchard.Azure.MediaServices.Admin.Settings {

    export class StringItem {
        constructor(value: string) {
            this.value = ko.observable(value);
        }

        public value: KnockoutObservable<string>;
    }

    export class EncodingPreset {
        constructor(name: string, customXml: string) {
            this.name = ko.observable(name);
            this.customXml = ko.observable(customXml);
            this.isExpanded = ko.observable(false);
            this.type = ko.computed(function () {
                var customXml: string = this.customXml();
                if (!!customXml && customXml.length > 0)
                    return "Custom preset";
                return "Standard preset";
            }, this);
        }

        public name: KnockoutObservable<string>;
        public customXml: KnockoutObservable<string>;
        public isExpanded: KnockoutObservable<boolean>;
        public type: KnockoutComputed<string>;

        public toggle() {
            this.isExpanded(!this.isExpanded());
        }
    }

    export interface IClientViewModel {
        wamsEncodingPresets: KnockoutObservableArray<EncodingPreset>;
        defaultWamsEncodingPresetIndex: KnockoutObservable<number>;
        subtitleLanguages: KnockoutObservableArray<StringItem>;
    }

    export var clientViewModel: IClientViewModel = {
        wamsEncodingPresets: ko.observableArray<EncodingPreset>(),
        defaultWamsEncodingPresetIndex: ko.observable<number>(),
        subtitleLanguages: ko.observableArray<StringItem>()
    };

    export function deleteWamsEncodingPreset(preset: EncodingPreset) {
        var removedIndex = clientViewModel.wamsEncodingPresets.indexOf(preset);
        clientViewModel.wamsEncodingPresets.remove(preset);
        if (removedIndex === clientViewModel.defaultWamsEncodingPresetIndex())
            clientViewModel.defaultWamsEncodingPresetIndex(0);
        else if (removedIndex < clientViewModel.defaultWamsEncodingPresetIndex())
            clientViewModel.defaultWamsEncodingPresetIndex(clientViewModel.defaultWamsEncodingPresetIndex() - 1);
    }

    export function addNewWamsEncodingPreset() {
        clientViewModel.wamsEncodingPresets.push(new EncodingPreset("Unnamed", null));
        $("#presets-table tbody:first-of-type tr:last-of-type td:nth-child(2) input").focus().select();
    }

    export function deleteSubtitleLanguage(languageCultureCode: StringItem) {
        clientViewModel.subtitleLanguages.remove(languageCultureCode);
    }

    export function addNewSubtitleLanguage() {
        clientViewModel.subtitleLanguages.push(new StringItem("Unnamed"));
        $("#languages-table tbody:first-of-type tr:last-of-type td:nth-child(1) input").focus().select();
    }

    $(function () {
        $.each(initWamsEncodingPresets, function (presetIndex: number, preset: any) {
            clientViewModel.wamsEncodingPresets.push(new EncodingPreset(preset.name, preset.customXml));
        });

        clientViewModel.defaultWamsEncodingPresetIndex(initDefaultWamsEncodingPresetIndex);

        $.each(initSubtitleLanguages, function (languageIndex: number, languageCultureCode: string) {
            clientViewModel.subtitleLanguages.push(new StringItem(languageCultureCode));
        });

        ko.applyBindings(clientViewModel);

        var localStorage = window["localStorage"];
        $("#tabs").tabs({
            activate: function () {
                if (localStorage && localStorage.setItem)
                    localStorage.setItem("selectedCloudMediaSettingsTab", $("#tabs").tabs("option", "active"));
            },
            active: localStorage && localStorage.getItem ? localStorage.getItem("selectedCloudMediaSettingsTab") : null
        }).show();
    });
}