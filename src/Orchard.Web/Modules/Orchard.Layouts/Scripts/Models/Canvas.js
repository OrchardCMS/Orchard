var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Canvas = function (data, htmlId, htmlClass, htmlStyle, isTemplated, rule, children) {
        LayoutEditor.Element.call(this, "Canvas", data, htmlId, htmlClass, htmlStyle, isTemplated, rule);
        LayoutEditor.Container.call(this, ["Grid", "Content"], children);

        this.toObject = function () {
            var result = this.elementToObject();
            result.children = this.childrenToObject();
            return result;
        };
    };

    LayoutEditor.Canvas.from = function (value) {
        return new LayoutEditor.Canvas(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.rule,
            LayoutEditor.childrenFrom(value.children));
    };

})(LayoutEditor || (LayoutEditor = {}));
