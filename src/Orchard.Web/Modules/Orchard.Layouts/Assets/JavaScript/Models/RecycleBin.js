var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.RecycleBin = function () {
        this.elements = [];

        this.add = function(element) {
            this.elements.push(element);
        };

        this.toObject = function () {
            var result = {
                type: "RecycleBin",
                children: []
            };

            for (var i = 0; i < this.elements.length; i++) {
                var element = this.elements[i];
                var dto = element.toObject();
                result.children.push(dto);
            }

            return result;
        };
    };

})(LayoutEditor || (LayoutEditor = {}));
