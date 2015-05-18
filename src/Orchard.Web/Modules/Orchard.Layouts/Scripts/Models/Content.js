var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Content = function (data, htmlId, htmlClass, htmlStyle, isTemplated, contentType, contentTypeLabel, contentTypeClass, html, hasEditor) {
        LayoutEditor.Element.call(this, "Content", data, htmlId, htmlClass, htmlStyle, isTemplated);

        this.contentType = contentType;
        this.contentTypeLabel = contentTypeLabel;
        this.contentTypeClass = contentTypeClass;
        this.html = html;
        this.hasEditor = hasEditor;

        this.getInnerText = function () {
            return $($.parseHTML("<div>" + this.html + "</div>")).text();
        };

        // This function will be overwritten by the Content directive.
        this.setHtml = function (html) {
            this.html = html;
            this.htmlUnsafe = html;
        }

        this.toObject = function () {
            return {
                "type": "Content"
            };
        };

        this.toObject = function () {
            var result = this.elementToObject();
            result.contentType = this.contentType;
            result.contentTypeLabel = this.contentTypeLabel;
            result.contentTypeClass = this.contentTypeClass;
            result.html = this.html;
            result.hasEditor = hasEditor;
            return result;
        };

        this.setHtml(html);
    };

    LayoutEditor.Content.from = function (value) {
        var result = new LayoutEditor.Content(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.contentType,
            value.contentTypeLabel,
            value.contentTypeClass,
            value.html,
            value.hasEditor);

        return result;
    };

})(LayoutEditor || (LayoutEditor = {}));