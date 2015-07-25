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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtYWRtaW4tc2V0dGluZ3MudHMiXSwibmFtZXMiOlsiT3JjaGFyZCIsIk9yY2hhcmQuQXp1cmUiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4iLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MuU3RyaW5nSXRlbSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5BZG1pbi5TZXR0aW5ncy5TdHJpbmdJdGVtLmNvbnN0cnVjdG9yIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLkVuY29kaW5nUHJlc2V0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLkVuY29kaW5nUHJlc2V0LmNvbnN0cnVjdG9yIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLkVuY29kaW5nUHJlc2V0LnRvZ2dsZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5BZG1pbi5TZXR0aW5ncy5kZWxldGVXYW1zRW5jb2RpbmdQcmVzZXQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MuYWRkTmV3V2Ftc0VuY29kaW5nUHJlc2V0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFkbWluLlNldHRpbmdzLmRlbGV0ZVN1YnRpdGxlTGFuZ3VhZ2UiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3MuYWRkTmV3U3VidGl0bGVMYW5ndWFnZSJdLCJtYXBwaW5ncyI6IkFBQUEsNENBQTRDO0FBQzVDLDhDQUE4QztBQUM5Qyw4Q0FBOEM7QUFNOUMsSUFBTyxPQUFPLENBMEZiO0FBMUZELFdBQU8sT0FBTztJQUFDQSxJQUFB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VUFBVSxDQUFDLG1CQUFtQixDQUFDLENBQUMsQ0FBQzt3QkFDaEYsQ0FBQyxDQUFDLENBQUM7d0JBRUgsRUFBRSxDQUFDLGFBQWEsQ0FBQyx3QkFBZSxDQUFDLENBQUM7d0JBRWxDLElBQUksWUFBWSxHQUFHLE1BQU0sQ0FBQyxjQUFjLENBQUMsQ0FBQzt3QkFDMUMsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxDQUFDLElBQUksQ0FBQzs0QkFDWixRQUFRLEVBQUU7Z0NBQ04sRUFBRSxDQUFDLENBQUMsWUFBWSxJQUFJLFlBQVksQ0FBQyxPQUFPLENBQUM7b0NBQ3JDLFlBQVksQ0FBQyxPQUFPLENBQUMsK0JBQStCLEVBQUUsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxDQUFDLElBQUksQ0FBQyxRQUFRLEVBQUUsUUFBUSxDQUFDLENBQUMsQ0FBQzs0QkFDbkcsQ0FBQzs0QkFDRCxNQUFNLEVBQUUsWUFBWSxJQUFJLFlBQVksQ0FBQyxPQUFPLEdBQUcsWUFBWSxDQUFDLE9BQU8sQ0FBQywrQkFBK0IsQ0FBQyxHQUFHLElBQUk7eUJBQzlHLENBQUMsQ0FBQyxJQUFJLEVBQUUsQ0FBQztvQkFDZCxDQUFDLENBQUNBLENBQUNBO2dCQUNQQSxDQUFDQSxFQTFGd0NELFFBQVFBLEdBQVJBLGNBQVFBLEtBQVJBLGNBQVFBLFFBMEZoREE7WUFBREEsQ0FBQ0EsRUExRmtDRCxLQUFLQSxHQUFMQSxtQkFBS0EsS0FBTEEsbUJBQUtBLFFBMEZ2Q0E7UUFBREEsQ0FBQ0EsRUExRm9CRCxhQUFhQSxHQUFiQSxtQkFBYUEsS0FBYkEsbUJBQWFBLFFBMEZqQ0E7SUFBREEsQ0FBQ0EsRUExRmNELEtBQUtBLEdBQUxBLGFBQUtBLEtBQUxBLGFBQUtBLFFBMEZuQkE7QUFBREEsQ0FBQ0EsRUExRk0sT0FBTyxLQUFQLE9BQU8sUUEwRmIiLCJmaWxlIjoiY2xvdWRtZWRpYS1hZG1pbi1zZXR0aW5ncy5qcyIsInNvdXJjZXNDb250ZW50IjpbIi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2pxdWVyeS5kLnRzXCIgLz5cbi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2pxdWVyeXVpLmQudHNcIiAvPlxuLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3Mva25vY2tvdXQuZC50c1wiIC8+XG5cbmRlY2xhcmUgdmFyIGluaXRXYW1zRW5jb2RpbmdQcmVzZXRzOiBhbnlbXTtcbmRlY2xhcmUgdmFyIGluaXREZWZhdWx0V2Ftc0VuY29kaW5nUHJlc2V0SW5kZXg6IG51bWJlcjtcbmRlY2xhcmUgdmFyIGluaXRTdWJ0aXRsZUxhbmd1YWdlczogc3RyaW5nW107XG5cbm1vZHVsZSBPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQWRtaW4uU2V0dGluZ3Mge1xuXG4gICAgZXhwb3J0IGNsYXNzIFN0cmluZ0l0ZW0ge1xuICAgICAgICBjb25zdHJ1Y3Rvcih2YWx1ZTogc3RyaW5nKSB7XG4gICAgICAgICAgICB0aGlzLnZhbHVlID0ga28ub2JzZXJ2YWJsZSh2YWx1ZSk7XG4gICAgICAgIH1cblxuICAgICAgICBwdWJsaWMgdmFsdWU6IEtub2Nrb3V0T2JzZXJ2YWJsZTxzdHJpbmc+O1xuICAgIH1cblxuICAgIGV4cG9ydCBjbGFzcyBFbmNvZGluZ1ByZXNldCB7XG4gICAgICAgIGNvbnN0cnVjdG9yKG5hbWU6IHN0cmluZywgY3VzdG9tWG1sOiBzdHJpbmcpIHtcbiAgICAgICAgICAgIHRoaXMubmFtZSA9IGtvLm9ic2VydmFibGUobmFtZSk7XG4gICAgICAgICAgICB0aGlzLmN1c3RvbVhtbCA9IGtvLm9ic2VydmFibGUoY3VzdG9tWG1sKTtcbiAgICAgICAgICAgIHRoaXMuaXNFeHBhbmRlZCA9IGtvLm9ic2VydmFibGUoZmFsc2UpO1xuICAgICAgICAgICAgdGhpcy50eXBlID0ga28uY29tcHV0ZWQoZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIHZhciBjdXN0b21YbWw6IHN0cmluZyA9IHRoaXMuY3VzdG9tWG1sKCk7XG4gICAgICAgICAgICAgICAgaWYgKCEhY3VzdG9tWG1sICYmIGN1c3RvbVhtbC5sZW5ndGggPiAwKVxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gXCJDdXN0b20gcHJlc2V0XCI7XG4gICAgICAgICAgICAgICAgcmV0dXJuIFwiU3RhbmRhcmQgcHJlc2V0XCI7XG4gICAgICAgICAgICB9LCB0aGlzKTtcbiAgICAgICAgfVxuXG4gICAgICAgIHB1YmxpYyBuYW1lOiBLbm9ja291dE9ic2VydmFibGU8c3RyaW5nPjtcbiAgICAgICAgcHVibGljIGN1c3RvbVhtbDogS25vY2tvdXRPYnNlcnZhYmxlPHN0cmluZz47XG4gICAgICAgIHB1YmxpYyBpc0V4cGFuZGVkOiBLbm9ja291dE9ic2VydmFibGU8Ym9vbGVhbj47XG4gICAgICAgIHB1YmxpYyB0eXBlOiBLbm9ja291dENvbXB1dGVkPHN0cmluZz47XG5cbiAgICAgICAgcHVibGljIHRvZ2dsZSgpIHtcbiAgICAgICAgICAgIHRoaXMuaXNFeHBhbmRlZCghdGhpcy5pc0V4cGFuZGVkKCkpO1xuICAgICAgICB9XG4gICAgfVxuXG4gICAgZXhwb3J0IGludGVyZmFjZSBJQ2xpZW50Vmlld01vZGVsIHtcbiAgICAgICAgd2Ftc0VuY29kaW5nUHJlc2V0czogS25vY2tvdXRPYnNlcnZhYmxlQXJyYXk8RW5jb2RpbmdQcmVzZXQ+O1xuICAgICAgICBkZWZhdWx0V2Ftc0VuY29kaW5nUHJlc2V0SW5kZXg6IEtub2Nrb3V0T2JzZXJ2YWJsZTxudW1iZXI+O1xuICAgICAgICBzdWJ0aXRsZUxhbmd1YWdlczogS25vY2tvdXRPYnNlcnZhYmxlQXJyYXk8U3RyaW5nSXRlbT47XG4gICAgfVxuXG4gICAgZXhwb3J0IHZhciBjbGllbnRWaWV3TW9kZWw6IElDbGllbnRWaWV3TW9kZWwgPSB7XG4gICAgICAgIHdhbXNFbmNvZGluZ1ByZXNldHM6IGtvLm9ic2VydmFibGVBcnJheTxFbmNvZGluZ1ByZXNldD4oKSxcbiAgICAgICAgZGVmYXVsdFdhbXNFbmNvZGluZ1ByZXNldEluZGV4OiBrby5vYnNlcnZhYmxlPG51bWJlcj4oKSxcbiAgICAgICAgc3VidGl0bGVMYW5ndWFnZXM6IGtvLm9ic2VydmFibGVBcnJheTxTdHJpbmdJdGVtPigpXG4gICAgfTtcblxuICAgIGV4cG9ydCBmdW5jdGlvbiBkZWxldGVXYW1zRW5jb2RpbmdQcmVzZXQocHJlc2V0OiBFbmNvZGluZ1ByZXNldCkge1xuICAgICAgICB2YXIgcmVtb3ZlZEluZGV4ID0gY2xpZW50Vmlld01vZGVsLndhbXNFbmNvZGluZ1ByZXNldHMuaW5kZXhPZihwcmVzZXQpO1xuICAgICAgICBjbGllbnRWaWV3TW9kZWwud2Ftc0VuY29kaW5nUHJlc2V0cy5yZW1vdmUocHJlc2V0KTtcbiAgICAgICAgaWYgKHJlbW92ZWRJbmRleCA9PT0gY2xpZW50Vmlld01vZGVsLmRlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleCgpKVxuICAgICAgICAgICAgY2xpZW50Vmlld01vZGVsLmRlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleCgwKTtcbiAgICAgICAgZWxzZSBpZiAocmVtb3ZlZEluZGV4IDwgY2xpZW50Vmlld01vZGVsLmRlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleCgpKVxuICAgICAgICAgICAgY2xpZW50Vmlld01vZGVsLmRlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleChjbGllbnRWaWV3TW9kZWwuZGVmYXVsdFdhbXNFbmNvZGluZ1ByZXNldEluZGV4KCkgLSAxKTtcbiAgICB9XG5cbiAgICBleHBvcnQgZnVuY3Rpb24gYWRkTmV3V2Ftc0VuY29kaW5nUHJlc2V0KCkge1xuICAgICAgICBjbGllbnRWaWV3TW9kZWwud2Ftc0VuY29kaW5nUHJlc2V0cy5wdXNoKG5ldyBFbmNvZGluZ1ByZXNldChcIlVubmFtZWRcIiwgbnVsbCkpO1xuICAgICAgICAkKFwiI3ByZXNldHMtdGFibGUgdGJvZHk6Zmlyc3Qtb2YtdHlwZSB0cjpsYXN0LW9mLXR5cGUgdGQ6bnRoLWNoaWxkKDIpIGlucHV0XCIpLmZvY3VzKCkuc2VsZWN0KCk7XG4gICAgfVxuXG4gICAgZXhwb3J0IGZ1bmN0aW9uIGRlbGV0ZVN1YnRpdGxlTGFuZ3VhZ2UobGFuZ3VhZ2VDdWx0dXJlQ29kZTogU3RyaW5nSXRlbSkge1xuICAgICAgICBjbGllbnRWaWV3TW9kZWwuc3VidGl0bGVMYW5ndWFnZXMucmVtb3ZlKGxhbmd1YWdlQ3VsdHVyZUNvZGUpO1xuICAgIH1cblxuICAgIGV4cG9ydCBmdW5jdGlvbiBhZGROZXdTdWJ0aXRsZUxhbmd1YWdlKCkge1xuICAgICAgICBjbGllbnRWaWV3TW9kZWwuc3VidGl0bGVMYW5ndWFnZXMucHVzaChuZXcgU3RyaW5nSXRlbShcIlVubmFtZWRcIikpO1xuICAgICAgICAkKFwiI2xhbmd1YWdlcy10YWJsZSB0Ym9keTpmaXJzdC1vZi10eXBlIHRyOmxhc3Qtb2YtdHlwZSB0ZDpudGgtY2hpbGQoMSkgaW5wdXRcIikuZm9jdXMoKS5zZWxlY3QoKTtcbiAgICB9XG5cbiAgICAkKGZ1bmN0aW9uICgpIHtcbiAgICAgICAgJC5lYWNoKGluaXRXYW1zRW5jb2RpbmdQcmVzZXRzLCBmdW5jdGlvbiAocHJlc2V0SW5kZXg6IG51bWJlciwgcHJlc2V0OiBhbnkpIHtcbiAgICAgICAgICAgIGNsaWVudFZpZXdNb2RlbC53YW1zRW5jb2RpbmdQcmVzZXRzLnB1c2gobmV3IEVuY29kaW5nUHJlc2V0KHByZXNldC5uYW1lLCBwcmVzZXQuY3VzdG9tWG1sKSk7XG4gICAgICAgIH0pO1xuXG4gICAgICAgIGNsaWVudFZpZXdNb2RlbC5kZWZhdWx0V2Ftc0VuY29kaW5nUHJlc2V0SW5kZXgoaW5pdERlZmF1bHRXYW1zRW5jb2RpbmdQcmVzZXRJbmRleCk7XG5cbiAgICAgICAgJC5lYWNoKGluaXRTdWJ0aXRsZUxhbmd1YWdlcywgZnVuY3Rpb24gKGxhbmd1YWdlSW5kZXg6IG51bWJlciwgbGFuZ3VhZ2VDdWx0dXJlQ29kZTogc3RyaW5nKSB7XG4gICAgICAgICAgICBjbGllbnRWaWV3TW9kZWwuc3VidGl0bGVMYW5ndWFnZXMucHVzaChuZXcgU3RyaW5nSXRlbShsYW5ndWFnZUN1bHR1cmVDb2RlKSk7XG4gICAgICAgIH0pO1xuXG4gICAgICAgIGtvLmFwcGx5QmluZGluZ3MoY2xpZW50Vmlld01vZGVsKTtcblxuICAgICAgICB2YXIgbG9jYWxTdG9yYWdlID0gd2luZG93W1wibG9jYWxTdG9yYWdlXCJdO1xuICAgICAgICAkKFwiI3RhYnNcIikudGFicyh7XG4gICAgICAgICAgICBhY3RpdmF0ZTogZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIGlmIChsb2NhbFN0b3JhZ2UgJiYgbG9jYWxTdG9yYWdlLnNldEl0ZW0pXG4gICAgICAgICAgICAgICAgICAgIGxvY2FsU3RvcmFnZS5zZXRJdGVtKFwic2VsZWN0ZWRDbG91ZE1lZGlhU2V0dGluZ3NUYWJcIiwgJChcIiN0YWJzXCIpLnRhYnMoXCJvcHRpb25cIiwgXCJhY3RpdmVcIikpO1xuICAgICAgICAgICAgfSxcbiAgICAgICAgICAgIGFjdGl2ZTogbG9jYWxTdG9yYWdlICYmIGxvY2FsU3RvcmFnZS5nZXRJdGVtID8gbG9jYWxTdG9yYWdlLmdldEl0ZW0oXCJzZWxlY3RlZENsb3VkTWVkaWFTZXR0aW5nc1RhYlwiKSA6IG51bGxcbiAgICAgICAgfSkuc2hvdygpO1xuICAgIH0pO1xufSJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==