var LayoutEditor;
(function (LayoutEditor) {

    Array.prototype.move = function (from, to) {
        this.splice(to, 0, this.splice(from, 1)[0]);
    };

    LayoutEditor.childrenFrom = function(values) {
        return _(values).map(function(value) {
            return LayoutEditor.elementFrom(value);
        });
    };

    var registerFactory = LayoutEditor.registerFactory = function(type, factory) {
        var factories = LayoutEditor.factories = LayoutEditor.factories || {};
        factories[type] = factory;
    };

    registerFactory("Canvas", function (value) { return LayoutEditor.Canvas.from(value); });
    registerFactory("Grid", function(value) { return LayoutEditor.Grid.from(value); });
    registerFactory("Row", function(value) { return LayoutEditor.Row.from(value); });
    registerFactory("Column", function(value) { return LayoutEditor.Column.from(value); });
    registerFactory("Content", function(value) { return LayoutEditor.Content.from(value); });

    LayoutEditor.elementFrom = function (value) {
        var factory = LayoutEditor.factories[value.type];

        if (!factory)
            throw new Error("No element with type \"" + value.type + "\" was found.");

        var element = factory(value);
        return element;
    };

    LayoutEditor.setModel = function (elementSelector, model) {
        $(elementSelector).scope().element = model;
    };

    LayoutEditor.getModel = function (elementSelector) {
        return $(elementSelector).scope().element;
    };

})(LayoutEditor || (LayoutEditor = {}));
var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Editor = function (config, canvasData) {
        this.config = config;
        this.canvas = LayoutEditor.Canvas.from(canvasData);
        this.initialState = JSON.stringify(this.canvas.toObject());
        this.activeElement = null;
        this.focusedElement = null;
        this.dropTargetElement = null;
        this.isDragging = false;
        this.inlineEditingIsActive = false;
        this.isResizing = false;

        this.resetToolboxElements = function () {
            this.toolboxElements = [
                LayoutEditor.Row.from({
                    children: []
                })
            ];
        };

        this.isDirty = function() {
            var currentState = JSON.stringify(this.canvas.toObject());
            return this.initialState != currentState;
        };

        this.resetToolboxElements();
        this.canvas.setEditor(this);
    };

})(LayoutEditor || (LayoutEditor = {}));

var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Element = function (type, data, htmlId, htmlClass, htmlStyle, isTemplated, rule) {
        if (!type)
            throw new Error("Parameter 'type' is required.");

        this.type = type;
        this.data = data;
        this.htmlId = htmlId;
        this.htmlClass = htmlClass;
        this.htmlStyle = htmlStyle;
        this.isTemplated = isTemplated;
        this.rule = rule;

        this.editor = null;
        this.parent = null;
        this.setIsFocusedEventHandlers = [];

        this.setEditor = function (editor) {
            this.editor = editor;
            if (!!this.children && _.isArray(this.children)) {
                _(this.children).each(function (child) {
                    child.setEditor(editor);
                });
            }
        };

        this.setParent = function(parentElement) {
            this.parent = parentElement;

            if (!!this.parent.linkChild)
                this.parent.linkChild(this);
        };

        this.setIsTemplated = function (value) {
            this.isTemplated = value;
            if (!!this.children && _.isArray(this.children)) {
                _(this.children).each(function (child) {
                    child.setIsTemplated(value);
                });
            }
        };

        this.getIsActive = function () {
            if (!this.editor)
                return false;
            return this.editor.activeElement === this && !this.getIsFocused();
        };

        this.setIsActive = function (value) {
            if (!this.editor)
                return;
            if (this.editor.isDragging || this.editor.inlineEditingIsActive || this.editor.isResizing)
                return;

            if (value)
                this.editor.activeElement = this;
            else
                this.editor.activeElement = this.parent;
        };

        this.getIsFocused = function () {
            if (!this.editor)
                return false;
            return this.editor.focusedElement === this;
        };

        this.allowSealedFocus = function() {
            return false;
        };

        this.setIsFocused = function () {
            if (!this.editor)
                return;
            if (this.isTemplated && !this.allowSealedFocus())
                return;
            if (this.editor.isDragging || this.editor.inlineEditingIsActive || this.editor.isResizing)
                return;

            this.editor.focusedElement = this;
            _(this.setIsFocusedEventHandlers).each(function (item) {
                try {
                    item();
                }
                catch (ex) {
                    // Ignore.
                }
            });
        };

        this.getIsSelected = function () {
            if (this.getIsFocused())
                return true;

            if (!!this.children && _.isArray(this.children)) {
                return _(this.children).any(function(child) {
                    return child.getIsSelected();
                });
            }

            return false;
        };

        this.getIsDropTarget = function () {
            if (!this.editor)
                return false;
            return this.editor.dropTargetElement === this;
        }

        this.setIsDropTarget = function (value) {
            if (!this.editor)
                return;
            if (value)
                this.editor.dropTargetElement = this;
            else
                this.editor.dropTargetElement = null;
        };

        this.canDelete = function () {
            return !!this.parent;
        };

        this.delete = function () {
            if (!!this.parent)
                this.parent.deleteChild(this);
        };

        this.canMoveUp = function () {
            if (!this.parent)
                return false;
            return this.parent.canMoveChildUp(this);
        };

        this.moveUp = function () {
            if (!!this.parent)
                this.parent.moveChildUp(this);
        };

        this.canMoveDown = function () {
            if (!this.parent)
                return false;
            return this.parent.canMoveChildDown(this);
        };

        this.moveDown = function () {
            if (!!this.parent)
                this.parent.moveChildDown(this);
        };

        this.elementToObject = function () {
            return {
                type: this.type,
                data: this.data,
                htmlId: this.htmlId,
                htmlClass: this.htmlClass,
                htmlStyle: this.htmlStyle,
                isTemplated: this.isTemplated,
                rule: this.rule
            };
        };

        this.getEditorObject = function() {
            return {};
        };

        this.copy = function (clipboardData) {
            var text = this.getInnerText();
            clipboardData.setData("text/plain", text);
            console.log(text);

            var data = this.toObject();
            var json = JSON.stringify(data, null, "\t");
            clipboardData.setData("text/json", json);
        };

        this.cut = function (clipboardData) {
            this.copy(clipboardData);
            this.delete();
        };

        this.paste = function (clipboardData) {
            if (!!this.parent)
                this.parent.paste(clipboardData);
        };
    };

})(LayoutEditor || (LayoutEditor = {}));
var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Container = function (allowedChildTypes, children) {

        this.allowedChildTypes = allowedChildTypes;
        this.children = children;
        this.isContainer = true;

        var _self = this;

        this.setChildren = function (children) {
            this.children = children;
            _(this.children).each(function (child) {
                child.parent = _self;
            });
        };

        this.setChildren(children);

        this.getIsSealed = function () {
            return _(this.children).any(function (child) {
                return child.isTemplated;
            });
        };

        this.addChild = function (child) {
            if (!_(this.children).contains(child) && (_(this.allowedChildTypes).contains(child.type) || child.isContainable))
                this.children.push(child);
            child.setEditor(this.editor);
            child.setIsTemplated(false);
            child.parent = this;
        };

        this.deleteChild = function (child) {
            var index = _(this.children).indexOf(child);
            if (index >= 0) {
                this.children.splice(index, 1);
                if (child.getIsActive())
                    this.editor.activeElement = null;
                if (child.getIsFocused()) {
                    // If the deleted child was focused, try to set new focus to the most appropriate sibling or parent.
                    if (this.children.length > index)
                        this.children[index].setIsFocused();
                    else if (index > 0)
                        this.children[index - 1].setIsFocused();
                    else
                        this.setIsFocused();
                }
            }
        };

        this.moveFocusPrevChild = function (child) {
            if (this.children.length < 2)
                return;
            var index = _(this.children).indexOf(child);
            if (index > 0)
                this.children[index - 1].setIsFocused();
        };

        this.moveFocusNextChild = function (child) {
            if (this.children.length < 2)
                return;
            var index = _(this.children).indexOf(child);
            if (index < this.children.length - 1)
                this.children[index + 1].setIsFocused();
        };

        this.insertChild = function (child, afterChild) {
            if (!_(this.children).contains(child)) {
                var index = Math.max(_(this.children).indexOf(afterChild), 0);
                this.children.splice(index + 1, 0, child);
                child.setEditor(this.editor);
                child.parent = this;
            }
        };

        this.moveChildUp = function (child) {
            if (!this.canMoveChildUp(child))
                return;
            var index = _(this.children).indexOf(child);
            this.children.move(index, index - 1);
        };

        this.moveChildDown = function (child) {
            if (!this.canMoveChildDown(child))
                return;
            var index = _(this.children).indexOf(child);
            this.children.move(index, index + 1);
        };

        this.canMoveChildUp = function (child) {
            var index = _(this.children).indexOf(child);
            return index > 0;
        };

        this.canMoveChildDown = function (child) {
            var index = _(this.children).indexOf(child);
            return index < this.children.length - 1;
        };

        this.childrenToObject = function () {
            return _(this.children).map(function (child) {
                return child.toObject();
            });
        };

        this.getInnerText = function () {
            return _(this.children).reduce(function (memo, child) {
                return memo + "\n" + child.getInnerText();
            }, "");
        }

        this.paste = function (clipboardData) {
            var json = clipboardData.getData("text/json");
            if (!!json) {
                var data = JSON.parse(json);
                var child = LayoutEditor.elementFrom(data);
                this.pasteChild(child);
            }
        };

        this.pasteChild = function (child) {
            if (_(this.allowedChildTypes).contains(child.type) || child.isContainable) {
                this.addChild(child);
                child.setIsFocused();
            }
            else if (!!this.parent)
                this.parent.pasteChild(child);
        }
    };

})(LayoutEditor || (LayoutEditor = {}));
var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Canvas = function (data, htmlId, htmlClass, htmlStyle, isTemplated, rule, children) {
        LayoutEditor.Element.call(this, "Canvas", data, htmlId, htmlClass, htmlStyle, isTemplated, rule);
        LayoutEditor.Container.call(this, ["Canvas", "Grid", "Content"], children);

        this.isContainable = true;

        this.toObject = function () {
            var result = this.elementToObject();
            result.children = this.childrenToObject();
            return result;
        };

        this.allowSealedFocus = function() {
            return this.children.length === 0;
        };
    };

    LayoutEditor.Canvas.from = function (value) {
        var result = new LayoutEditor.Canvas(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.rule,
            LayoutEditor.childrenFrom(value.children));

        result.toolboxIcon = value.toolboxIcon;
        result.toolboxLabel = value.toolboxLabel;
        result.toolboxDescription = value.toolboxDescription;

        return result;
    };

})(LayoutEditor || (LayoutEditor = {}));

