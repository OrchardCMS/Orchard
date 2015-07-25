/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var CloudVideoEdit;
            (function (CloudVideoEdit) {
                function hasCorsSupport() {
                    return 'withCredentials' in new XMLHttpRequest();
                }
                $(function () {
                    var corsSupported = hasCorsSupport();
                    if (corsSupported) {
                        CloudVideoEdit.initializeUploadDirect();
                    }
                    else {
                        CloudVideoEdit.initializeUploadProxied();
                    }
                    var localStorage = window["localStorage"];
                    var isCreating = $("#tabs").data("cloudvideo-iscreating");
                    $("#tabs").tabs({
                        activate: function () {
                            if (localStorage && localStorage.setItem)
                                localStorage.setItem("selectedCloudVideoTab", $("#tabs").tabs("option", "active"));
                        },
                        active: !isCreating && localStorage && localStorage.getItem ? localStorage.getItem("selectedCloudVideoTab") : null
                    }).show();
                });
            })(CloudVideoEdit = MediaServices.CloudVideoEdit || (MediaServices.CloudVideoEdit = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtZWRpdC1jbG91ZHZpZGVvcGFydC50cyJdLCJuYW1lcyI6WyJPcmNoYXJkIiwiT3JjaGFyZC5BenVyZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5DbG91ZFZpZGVvRWRpdC5oYXNDb3JzU3VwcG9ydCJdLCJtYXBwaW5ncyI6IkFBQUEsNENBQTRDO0FBQzVDLDhDQUE4QztBQUU5QyxJQUFPLE9BQU8sQ0F3QmI7QUF4QkQsV0FBTyxPQUFPO0lBQUNBLElBQUFBLEtBQUtBLENBd0JuQkE7SUF4QmNBLFdBQUFBLEtBQUtBO1FBQUNDLElBQUFBLGFBQWFBLENBd0JqQ0E7UUF4Qm9CQSxXQUFBQSxhQUFhQTtZQUFDQyxJQUFBQSxjQUFjQSxDQXdCaERBO1lBeEJrQ0EsV0FBQUEsY0FBY0EsRUFBQ0EsQ0FBQ0E7Z0JBQy9DQztvQkFDSUMsTUFBTUEsQ0FBQ0EsaUJBQWlCQSxJQUFJQSxJQUFJQSxjQUFjQSxFQUFFQSxDQUFDQTtnQkFDckRBLENBQUNBO2dCQUVERCxDQUFDQSxDQUFDQTtvQkFDRSxJQUFJLGFBQWEsR0FBRyxjQUFjLEVBQUUsQ0FBQztvQkFFckMsRUFBRSxDQUFDLENBQUMsYUFBYSxDQUFDLENBQUMsQ0FBQzt3QkFDaEIscUNBQXNCLEVBQUUsQ0FBQztvQkFDN0IsQ0FBQztvQkFBQyxJQUFJLENBQUMsQ0FBQzt3QkFDSixzQ0FBdUIsRUFBRSxDQUFDO29CQUM5QixDQUFDO29CQUVELElBQUksWUFBWSxHQUFHLE1BQU0sQ0FBQyxjQUFjLENBQUMsQ0FBQztvQkFDMUMsSUFBSSxVQUFVLEdBQVksQ0FBQyxDQUFDLE9BQU8sQ0FBQyxDQUFDLElBQUksQ0FBQyx1QkFBdUIsQ0FBQyxDQUFDO29CQUNuRSxDQUFDLENBQUMsT0FBTyxDQUFDLENBQUMsSUFBSSxDQUFDO3dCQUNaLFFBQVEsRUFBRTs0QkFDTixFQUFFLENBQUMsQ0FBQyxZQUFZLElBQUksWUFBWSxDQUFDLE9BQU8sQ0FBQztnQ0FDckMsWUFBWSxDQUFDLE9BQU8sQ0FBQyx1QkFBdUIsRUFBRSxDQUFDLENBQUMsT0FBTyxDQUFDLENBQUMsSUFBSSxDQUFDLFFBQVEsRUFBRSxRQUFRLENBQUMsQ0FBQyxDQUFDO3dCQUMzRixDQUFDO3dCQUNELE1BQU0sRUFBRSxDQUFDLFVBQVUsSUFBSSxZQUFZLElBQUksWUFBWSxDQUFDLE9BQU8sR0FBRyxZQUFZLENBQUMsT0FBTyxDQUFDLHVCQUF1QixDQUFDLEdBQUcsSUFBSTtxQkFDckgsQ0FBQyxDQUFDLElBQUksRUFBRSxDQUFDO2dCQUNkLENBQUMsQ0FBQ0EsQ0FBQ0E7WUFDUEEsQ0FBQ0EsRUF4QmtDRCxjQUFjQSxHQUFkQSw0QkFBY0EsS0FBZEEsNEJBQWNBLFFBd0JoREE7UUFBREEsQ0FBQ0EsRUF4Qm9CRCxhQUFhQSxHQUFiQSxtQkFBYUEsS0FBYkEsbUJBQWFBLFFBd0JqQ0E7SUFBREEsQ0FBQ0EsRUF4QmNELEtBQUtBLEdBQUxBLGFBQUtBLEtBQUxBLGFBQUtBLFFBd0JuQkE7QUFBREEsQ0FBQ0EsRUF4Qk0sT0FBTyxLQUFQLE9BQU8sUUF3QmIiLCJmaWxlIjoiY2xvdWRtZWRpYS1lZGl0LWNsb3VkdmlkZW9wYXJ0LmpzIiwic291cmNlc0NvbnRlbnQiOlsiLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5LmQudHNcIiAvPlxuLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5dWkuZC50c1wiIC8+XG5cbm1vZHVsZSBPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQ2xvdWRWaWRlb0VkaXQge1xuICAgIGZ1bmN0aW9uIGhhc0NvcnNTdXBwb3J0KCkge1xuICAgICAgICByZXR1cm4gJ3dpdGhDcmVkZW50aWFscycgaW4gbmV3IFhNTEh0dHBSZXF1ZXN0KCk7XG4gICAgfVxuXG4gICAgJChmdW5jdGlvbigpIHtcbiAgICAgICAgdmFyIGNvcnNTdXBwb3J0ZWQgPSBoYXNDb3JzU3VwcG9ydCgpO1xuXG4gICAgICAgIGlmIChjb3JzU3VwcG9ydGVkKSB7XG4gICAgICAgICAgICBpbml0aWFsaXplVXBsb2FkRGlyZWN0KCk7XG4gICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICBpbml0aWFsaXplVXBsb2FkUHJveGllZCgpO1xuICAgICAgICB9XG5cbiAgICAgICAgdmFyIGxvY2FsU3RvcmFnZSA9IHdpbmRvd1tcImxvY2FsU3RvcmFnZVwiXTtcbiAgICAgICAgdmFyIGlzQ3JlYXRpbmc6IGJvb2xlYW4gPSAkKFwiI3RhYnNcIikuZGF0YShcImNsb3VkdmlkZW8taXNjcmVhdGluZ1wiKTtcbiAgICAgICAgJChcIiN0YWJzXCIpLnRhYnMoe1xuICAgICAgICAgICAgYWN0aXZhdGU6IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICBpZiAobG9jYWxTdG9yYWdlICYmIGxvY2FsU3RvcmFnZS5zZXRJdGVtKVxuICAgICAgICAgICAgICAgICAgICBsb2NhbFN0b3JhZ2Uuc2V0SXRlbShcInNlbGVjdGVkQ2xvdWRWaWRlb1RhYlwiLCAkKFwiI3RhYnNcIikudGFicyhcIm9wdGlvblwiLCBcImFjdGl2ZVwiKSk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgYWN0aXZlOiAhaXNDcmVhdGluZyAmJiBsb2NhbFN0b3JhZ2UgJiYgbG9jYWxTdG9yYWdlLmdldEl0ZW0gPyBsb2NhbFN0b3JhZ2UuZ2V0SXRlbShcInNlbGVjdGVkQ2xvdWRWaWRlb1RhYlwiKSA6IG51bGxcbiAgICAgICAgfSkuc2hvdygpO1xuICAgIH0pO1xufSJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==