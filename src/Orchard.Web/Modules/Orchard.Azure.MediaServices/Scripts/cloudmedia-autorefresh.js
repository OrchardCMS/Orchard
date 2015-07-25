/// <reference path="Typings/jquery.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var AutoRefresh;
            (function (AutoRefresh) {
                // Periodically refresh elements.
                $(function () {
                    $("[data-refresh-url]").each(function () {
                        var self = $(this);
                        var update = function () {
                            var container = self;
                            var url = container.data("refresh-url");
                            $.ajax({
                                url: url,
                                cache: false
                            }).then(function (html) {
                                container.html(html);
                                setTimeout(update, 5000);
                            });
                        };
                        setTimeout(update, 5000);
                    });
                });
            })(AutoRefresh = MediaServices.AutoRefresh || (MediaServices.AutoRefresh = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtYXV0b3JlZnJlc2gudHMiXSwibmFtZXMiOlsiT3JjaGFyZCIsIk9yY2hhcmQuQXp1cmUiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQXV0b1JlZnJlc2giXSwibWFwcGluZ3MiOiJBQUFBLDRDQUE0QztBQUU1QyxJQUFPLE9BQU8sQ0FxQmI7QUFyQkQsV0FBTyxPQUFPO0lBQUNBLElBQUFBLEtBQUtBLENBcUJuQkE7SUFyQmNBLFdBQUFBLEtBQUtBO1FBQUNDLElBQUFBLGFBQWFBLENBcUJqQ0E7UUFyQm9CQSxXQUFBQSxhQUFhQTtZQUFDQyxJQUFBQSxXQUFXQSxDQXFCN0NBO1lBckJrQ0EsV0FBQUEsV0FBV0EsRUFBQ0EsQ0FBQ0E7Z0JBQzVDQyxBQUNBQSxpQ0FEaUNBO2dCQUNqQ0EsQ0FBQ0EsQ0FBQ0E7b0JBQ0VBLENBQUNBLENBQUNBLG9CQUFvQkEsQ0FBQ0EsQ0FBQ0EsSUFBSUEsQ0FBQ0E7d0JBQ3pCLElBQUksSUFBSSxHQUFHLENBQUMsQ0FBQyxJQUFJLENBQUMsQ0FBQzt3QkFDbkIsSUFBSSxNQUFNLEdBQUc7NEJBQ1QsSUFBSSxTQUFTLEdBQUcsSUFBSSxDQUFDOzRCQUNyQixJQUFJLEdBQUcsR0FBRyxTQUFTLENBQUMsSUFBSSxDQUFDLGFBQWEsQ0FBQyxDQUFDOzRCQUV4QyxDQUFDLENBQUMsSUFBSSxDQUFDO2dDQUNILEdBQUcsRUFBRSxHQUFHO2dDQUNSLEtBQUssRUFBRSxLQUFLOzZCQUNmLENBQUMsQ0FBQyxJQUFJLENBQUMsVUFBQSxJQUFJO2dDQUNSLFNBQVMsQ0FBQyxJQUFJLENBQUMsSUFBSSxDQUFDLENBQUM7Z0NBQ3JCLFVBQVUsQ0FBQyxNQUFNLEVBQUUsSUFBSSxDQUFDLENBQUM7NEJBQzdCLENBQUMsQ0FBQyxDQUFDO3dCQUNQLENBQUMsQ0FBQzt3QkFFRixVQUFVLENBQUMsTUFBTSxFQUFFLElBQUksQ0FBQyxDQUFDO29CQUM3QixDQUFDLENBQUNBLENBQUNBO2dCQUNQQSxDQUFDQSxDQUFDQSxDQUFDQTtZQUNQQSxDQUFDQSxFQXJCa0NELFdBQVdBLEdBQVhBLHlCQUFXQSxLQUFYQSx5QkFBV0EsUUFxQjdDQTtRQUFEQSxDQUFDQSxFQXJCb0JELGFBQWFBLEdBQWJBLG1CQUFhQSxLQUFiQSxtQkFBYUEsUUFxQmpDQTtJQUFEQSxDQUFDQSxFQXJCY0QsS0FBS0EsR0FBTEEsYUFBS0EsS0FBTEEsYUFBS0EsUUFxQm5CQTtBQUFEQSxDQUFDQSxFQXJCTSxPQUFPLEtBQVAsT0FBTyxRQXFCYiIsImZpbGUiOiJjbG91ZG1lZGlhLWF1dG9yZWZyZXNoLmpzIiwic291cmNlc0NvbnRlbnQiOlsiLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5LmQudHNcIiAvPlxuXG5tb2R1bGUgT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkF1dG9SZWZyZXNoIHtcbiAgICAvLyBQZXJpb2RpY2FsbHkgcmVmcmVzaCBlbGVtZW50cy5cbiAgICAkKCgpID0+IHtcbiAgICAgICAgJChcIltkYXRhLXJlZnJlc2gtdXJsXVwiKS5lYWNoKGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHZhciBzZWxmID0gJCh0aGlzKTtcbiAgICAgICAgICAgIHZhciB1cGRhdGUgPSAoKSA9PiB7XG4gICAgICAgICAgICAgICAgdmFyIGNvbnRhaW5lciA9IHNlbGY7XG4gICAgICAgICAgICAgICAgdmFyIHVybCA9IGNvbnRhaW5lci5kYXRhKFwicmVmcmVzaC11cmxcIik7XG5cbiAgICAgICAgICAgICAgICAkLmFqYXgoe1xuICAgICAgICAgICAgICAgICAgICB1cmw6IHVybCxcbiAgICAgICAgICAgICAgICAgICAgY2FjaGU6IGZhbHNlXG4gICAgICAgICAgICAgICAgfSkudGhlbihodG1sID0+IHtcbiAgICAgICAgICAgICAgICAgICAgY29udGFpbmVyLmh0bWwoaHRtbCk7XG4gICAgICAgICAgICAgICAgICAgIHNldFRpbWVvdXQodXBkYXRlLCA1MDAwKTtcbiAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgIH07XG5cbiAgICAgICAgICAgIHNldFRpbWVvdXQodXBkYXRlLCA1MDAwKTtcbiAgICAgICAgfSk7XG4gICAgfSk7XG59Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9