var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Grid = function (data, htmlId, htmlClass, htmlStyle, isTemplated, rule, children) {
        LayoutEditor.Element.call(this, "Grid", data, htmlId, htmlClass, htmlStyle, isTemplated, rule);
        LayoutEditor.Container.call(this, ["Row"], children);

        this.toObject = function () {
            var result = this.elementToObject();
            result.children = this.childrenToObject();
            return result;
        };
    };

    LayoutEditor.Grid.from = function (value) {
        var result = new LayoutEditor.Grid(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.rule,
            LayoutEditor.childrenFrom(value.children));
        result.toolboxIcon = value.toolboxIcon;
        result.toolboxLabel = value.toolboxLabel;
        result.toolboxDescription = value.toolboxDescription;
        return result;
    };

})(LayoutEditor || (LayoutEditor = {}));
var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Row = function (data, htmlId, htmlClass, htmlStyle, isTemplated, rule, children) {
        LayoutEditor.Element.call(this, "Row", data, htmlId, htmlClass, htmlStyle, isTemplated, rule);
        LayoutEditor.Container.call(this, ["Column"], children);

        var _self = this;

        function _getTotalColumnsWidth() {
            return _(_self.children).reduce(function (memo, child) {
                return memo + child.offset + child.width;
            }, 0);
        }

        // Implements a simple algorithm to distribute space (either positive or negative)
        // between the child columns of the row. Negative space is distributed when making
        // room for a new column (e.c. clipboard paste or dropping from the toolbox) while
        // positive space is distributed when filling the grap of a removed column.
        function _distributeSpace(space) {
            if (space == 0)
                return true;
             
            var undistributedSpace = space;

            if (undistributedSpace < 0) {
                var vacantSpace = 12 - _getTotalColumnsWidth();
                undistributedSpace += vacantSpace;
                if (undistributedSpace > 0)
                    undistributedSpace = 0;
            }

            // If space is negative, try to decrease offsets first.
            while (undistributedSpace < 0 && _(_self.children).any(function (column) { return column.offset > 0; })) { // While there is still offset left to remove.
                for (i = 0; i < _self.children.length && undistributedSpace < 0; i++) {
                    var column = _self.children[i];
                    if (column.offset > 0) {
                        column.offset--;
                        undistributedSpace++;
                    }
                }
            }

            function hasWidth(column) {
                if (undistributedSpace > 0)
                    return column.width < 12;
                else if (undistributedSpace < 0)
                    return column.width > 1;
                return false;
            }

            // Try to distribute remaining space (could be negative or positive) using widths.
            while (undistributedSpace != 0) {
                // Any more column width available for distribution?
                if (!_(_self.children).any(hasWidth))
                    break;
                for (i = 0; i < _self.children.length && undistributedSpace != 0; i++) {
                    var column = _self.children[i % _self.children.length];
                    if (hasWidth(column)) {
                        var delta = undistributedSpace / Math.abs(undistributedSpace);
                        column.width += delta;
                        undistributedSpace -= delta;
                    }
                }                
            }

            return undistributedSpace == 0;
        }

        var _isAddingColumn = false;

        this.canAddColumn = function () {
            return this.children.length < 12;
        };

        this.beginAddColumn = function (newColumnWidth) {
            if (!!_isAddingColumn)
                throw new Error("Column add operation is already in progress.")
            _(this.children).each(function (column) {
                column.beginChange();
            });
            if (_distributeSpace(-newColumnWidth)) {
                _isAddingColumn = true;
                return true;
            }
            _(this.children).each(function (column) {
                column.rollbackChange();
            });
            return false;
        };

        this.commitAddColumn = function () {
            if (!_isAddingColumn)
                throw new Error("No column add operation in progress.")
            _(this.children).each(function (column) {
                column.commitChange();
            });
            _isAddingColumn = false;
        };

        this.rollbackAddColumn = function () {
            if (!_isAddingColumn)
                throw new Error("No column add operation in progress.")
            _(this.children).each(function (column) {
                column.rollbackChange();
            });
            _isAddingColumn = false;
        };

        var _baseDeleteChild = this.deleteChild;
        this.deleteChild = function (column) { 
            var width = column.width;
            _baseDeleteChild.call(this, column);
            _distributeSpace(width);
        };

        this.canContractColumnRight = function (column, connectAdjacent) {
            var index = _(this.children).indexOf(column);
            if (index >= 0)
                return column.width > 1;
            return false;
        };

        this.contractColumnRight = function (column, connectAdjacent) {
            if (!this.canContractColumnRight(column, connectAdjacent))
                return;

            var index = _(this.children).indexOf(column);
            if (index >= 0) {
                if (column.width > 1) {
                    column.width--;
                    if (this.children.length > index + 1) {
                        var nextColumn = this.children[index + 1];
                        if (connectAdjacent && nextColumn.offset == 0)
                            nextColumn.width++;
                        else
                            nextColumn.offset++;
                    }
                }
            }
        };

        this.canExpandColumnRight = function (column, connectAdjacent) {
            var index = _(this.children).indexOf(column);
            if (index >= 0) {
                if (column.width >= 12)
                    return false;
                if (this.children.length > index + 1) {
                    var nextColumn = this.children[index + 1];
                    if (connectAdjacent && nextColumn.offset == 0)
                        return nextColumn.width > 1;
                    else
                        return nextColumn.offset > 0;
                }
                return _getTotalColumnsWidth() < 12;
            }
            return false;
        };

        this.expandColumnRight = function (column, connectAdjacent) {
            if (!this.canExpandColumnRight(column, connectAdjacent))
                return;

            var index = _(this.children).indexOf(column);
            if (index >= 0) {
                if (this.children.length > index + 1) {
                    var nextColumn = this.children[index + 1];
                    if (connectAdjacent && nextColumn.offset == 0)
                        nextColumn.width--;
                    else
                        nextColumn.offset--;
                }
                column.width++;
            }
        };

        this.canExpandColumnLeft = function (column, connectAdjacent) {
            var index = _(this.children).indexOf(column);
            if (index >= 0) {
                if (column.width >= 12)
                    return false;
                if (index > 0) {
                    var prevColumn = this.children[index - 1];
                    if (connectAdjacent && column.offset == 0)
                        return prevColumn.width > 1;
                }
                return column.offset > 0;
            }
            return false;
        };

        this.expandColumnLeft = function (column, connectAdjacent) {
            if (!this.canExpandColumnLeft(column, connectAdjacent))
                return;

            var index = _(this.children).indexOf(column);
            if (index >= 0) {
                if (index > 0) {
                    var prevColumn = this.children[index - 1];
                    if (connectAdjacent && column.offset == 0)
                        prevColumn.width--;
                    else
                        column.offset--;
                }
                else
                    column.offset--;
                column.width++;
            }
        };

        this.canContractColumnLeft = function (column, connectAdjacent) {
            var index = _(this.children).indexOf(column);
            if (index >= 0)
                return column.width > 1;
            return false;
        };

        this.contractColumnLeft = function (column, connectAdjacent) {
            if (!this.canContractColumnLeft(column, connectAdjacent))
                return;

            var index = _(this.children).indexOf(column);
            if (index >= 0) {
                if (index > 0) {
                    var prevColumn = this.children[index - 1];
                    if (connectAdjacent && column.offset == 0)
                        prevColumn.width++;
                    else
                        column.offset++;
                }
                else
                    column.offset++;
                column.width--;
            }
        };

        this.evenColumns = function () {
            if (this.children.length == 0)
                return;

            var evenWidth = Math.floor(12 / this.children.length);
            _(this.children).each(function (column) {
                column.width = evenWidth;
                column.offset = 0;
            });

            var rest = 12 % this.children.length;
            if (rest > 0)
                _distributeSpace(rest);
        };

        var _basePasteChild = this.pasteChild;
        this.pasteChild = function (child) {
            if (child.type == "Column") {
                if (this.beginAddColumn(child.width)) {
                    this.commitAddColumn();
                    _basePasteChild.call(this, child)
                }
            }
            else if (!!this.parent)
                this.parent.pasteChild(child);
        }

        this.toObject = function () {
            var result = this.elementToObject();
            result.children = this.childrenToObject();
            return result;
        };
    };

    LayoutEditor.Row.from = function (value) {
        var result = new LayoutEditor.Row(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.rule,
            LayoutEditor.childrenFrom(value.children));
        result.toolboxIcon = value.toolboxIcon;
        result.toolboxLabel = value.toolboxLabel;
        result.toolboxDescription = value.toolboxDescription;
        return result;
    };

})(LayoutEditor || (LayoutEditor = {}));
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

        this.allowSealedFocus = function () {
            return this.children.length === 0;
        };

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
            return this.parent.canContractColumnRight(this, connectAdjacent);
        };

        this.contractRight = function (connectAdjacent) {
            this.parent.contractColumnRight(this, connectAdjacent);
        };

        this.canExpandRight = function (connectAdjacent) {
            return this.parent.canExpandColumnRight(this, connectAdjacent);
        };

        this.expandRight = function (connectAdjacent) {
            this.parent.expandColumnRight(this, connectAdjacent);
        };

        this.canExpandLeft = function (connectAdjacent) {
            return this.parent.canExpandColumnLeft(this, connectAdjacent);
        };

        this.expandLeft = function (connectAdjacent) {
            this.parent.expandColumnLeft(this, connectAdjacent);
        };

        this.canContractLeft = function (connectAdjacent) {
            return this.parent.canContractColumnLeft(this, connectAdjacent);
        };

        this.contractLeft = function (connectAdjacent) {
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
var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Content = function (data, htmlId, htmlClass, htmlStyle, isTemplated, contentType, contentTypeLabel, contentTypeClass, html, hasEditor, rule) {
        LayoutEditor.Element.call(this, "Content", data, htmlId, htmlClass, htmlStyle, isTemplated, rule);

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
            value.hasEditor,
            value.rule);

        return result;
    };

})(LayoutEditor || (LayoutEditor = {}));
var LayoutEditor;
(function ($, LayoutEditor) {

    LayoutEditor.Html = function (data, htmlId, htmlClass, htmlStyle, isTemplated, contentType, contentTypeLabel, contentTypeClass, html, hasEditor, rule) {
        LayoutEditor.Element.call(this, "Html", data, htmlId, htmlClass, htmlStyle, isTemplated, rule);

        this.contentType = contentType;
        this.contentTypeLabel = contentTypeLabel;
        this.contentTypeClass = contentTypeClass;
        this.html = html;
        this.hasEditor = hasEditor;
        this.isContainable = true;

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
                "type": "Html"
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

        var getEditorObject = this.getEditorObject;
        this.getEditorObject = function () {
            var dto = getEditorObject();
            return $.extend(dto, {
                Content: this.html
            });
        }

        this.setHtml(html);
    };

    LayoutEditor.Html.from = function (value) {
        var result = new LayoutEditor.Html(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.contentType,
            value.contentTypeLabel,
            value.contentTypeClass,
            value.html,
            value.hasEditor,
            value.rule);

        return result;
    };

    LayoutEditor.registerFactory("Html", function(value) { return LayoutEditor.Html.from(value); });

})(jQuery, LayoutEditor || (LayoutEditor = {}));
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIkhlbHBlcnMuanMiLCJFZGl0b3IuanMiLCJFbGVtZW50LmpzIiwiQ29udGFpbmVyLmpzIiwiQ2FudmFzLmpzIiwiR3JpZC5qcyIsIlJvdy5qcyIsIkNvbHVtbi5qcyIsIkNvbnRlbnQuanMiLCJIdG1sLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3pDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNoQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNqTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNwSUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3pCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDNUJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzVSQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3ZJQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3pEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EiLCJmaWxlIjoiTW9kZWxzLmpzIiwic291cmNlc0NvbnRlbnQiOlsidmFyIExheW91dEVkaXRvcjtcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XG5cbiAgICBBcnJheS5wcm90b3R5cGUubW92ZSA9IGZ1bmN0aW9uIChmcm9tLCB0bykge1xuICAgICAgICB0aGlzLnNwbGljZSh0bywgMCwgdGhpcy5zcGxpY2UoZnJvbSwgMSlbMF0pO1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tID0gZnVuY3Rpb24odmFsdWVzKSB7XG4gICAgICAgIHJldHVybiBfKHZhbHVlcykubWFwKGZ1bmN0aW9uKHZhbHVlKSB7XG4gICAgICAgICAgICByZXR1cm4gTGF5b3V0RWRpdG9yLmVsZW1lbnRGcm9tKHZhbHVlKTtcbiAgICAgICAgfSk7XG4gICAgfTtcblxuICAgIHZhciByZWdpc3RlckZhY3RvcnkgPSBMYXlvdXRFZGl0b3IucmVnaXN0ZXJGYWN0b3J5ID0gZnVuY3Rpb24odHlwZSwgZmFjdG9yeSkge1xuICAgICAgICB2YXIgZmFjdG9yaWVzID0gTGF5b3V0RWRpdG9yLmZhY3RvcmllcyA9IExheW91dEVkaXRvci5mYWN0b3JpZXMgfHwge307XG4gICAgICAgIGZhY3Rvcmllc1t0eXBlXSA9IGZhY3Rvcnk7XG4gICAgfTtcblxuICAgIHJlZ2lzdGVyRmFjdG9yeShcIkdyaWRcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5HcmlkLmZyb20odmFsdWUpOyB9KTtcbiAgICByZWdpc3RlckZhY3RvcnkoXCJSb3dcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5Sb3cuZnJvbSh2YWx1ZSk7IH0pO1xuICAgIHJlZ2lzdGVyRmFjdG9yeShcIkNvbHVtblwiLCBmdW5jdGlvbih2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkNvbHVtbi5mcm9tKHZhbHVlKTsgfSk7XG4gICAgcmVnaXN0ZXJGYWN0b3J5KFwiQ29udGVudFwiLCBmdW5jdGlvbih2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkNvbnRlbnQuZnJvbSh2YWx1ZSk7IH0pO1xuXG4gICAgTGF5b3V0RWRpdG9yLmVsZW1lbnRGcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgIHZhciBmYWN0b3J5ID0gTGF5b3V0RWRpdG9yLmZhY3Rvcmllc1t2YWx1ZS50eXBlXTtcblxuICAgICAgICBpZiAoIWZhY3RvcnkpXG4gICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJObyBlbGVtZW50IHdpdGggdHlwZSBcXFwiXCIgKyB2YWx1ZS50eXBlICsgXCJcXFwiIHdhcyBmb3VuZC5cIik7XG5cbiAgICAgICAgdmFyIGVsZW1lbnQgPSBmYWN0b3J5KHZhbHVlKTtcbiAgICAgICAgcmV0dXJuIGVsZW1lbnQ7XG4gICAgfTtcblxuICAgIExheW91dEVkaXRvci5zZXRNb2RlbCA9IGZ1bmN0aW9uIChlbGVtZW50U2VsZWN0b3IsIG1vZGVsKSB7XG4gICAgICAgICQoZWxlbWVudFNlbGVjdG9yKS5zY29wZSgpLmVsZW1lbnQgPSBtb2RlbDtcbiAgICB9O1xuXG4gICAgTGF5b3V0RWRpdG9yLmdldE1vZGVsID0gZnVuY3Rpb24gKGVsZW1lbnRTZWxlY3Rvcikge1xuICAgICAgICByZXR1cm4gJChlbGVtZW50U2VsZWN0b3IpLnNjb3BlKCkuZWxlbWVudDtcbiAgICB9O1xuXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XG5cbiAgICBMYXlvdXRFZGl0b3IuRWRpdG9yID0gZnVuY3Rpb24gKGNvbmZpZywgY2FudmFzRGF0YSkge1xuICAgICAgICB0aGlzLmNvbmZpZyA9IGNvbmZpZztcbiAgICAgICAgdGhpcy5jYW52YXMgPSBMYXlvdXRFZGl0b3IuQ2FudmFzLmZyb20oY2FudmFzRGF0YSk7XG4gICAgICAgIHRoaXMuaW5pdGlhbFN0YXRlID0gSlNPTi5zdHJpbmdpZnkodGhpcy5jYW52YXMudG9PYmplY3QoKSk7XG4gICAgICAgIHRoaXMuYWN0aXZlRWxlbWVudCA9IG51bGw7XG4gICAgICAgIHRoaXMuZm9jdXNlZEVsZW1lbnQgPSBudWxsO1xuICAgICAgICB0aGlzLmRyb3BUYXJnZXRFbGVtZW50ID0gbnVsbDtcbiAgICAgICAgdGhpcy5pc0RyYWdnaW5nID0gZmFsc2U7XG4gICAgICAgIHRoaXMuaW5saW5lRWRpdGluZ0lzQWN0aXZlID0gZmFsc2U7XG4gICAgICAgIHRoaXMuaXNSZXNpemluZyA9IGZhbHNlO1xuXG4gICAgICAgIHRoaXMucmVzZXRUb29sYm94RWxlbWVudHMgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICB0aGlzLnRvb2xib3hFbGVtZW50cyA9IFtcbiAgICAgICAgICAgICAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20oe1xuICAgICAgICAgICAgICAgICAgICBjaGlsZHJlbjogW11cbiAgICAgICAgICAgICAgICB9KVxuICAgICAgICAgICAgXTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmlzRGlydHkgPSBmdW5jdGlvbigpIHtcbiAgICAgICAgICAgIHZhciBjdXJyZW50U3RhdGUgPSBKU09OLnN0cmluZ2lmeSh0aGlzLmNhbnZhcy50b09iamVjdCgpKTtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmluaXRpYWxTdGF0ZSAhPSBjdXJyZW50U3RhdGU7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5yZXNldFRvb2xib3hFbGVtZW50cygpO1xuICAgICAgICB0aGlzLmNhbnZhcy5zZXRFZGl0b3IodGhpcyk7XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpO1xuIiwidmFyIExheW91dEVkaXRvcjtcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XG5cbiAgICBMYXlvdXRFZGl0b3IuRWxlbWVudCA9IGZ1bmN0aW9uICh0eXBlLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCkge1xuICAgICAgICBpZiAoIXR5cGUpXG4gICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJQYXJhbWV0ZXIgJ3R5cGUnIGlzIHJlcXVpcmVkLlwiKTtcblxuICAgICAgICB0aGlzLnR5cGUgPSB0eXBlO1xuICAgICAgICB0aGlzLmRhdGEgPSBkYXRhO1xuICAgICAgICB0aGlzLmh0bWxJZCA9IGh0bWxJZDtcbiAgICAgICAgdGhpcy5odG1sQ2xhc3MgPSBodG1sQ2xhc3M7XG4gICAgICAgIHRoaXMuaHRtbFN0eWxlID0gaHRtbFN0eWxlO1xuICAgICAgICB0aGlzLmlzVGVtcGxhdGVkID0gaXNUZW1wbGF0ZWQ7XG5cbiAgICAgICAgdGhpcy5lZGl0b3IgPSBudWxsO1xuICAgICAgICB0aGlzLnBhcmVudCA9IG51bGw7XG4gICAgICAgIHRoaXMuc2V0SXNGb2N1c2VkRXZlbnRIYW5kbGVycyA9IFtdO1xuXG4gICAgICAgIHRoaXMuc2V0RWRpdG9yID0gZnVuY3Rpb24gKGVkaXRvcikge1xuICAgICAgICAgICAgdGhpcy5lZGl0b3IgPSBlZGl0b3I7XG4gICAgICAgICAgICBpZiAoISF0aGlzLmNoaWxkcmVuICYmIF8uaXNBcnJheSh0aGlzLmNoaWxkcmVuKSkge1xuICAgICAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgICAgICAgICAgY2hpbGQuc2V0RWRpdG9yKGVkaXRvcik7XG4gICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5zZXRQYXJlbnQgPSBmdW5jdGlvbihwYXJlbnRFbGVtZW50KSB7XG4gICAgICAgICAgICB0aGlzLnBhcmVudCA9IHBhcmVudEVsZW1lbnQ7XG5cbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50LmxpbmtDaGlsZClcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5saW5rQ2hpbGQodGhpcyk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5zZXRJc1RlbXBsYXRlZCA9IGZ1bmN0aW9uICh2YWx1ZSkge1xuICAgICAgICAgICAgdGhpcy5pc1RlbXBsYXRlZCA9IHZhbHVlO1xuICAgICAgICAgICAgaWYgKCEhdGhpcy5jaGlsZHJlbiAmJiBfLmlzQXJyYXkodGhpcy5jaGlsZHJlbikpIHtcbiAgICAgICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICAgICAgICAgIGNoaWxkLnNldElzVGVtcGxhdGVkKHZhbHVlKTtcbiAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmdldElzQWN0aXZlID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgICAgICByZXR1cm4gdGhpcy5lZGl0b3IuYWN0aXZlRWxlbWVudCA9PT0gdGhpcyAmJiAhdGhpcy5nZXRJc0ZvY3VzZWQoKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnNldElzQWN0aXZlID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxuICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgICAgIGlmICh0aGlzLmVkaXRvci5pc0RyYWdnaW5nIHx8IHRoaXMuZWRpdG9yLmlubGluZUVkaXRpbmdJc0FjdGl2ZSB8fCB0aGlzLmVkaXRvci5pc1Jlc2l6aW5nKVxuICAgICAgICAgICAgICAgIHJldHVybjtcblxuICAgICAgICAgICAgaWYgKHZhbHVlKVxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSB0aGlzO1xuICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSB0aGlzLnBhcmVudDtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmdldElzRm9jdXNlZCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZWRpdG9yLmZvY3VzZWRFbGVtZW50ID09PSB0aGlzO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuc2V0SXNGb2N1c2VkID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcbiAgICAgICAgICAgIFx0cmV0dXJuO1xuICAgICAgICAgICAgaWYgKHRoaXMuaXNUZW1wbGF0ZWQpXG4gICAgICAgICAgICBcdHJldHVybjtcbiAgICAgICAgICAgIGlmICh0aGlzLmVkaXRvci5pc0RyYWdnaW5nIHx8IHRoaXMuZWRpdG9yLmlubGluZUVkaXRpbmdJc0FjdGl2ZSB8fCB0aGlzLmVkaXRvci5pc1Jlc2l6aW5nKVxuICAgICAgICAgICAgICAgIHJldHVybjtcblxuICAgICAgICAgICAgdGhpcy5lZGl0b3IuZm9jdXNlZEVsZW1lbnQgPSB0aGlzO1xuICAgICAgICAgICAgXyh0aGlzLnNldElzRm9jdXNlZEV2ZW50SGFuZGxlcnMpLmVhY2goZnVuY3Rpb24gKGl0ZW0pIHtcbiAgICAgICAgICAgICAgICB0cnkge1xuICAgICAgICAgICAgICAgICAgICBpdGVtKCk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIGNhdGNoIChleCkge1xuICAgICAgICAgICAgICAgICAgICAvLyBJZ25vcmUuXG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfSk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5nZXRJc1NlbGVjdGVkID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKHRoaXMuZ2V0SXNGb2N1c2VkKCkpXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XG5cbiAgICAgICAgICAgIGlmICghIXRoaXMuY2hpbGRyZW4gJiYgXy5pc0FycmF5KHRoaXMuY2hpbGRyZW4pKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikuYW55KGZ1bmN0aW9uKGNoaWxkKSB7XG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjaGlsZC5nZXRJc1NlbGVjdGVkKCk7XG4gICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICB9XG5cbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmdldElzRHJvcFRhcmdldCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZWRpdG9yLmRyb3BUYXJnZXRFbGVtZW50ID09PSB0aGlzO1xuICAgICAgICB9XG5cbiAgICAgICAgdGhpcy5zZXRJc0Ryb3BUYXJnZXQgPSBmdW5jdGlvbiAodmFsdWUpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgaWYgKHZhbHVlKVxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmRyb3BUYXJnZXRFbGVtZW50ID0gdGhpcztcbiAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5kcm9wVGFyZ2V0RWxlbWVudCA9IG51bGw7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5kZWxldGUgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoISF0aGlzLnBhcmVudClcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5kZWxldGVDaGlsZCh0aGlzKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNhbk1vdmVVcCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5wYXJlbnQpXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbk1vdmVDaGlsZFVwKHRoaXMpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMubW92ZVVwID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCEhdGhpcy5wYXJlbnQpXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQubW92ZUNoaWxkVXAodGhpcyk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5Nb3ZlRG93biA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5wYXJlbnQpXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbk1vdmVDaGlsZERvd24odGhpcyk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5tb3ZlRG93biA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50Lm1vdmVDaGlsZERvd24odGhpcyk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5lbGVtZW50VG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4ge1xuICAgICAgICAgICAgICAgIHR5cGU6IHRoaXMudHlwZSxcbiAgICAgICAgICAgICAgICBkYXRhOiB0aGlzLmRhdGEsXG4gICAgICAgICAgICAgICAgaHRtbElkOiB0aGlzLmh0bWxJZCxcbiAgICAgICAgICAgICAgICBodG1sQ2xhc3M6IHRoaXMuaHRtbENsYXNzLFxuICAgICAgICAgICAgICAgIGh0bWxTdHlsZTogdGhpcy5odG1sU3R5bGUsXG4gICAgICAgICAgICAgICAgaXNUZW1wbGF0ZWQ6IHRoaXMuaXNUZW1wbGF0ZWRcbiAgICAgICAgICAgIH07XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5nZXRFZGl0b3JPYmplY3QgPSBmdW5jdGlvbigpIHtcbiAgICAgICAgICAgIHJldHVybiB7fTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNvcHkgPSBmdW5jdGlvbiAoY2xpcGJvYXJkRGF0YSkge1xuICAgICAgICAgICAgdmFyIHRleHQgPSB0aGlzLmdldElubmVyVGV4dCgpO1xuICAgICAgICAgICAgY2xpcGJvYXJkRGF0YS5zZXREYXRhKFwidGV4dC9wbGFpblwiLCB0ZXh0KTtcbiAgICAgICAgICAgIGNvbnNvbGUubG9nKHRleHQpO1xuXG4gICAgICAgICAgICB2YXIgZGF0YSA9IHRoaXMudG9PYmplY3QoKTtcbiAgICAgICAgICAgIHZhciBqc29uID0gSlNPTi5zdHJpbmdpZnkoZGF0YSwgbnVsbCwgXCJcXHRcIik7XG4gICAgICAgICAgICBjbGlwYm9hcmREYXRhLnNldERhdGEoXCJ0ZXh0L2pzb25cIiwganNvbik7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jdXQgPSBmdW5jdGlvbiAoY2xpcGJvYXJkRGF0YSkge1xuICAgICAgICAgICAgdGhpcy5jb3B5KGNsaXBib2FyZERhdGEpO1xuICAgICAgICAgICAgdGhpcy5kZWxldGUoKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnBhc3RlID0gZnVuY3Rpb24gKGNsaXBib2FyZERhdGEpIHtcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50LnBhc3RlKGNsaXBib2FyZERhdGEpO1xuICAgICAgICB9O1xuICAgIH07XG5cbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcblxuICAgIExheW91dEVkaXRvci5Db250YWluZXIgPSBmdW5jdGlvbiAoYWxsb3dlZENoaWxkVHlwZXMsIGNoaWxkcmVuKSB7XG5cbiAgICAgICAgdGhpcy5hbGxvd2VkQ2hpbGRUeXBlcyA9IGFsbG93ZWRDaGlsZFR5cGVzO1xuICAgICAgICB0aGlzLmNoaWxkcmVuID0gY2hpbGRyZW47XG4gICAgICAgIHRoaXMuaXNDb250YWluZXIgPSB0cnVlO1xuXG4gICAgICAgIHZhciBfc2VsZiA9IHRoaXM7XG5cbiAgICAgICAgdGhpcy5zZXRDaGlsZHJlbiA9IGZ1bmN0aW9uIChjaGlsZHJlbikge1xuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgICAgIGNoaWxkLnBhcmVudCA9IF9zZWxmO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5zZXRDaGlsZHJlbihjaGlsZHJlbik7XG5cbiAgICAgICAgdGhpcy5nZXRJc1NlYWxlZCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHJldHVybiBfKHRoaXMuY2hpbGRyZW4pLmFueShmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gY2hpbGQuaXNUZW1wbGF0ZWQ7XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmFkZENoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICBpZiAoIV8odGhpcy5jaGlsZHJlbikuY29udGFpbnMoY2hpbGQpICYmIChfKHRoaXMuYWxsb3dlZENoaWxkVHlwZXMpLmNvbnRhaW5zKGNoaWxkLnR5cGUpIHx8IGNoaWxkLmlzQ29udGFpbmFibGUpKVxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4ucHVzaChjaGlsZCk7XG4gICAgICAgICAgICBjaGlsZC5zZXRFZGl0b3IodGhpcy5lZGl0b3IpO1xuICAgICAgICAgICAgY2hpbGQuc2V0SXNUZW1wbGF0ZWQoZmFsc2UpO1xuICAgICAgICAgICAgY2hpbGQucGFyZW50ID0gdGhpcztcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmRlbGV0ZUNoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcbiAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuLnNwbGljZShpbmRleCwgMSk7XG4gICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzQWN0aXZlKCkpXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSBudWxsO1xuICAgICAgICAgICAgICAgIGlmIChjaGlsZC5nZXRJc0ZvY3VzZWQoKSkge1xuICAgICAgICAgICAgICAgICAgICAvLyBJZiB0aGUgZGVsZXRlZCBjaGlsZCB3YXMgZm9jdXNlZCwgdHJ5IHRvIHNldCBuZXcgZm9jdXMgdG8gdGhlIG1vc3QgYXBwcm9wcmlhdGUgc2libGluZyBvciBwYXJlbnQuXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4KVxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbltpbmRleF0uc2V0SXNGb2N1c2VkKCk7XG4gICAgICAgICAgICAgICAgICAgIGVsc2UgaWYgKGluZGV4ID4gMClcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXS5zZXRJc0ZvY3VzZWQoKTtcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5zZXRJc0ZvY3VzZWQoKTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5tb3ZlRm9jdXNQcmV2Q2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA8IDIpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA+IDApXG4gICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdLnNldElzRm9jdXNlZCgpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMubW92ZUZvY3VzTmV4dENoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPCAyKVxuICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XG4gICAgICAgICAgICBpZiAoaW5kZXggPCB0aGlzLmNoaWxkcmVuLmxlbmd0aCAtIDEpXG4gICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbltpbmRleCArIDFdLnNldElzRm9jdXNlZCgpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuaW5zZXJ0Q2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQsIGFmdGVyQ2hpbGQpIHtcbiAgICAgICAgICAgIGlmICghXyh0aGlzLmNoaWxkcmVuKS5jb250YWlucyhjaGlsZCkpIHtcbiAgICAgICAgICAgICAgICB2YXIgaW5kZXggPSBNYXRoLm1heChfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoYWZ0ZXJDaGlsZCksIDApO1xuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4uc3BsaWNlKGluZGV4ICsgMSwgMCwgY2hpbGQpO1xuICAgICAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XG4gICAgICAgICAgICAgICAgY2hpbGQucGFyZW50ID0gdGhpcztcbiAgICAgICAgICAgIH1cbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLm1vdmVDaGlsZFVwID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuTW92ZUNoaWxkVXAoY2hpbGQpKVxuICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XG4gICAgICAgICAgICB0aGlzLmNoaWxkcmVuLm1vdmUoaW5kZXgsIGluZGV4IC0gMSk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5tb3ZlQ2hpbGREb3duID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuTW92ZUNoaWxkRG93bihjaGlsZCkpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcbiAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4ubW92ZShpbmRleCwgaW5kZXggKyAxKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNhbk1vdmVDaGlsZFVwID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xuICAgICAgICAgICAgcmV0dXJuIGluZGV4ID4gMDtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNhbk1vdmVDaGlsZERvd24gPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XG4gICAgICAgICAgICByZXR1cm4gaW5kZXggPCB0aGlzLmNoaWxkcmVuLmxlbmd0aCAtIDE7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jaGlsZHJlblRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikubWFwKGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgICAgIHJldHVybiBjaGlsZC50b09iamVjdCgpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5nZXRJbm5lclRleHQgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5yZWR1Y2UoZnVuY3Rpb24gKG1lbW8sIGNoaWxkKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIG1lbW8gKyBcIlxcblwiICsgY2hpbGQuZ2V0SW5uZXJUZXh0KCk7XG4gICAgICAgICAgICB9LCBcIlwiKTtcbiAgICAgICAgfVxuXG4gICAgICAgIHRoaXMucGFzdGUgPSBmdW5jdGlvbiAoY2xpcGJvYXJkRGF0YSkge1xuICAgICAgICAgICAgdmFyIGpzb24gPSBjbGlwYm9hcmREYXRhLmdldERhdGEoXCJ0ZXh0L2pzb25cIik7XG4gICAgICAgICAgICBpZiAoISFqc29uKSB7XG4gICAgICAgICAgICAgICAgdmFyIGRhdGEgPSBKU09OLnBhcnNlKGpzb24pO1xuICAgICAgICAgICAgICAgIHZhciBjaGlsZCA9IExheW91dEVkaXRvci5lbGVtZW50RnJvbShkYXRhKTtcbiAgICAgICAgICAgICAgICB0aGlzLnBhc3RlQ2hpbGQoY2hpbGQpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMucGFzdGVDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgaWYgKF8odGhpcy5hbGxvd2VkQ2hpbGRUeXBlcykuY29udGFpbnMoY2hpbGQudHlwZSkgfHwgY2hpbGQuaXNDb250YWluYWJsZSkge1xuICAgICAgICAgICAgICAgIHRoaXMuYWRkQ2hpbGQoY2hpbGQpO1xuICAgICAgICAgICAgICAgIGNoaWxkLnNldElzRm9jdXNlZCgpO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgZWxzZSBpZiAoISF0aGlzLnBhcmVudClcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5wYXN0ZUNoaWxkKGNoaWxkKTtcbiAgICAgICAgfVxuICAgIH07XG5cbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcblxuICAgIExheW91dEVkaXRvci5DYW52YXMgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNoaWxkcmVuKSB7XG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJDYW52YXNcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpO1xuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiR3JpZFwiLCBcIkNvbnRlbnRcIl0sIGNoaWxkcmVuKTtcblxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgICAgIH07XG4gICAgfTtcblxuICAgIExheW91dEVkaXRvci5DYW52YXMuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xuICAgICAgICByZXR1cm4gbmV3IExheW91dEVkaXRvci5DYW52YXMoXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpO1xuIiwidmFyIExheW91dEVkaXRvcjtcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XG5cbiAgICBMYXlvdXRFZGl0b3IuR3JpZCA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgY2hpbGRyZW4pIHtcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkdyaWRcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpO1xuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiUm93XCJdLCBjaGlsZHJlbik7XG5cbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xuICAgICAgICAgICAgcmVzdWx0LmNoaWxkcmVuID0gdGhpcy5jaGlsZHJlblRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgICAgICB9O1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IuR3JpZC5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkdyaWQoXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XG4gICAgICAgIHJlc3VsdC50b29sYm94SWNvbiA9IHZhbHVlLnRvb2xib3hJY29uO1xuICAgICAgICByZXN1bHQudG9vbGJveExhYmVsID0gdmFsdWUudG9vbGJveExhYmVsO1xuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xuICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgIH07XG5cbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcblxuICAgIExheW91dEVkaXRvci5Sb3cgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNoaWxkcmVuKSB7XG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJSb3dcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpO1xuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiQ29sdW1uXCJdLCBjaGlsZHJlbik7XG5cbiAgICAgICAgdmFyIF9zZWxmID0gdGhpcztcblxuICAgICAgICBmdW5jdGlvbiBfZ2V0VG90YWxDb2x1bW5zV2lkdGgoKSB7XG4gICAgICAgICAgICByZXR1cm4gXyhfc2VsZi5jaGlsZHJlbikucmVkdWNlKGZ1bmN0aW9uIChtZW1vLCBjaGlsZCkge1xuICAgICAgICAgICAgICAgIHJldHVybiBtZW1vICsgY2hpbGQub2Zmc2V0ICsgY2hpbGQud2lkdGg7XG4gICAgICAgICAgICB9LCAwKTtcbiAgICAgICAgfVxuXG4gICAgICAgIC8vIEltcGxlbWVudHMgYSBzaW1wbGUgYWxnb3JpdGhtIHRvIGRpc3RyaWJ1dGUgc3BhY2UgKGVpdGhlciBwb3NpdGl2ZSBvciBuZWdhdGl2ZSlcbiAgICAgICAgLy8gYmV0d2VlbiB0aGUgY2hpbGQgY29sdW1ucyBvZiB0aGUgcm93LiBOZWdhdGl2ZSBzcGFjZSBpcyBkaXN0cmlidXRlZCB3aGVuIG1ha2luZ1xuICAgICAgICAvLyByb29tIGZvciBhIG5ldyBjb2x1bW4gKGUuYy4gY2xpcGJvYXJkIHBhc3RlIG9yIGRyb3BwaW5nIGZyb20gdGhlIHRvb2xib3gpIHdoaWxlXG4gICAgICAgIC8vIHBvc2l0aXZlIHNwYWNlIGlzIGRpc3RyaWJ1dGVkIHdoZW4gZmlsbGluZyB0aGUgZ3JhcCBvZiBhIHJlbW92ZWQgY29sdW1uLlxuICAgICAgICBmdW5jdGlvbiBfZGlzdHJpYnV0ZVNwYWNlKHNwYWNlKSB7XG4gICAgICAgICAgICBpZiAoc3BhY2UgPT0gMClcbiAgICAgICAgICAgICAgICByZXR1cm4gdHJ1ZTtcbiAgICAgICAgICAgICBcbiAgICAgICAgICAgIHZhciB1bmRpc3RyaWJ1dGVkU3BhY2UgPSBzcGFjZTtcblxuICAgICAgICAgICAgaWYgKHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDApIHtcbiAgICAgICAgICAgICAgICB2YXIgdmFjYW50U3BhY2UgPSAxMiAtIF9nZXRUb3RhbENvbHVtbnNXaWR0aCgpO1xuICAgICAgICAgICAgICAgIHVuZGlzdHJpYnV0ZWRTcGFjZSArPSB2YWNhbnRTcGFjZTtcbiAgICAgICAgICAgICAgICBpZiAodW5kaXN0cmlidXRlZFNwYWNlID4gMClcbiAgICAgICAgICAgICAgICAgICAgdW5kaXN0cmlidXRlZFNwYWNlID0gMDtcbiAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgLy8gSWYgc3BhY2UgaXMgbmVnYXRpdmUsIHRyeSB0byBkZWNyZWFzZSBvZmZzZXRzIGZpcnN0LlxuICAgICAgICAgICAgd2hpbGUgKHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDAgJiYgXyhfc2VsZi5jaGlsZHJlbikuYW55KGZ1bmN0aW9uIChjb2x1bW4pIHsgcmV0dXJuIGNvbHVtbi5vZmZzZXQgPiAwOyB9KSkgeyAvLyBXaGlsZSB0aGVyZSBpcyBzdGlsbCBvZmZzZXQgbGVmdCB0byByZW1vdmUuXG4gICAgICAgICAgICAgICAgZm9yIChpID0gMDsgaSA8IF9zZWxmLmNoaWxkcmVuLmxlbmd0aCAmJiB1bmRpc3RyaWJ1dGVkU3BhY2UgPCAwOyBpKyspIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIGNvbHVtbiA9IF9zZWxmLmNoaWxkcmVuW2ldO1xuICAgICAgICAgICAgICAgICAgICBpZiAoY29sdW1uLm9mZnNldCA+IDApIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQtLTtcbiAgICAgICAgICAgICAgICAgICAgICAgIHVuZGlzdHJpYnV0ZWRTcGFjZSsrO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICBmdW5jdGlvbiBoYXNXaWR0aChjb2x1bW4pIHtcbiAgICAgICAgICAgICAgICBpZiAodW5kaXN0cmlidXRlZFNwYWNlID4gMClcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA8IDEyO1xuICAgICAgICAgICAgICAgIGVsc2UgaWYgKHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDApXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ud2lkdGggPiAxO1xuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgLy8gVHJ5IHRvIGRpc3RyaWJ1dGUgcmVtYWluaW5nIHNwYWNlIChjb3VsZCBiZSBuZWdhdGl2ZSBvciBwb3NpdGl2ZSkgdXNpbmcgd2lkdGhzLlxuICAgICAgICAgICAgd2hpbGUgKHVuZGlzdHJpYnV0ZWRTcGFjZSAhPSAwKSB7XG4gICAgICAgICAgICAgICAgLy8gQW55IG1vcmUgY29sdW1uIHdpZHRoIGF2YWlsYWJsZSBmb3IgZGlzdHJpYnV0aW9uP1xuICAgICAgICAgICAgICAgIGlmICghXyhfc2VsZi5jaGlsZHJlbikuYW55KGhhc1dpZHRoKSlcbiAgICAgICAgICAgICAgICAgICAgYnJlYWs7XG4gICAgICAgICAgICAgICAgZm9yIChpID0gMDsgaSA8IF9zZWxmLmNoaWxkcmVuLmxlbmd0aCAmJiB1bmRpc3RyaWJ1dGVkU3BhY2UgIT0gMDsgaSsrKSB7XG4gICAgICAgICAgICAgICAgICAgIHZhciBjb2x1bW4gPSBfc2VsZi5jaGlsZHJlbltpICUgX3NlbGYuY2hpbGRyZW4ubGVuZ3RoXTtcbiAgICAgICAgICAgICAgICAgICAgaWYgKGhhc1dpZHRoKGNvbHVtbikpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBkZWx0YSA9IHVuZGlzdHJpYnV0ZWRTcGFjZSAvIE1hdGguYWJzKHVuZGlzdHJpYnV0ZWRTcGFjZSk7XG4gICAgICAgICAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGggKz0gZGVsdGE7XG4gICAgICAgICAgICAgICAgICAgICAgICB1bmRpc3RyaWJ1dGVkU3BhY2UgLT0gZGVsdGE7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICB9ICAgICAgICAgICAgICAgIFxuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICByZXR1cm4gdW5kaXN0cmlidXRlZFNwYWNlID09IDA7XG4gICAgICAgIH1cblxuICAgICAgICB2YXIgX2lzQWRkaW5nQ29sdW1uID0gZmFsc2U7XG5cbiAgICAgICAgdGhpcy5jYW5BZGRDb2x1bW4gPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4gdGhpcy5jaGlsZHJlbi5sZW5ndGggPCAxMjtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmJlZ2luQWRkQ29sdW1uID0gZnVuY3Rpb24gKG5ld0NvbHVtbldpZHRoKSB7XG4gICAgICAgICAgICBpZiAoISFfaXNBZGRpbmdDb2x1bW4pXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiQ29sdW1uIGFkZCBvcGVyYXRpb24gaXMgYWxyZWFkeSBpbiBwcm9ncmVzcy5cIilcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XG4gICAgICAgICAgICAgICAgY29sdW1uLmJlZ2luQ2hhbmdlKCk7XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgICAgIGlmIChfZGlzdHJpYnV0ZVNwYWNlKC1uZXdDb2x1bW5XaWR0aCkpIHtcbiAgICAgICAgICAgICAgICBfaXNBZGRpbmdDb2x1bW4gPSB0cnVlO1xuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcbiAgICAgICAgICAgICAgICBjb2x1bW4ucm9sbGJhY2tDaGFuZ2UoKTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY29tbWl0QWRkQ29sdW1uID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCFfaXNBZGRpbmdDb2x1bW4pXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiTm8gY29sdW1uIGFkZCBvcGVyYXRpb24gaW4gcHJvZ3Jlc3MuXCIpXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xuICAgICAgICAgICAgICAgIGNvbHVtbi5jb21taXRDaGFuZ2UoKTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgX2lzQWRkaW5nQ29sdW1uID0gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5yb2xsYmFja0FkZENvbHVtbiA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghX2lzQWRkaW5nQ29sdW1uKVxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIk5vIGNvbHVtbiBhZGQgb3BlcmF0aW9uIGluIHByb2dyZXNzLlwiKVxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcbiAgICAgICAgICAgICAgICBjb2x1bW4ucm9sbGJhY2tDaGFuZ2UoKTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgX2lzQWRkaW5nQ29sdW1uID0gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdmFyIF9iYXNlRGVsZXRlQ2hpbGQgPSB0aGlzLmRlbGV0ZUNoaWxkO1xuICAgICAgICB0aGlzLmRlbGV0ZUNoaWxkID0gZnVuY3Rpb24gKGNvbHVtbikgeyBcbiAgICAgICAgICAgIHZhciB3aWR0aCA9IGNvbHVtbi53aWR0aDtcbiAgICAgICAgICAgIF9iYXNlRGVsZXRlQ2hpbGQuY2FsbCh0aGlzLCBjb2x1bW4pO1xuICAgICAgICAgICAgX2Rpc3RyaWJ1dGVTcGFjZSh3aWR0aCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5Db250cmFjdENvbHVtblJpZ2h0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKVxuICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ud2lkdGggPiAxO1xuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY29udHJhY3RDb2x1bW5SaWdodCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgaWYgKCF0aGlzLmNhbkNvbnRyYWN0Q29sdW1uUmlnaHQoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpKVxuICAgICAgICAgICAgICAgIHJldHVybjtcblxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xuICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ud2lkdGggPiAxKSB7XG4gICAgICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aC0tO1xuICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPiBpbmRleCArIDEpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBuZXh0Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCArIDFdO1xuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBuZXh0Q29sdW1uLm9mZnNldCA9PSAwKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ud2lkdGgrKztcbiAgICAgICAgICAgICAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBuZXh0Q29sdW1uLm9mZnNldCsrO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuRXhwYW5kQ29sdW1uUmlnaHQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcbiAgICAgICAgICAgICAgICBpZiAoY29sdW1uLndpZHRoID49IDEyKVxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID4gaW5kZXggKyAxKSB7XG4gICAgICAgICAgICAgICAgICAgIHZhciBuZXh0Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCArIDFdO1xuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIG5leHRDb2x1bW4ub2Zmc2V0ID09IDApXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gbmV4dENvbHVtbi53aWR0aCA+IDE7XG4gICAgICAgICAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBuZXh0Q29sdW1uLm9mZnNldCA+IDA7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIHJldHVybiBfZ2V0VG90YWxDb2x1bW5zV2lkdGgoKSA8IDEyO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZXhwYW5kQ29sdW1uUmlnaHQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5FeHBhbmRDb2x1bW5SaWdodChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID4gaW5kZXggKyAxKSB7XG4gICAgICAgICAgICAgICAgICAgIHZhciBuZXh0Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCArIDFdO1xuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIG5leHRDb2x1bW4ub2Zmc2V0ID09IDApXG4gICAgICAgICAgICAgICAgICAgICAgICBuZXh0Q29sdW1uLndpZHRoLS07XG4gICAgICAgICAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ub2Zmc2V0LS07XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aCsrO1xuICAgICAgICAgICAgfVxuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuRXhwYW5kQ29sdW1uTGVmdCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xuICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ud2lkdGggPj0gMTIpXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgICAgICBpZiAoaW5kZXggPiAwKSB7XG4gICAgICAgICAgICAgICAgICAgIHZhciBwcmV2Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdO1xuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIGNvbHVtbi5vZmZzZXQgPT0gMClcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBwcmV2Q29sdW1uLndpZHRoID4gMTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi5vZmZzZXQgPiAwO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZXhwYW5kQ29sdW1uTGVmdCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgaWYgKCF0aGlzLmNhbkV4cGFuZENvbHVtbkxlZnQoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpKVxuICAgICAgICAgICAgICAgIHJldHVybjtcblxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xuICAgICAgICAgICAgICAgIGlmIChpbmRleCA+IDApIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIHByZXZDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4IC0gMV07XG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgY29sdW1uLm9mZnNldCA9PSAwKVxuICAgICAgICAgICAgICAgICAgICAgICAgcHJldkNvbHVtbi53aWR0aC0tO1xuICAgICAgICAgICAgICAgICAgICBlbHNlXG4gICAgICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0LS07XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldC0tO1xuICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aCsrO1xuICAgICAgICAgICAgfVxuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuQ29udHJhY3RDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKVxuICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ud2lkdGggPiAxO1xuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY29udHJhY3RDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuQ29udHJhY3RDb2x1bW5MZWZ0KGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSlcbiAgICAgICAgICAgICAgICByZXR1cm47XG5cbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcbiAgICAgICAgICAgICAgICBpZiAoaW5kZXggPiAwKSB7XG4gICAgICAgICAgICAgICAgICAgIHZhciBwcmV2Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdO1xuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIGNvbHVtbi5vZmZzZXQgPT0gMClcbiAgICAgICAgICAgICAgICAgICAgICAgIHByZXZDb2x1bW4ud2lkdGgrKztcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldCsrO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICBlbHNlXG4gICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQrKztcbiAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGgtLTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmV2ZW5Db2x1bW5zID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID09IDApXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuXG4gICAgICAgICAgICB2YXIgZXZlbldpZHRoID0gTWF0aC5mbG9vcigxMiAvIHRoaXMuY2hpbGRyZW4ubGVuZ3RoKTtcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XG4gICAgICAgICAgICAgICAgY29sdW1uLndpZHRoID0gZXZlbldpZHRoO1xuICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQgPSAwO1xuICAgICAgICAgICAgfSk7XG5cbiAgICAgICAgICAgIHZhciByZXN0ID0gMTIgJSB0aGlzLmNoaWxkcmVuLmxlbmd0aDtcbiAgICAgICAgICAgIGlmIChyZXN0ID4gMClcbiAgICAgICAgICAgICAgICBfZGlzdHJpYnV0ZVNwYWNlKHJlc3QpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHZhciBfYmFzZVBhc3RlQ2hpbGQgPSB0aGlzLnBhc3RlQ2hpbGQ7XG4gICAgICAgIHRoaXMucGFzdGVDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgaWYgKGNoaWxkLnR5cGUgPT0gXCJDb2x1bW5cIikge1xuICAgICAgICAgICAgICAgIGlmICh0aGlzLmJlZ2luQWRkQ29sdW1uKGNoaWxkLndpZHRoKSkge1xuICAgICAgICAgICAgICAgICAgICB0aGlzLmNvbW1pdEFkZENvbHVtbigpO1xuICAgICAgICAgICAgICAgICAgICBfYmFzZVBhc3RlQ2hpbGQuY2FsbCh0aGlzLCBjaGlsZClcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBlbHNlIGlmICghIXRoaXMucGFyZW50KVxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50LnBhc3RlQ2hpbGQoY2hpbGQpO1xuICAgICAgICB9XG5cbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xuICAgICAgICAgICAgcmVzdWx0LmNoaWxkcmVuID0gdGhpcy5jaGlsZHJlblRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgICAgICB9O1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuUm93KFxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxuICAgICAgICAgICAgTGF5b3V0RWRpdG9yLmNoaWxkcmVuRnJvbSh2YWx1ZS5jaGlsZHJlbikpO1xuICAgICAgICByZXN1bHQudG9vbGJveEljb24gPSB2YWx1ZS50b29sYm94SWNvbjtcbiAgICAgICAgcmVzdWx0LnRvb2xib3hMYWJlbCA9IHZhbHVlLnRvb2xib3hMYWJlbDtcbiAgICAgICAgcmVzdWx0LnRvb2xib3hEZXNjcmlwdGlvbiA9IHZhbHVlLnRvb2xib3hEZXNjcmlwdGlvbjtcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcbiAgICB9O1xuXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XG5cbiAgICBMYXlvdXRFZGl0b3IuQ29sdW1uID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCB3aWR0aCwgb2Zmc2V0LCBjaGlsZHJlbikge1xuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiQ29sdW1uXCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkKTtcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIkdyaWRcIiwgXCJDb250ZW50XCJdLCBjaGlsZHJlbik7XG5cbiAgICAgICAgdGhpcy53aWR0aCA9IHdpZHRoO1xuICAgICAgICB0aGlzLm9mZnNldCA9IG9mZnNldDtcblxuICAgICAgICB2YXIgX2hhc1BlbmRpbmdDaGFuZ2UgPSBmYWxzZTtcbiAgICAgICAgdmFyIF9vcmlnV2lkdGggPSAwO1xuICAgICAgICB2YXIgX29yaWdPZmZzZXQgPSAwO1xuXG4gICAgICAgIHRoaXMuYmVnaW5DaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoISFfaGFzUGVuZGluZ0NoYW5nZSlcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gYWxyZWFkeSBoYXMgYSBwZW5kaW5nIGNoYW5nZS5cIilcbiAgICAgICAgICAgIF9oYXNQZW5kaW5nQ2hhbmdlID0gdHJ1ZTtcbiAgICAgICAgICAgIF9vcmlnV2lkdGggPSB0aGlzLndpZHRoO1xuICAgICAgICAgICAgX29yaWdPZmZzZXQgPSB0aGlzLm9mZnNldDtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNvbW1pdENoYW5nZSA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghX2hhc1BlbmRpbmdDaGFuZ2UpXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiQ29sdW1uIGhhcyBubyBwZW5kaW5nIGNoYW5nZS5cIilcbiAgICAgICAgICAgIF9vcmlnV2lkdGggPSAwO1xuICAgICAgICAgICAgX29yaWdPZmZzZXQgPSAwO1xuICAgICAgICAgICAgX2hhc1BlbmRpbmdDaGFuZ2UgPSBmYWxzZTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnJvbGxiYWNrQ2hhbmdlID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCFfaGFzUGVuZGluZ0NoYW5nZSlcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gaGFzIG5vIHBlbmRpbmcgY2hhbmdlLlwiKVxuICAgICAgICAgICAgdGhpcy53aWR0aCA9IF9vcmlnV2lkdGg7XG4gICAgICAgICAgICB0aGlzLm9mZnNldCA9IF9vcmlnT2Zmc2V0O1xuICAgICAgICAgICAgX29yaWdXaWR0aCA9IDA7XG4gICAgICAgICAgICBfb3JpZ09mZnNldCA9IDA7XG4gICAgICAgICAgICBfaGFzUGVuZGluZ0NoYW5nZSA9IGZhbHNlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuU3BsaXQgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4gdGhpcy53aWR0aCA+IDE7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5zcGxpdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5TcGxpdCgpKVxuICAgICAgICAgICAgICAgIHJldHVybjtcblxuICAgICAgICAgICAgdmFyIG5ld0NvbHVtbldpZHRoID0gTWF0aC5mbG9vcih0aGlzLndpZHRoIC8gMik7XG4gICAgICAgICAgICB2YXIgbmV3Q29sdW1uID0gTGF5b3V0RWRpdG9yLkNvbHVtbi5mcm9tKHtcbiAgICAgICAgICAgICAgICBkYXRhOiBudWxsLFxuICAgICAgICAgICAgICAgIGh0bWxJZDogbnVsbCxcbiAgICAgICAgICAgICAgICBodG1sQ2xhc3M6IG51bGwsXG4gICAgICAgICAgICAgICAgaHRtbFN0eWxlOiBudWxsLFxuICAgICAgICAgICAgICAgIHdpZHRoOiBuZXdDb2x1bW5XaWR0aCxcbiAgICAgICAgICAgICAgICBvZmZzZXQ6IDAsXG4gICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXG4gICAgICAgICAgICB9KTtcbiAgICAgICAgICAgIFxuICAgICAgICAgICAgdGhpcy53aWR0aCA9IHRoaXMud2lkdGggLSBuZXdDb2x1bW5XaWR0aDtcbiAgICAgICAgICAgIHRoaXMucGFyZW50Lmluc2VydENoaWxkKG5ld0NvbHVtbiwgdGhpcyk7XG4gICAgICAgICAgICBuZXdDb2x1bW4uc2V0SXNGb2N1c2VkKCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5Db250cmFjdFJpZ2h0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkNvbnRyYWN0Q29sdW1uUmlnaHQodGhpcywgY29ubmVjdEFkamFjZW50KTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNvbnRyYWN0UmlnaHQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICB0aGlzLnBhcmVudC5jb250cmFjdENvbHVtblJpZ2h0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5FeHBhbmRSaWdodCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5FeHBhbmRDb2x1bW5SaWdodCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZXhwYW5kUmlnaHQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICB0aGlzLnBhcmVudC5leHBhbmRDb2x1bW5SaWdodCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuRXhwYW5kTGVmdCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5FeHBhbmRDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5leHBhbmRMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgdGhpcy5wYXJlbnQuZXhwYW5kQ29sdW1uTGVmdCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuQ29udHJhY3RMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkNvbnRyYWN0Q29sdW1uTGVmdCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY29udHJhY3RMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgdGhpcy5wYXJlbnQuY29udHJhY3RDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xuICAgICAgICAgICAgcmVzdWx0LndpZHRoID0gdGhpcy53aWR0aDtcbiAgICAgICAgICAgIHJlc3VsdC5vZmZzZXQgPSB0aGlzLm9mZnNldDtcbiAgICAgICAgICAgIHJlc3VsdC5jaGlsZHJlbiA9IHRoaXMuY2hpbGRyZW5Ub09iamVjdCgpO1xuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcbiAgICAgICAgfTtcbiAgICB9O1xuXG4gICAgTGF5b3V0RWRpdG9yLkNvbHVtbi5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkNvbHVtbihcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcbiAgICAgICAgICAgIHZhbHVlLndpZHRoLFxuICAgICAgICAgICAgdmFsdWUub2Zmc2V0LFxuICAgICAgICAgICAgTGF5b3V0RWRpdG9yLmNoaWxkcmVuRnJvbSh2YWx1ZS5jaGlsZHJlbikpO1xuICAgICAgICByZXN1bHQudG9vbGJveEljb24gPSB2YWx1ZS50b29sYm94SWNvbjtcbiAgICAgICAgcmVzdWx0LnRvb2xib3hMYWJlbCA9IHZhbHVlLnRvb2xib3hMYWJlbDtcbiAgICAgICAgcmVzdWx0LnRvb2xib3hEZXNjcmlwdGlvbiA9IHZhbHVlLnRvb2xib3hEZXNjcmlwdGlvbjtcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcbiAgICB9O1xuXG4gICAgTGF5b3V0RWRpdG9yLkNvbHVtbi50aW1lcyA9IGZ1bmN0aW9uICh2YWx1ZSkge1xuICAgICAgICByZXR1cm4gXy50aW1lcyh2YWx1ZSwgZnVuY3Rpb24gKG4pIHtcbiAgICAgICAgICAgIHJldHVybiBMYXlvdXRFZGl0b3IuQ29sdW1uLmZyb20oe1xuICAgICAgICAgICAgICAgIGRhdGE6IG51bGwsXG4gICAgICAgICAgICAgICAgaHRtbElkOiBudWxsLFxuICAgICAgICAgICAgICAgIGh0bWxDbGFzczogbnVsbCxcbiAgICAgICAgICAgICAgICBpc1RlbXBsYXRlZDogZmFsc2UsXG4gICAgICAgICAgICAgICAgd2lkdGg6IDEyIC8gdmFsdWUsXG4gICAgICAgICAgICAgICAgb2Zmc2V0OiAwLFxuICAgICAgICAgICAgICAgIGNoaWxkcmVuOiBbXVxuICAgICAgICAgICAgfSk7XG4gICAgICAgIH0pO1xuICAgIH07XG5cbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcblxuICAgIExheW91dEVkaXRvci5Db250ZW50ID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBjb250ZW50VHlwZSwgY29udGVudFR5cGVMYWJlbCwgY29udGVudFR5cGVDbGFzcywgaHRtbCwgaGFzRWRpdG9yKSB7XG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJDb250ZW50XCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkKTtcblxuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlID0gY29udGVudFR5cGU7XG4gICAgICAgIHRoaXMuY29udGVudFR5cGVMYWJlbCA9IGNvbnRlbnRUeXBlTGFiZWw7XG4gICAgICAgIHRoaXMuY29udGVudFR5cGVDbGFzcyA9IGNvbnRlbnRUeXBlQ2xhc3M7XG4gICAgICAgIHRoaXMuaHRtbCA9IGh0bWw7XG4gICAgICAgIHRoaXMuaGFzRWRpdG9yID0gaGFzRWRpdG9yO1xuXG4gICAgICAgIHRoaXMuZ2V0SW5uZXJUZXh0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgcmV0dXJuICQoJC5wYXJzZUhUTUwoXCI8ZGl2PlwiICsgdGhpcy5odG1sICsgXCI8L2Rpdj5cIikpLnRleHQoKTtcbiAgICAgICAgfTtcblxuICAgICAgICAvLyBUaGlzIGZ1bmN0aW9uIHdpbGwgYmUgb3ZlcndyaXR0ZW4gYnkgdGhlIENvbnRlbnQgZGlyZWN0aXZlLlxuICAgICAgICB0aGlzLnNldEh0bWwgPSBmdW5jdGlvbiAoaHRtbCkge1xuICAgICAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcbiAgICAgICAgICAgIHRoaXMuaHRtbFVuc2FmZSA9IGh0bWw7XG4gICAgICAgIH1cblxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgICAgICBcInR5cGVcIjogXCJDb250ZW50XCJcbiAgICAgICAgICAgIH07XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlID0gdGhpcy5jb250ZW50VHlwZTtcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZUxhYmVsID0gdGhpcy5jb250ZW50VHlwZUxhYmVsO1xuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlQ2xhc3MgPSB0aGlzLmNvbnRlbnRUeXBlQ2xhc3M7XG4gICAgICAgICAgICByZXN1bHQuaHRtbCA9IHRoaXMuaHRtbDtcbiAgICAgICAgICAgIHJlc3VsdC5oYXNFZGl0b3IgPSBoYXNFZGl0b3I7XG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuc2V0SHRtbChodG1sKTtcbiAgICB9O1xuXG4gICAgTGF5b3V0RWRpdG9yLkNvbnRlbnQuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xuICAgICAgICB2YXIgcmVzdWx0ID0gbmV3IExheW91dEVkaXRvci5Db250ZW50KFxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGUsXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZUxhYmVsLFxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVDbGFzcyxcbiAgICAgICAgICAgIHZhbHVlLmh0bWwsXG4gICAgICAgICAgICB2YWx1ZS5oYXNFZGl0b3IpO1xuXG4gICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKCQsIExheW91dEVkaXRvcikge1xuXG4gICAgTGF5b3V0RWRpdG9yLkh0bWwgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNvbnRlbnRUeXBlLCBjb250ZW50VHlwZUxhYmVsLCBjb250ZW50VHlwZUNsYXNzLCBodG1sLCBoYXNFZGl0b3IpIHtcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkh0bWxcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpO1xuXG4gICAgICAgIHRoaXMuY29udGVudFR5cGUgPSBjb250ZW50VHlwZTtcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUxhYmVsID0gY29udGVudFR5cGVMYWJlbDtcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUNsYXNzID0gY29udGVudFR5cGVDbGFzcztcbiAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcbiAgICAgICAgdGhpcy5oYXNFZGl0b3IgPSBoYXNFZGl0b3I7XG4gICAgICAgIHRoaXMuaXNDb250YWluYWJsZSA9IHRydWU7XG5cbiAgICAgICAgdGhpcy5nZXRJbm5lclRleHQgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4gJCgkLnBhcnNlSFRNTChcIjxkaXY+XCIgKyB0aGlzLmh0bWwgKyBcIjwvZGl2PlwiKSkudGV4dCgpO1xuICAgICAgICB9O1xuXG4gICAgICAgIC8vIFRoaXMgZnVuY3Rpb24gd2lsbCBiZSBvdmVyd3JpdHRlbiBieSB0aGUgQ29udGVudCBkaXJlY3RpdmUuXG4gICAgICAgIHRoaXMuc2V0SHRtbCA9IGZ1bmN0aW9uIChodG1sKSB7XG4gICAgICAgICAgICB0aGlzLmh0bWwgPSBodG1sO1xuICAgICAgICAgICAgdGhpcy5odG1sVW5zYWZlID0gaHRtbDtcbiAgICAgICAgfVxuXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4ge1xuICAgICAgICAgICAgICAgIFwidHlwZVwiOiBcIkh0bWxcIlxuICAgICAgICAgICAgfTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGUgPSB0aGlzLmNvbnRlbnRUeXBlO1xuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlTGFiZWwgPSB0aGlzLmNvbnRlbnRUeXBlTGFiZWw7XG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGVDbGFzcyA9IHRoaXMuY29udGVudFR5cGVDbGFzcztcbiAgICAgICAgICAgIHJlc3VsdC5odG1sID0gdGhpcy5odG1sO1xuICAgICAgICAgICAgcmVzdWx0Lmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgICAgIH07XG5cbiAgICAgICAgdmFyIGdldEVkaXRvck9iamVjdCA9IHRoaXMuZ2V0RWRpdG9yT2JqZWN0O1xuICAgICAgICB0aGlzLmdldEVkaXRvck9iamVjdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHZhciBkdG8gPSBnZXRFZGl0b3JPYmplY3QoKTtcbiAgICAgICAgICAgIHJldHVybiAkLmV4dGVuZChkdG8sIHtcbiAgICAgICAgICAgICAgICBDb250ZW50OiB0aGlzLmh0bWxcbiAgICAgICAgICAgIH0pO1xuICAgICAgICB9XG5cbiAgICAgICAgdGhpcy5zZXRIdG1sKGh0bWwpO1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IuSHRtbC5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkh0bWwoXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZSxcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlTGFiZWwsXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZUNsYXNzLFxuICAgICAgICAgICAgdmFsdWUuaHRtbCxcbiAgICAgICAgICAgIHZhbHVlLmhhc0VkaXRvcik7XG5cbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcbiAgICB9O1xuXG4gICAgTGF5b3V0RWRpdG9yLnJlZ2lzdGVyRmFjdG9yeShcIkh0bWxcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5IdG1sLmZyb20odmFsdWUpOyB9KTtcblxufSkoalF1ZXJ5LCBMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9
