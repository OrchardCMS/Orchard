var LayoutEditor;
(function (LayoutEditor) {
    LayoutEditor.Column = function (data, htmlId, htmlClass, htmlStyle, isTemplated, width, offset, collapsible, rule, children) {
        LayoutEditor.Element.call(this, "Column", data, htmlId, htmlClass, htmlStyle, isTemplated, rule);
        LayoutEditor.Container.call(this, ["Grid", "Content"], children);

        this.width = width;
        this.offset = offset;
        this.collapsible = collapsible;

        var _hasPendingChange = false;
        var _origWidth = 0;
        var _origOffset = 0;

        this.beginChange = function () {
            if (!!_hasPendingChange)
                throw new Error("Column already has a pending change.");
            _hasPendingChange = true;
            _origWidth = this.width;
            _origOffset = this.offset;
        };

        this.commitChange = function () {
            if (!_hasPendingChange)
                throw new Error("Column has no pending change.");
            _origWidth = 0;
            _origOffset = 0;
            _hasPendingChange = false;
        };

        this.rollbackChange = function () {
            if (!_hasPendingChange)
                throw new Error("Column has no pending change.");
            this.width = _origWidth;
            this.offset = _origOffset;
            _origWidth = 0;
            _origOffset = 0;
            _hasPendingChange = false;
        };

        this.canSplit = function () {
            if (this.isTemplated)
                return false;
            return this.width > 1;
        };

        this.split = function () {
            if (!this.canSplit())
                return;

            var newColumnWidth = Math.floor(this.width / 2);
            var newColumn = LayoutEditor.Column.from({
                data: null,
                htmlId: null,
                htmlClass: null,
                htmlStyle: null,
                width: newColumnWidth,
                offset: 0,
                children: []
            });

            this.width = this.width - newColumnWidth;
            this.parent.insertChild(newColumn, this);
            newColumn.setIsFocused();
        };

        this.canContractRight = function (connectAdjacent) {
            if (this.isTemplated)
                return false;
            return this.parent.canContractColumnRight(this, connectAdjacent);
        };

        this.contractRight = function (connectAdjacent) {
            if (!this.canContractRight(connectAdjacent))
                return;
            this.parent.contractColumnRight(this, connectAdjacent);
        };

        this.canExpandRight = function (connectAdjacent) {
            if (this.isTemplated)
                return false;
            return this.parent.canExpandColumnRight(this, connectAdjacent);
        };

        this.expandRight = function (connectAdjacent) {
            if (!this.canExpandRight(connectAdjacent))
                return;
            this.parent.expandColumnRight(this, connectAdjacent);
        };

        this.canExpandLeft = function (connectAdjacent) {
            if (this.isTemplated)
                return false;
            return this.parent.canExpandColumnLeft(this, connectAdjacent);
        };

        this.expandLeft = function (connectAdjacent) {
            if (!this.canExpandLeft(connectAdjacent))
                return;
            this.parent.expandColumnLeft(this, connectAdjacent);
        };

        this.canContractLeft = function (connectAdjacent) {
            if (this.isTemplated)
                return false;
            return this.parent.canContractColumnLeft(this, connectAdjacent);
        };

        this.contractLeft = function (connectAdjacent) {
            if (!this.canContractLeft(connectAdjacent))
                return;
            this.parent.contractColumnLeft(this, connectAdjacent);
        };

        this.toObject = function () {
            var result = this.elementToObject();
            result.width = this.width;
            result.offset = this.offset;
            result.collapsible = this.collapsible;
            result.children = this.childrenToObject();
            return result;
        };
    };

    LayoutEditor.Column.from = function (value) {
        var result = new LayoutEditor.Column(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.width,
            value.offset,
            value.collapsible,
            value.rule,
            LayoutEditor.childrenFrom(value.children));
        result.toolboxIcon = value.toolboxIcon;
        result.toolboxLabel = value.toolboxLabel;
        result.toolboxDescription = value.toolboxDescription;
        return result;
    };

    LayoutEditor.Column.times = function (value) {
        return _.times(value, function (n) {
            return LayoutEditor.Column.from({
                data: null,
                htmlId: null,
                htmlClass: null,
                isTemplated: false,
                width: 12 / value,
                offset: 0,
                collapsible: null,
                children: []
            });
        });
    };
})(LayoutEditor || (LayoutEditor = {}));