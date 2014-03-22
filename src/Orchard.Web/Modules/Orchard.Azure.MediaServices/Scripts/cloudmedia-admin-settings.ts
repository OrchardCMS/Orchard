/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/knockout.d.ts" />

declare var initWamsEncodingPresets: string[];
declare var initDefaultWamsEncodingPresetIndex: number;
declare var initSubtitleLanguages: string[];

module Orchard.Azure.MediaServices.Admin.Settings
{
    export class StringItem
    {
        constructor(value: string)
        {
            this.value = ko.observable(value);
        }

        public value: KnockoutObservable<string>;
    }

    export interface IClientViewModel
    {
        wamsEncodingPresets: KnockoutObservableArray<StringItem>;
        defaultWamsEncodingPresetIndex: KnockoutObservable<number>;
        subtitleLanguages: KnockoutObservableArray<StringItem>;
    }

    export var clientViewModel: IClientViewModel = {
        wamsEncodingPresets: ko.observableArray<StringItem>(),
        defaultWamsEncodingPresetIndex: ko.observable<number>(),
        subtitleLanguages: ko.observableArray<StringItem>()
    };

    export function deleteWamsEncodingPreset(preset: StringItem)
    {
        var removedIndex = clientViewModel.wamsEncodingPresets.indexOf(preset);
        clientViewModel.wamsEncodingPresets.remove(preset);
        if (removedIndex === clientViewModel.defaultWamsEncodingPresetIndex())
            clientViewModel.defaultWamsEncodingPresetIndex(0);
        else if (removedIndex < clientViewModel.defaultWamsEncodingPresetIndex())
            clientViewModel.defaultWamsEncodingPresetIndex(clientViewModel.defaultWamsEncodingPresetIndex() - 1);
    }

    export function addNewWamsEncodingPreset()
    {
        clientViewModel.wamsEncodingPresets.push(new StringItem("Unnamed"));
        $("#presets-table tbody:first-of-type tr:last-of-type td:nth-child(2) input").focus().select();
    }

    export function deleteSubtitleLanguage(languageCultureCode: StringItem)
    {
        clientViewModel.subtitleLanguages.remove(languageCultureCode);
    }

    export function addNewSubtitleLanguage()
    {
        clientViewModel.subtitleLanguages.push(new StringItem("Unnamed"));
        $("#languages-table tbody:first-of-type tr:last-of-type td:nth-child(1) input").focus().select();
    }

    $(function ()
    {
        $.each(initWamsEncodingPresets, function (presetIndex: number, presetName: string)
        {
            clientViewModel.wamsEncodingPresets.push(new StringItem(presetName));
        });

        clientViewModel.defaultWamsEncodingPresetIndex(initDefaultWamsEncodingPresetIndex);

        $.each(initSubtitleLanguages, function (languageIndex: number, languageCultureCode: string)
        {
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
