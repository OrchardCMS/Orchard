/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />
/// <reference path="Typings/knockout.d.ts" />
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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtYWRtaW4tc2V0dGluZ3MudHMiXSwibmFtZXMiOlsiT3JjaGFyZCIsIk9yY2hhcmQuQXp1cmUiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4iLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MuU3RyaW5nSXRlbSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5BZG1pbi5TZXR0aW5ncy5TdHJpbmdJdGVtLmNvbnN0cnVjdG9yIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLkVuY29kaW5nUHJlc2V0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLkVuY29kaW5nUHJlc2V0LmNvbnN0cnVjdG9yIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLkVuY29kaW5nUHJlc2V0LnRvZ2dsZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5BZG1pbi5TZXR0aW5ncy5kZWxldGVXYW1zRW5jb2RpbmdQcmVzZXQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MuYWRkTmV3V2Ftc0VuY29kaW5nUHJlc2V0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLmRlbGV0ZVN1YnRpdGxlTGFuZ3VhZ2UiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MuYWRkTmV3U3VidGl0bGVMYW5ndWFnZSJdLCJtYXBwaW5ncyI6IkFBQUEsNENBQTRDO0FBQzVDLDhDQUE4QztBQUM5Qyw4Q0FBOEM7QUFNOUMsSUFBTyxPQUFPLENBMEZiO0FBMUZELFdBQU8sT0FBTztJQUFDQSxJQUFB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VUFBVSxDQUFDLG1CQUFtQixDQUFDLENBQUMsQ0FBQzt3QkFDaEYsQ0FBQyxDQUFDLENBQUM7d0JBRUgsRUFBRSxDQUFDLGFBQWEsQ0FBQyx3QkFBZSxDQUFDLENBQUM7d0JBRWxDLElBQUksWUFBWSxHQUFHLE1BQU0sQ0FBQyxjQUFjLENBQUMsQ0FBQzt3QkFDMUMsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxDQUFDLElBQUksQ0FBQzs0QkFDWixRQUFRLEVBQUU7Z0NBQ04sRUFBRSxDQUFDLENBQUMsWUFBWSxJQUFJLFlBQVksQ0FBQyxPQUFPLENBQUM7b0NBQ3JDLFlBQVksQ0FBQyxPQUFPLENBQUMsK0JBQStCLEVBQUUsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxDQUFDLElBQUksQ0FBQyxRQUFRLEVBQUUsUUFBUSxDQUFDLENBQUMsQ0FBQzs0QkFDbkcsQ0FBQzs0QkFDRCxNQUFNLEVBQUUsWUFBWSxJQUFJLFlBQVksQ0FBQyxPQUFPLEdBQUcsWUFBWSxDQUFDLE9BQU8sQ0FBQywrQkFBK0IsQ0FBQyxHQUFHLElBQUk7eUJBQzlHLENBQUMsQ0FBQyxJQUFJLEVBQUUsQ0FBQztvQkFDZCxDQUFDLENBQUNBLENBQUNBO2dCQUNQQSxDQUFDQSxFQTFGd0NELFFBQVFBLEdBQVJBLGNBQVFBLEtBQVJBLGNBQVFBLFFBMEZoREE7WUFBREEsQ0FBQ0EsRUExRmtDRCxLQUFLQSxHQUFMQSxtQkFBS0EsS0FBTEEsbUJBQUtBLFFBMEZ2Q0E7UUFBREEsQ0FBQ0EsRUExRm9CRCxhQUFhQSxHQUFiQSxtQkFBYUEsS0FBYkEsbUJBQWFBLFFBMEZqQ0E7SUFBREEsQ0FBQ0EsRUExRmNELEtBQUtBLEdBQUxBLGFBQUtBLEtBQUxBLGFBQUtBLFFBMEZuQkE7QUFBREEsQ0FBQ0EsRUExRk0sT0FBTyxLQUFQLE9BQU8sUUEwRmIiLCJmaWxlIjoiY2xvdWRtZWRpYS1hZG1pbi1zZXR0aW5ncy5qcyIsInNvdXJjZXNDb250ZW50IjpbIi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2pxdWVyeS5kLnRzXCIgLz5cclxuLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5dWkuZC50c1wiIC8+XHJcbi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2tub2Nrb3V0LmQudHNcIiAvPlxyXG5cclxuZGVjbGFyZSB2YXIgaW5pdFdhbXNFbmNvZGluZ1ByZXNldHM6IGFueVtdO1xyXG5kZWNsYXJlIHZhciBpbml0RGVmYXVsdFdhbXNFbmNvZGluZ1ByZXNldEluZGV4OiBudW1iZXI7XHJcbmRlY2xhcmUgdmFyIGluaXRTdWJ0aXRsZUxhbmd1YWdlczogc3RyaW5nW107XHJcblxyXG5tb2R1bGUgT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgU3RyaW5nSXRlbSB7XHJcbiAgICAgICAgY29uc3RydWN0b3IodmFsdWU6IHN0cmluZykge1xyXG4gICAgICAgICAgICB0aGlzLnZhbHVlID0ga28ub2JzZXJ2YWJsZSh2YWx1ZSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwdWJsaWMgdmFsdWU6IEtub2Nrb3V0T2JzZXJ2YWJsZTxzdHJpbmc+O1xyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBFbmNvZGluZ1ByZXNldCB7XHJcbiAgICAgICAgY29uc3RydWN0b3IobmFtZTogc3RyaW5nLCBjdXN0b21YbWw6IHN0cmluZykge1xyXG4gICAgICAgICAgICB0aGlzLm5hbWUgPSBrby5vYnNlcnZhYmxlKG5hbWUpO1xyXG4gICAgICAgICAgICB0aGlzLmN1c3RvbVhtbCA9IGtvLm9ic2VydmFibGUoY3VzdG9tWG1sKTtcclxuICAgICAgICAgICAgdGhpcy5pc0V4cGFuZGVkID0ga28ub2JzZXJ2YWJsZShmYWxzZSk7XHJcbiAgICAgICAgICAgIHRoaXMudHlwZSA9IGtvLmNvbXB1dGVkKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHZhciBjdXN0b21YbWw6IHN0cmluZyA9IHRoaXMuY3VzdG9tWG1sKCk7XHJcbiAgICAgICAgICAgICAgICBpZiAoISFjdXN0b21YbWwgJiYgY3VzdG9tWG1sLmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIFwiQ3VzdG9tIHByZXNldFwiO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIFwiU3RhbmRhcmQgcHJlc2V0XCI7XHJcbiAgICAgICAgICAgIH0sIHRoaXMpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHVibGljIG5hbWU6IEtub2Nrb3V0T2JzZXJ2YWJsZTxzdHJpbmc+O1xyXG4gICAgICAgIHB1YmxpYyBjdXN0b21YbWw6IEtub2Nrb3V0T2JzZXJ2YWJsZTxzdHJpbmc+O1xyXG4gICAgICAgIHB1YmxpYyBpc0V4cGFuZGVkOiBLbm9ja291dE9ic2VydmFibGU8Ym9vbGVhbj47XHJcbiAgICAgICAgcHVibGljIHR5cGU6IEtub2Nrb3V0Q29tcHV0ZWQ8c3RyaW5nPjtcclxuXHJcbiAgICAgICAgcHVibGljIHRvZ2dsZSgpIHtcclxuICAgICAgICAgICAgdGhpcy5pc0V4cGFuZGVkKCF0aGlzLmlzRXhwYW5kZWQoKSk7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCBpbnRlcmZhY2UgSUNsaWVudFZpZXdNb2RlbCB7XHJcbiAgICAgICAgd2Ftc0VuY29kaW5nUHJlc2V0czogS25vY2tvdXRPYnNlcnZhYmxlQXJyYXk8RW5jb2RpbmdQcmVzZXQ+O1xyXG4gICAgICAgIGRlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleDogS25vY2tvdXRPYnNlcnZhYmxlPG51bWJlcj47XHJcbiAgICAgICAgc3VidGl0bGVMYW5ndWFnZXM6IEtub2Nrb3V0T2JzZXJ2YWJsZUFycmF5PFN0cmluZ0l0ZW0+O1xyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCB2YXIgY2xpZW50Vmlld01vZGVsOiBJQ2xpZW50Vmlld01vZGVsID0ge1xyXG4gICAgICAgIHdhbXNFbmNvZGluZ1ByZXNldHM6IGtvLm9ic2VydmFibGVBcnJheTxFbmNvZGluZ1ByZXNldD4oKSxcclxuICAgICAgICBkZWZhdWx0V2Ftc0VuY29kaW5nUHJlc2V0SW5kZXg6IGtvLm9ic2VydmFibGU8bnVtYmVyPigpLFxyXG4gICAgICAgIHN1YnRpdGxlTGFuZ3VhZ2VzOiBrby5vYnNlcnZhYmxlQXJyYXk8U3RyaW5nSXRlbT4oKVxyXG4gICAgfTtcclxuXHJcbiAgICBleHBvcnQgZnVuY3Rpb24gZGVsZXRlV2Ftc0VuY29kaW5nUHJlc2V0KHByZXNldDogRW5jb2RpbmdQcmVzZXQpIHtcclxuICAgICAgICB2YXIgcmVtb3ZlZEluZGV4ID0gY2xpZW50Vmlld01vZGVsLndhbXNFbmNvZGluZ1ByZXNldHMuaW5kZXhPZihwcmVzZXQpO1xyXG4gICAgICAgIGNsaWVudFZpZXdNb2RlbC53YW1zRW5jb2RpbmdQcmVzZXRzLnJlbW92ZShwcmVzZXQpO1xyXG4gICAgICAgIGlmIChyZW1vdmVkSW5kZXggPT09IGNsaWVudFZpZXdNb2RlbC5kZWZhdWx0V2Ftc0VuY29kaW5nUHJlc2V0SW5kZXgoKSlcclxuICAgICAgICAgICAgY2xpZW50Vmlld01vZGVsLmRlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleCgwKTtcclxuICAgICAgICBlbHNlIGlmIChyZW1vdmVkSW5kZXggPCBjbGllbnRWaWV3TW9kZWwuZGVmYXVsdFdhbXNFbmNvZGluZ1ByZXNldEluZGV4KCkpXHJcbiAgICAgICAgICAgIGNsaWVudFZpZXdNb2RlbC5kZWZhdWx0V2Ftc0VuY29kaW5nUHJlc2V0SW5kZXgoY2xpZW50Vmlld01vZGVsLmRlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleCgpIC0gMSk7XHJcbiAgICB9XHJcblxyXG4gICAgZXhwb3J0IGZ1bmN0aW9uIGFkZE5ld1dhbXNFbmNvZGluZ1ByZXNldCgpIHtcclxuICAgICAgICBjbGllbnRWaWV3TW9kZWwud2Ftc0VuY29kaW5nUHJlc2V0cy5wdXNoKG5ldyBFbmNvZGluZ1ByZXNldChcIlVubmFtZWRcIiwgbnVsbCkpO1xyXG4gICAgICAgICQoXCIjcHJlc2V0cy10YWJsZSB0Ym9keTpmaXJzdC1vZi10eXBlIHRyOmxhc3Qtb2YtdHlwZSB0ZDpudGgtY2hpbGQoMikgaW5wdXRcIikuZm9jdXMoKS5zZWxlY3QoKTtcclxuICAgIH1cclxuXHJcbiAgICBleHBvcnQgZnVuY3Rpb24gZGVsZXRlU3VidGl0bGVMYW5ndWFnZShsYW5ndWFnZUN1bHR1cmVDb2RlOiBTdHJpbmdJdGVtKSB7XHJcbiAgICAgICAgY2xpZW50Vmlld01vZGVsLnN1YnRpdGxlTGFuZ3VhZ2VzLnJlbW92ZShsYW5ndWFnZUN1bHR1cmVDb2RlKTtcclxuICAgIH1cclxuXHJcbiAgICBleHBvcnQgZnVuY3Rpb24gYWRkTmV3U3VidGl0bGVMYW5ndWFnZSgpIHtcclxuICAgICAgICBjbGllbnRWaWV3TW9kZWwuc3VidGl0bGVMYW5ndWFnZXMucHVzaChuZXcgU3RyaW5nSXRlbShcIlVubmFtZWRcIikpO1xyXG4gICAgICAgICQoXCIjbGFuZ3VhZ2VzLXRhYmxlIHRib2R5OmZpcnN0LW9mLXR5cGUgdHI6bGFzdC1vZi10eXBlIHRkOm50aC1jaGlsZCgxKSBpbnB1dFwiKS5mb2N1cygpLnNlbGVjdCgpO1xyXG4gICAgfVxyXG5cclxuICAgICQoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICQuZWFjaChpbml0V2Ftc0VuY29kaW5nUHJlc2V0cywgZnVuY3Rpb24gKHByZXNldEluZGV4OiBudW1iZXIsIHByZXNldDogYW55KSB7XHJcbiAgICAgICAgICAgIGNsaWVudFZpZXdNb2RlbC53YW1zRW5jb2RpbmdQcmVzZXRzLnB1c2gobmV3IEVuY29kaW5nUHJlc2V0KHByZXNldC5uYW1lLCBwcmVzZXQuY3VzdG9tWG1sKSk7XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgIGNsaWVudFZpZXdNb2RlbC5kZWZhdWx0V2Ftc0VuY29kaW5nUHJlc2V0SW5kZXgoaW5pdERlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleCk7XHJcblxyXG4gICAgICAgICQuZWFjaChpbml0U3VidGl0bGVMYW5ndWFnZXMsIGZ1bmN0aW9uIChsYW5ndWFnZUluZGV4OiBudW1iZXIsIGxhbmd1YWdlQ3VsdHVyZUNvZGU6IHN0cmluZykge1xyXG4gICAgICAgICAgICBjbGllbnRWaWV3TW9kZWwuc3VidGl0bGVMYW5ndWFnZXMucHVzaChuZXcgU3RyaW5nSXRlbShsYW5ndWFnZUN1bHR1cmVDb2RlKSk7XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgIGtvLmFwcGx5QmluZGluZ3MoY2xpZW50Vmlld01vZGVsKTtcclxuXHJcbiAgICAgICAgdmFyIGxvY2FsU3RvcmFnZSA9IHdpbmRvd1tcImxvY2FsU3RvcmFnZVwiXTtcclxuICAgICAgICAkKFwiI3RhYnNcIikudGFicyh7XHJcbiAgICAgICAgICAgIGFjdGl2YXRlOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAobG9jYWxTdG9yYWdlICYmIGxvY2FsU3RvcmFnZS5zZXRJdGVtKVxyXG4gICAgICAgICAgICAgICAgICAgIGxvY2FsU3RvcmFnZS5zZXRJdGVtKFwic2VsZWN0ZWRDbG91ZE1lZGlhU2V0dGluZ3NUYWJcIiwgJChcIiN0YWJzXCIpLnRhYnMoXCJvcHRpb25cIiwgXCJhY3RpdmVcIikpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBhY3RpdmU6IGxvY2FsU3RvcmFnZSAmJiBsb2NhbFN0b3JhZ2UuZ2V0SXRlbSA/IGxvY2FsU3RvcmFnZS5nZXRJdGVtKFwic2VsZWN0ZWRDbG91ZE1lZGlhU2V0dGluZ3NUYWJcIikgOiBudWxsXHJcbiAgICAgICAgfSkuc2hvdygpO1xyXG4gICAgfSk7XHJcbn0iXSwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=