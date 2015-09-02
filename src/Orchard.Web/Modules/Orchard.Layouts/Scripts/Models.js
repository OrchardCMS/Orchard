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
            this.parent.onChildAdded(this);

            var currentAncestor = parentElement;
            while (!!currentAncestor) {
                currentAncestor.onDescendantAdded(this, parentElement);
                currentAncestor = currentAncestor.parent;
            }
        };

        this.setIsTemplated = function (value) {
            this.isTemplated = value;
            if (!!this.children && _.isArray(this.children)) {
                _(this.children).each(function (child) {
                    child.setIsTemplated(value);
                });
            }
        };

        this.applyElementEditorModel = function() { /* Virtual */ };

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

        this.setIsFocused = function () {
            if (!this.editor)
                return;
            if (this.isTemplated)
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

        var self = this;

        this.onChildAdded = function (element) { /* Virtual */ };
        this.onDescendantAdded = function (element, parentElement) { /* Virtual */ };

        this.setChildren = function (children) {
            this.children = children;
            _(this.children).each(function (child) {
                child.setParent(self);
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
            child.setParent(this);
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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIkhlbHBlcnMuanMiLCJFZGl0b3IuanMiLCJFbGVtZW50LmpzIiwiQ29udGFpbmVyLmpzIiwiQ2FudmFzLmpzIiwiR3JpZC5qcyIsIlJvdy5qcyIsIkNvbHVtbi5qcyIsIkNvbnRlbnQuanMiLCJIdG1sLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDMUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2hDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM1TEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN2SUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2xDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM3QkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM3UkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUMxSUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzFEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSIsImZpbGUiOiJNb2RlbHMuanMiLCJzb3VyY2VzQ29udGVudCI6WyJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIEFycmF5LnByb3RvdHlwZS5tb3ZlID0gZnVuY3Rpb24gKGZyb20sIHRvKSB7XHJcbiAgICAgICAgdGhpcy5zcGxpY2UodG8sIDAsIHRoaXMuc3BsaWNlKGZyb20sIDEpWzBdKTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLmNoaWxkcmVuRnJvbSA9IGZ1bmN0aW9uKHZhbHVlcykge1xyXG4gICAgICAgIHJldHVybiBfKHZhbHVlcykubWFwKGZ1bmN0aW9uKHZhbHVlKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBMYXlvdXRFZGl0b3IuZWxlbWVudEZyb20odmFsdWUpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfTtcclxuXHJcbiAgICB2YXIgcmVnaXN0ZXJGYWN0b3J5ID0gTGF5b3V0RWRpdG9yLnJlZ2lzdGVyRmFjdG9yeSA9IGZ1bmN0aW9uKHR5cGUsIGZhY3RvcnkpIHtcclxuICAgICAgICB2YXIgZmFjdG9yaWVzID0gTGF5b3V0RWRpdG9yLmZhY3RvcmllcyA9IExheW91dEVkaXRvci5mYWN0b3JpZXMgfHwge307XHJcbiAgICAgICAgZmFjdG9yaWVzW3R5cGVdID0gZmFjdG9yeTtcclxuICAgIH07XHJcblxyXG4gICAgcmVnaXN0ZXJGYWN0b3J5KFwiQ2FudmFzXCIsIGZ1bmN0aW9uICh2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkNhbnZhcy5mcm9tKHZhbHVlKTsgfSk7XHJcbiAgICByZWdpc3RlckZhY3RvcnkoXCJHcmlkXCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuR3JpZC5mcm9tKHZhbHVlKTsgfSk7XHJcbiAgICByZWdpc3RlckZhY3RvcnkoXCJSb3dcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5Sb3cuZnJvbSh2YWx1ZSk7IH0pO1xyXG4gICAgcmVnaXN0ZXJGYWN0b3J5KFwiQ29sdW1uXCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuQ29sdW1uLmZyb20odmFsdWUpOyB9KTtcclxuICAgIHJlZ2lzdGVyRmFjdG9yeShcIkNvbnRlbnRcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5Db250ZW50LmZyb20odmFsdWUpOyB9KTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuZWxlbWVudEZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICB2YXIgZmFjdG9yeSA9IExheW91dEVkaXRvci5mYWN0b3JpZXNbdmFsdWUudHlwZV07XHJcblxyXG4gICAgICAgIGlmICghZmFjdG9yeSlcclxuICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiTm8gZWxlbWVudCB3aXRoIHR5cGUgXFxcIlwiICsgdmFsdWUudHlwZSArIFwiXFxcIiB3YXMgZm91bmQuXCIpO1xyXG5cclxuICAgICAgICB2YXIgZWxlbWVudCA9IGZhY3RvcnkodmFsdWUpO1xyXG4gICAgICAgIHJldHVybiBlbGVtZW50O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3Iuc2V0TW9kZWwgPSBmdW5jdGlvbiAoZWxlbWVudFNlbGVjdG9yLCBtb2RlbCkge1xyXG4gICAgICAgICQoZWxlbWVudFNlbGVjdG9yKS5zY29wZSgpLmVsZW1lbnQgPSBtb2RlbDtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLmdldE1vZGVsID0gZnVuY3Rpb24gKGVsZW1lbnRTZWxlY3Rvcikge1xyXG4gICAgICAgIHJldHVybiAkKGVsZW1lbnRTZWxlY3Rvcikuc2NvcGUoKS5lbGVtZW50O1xyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5FZGl0b3IgPSBmdW5jdGlvbiAoY29uZmlnLCBjYW52YXNEYXRhKSB7XHJcbiAgICAgICAgdGhpcy5jb25maWcgPSBjb25maWc7XHJcbiAgICAgICAgdGhpcy5jYW52YXMgPSBMYXlvdXRFZGl0b3IuQ2FudmFzLmZyb20oY2FudmFzRGF0YSk7XHJcbiAgICAgICAgdGhpcy5pbml0aWFsU3RhdGUgPSBKU09OLnN0cmluZ2lmeSh0aGlzLmNhbnZhcy50b09iamVjdCgpKTtcclxuICAgICAgICB0aGlzLmFjdGl2ZUVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuZm9jdXNlZEVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuZHJvcFRhcmdldEVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuaXNEcmFnZ2luZyA9IGZhbHNlO1xyXG4gICAgICAgIHRoaXMuaW5saW5lRWRpdGluZ0lzQWN0aXZlID0gZmFsc2U7XHJcbiAgICAgICAgdGhpcy5pc1Jlc2l6aW5nID0gZmFsc2U7XHJcblxyXG4gICAgICAgIHRoaXMucmVzZXRUb29sYm94RWxlbWVudHMgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHRoaXMudG9vbGJveEVsZW1lbnRzID0gW1xyXG4gICAgICAgICAgICAgICAgTGF5b3V0RWRpdG9yLlJvdy5mcm9tKHtcclxuICAgICAgICAgICAgICAgICAgICBjaGlsZHJlbjogW11cclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgIF07XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5pc0RpcnR5ID0gZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHZhciBjdXJyZW50U3RhdGUgPSBKU09OLnN0cmluZ2lmeSh0aGlzLmNhbnZhcy50b09iamVjdCgpKTtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuaW5pdGlhbFN0YXRlICE9IGN1cnJlbnRTdGF0ZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnJlc2V0VG9vbGJveEVsZW1lbnRzKCk7XHJcbiAgICAgICAgdGhpcy5jYW52YXMuc2V0RWRpdG9yKHRoaXMpO1xyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTtcclxuIiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuRWxlbWVudCA9IGZ1bmN0aW9uICh0eXBlLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSkge1xyXG4gICAgICAgIGlmICghdHlwZSlcclxuICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiUGFyYW1ldGVyICd0eXBlJyBpcyByZXF1aXJlZC5cIik7XHJcblxyXG4gICAgICAgIHRoaXMudHlwZSA9IHR5cGU7XHJcbiAgICAgICAgdGhpcy5kYXRhID0gZGF0YTtcclxuICAgICAgICB0aGlzLmh0bWxJZCA9IGh0bWxJZDtcclxuICAgICAgICB0aGlzLmh0bWxDbGFzcyA9IGh0bWxDbGFzcztcclxuICAgICAgICB0aGlzLmh0bWxTdHlsZSA9IGh0bWxTdHlsZTtcclxuICAgICAgICB0aGlzLmlzVGVtcGxhdGVkID0gaXNUZW1wbGF0ZWQ7XHJcbiAgICAgICAgdGhpcy5ydWxlID0gcnVsZTtcclxuXHJcbiAgICAgICAgdGhpcy5lZGl0b3IgPSBudWxsO1xyXG4gICAgICAgIHRoaXMucGFyZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLnNldElzRm9jdXNlZEV2ZW50SGFuZGxlcnMgPSBbXTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXRFZGl0b3IgPSBmdW5jdGlvbiAoZWRpdG9yKSB7XHJcbiAgICAgICAgICAgIHRoaXMuZWRpdG9yID0gZWRpdG9yO1xyXG4gICAgICAgICAgICBpZiAoISF0aGlzLmNoaWxkcmVuICYmIF8uaXNBcnJheSh0aGlzLmNoaWxkcmVuKSkge1xyXG4gICAgICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcihlZGl0b3IpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNldFBhcmVudCA9IGZ1bmN0aW9uKHBhcmVudEVsZW1lbnQpIHtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQgPSBwYXJlbnRFbGVtZW50O1xyXG4gICAgICAgICAgICB0aGlzLnBhcmVudC5vbkNoaWxkQWRkZWQodGhpcyk7XHJcblxyXG4gICAgICAgICAgICB2YXIgY3VycmVudEFuY2VzdG9yID0gcGFyZW50RWxlbWVudDtcclxuICAgICAgICAgICAgd2hpbGUgKCEhY3VycmVudEFuY2VzdG9yKSB7XHJcbiAgICAgICAgICAgICAgICBjdXJyZW50QW5jZXN0b3Iub25EZXNjZW5kYW50QWRkZWQodGhpcywgcGFyZW50RWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICBjdXJyZW50QW5jZXN0b3IgPSBjdXJyZW50QW5jZXN0b3IucGFyZW50O1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXRJc1RlbXBsYXRlZCA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgICAgICB0aGlzLmlzVGVtcGxhdGVkID0gdmFsdWU7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMuY2hpbGRyZW4gJiYgXy5pc0FycmF5KHRoaXMuY2hpbGRyZW4pKSB7XHJcbiAgICAgICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY2hpbGQuc2V0SXNUZW1wbGF0ZWQodmFsdWUpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmFwcGx5RWxlbWVudEVkaXRvck1vZGVsID0gZnVuY3Rpb24oKSB7IC8qIFZpcnR1YWwgKi8gfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJc0FjdGl2ZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcclxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPT09IHRoaXMgJiYgIXRoaXMuZ2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXRJc0FjdGl2ZSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5lZGl0b3IuaXNEcmFnZ2luZyB8fCB0aGlzLmVkaXRvci5pbmxpbmVFZGl0aW5nSXNBY3RpdmUgfHwgdGhpcy5lZGl0b3IuaXNSZXNpemluZylcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIGlmICh2YWx1ZSlcclxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSB0aGlzO1xyXG4gICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5hY3RpdmVFbGVtZW50ID0gdGhpcy5wYXJlbnQ7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJc0ZvY3VzZWQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmVkaXRvci5mb2N1c2VkRWxlbWVudCA9PT0gdGhpcztcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNldElzRm9jdXNlZCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgaWYgKHRoaXMuaXNUZW1wbGF0ZWQgJiYgdGhpcy5pc1RlbXBsYXRlZENvbnRhaW5lciAhPSB0cnVlKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5lZGl0b3IuaXNEcmFnZ2luZyB8fCB0aGlzLmVkaXRvci5pbmxpbmVFZGl0aW5nSXNBY3RpdmUgfHwgdGhpcy5lZGl0b3IuaXNSZXNpemluZylcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuZWRpdG9yLmZvY3VzZWRFbGVtZW50ID0gdGhpcztcclxuICAgICAgICAgICAgXyh0aGlzLnNldElzRm9jdXNlZEV2ZW50SGFuZGxlcnMpLmVhY2goZnVuY3Rpb24gKGl0ZW0pIHtcclxuICAgICAgICAgICAgICAgIHRyeSB7XHJcbiAgICAgICAgICAgICAgICAgICAgaXRlbSgpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgY2F0Y2ggKGV4KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gSWdub3JlLlxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmdldElzU2VsZWN0ZWQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmdldElzRm9jdXNlZCgpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcblxyXG4gICAgICAgICAgICBpZiAoISF0aGlzLmNoaWxkcmVuICYmIF8uaXNBcnJheSh0aGlzLmNoaWxkcmVuKSkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikuYW55KGZ1bmN0aW9uKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNoaWxkLmdldElzU2VsZWN0ZWQoKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJc0Ryb3BUYXJnZXQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmVkaXRvci5kcm9wVGFyZ2V0RWxlbWVudCA9PT0gdGhpcztcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMuc2V0SXNEcm9wVGFyZ2V0ID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIGlmICh2YWx1ZSlcclxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmRyb3BUYXJnZXRFbGVtZW50ID0gdGhpcztcclxuICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgdGhpcy5lZGl0b3IuZHJvcFRhcmdldEVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuRGVsZXRlID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gISF0aGlzLnBhcmVudDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmRlbGV0ZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCEhdGhpcy5wYXJlbnQpXHJcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5kZWxldGVDaGlsZCh0aGlzKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbk1vdmVVcCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLnBhcmVudClcclxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbk1vdmVDaGlsZFVwKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZVVwID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoISF0aGlzLnBhcmVudClcclxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50Lm1vdmVDaGlsZFVwKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuTW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5wYXJlbnQpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5Nb3ZlQ2hpbGREb3duKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQubW92ZUNoaWxkRG93bih0aGlzKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmVsZW1lbnRUb09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgIHR5cGU6IHRoaXMudHlwZSxcclxuICAgICAgICAgICAgICAgIGRhdGE6IHRoaXMuZGF0YSxcclxuICAgICAgICAgICAgICAgIGh0bWxJZDogdGhpcy5odG1sSWQsXHJcbiAgICAgICAgICAgICAgICBodG1sQ2xhc3M6IHRoaXMuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICAgICAgaHRtbFN0eWxlOiB0aGlzLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgICAgIGlzVGVtcGxhdGVkOiB0aGlzLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICAgICAgcnVsZTogdGhpcy5ydWxlXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRFZGl0b3JPYmplY3QgPSBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHt9O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29weSA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XHJcbiAgICAgICAgICAgIHZhciB0ZXh0ID0gdGhpcy5nZXRJbm5lclRleHQoKTtcclxuICAgICAgICAgICAgY2xpcGJvYXJkRGF0YS5zZXREYXRhKFwidGV4dC9wbGFpblwiLCB0ZXh0KTtcclxuXHJcbiAgICAgICAgICAgIHZhciBkYXRhID0gdGhpcy50b09iamVjdCgpO1xyXG4gICAgICAgICAgICB2YXIganNvbiA9IEpTT04uc3RyaW5naWZ5KGRhdGEsIG51bGwsIFwiXFx0XCIpO1xyXG4gICAgICAgICAgICBjbGlwYm9hcmREYXRhLnNldERhdGEoXCJ0ZXh0L2pzb25cIiwganNvbik7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jdXQgPSBmdW5jdGlvbiAoY2xpcGJvYXJkRGF0YSkge1xyXG4gICAgICAgICAgICB0aGlzLmNvcHkoY2xpcGJvYXJkRGF0YSk7XHJcbiAgICAgICAgICAgIHRoaXMuZGVsZXRlKCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5wYXN0ZSA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGUoY2xpcGJvYXJkRGF0YSk7XHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyID0gZnVuY3Rpb24gKGFsbG93ZWRDaGlsZFR5cGVzLCBjaGlsZHJlbikge1xyXG5cclxuICAgICAgICB0aGlzLmFsbG93ZWRDaGlsZFR5cGVzID0gYWxsb3dlZENoaWxkVHlwZXM7XHJcbiAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xyXG4gICAgICAgIHRoaXMuaXNDb250YWluZXIgPSB0cnVlO1xyXG5cclxuICAgICAgICB2YXIgc2VsZiA9IHRoaXM7XHJcblxyXG4gICAgICAgIHRoaXMub25DaGlsZEFkZGVkID0gZnVuY3Rpb24gKGVsZW1lbnQpIHsgLyogVmlydHVhbCAqLyB9O1xyXG4gICAgICAgIHRoaXMub25EZXNjZW5kYW50QWRkZWQgPSBmdW5jdGlvbiAoZWxlbWVudCwgcGFyZW50RWxlbWVudCkgeyAvKiBWaXJ0dWFsICovIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4gPSBmdW5jdGlvbiAoY2hpbGRyZW4pIHtcclxuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5zZXRQYXJlbnQoc2VsZik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4oY2hpbGRyZW4pO1xyXG5cclxuICAgICAgICB0aGlzLmdldElzU2VhbGVkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5hbnkoZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gY2hpbGQuaXNUZW1wbGF0ZWQ7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuYWRkQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgaWYgKCFfKHRoaXMuY2hpbGRyZW4pLmNvbnRhaW5zKGNoaWxkKSAmJiAoXyh0aGlzLmFsbG93ZWRDaGlsZFR5cGVzKS5jb250YWlucyhjaGlsZC50eXBlKSB8fCBjaGlsZC5pc0NvbnRhaW5hYmxlKSlcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4ucHVzaChjaGlsZCk7XHJcbiAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XHJcbiAgICAgICAgICAgIGNoaWxkLnNldElzVGVtcGxhdGVkKGZhbHNlKTtcclxuICAgICAgICAgICAgY2hpbGQuc2V0UGFyZW50KHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZGVsZXRlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4uc3BsaWNlKGluZGV4LCAxKTtcclxuICAgICAgICAgICAgICAgIGlmIChjaGlsZC5nZXRJc0FjdGl2ZSgpKVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzRm9jdXNlZCgpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gSWYgdGhlIGRlbGV0ZWQgY2hpbGQgd2FzIGZvY3VzZWQsIHRyeSB0byBzZXQgbmV3IGZvY3VzIHRvIHRoZSBtb3N0IGFwcHJvcHJpYXRlIHNpYmxpbmcgb3IgcGFyZW50LlxyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuW2luZGV4XS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlIGlmIChpbmRleCA+IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuc2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLm1vdmVGb2N1c1ByZXZDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPCAyKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPiAwKVxyXG4gICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdLnNldElzRm9jdXNlZCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZUZvY3VzTmV4dENoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA8IDIpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA8IHRoaXMuY2hpbGRyZW4ubGVuZ3RoIC0gMSlcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW5baW5kZXggKyAxXS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmluc2VydENoaWxkID0gZnVuY3Rpb24gKGNoaWxkLCBhZnRlckNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICghXyh0aGlzLmNoaWxkcmVuKS5jb250YWlucyhjaGlsZCkpIHtcclxuICAgICAgICAgICAgICAgIHZhciBpbmRleCA9IE1hdGgubWF4KF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihhZnRlckNoaWxkKSwgMCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuLnNwbGljZShpbmRleCArIDEsIDAsIGNoaWxkKTtcclxuICAgICAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5wYXJlbnQgPSB0aGlzO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5tb3ZlQ2hpbGRVcCA9IGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuTW92ZUNoaWxkVXAoY2hpbGQpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xyXG4gICAgICAgICAgICB0aGlzLmNoaWxkcmVuLm1vdmUoaW5kZXgsIGluZGV4IC0gMSk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5tb3ZlQ2hpbGREb3duID0gZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Nb3ZlQ2hpbGREb3duKGNoaWxkKSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbi5tb3ZlKGluZGV4LCBpbmRleCArIDEpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuTW92ZUNoaWxkVXAgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgcmV0dXJuIGluZGV4ID4gMDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbk1vdmVDaGlsZERvd24gPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgcmV0dXJuIGluZGV4IDwgdGhpcy5jaGlsZHJlbi5sZW5ndGggLSAxO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2hpbGRyZW5Ub09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikubWFwKGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNoaWxkLnRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SW5uZXJUZXh0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5yZWR1Y2UoZnVuY3Rpb24gKG1lbW8sIGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gbWVtbyArIFwiXFxuXCIgKyBjaGlsZC5nZXRJbm5lclRleHQoKTtcclxuICAgICAgICAgICAgfSwgXCJcIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnBhc3RlID0gZnVuY3Rpb24gKGNsaXBib2FyZERhdGEpIHtcclxuICAgICAgICAgICAgdmFyIGpzb24gPSBjbGlwYm9hcmREYXRhLmdldERhdGEoXCJ0ZXh0L2pzb25cIik7XHJcbiAgICAgICAgICAgIGlmICghIWpzb24pIHtcclxuICAgICAgICAgICAgICAgIHZhciBkYXRhID0gSlNPTi5wYXJzZShqc29uKTtcclxuICAgICAgICAgICAgICAgIHZhciBjaGlsZCA9IExheW91dEVkaXRvci5lbGVtZW50RnJvbShkYXRhKTtcclxuICAgICAgICAgICAgICAgIHRoaXMucGFzdGVDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnBhc3RlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgaWYgKF8odGhpcy5hbGxvd2VkQ2hpbGRUeXBlcykuY29udGFpbnMoY2hpbGQudHlwZSkgfHwgY2hpbGQuaXNDb250YWluYWJsZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5hZGRDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGVDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5DYW52YXMgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUsIGNoaWxkcmVuKSB7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkNhbnZhc1wiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSk7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIkNhbnZhc1wiLCBcIkdyaWRcIiwgXCJDb250ZW50XCJdLCBjaGlsZHJlbik7XHJcblxyXG4gICAgICAgIHRoaXMuaXNDb250YWluYWJsZSA9IHRydWU7XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuQ2FudmFzLmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICB2YXIgcmVzdWx0ID0gbmV3IExheW91dEVkaXRvci5DYW52YXMoXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS5ydWxlLFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcblxyXG4gICAgICAgIHJlc3VsdC50b29sYm94SWNvbiA9IHZhbHVlLnRvb2xib3hJY29uO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94TGFiZWwgPSB2YWx1ZS50b29sYm94TGFiZWw7XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hEZXNjcmlwdGlvbiA9IHZhbHVlLnRvb2xib3hEZXNjcmlwdGlvbjtcclxuXHJcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7XHJcbiIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkdyaWQgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUsIGNoaWxkcmVuKSB7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkdyaWRcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUpO1xyXG4gICAgICAgIExheW91dEVkaXRvci5Db250YWluZXIuY2FsbCh0aGlzLCBbXCJSb3dcIl0sIGNoaWxkcmVuKTtcclxuXHJcbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jaGlsZHJlbiA9IHRoaXMuY2hpbGRyZW5Ub09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgICAgIH07XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5HcmlkLmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICB2YXIgcmVzdWx0ID0gbmV3IExheW91dEVkaXRvci5HcmlkKFxyXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxyXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcclxuICAgICAgICAgICAgdmFsdWUucnVsZSxcclxuICAgICAgICAgICAgTGF5b3V0RWRpdG9yLmNoaWxkcmVuRnJvbSh2YWx1ZS5jaGlsZHJlbikpO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94SWNvbiA9IHZhbHVlLnRvb2xib3hJY29uO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94TGFiZWwgPSB2YWx1ZS50b29sYm94TGFiZWw7XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hEZXNjcmlwdGlvbiA9IHZhbHVlLnRvb2xib3hEZXNjcmlwdGlvbjtcclxuICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5Sb3cgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUsIGNoaWxkcmVuKSB7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIlJvd1wiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSk7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIkNvbHVtblwiXSwgY2hpbGRyZW4pO1xyXG5cclxuICAgICAgICB2YXIgX3NlbGYgPSB0aGlzO1xyXG5cclxuICAgICAgICBmdW5jdGlvbiBfZ2V0VG90YWxDb2x1bW5zV2lkdGgoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBfKF9zZWxmLmNoaWxkcmVuKS5yZWR1Y2UoZnVuY3Rpb24gKG1lbW8sIGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gbWVtbyArIGNoaWxkLm9mZnNldCArIGNoaWxkLndpZHRoO1xyXG4gICAgICAgICAgICB9LCAwKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIC8vIEltcGxlbWVudHMgYSBzaW1wbGUgYWxnb3JpdGhtIHRvIGRpc3RyaWJ1dGUgc3BhY2UgKGVpdGhlciBwb3NpdGl2ZSBvciBuZWdhdGl2ZSlcclxuICAgICAgICAvLyBiZXR3ZWVuIHRoZSBjaGlsZCBjb2x1bW5zIG9mIHRoZSByb3cuIE5lZ2F0aXZlIHNwYWNlIGlzIGRpc3RyaWJ1dGVkIHdoZW4gbWFraW5nXHJcbiAgICAgICAgLy8gcm9vbSBmb3IgYSBuZXcgY29sdW1uIChlLmMuIGNsaXBib2FyZCBwYXN0ZSBvciBkcm9wcGluZyBmcm9tIHRoZSB0b29sYm94KSB3aGlsZVxyXG4gICAgICAgIC8vIHBvc2l0aXZlIHNwYWNlIGlzIGRpc3RyaWJ1dGVkIHdoZW4gZmlsbGluZyB0aGUgZ3JhcCBvZiBhIHJlbW92ZWQgY29sdW1uLlxyXG4gICAgICAgIGZ1bmN0aW9uIF9kaXN0cmlidXRlU3BhY2Uoc3BhY2UpIHtcclxuICAgICAgICAgICAgaWYgKHNwYWNlID09IDApXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdHJ1ZTtcclxuICAgICAgICAgICAgIFxyXG4gICAgICAgICAgICB2YXIgdW5kaXN0cmlidXRlZFNwYWNlID0gc3BhY2U7XHJcblxyXG4gICAgICAgICAgICBpZiAodW5kaXN0cmlidXRlZFNwYWNlIDwgMCkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHZhY2FudFNwYWNlID0gMTIgLSBfZ2V0VG90YWxDb2x1bW5zV2lkdGgoKTtcclxuICAgICAgICAgICAgICAgIHVuZGlzdHJpYnV0ZWRTcGFjZSArPSB2YWNhbnRTcGFjZTtcclxuICAgICAgICAgICAgICAgIGlmICh1bmRpc3RyaWJ1dGVkU3BhY2UgPiAwKVxyXG4gICAgICAgICAgICAgICAgICAgIHVuZGlzdHJpYnV0ZWRTcGFjZSA9IDA7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIC8vIElmIHNwYWNlIGlzIG5lZ2F0aXZlLCB0cnkgdG8gZGVjcmVhc2Ugb2Zmc2V0cyBmaXJzdC5cclxuICAgICAgICAgICAgd2hpbGUgKHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDAgJiYgXyhfc2VsZi5jaGlsZHJlbikuYW55KGZ1bmN0aW9uIChjb2x1bW4pIHsgcmV0dXJuIGNvbHVtbi5vZmZzZXQgPiAwOyB9KSkgeyAvLyBXaGlsZSB0aGVyZSBpcyBzdGlsbCBvZmZzZXQgbGVmdCB0byByZW1vdmUuXHJcbiAgICAgICAgICAgICAgICBmb3IgKGkgPSAwOyBpIDwgX3NlbGYuY2hpbGRyZW4ubGVuZ3RoICYmIHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDA7IGkrKykge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBjb2x1bW4gPSBfc2VsZi5jaGlsZHJlbltpXTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoY29sdW1uLm9mZnNldCA+IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldC0tO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB1bmRpc3RyaWJ1dGVkU3BhY2UrKztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGZ1bmN0aW9uIGhhc1dpZHRoKGNvbHVtbikge1xyXG4gICAgICAgICAgICAgICAgaWYgKHVuZGlzdHJpYnV0ZWRTcGFjZSA+IDApXHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA8IDEyO1xyXG4gICAgICAgICAgICAgICAgZWxzZSBpZiAodW5kaXN0cmlidXRlZFNwYWNlIDwgMClcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gY29sdW1uLndpZHRoID4gMTtcclxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgLy8gVHJ5IHRvIGRpc3RyaWJ1dGUgcmVtYWluaW5nIHNwYWNlIChjb3VsZCBiZSBuZWdhdGl2ZSBvciBwb3NpdGl2ZSkgdXNpbmcgd2lkdGhzLlxyXG4gICAgICAgICAgICB3aGlsZSAodW5kaXN0cmlidXRlZFNwYWNlICE9IDApIHtcclxuICAgICAgICAgICAgICAgIC8vIEFueSBtb3JlIGNvbHVtbiB3aWR0aCBhdmFpbGFibGUgZm9yIGRpc3RyaWJ1dGlvbj9cclxuICAgICAgICAgICAgICAgIGlmICghXyhfc2VsZi5jaGlsZHJlbikuYW55KGhhc1dpZHRoKSlcclxuICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgIGZvciAoaSA9IDA7IGkgPCBfc2VsZi5jaGlsZHJlbi5sZW5ndGggJiYgdW5kaXN0cmlidXRlZFNwYWNlICE9IDA7IGkrKykge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBjb2x1bW4gPSBfc2VsZi5jaGlsZHJlbltpICUgX3NlbGYuY2hpbGRyZW4ubGVuZ3RoXTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoaGFzV2lkdGgoY29sdW1uKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB2YXIgZGVsdGEgPSB1bmRpc3RyaWJ1dGVkU3BhY2UgLyBNYXRoLmFicyh1bmRpc3RyaWJ1dGVkU3BhY2UpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGggKz0gZGVsdGE7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHVuZGlzdHJpYnV0ZWRTcGFjZSAtPSBkZWx0YTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9ICAgICAgICAgICAgICAgIFxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdW5kaXN0cmlidXRlZFNwYWNlID09IDA7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgX2lzQWRkaW5nQ29sdW1uID0gZmFsc2U7XHJcblxyXG4gICAgICAgIHRoaXMuY2FuQWRkQ29sdW1uID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5jaGlsZHJlbi5sZW5ndGggPCAxMjtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmJlZ2luQWRkQ29sdW1uID0gZnVuY3Rpb24gKG5ld0NvbHVtbldpZHRoKSB7XHJcbiAgICAgICAgICAgIGlmICghIV9pc0FkZGluZ0NvbHVtbilcclxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIkNvbHVtbiBhZGQgb3BlcmF0aW9uIGlzIGFscmVhZHkgaW4gcHJvZ3Jlc3MuXCIpXHJcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4uYmVnaW5DaGFuZ2UoKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIGlmIChfZGlzdHJpYnV0ZVNwYWNlKC1uZXdDb2x1bW5XaWR0aCkpIHtcclxuICAgICAgICAgICAgICAgIF9pc0FkZGluZ0NvbHVtbiA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdHJ1ZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLnJvbGxiYWNrQ2hhbmdlKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jb21taXRBZGRDb2x1bW4gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghX2lzQWRkaW5nQ29sdW1uKVxyXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiTm8gY29sdW1uIGFkZCBvcGVyYXRpb24gaW4gcHJvZ3Jlc3MuXCIpXHJcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4uY29tbWl0Q2hhbmdlKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBfaXNBZGRpbmdDb2x1bW4gPSBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnJvbGxiYWNrQWRkQ29sdW1uID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoIV9pc0FkZGluZ0NvbHVtbilcclxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIk5vIGNvbHVtbiBhZGQgb3BlcmF0aW9uIGluIHByb2dyZXNzLlwiKVxyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLnJvbGxiYWNrQ2hhbmdlKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBfaXNBZGRpbmdDb2x1bW4gPSBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB2YXIgX2Jhc2VEZWxldGVDaGlsZCA9IHRoaXMuZGVsZXRlQ2hpbGQ7XHJcbiAgICAgICAgdGhpcy5kZWxldGVDaGlsZCA9IGZ1bmN0aW9uIChjb2x1bW4pIHsgXHJcbiAgICAgICAgICAgIHZhciB3aWR0aCA9IGNvbHVtbi53aWR0aDtcclxuICAgICAgICAgICAgX2Jhc2VEZWxldGVDaGlsZC5jYWxsKHRoaXMsIGNvbHVtbik7XHJcbiAgICAgICAgICAgIF9kaXN0cmlidXRlU3BhY2Uod2lkdGgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuQ29udHJhY3RDb2x1bW5SaWdodCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gY29sdW1uLndpZHRoID4gMTtcclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29udHJhY3RDb2x1bW5SaWdodCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuQ29udHJhY3RDb2x1bW5SaWdodChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ud2lkdGggPiAxKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY29sdW1uLndpZHRoLS07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID4gaW5kZXggKyAxKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBuZXh0Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCArIDFdO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIG5leHRDb2x1bW4ub2Zmc2V0ID09IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBuZXh0Q29sdW1uLndpZHRoKys7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ub2Zmc2V0Kys7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5FeHBhbmRDb2x1bW5SaWdodCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ud2lkdGggPj0gMTIpXHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID4gaW5kZXggKyAxKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG5leHRDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4ICsgMV07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBuZXh0Q29sdW1uLm9mZnNldCA9PSAwKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gbmV4dENvbHVtbi53aWR0aCA+IDE7XHJcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gbmV4dENvbHVtbi5vZmZzZXQgPiAwO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIF9nZXRUb3RhbENvbHVtbnNXaWR0aCgpIDwgMTI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZXhwYW5kQ29sdW1uUmlnaHQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmNhbkV4cGFuZENvbHVtblJpZ2h0KGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID4gaW5kZXggKyAxKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG5leHRDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4ICsgMV07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBuZXh0Q29sdW1uLm9mZnNldCA9PSAwKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBuZXh0Q29sdW1uLndpZHRoLS07XHJcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBuZXh0Q29sdW1uLm9mZnNldC0tO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgY29sdW1uLndpZHRoKys7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkV4cGFuZENvbHVtbkxlZnQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoY29sdW1uLndpZHRoID49IDEyKVxyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgIGlmIChpbmRleCA+IDApIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgcHJldkNvbHVtbiA9IHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIGNvbHVtbi5vZmZzZXQgPT0gMClcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHByZXZDb2x1bW4ud2lkdGggPiAxO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi5vZmZzZXQgPiAwO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmV4cGFuZENvbHVtbkxlZnQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmNhbkV4cGFuZENvbHVtbkxlZnQoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG5cclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoaW5kZXggPiAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHByZXZDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4IC0gMV07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBjb2x1bW4ub2Zmc2V0ID09IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHByZXZDb2x1bW4ud2lkdGgtLTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQtLTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0LS07XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGgrKztcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuQ29udHJhY3RDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMClcclxuICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ud2lkdGggPiAxO1xyXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jb250cmFjdENvbHVtbkxlZnQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmNhbkNvbnRyYWN0Q29sdW1uTGVmdChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIGlmIChpbmRleCA+IDApIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgcHJldkNvbHVtbiA9IHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIGNvbHVtbi5vZmZzZXQgPT0gMClcclxuICAgICAgICAgICAgICAgICAgICAgICAgcHJldkNvbHVtbi53aWR0aCsrO1xyXG4gICAgICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldCsrO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQrKztcclxuICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aC0tO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5ldmVuQ29sdW1ucyA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID09IDApXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICB2YXIgZXZlbldpZHRoID0gTWF0aC5mbG9vcigxMiAvIHRoaXMuY2hpbGRyZW4ubGVuZ3RoKTtcclxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcclxuICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aCA9IGV2ZW5XaWR0aDtcclxuICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQgPSAwO1xyXG4gICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIHZhciByZXN0ID0gMTIgJSB0aGlzLmNoaWxkcmVuLmxlbmd0aDtcclxuICAgICAgICAgICAgaWYgKHJlc3QgPiAwKVxyXG4gICAgICAgICAgICAgICAgX2Rpc3RyaWJ1dGVTcGFjZShyZXN0KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB2YXIgX2Jhc2VQYXN0ZUNoaWxkID0gdGhpcy5wYXN0ZUNoaWxkO1xyXG4gICAgICAgIHRoaXMucGFzdGVDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICBpZiAoY2hpbGQudHlwZSA9PSBcIkNvbHVtblwiKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5iZWdpbkFkZENvbHVtbihjaGlsZC53aWR0aCkpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmNvbW1pdEFkZENvbHVtbigpO1xyXG4gICAgICAgICAgICAgICAgICAgIF9iYXNlUGFzdGVDaGlsZC5jYWxsKHRoaXMsIGNoaWxkKVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGVsc2UgaWYgKCEhdGhpcy5wYXJlbnQpXHJcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5wYXN0ZUNoaWxkKGNoaWxkKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICB2YXIgcmVzdWx0ID0gbmV3IExheW91dEVkaXRvci5Sb3coXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS5ydWxlLFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hJY29uID0gdmFsdWUudG9vbGJveEljb247XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hMYWJlbCA9IHZhbHVlLnRvb2xib3hMYWJlbDtcclxuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xyXG4gICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICB9O1xyXG5cclxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XHJcbiAgICBMYXlvdXRFZGl0b3IuQ29sdW1uID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCB3aWR0aCwgb2Zmc2V0LCBjb2xsYXBzaWJsZSwgcnVsZSwgY2hpbGRyZW4pIHtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiQ29sdW1uXCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBydWxlKTtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiR3JpZFwiLCBcIkNvbnRlbnRcIl0sIGNoaWxkcmVuKTtcclxuICAgICAgICB0aGlzLmlzVGVtcGxhdGVkQ29udGFpbmVyID0gdHJ1ZTtcclxuICAgICAgICB0aGlzLndpZHRoID0gd2lkdGg7XHJcbiAgICAgICAgdGhpcy5vZmZzZXQgPSBvZmZzZXQ7XHJcbiAgICAgICAgdGhpcy5jb2xsYXBzaWJsZSA9IGNvbGxhcHNpYmxlO1xyXG5cclxuICAgICAgICB2YXIgX2hhc1BlbmRpbmdDaGFuZ2UgPSBmYWxzZTtcclxuICAgICAgICB2YXIgX29yaWdXaWR0aCA9IDA7XHJcbiAgICAgICAgdmFyIF9vcmlnT2Zmc2V0ID0gMDtcclxuXHJcbiAgICAgICAgdGhpcy5iZWdpbkNoYW5nZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCEhX2hhc1BlbmRpbmdDaGFuZ2UpXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gYWxyZWFkeSBoYXMgYSBwZW5kaW5nIGNoYW5nZS5cIik7XHJcbiAgICAgICAgICAgIF9oYXNQZW5kaW5nQ2hhbmdlID0gdHJ1ZTtcclxuICAgICAgICAgICAgX29yaWdXaWR0aCA9IHRoaXMud2lkdGg7XHJcbiAgICAgICAgICAgIF9vcmlnT2Zmc2V0ID0gdGhpcy5vZmZzZXQ7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jb21taXRDaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghX2hhc1BlbmRpbmdDaGFuZ2UpXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gaGFzIG5vIHBlbmRpbmcgY2hhbmdlLlwiKTtcclxuICAgICAgICAgICAgX29yaWdXaWR0aCA9IDA7XHJcbiAgICAgICAgICAgIF9vcmlnT2Zmc2V0ID0gMDtcclxuICAgICAgICAgICAgX2hhc1BlbmRpbmdDaGFuZ2UgPSBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnJvbGxiYWNrQ2hhbmdlID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoIV9oYXNQZW5kaW5nQ2hhbmdlKVxyXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiQ29sdW1uIGhhcyBubyBwZW5kaW5nIGNoYW5nZS5cIik7XHJcbiAgICAgICAgICAgIHRoaXMud2lkdGggPSBfb3JpZ1dpZHRoO1xyXG4gICAgICAgICAgICB0aGlzLm9mZnNldCA9IF9vcmlnT2Zmc2V0O1xyXG4gICAgICAgICAgICBfb3JpZ1dpZHRoID0gMDtcclxuICAgICAgICAgICAgX29yaWdPZmZzZXQgPSAwO1xyXG4gICAgICAgICAgICBfaGFzUGVuZGluZ0NoYW5nZSA9IGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuU3BsaXQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLndpZHRoID4gMTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNwbGl0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuU3BsaXQoKSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHZhciBuZXdDb2x1bW5XaWR0aCA9IE1hdGguZmxvb3IodGhpcy53aWR0aCAvIDIpO1xyXG4gICAgICAgICAgICB2YXIgbmV3Q29sdW1uID0gTGF5b3V0RWRpdG9yLkNvbHVtbi5mcm9tKHtcclxuICAgICAgICAgICAgICAgIGRhdGE6IG51bGwsXHJcbiAgICAgICAgICAgICAgICBodG1sSWQ6IG51bGwsXHJcbiAgICAgICAgICAgICAgICBodG1sQ2xhc3M6IG51bGwsXHJcbiAgICAgICAgICAgICAgICBodG1sU3R5bGU6IG51bGwsXHJcbiAgICAgICAgICAgICAgICB3aWR0aDogbmV3Q29sdW1uV2lkdGgsXHJcbiAgICAgICAgICAgICAgICBvZmZzZXQ6IDAsXHJcbiAgICAgICAgICAgICAgICBjaGlsZHJlbjogW11cclxuICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLndpZHRoID0gdGhpcy53aWR0aCAtIG5ld0NvbHVtbldpZHRoO1xyXG4gICAgICAgICAgICB0aGlzLnBhcmVudC5pbnNlcnRDaGlsZChuZXdDb2x1bW4sIHRoaXMpO1xyXG4gICAgICAgICAgICBuZXdDb2x1bW4uc2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5Db250cmFjdFJpZ2h0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5wYXJlbnQuY2FuQ29udHJhY3RDb2x1bW5SaWdodCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29udHJhY3RSaWdodCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQuY29udHJhY3RDb2x1bW5SaWdodCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuRXhwYW5kUmlnaHQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5FeHBhbmRDb2x1bW5SaWdodCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZXhwYW5kUmlnaHQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHRoaXMucGFyZW50LmV4cGFuZENvbHVtblJpZ2h0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5FeHBhbmRMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5wYXJlbnQuY2FuRXhwYW5kQ29sdW1uTGVmdCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZXhwYW5kTGVmdCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQuZXhwYW5kQ29sdW1uTGVmdCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuQ29udHJhY3RMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5wYXJlbnQuY2FuQ29udHJhY3RDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jb250cmFjdExlZnQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHRoaXMucGFyZW50LmNvbnRyYWN0Q29sdW1uTGVmdCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQud2lkdGggPSB0aGlzLndpZHRoO1xyXG4gICAgICAgICAgICByZXN1bHQub2Zmc2V0ID0gdGhpcy5vZmZzZXQ7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jb2xsYXBzaWJsZSA9IHRoaXMuY29sbGFwc2libGU7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jaGlsZHJlbiA9IHRoaXMuY2hpbGRyZW5Ub09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgICAgIH07XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5Db2x1bW4uZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkNvbHVtbihcclxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXHJcbiAgICAgICAgICAgIHZhbHVlLndpZHRoLFxyXG4gICAgICAgICAgICB2YWx1ZS5vZmZzZXQsXHJcbiAgICAgICAgICAgIHZhbHVlLmNvbGxhcHNpYmxlLFxyXG4gICAgICAgICAgICB2YWx1ZS5ydWxlLFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hJY29uID0gdmFsdWUudG9vbGJveEljb247XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hMYWJlbCA9IHZhbHVlLnRvb2xib3hMYWJlbDtcclxuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xyXG4gICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5Db2x1bW4udGltZXMgPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICByZXR1cm4gXy50aW1lcyh2YWx1ZSwgZnVuY3Rpb24gKG4pIHtcclxuICAgICAgICAgICAgcmV0dXJuIExheW91dEVkaXRvci5Db2x1bW4uZnJvbSh7XHJcbiAgICAgICAgICAgICAgICBkYXRhOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaHRtbElkOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaHRtbENsYXNzOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaXNUZW1wbGF0ZWQ6IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgd2lkdGg6IDEyIC8gdmFsdWUsXHJcbiAgICAgICAgICAgICAgICBvZmZzZXQ6IDAsXHJcbiAgICAgICAgICAgICAgICBjb2xsYXBzaWJsZTogbnVsbCxcclxuICAgICAgICAgICAgICAgIGNoaWxkcmVuOiBbXVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9KTtcclxuICAgIH07XHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5Db250ZW50ID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBjb250ZW50VHlwZSwgY29udGVudFR5cGVMYWJlbCwgY29udGVudFR5cGVDbGFzcywgaHRtbCwgaGFzRWRpdG9yLCBydWxlKSB7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkNvbnRlbnRcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUpO1xyXG5cclxuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlID0gY29udGVudFR5cGU7XHJcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUxhYmVsID0gY29udGVudFR5cGVMYWJlbDtcclxuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlQ2xhc3MgPSBjb250ZW50VHlwZUNsYXNzO1xyXG4gICAgICAgIHRoaXMuaHRtbCA9IGh0bWw7XHJcbiAgICAgICAgdGhpcy5oYXNFZGl0b3IgPSBoYXNFZGl0b3I7XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SW5uZXJUZXh0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gJCgkLnBhcnNlSFRNTChcIjxkaXY+XCIgKyB0aGlzLmh0bWwgKyBcIjwvZGl2PlwiKSkudGV4dCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIC8vIFRoaXMgZnVuY3Rpb24gd2lsbCBiZSBvdmVyd3JpdHRlbiBieSB0aGUgQ29udGVudCBkaXJlY3RpdmUuXHJcbiAgICAgICAgdGhpcy5zZXRIdG1sID0gZnVuY3Rpb24gKGh0bWwpIHtcclxuICAgICAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcclxuICAgICAgICAgICAgdGhpcy5odG1sVW5zYWZlID0gaHRtbDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICBcInR5cGVcIjogXCJDb250ZW50XCJcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlID0gdGhpcy5jb250ZW50VHlwZTtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlTGFiZWwgPSB0aGlzLmNvbnRlbnRUeXBlTGFiZWw7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZUNsYXNzID0gdGhpcy5jb250ZW50VHlwZUNsYXNzO1xyXG4gICAgICAgICAgICByZXN1bHQuaHRtbCA9IHRoaXMuaHRtbDtcclxuICAgICAgICAgICAgcmVzdWx0Lmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNldEh0bWwoaHRtbCk7XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5Db250ZW50LmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICB2YXIgcmVzdWx0ID0gbmV3IExheW91dEVkaXRvci5Db250ZW50KFxyXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxyXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlTGFiZWwsXHJcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlQ2xhc3MsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWwsXHJcbiAgICAgICAgICAgIHZhbHVlLmhhc0VkaXRvcixcclxuICAgICAgICAgICAgdmFsdWUucnVsZSk7XHJcblxyXG4gICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICB9O1xyXG5cclxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoJCwgTGF5b3V0RWRpdG9yKSB7XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkh0bWwgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNvbnRlbnRUeXBlLCBjb250ZW50VHlwZUxhYmVsLCBjb250ZW50VHlwZUNsYXNzLCBodG1sLCBoYXNFZGl0b3IsIHJ1bGUpIHtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiSHRtbFwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSk7XHJcblxyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGUgPSBjb250ZW50VHlwZTtcclxuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlTGFiZWwgPSBjb250ZW50VHlwZUxhYmVsO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGVDbGFzcyA9IGNvbnRlbnRUeXBlQ2xhc3M7XHJcbiAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcclxuICAgICAgICB0aGlzLmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcclxuICAgICAgICB0aGlzLmlzQ29udGFpbmFibGUgPSB0cnVlO1xyXG5cclxuICAgICAgICB0aGlzLmdldElubmVyVGV4dCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuICQoJC5wYXJzZUhUTUwoXCI8ZGl2PlwiICsgdGhpcy5odG1sICsgXCI8L2Rpdj5cIikpLnRleHQoKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICAvLyBUaGlzIGZ1bmN0aW9uIHdpbGwgYmUgb3ZlcndyaXR0ZW4gYnkgdGhlIENvbnRlbnQgZGlyZWN0aXZlLlxyXG4gICAgICAgIHRoaXMuc2V0SHRtbCA9IGZ1bmN0aW9uIChodG1sKSB7XHJcbiAgICAgICAgICAgIHRoaXMuaHRtbCA9IGh0bWw7XHJcbiAgICAgICAgICAgIHRoaXMuaHRtbFVuc2FmZSA9IGh0bWw7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICAgICAgXCJ0eXBlXCI6IFwiSHRtbFwiXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZSA9IHRoaXMuY29udGVudFR5cGU7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZUxhYmVsID0gdGhpcy5jb250ZW50VHlwZUxhYmVsO1xyXG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGVDbGFzcyA9IHRoaXMuY29udGVudFR5cGVDbGFzcztcclxuICAgICAgICAgICAgcmVzdWx0Lmh0bWwgPSB0aGlzLmh0bWw7XHJcbiAgICAgICAgICAgIHJlc3VsdC5oYXNFZGl0b3IgPSBoYXNFZGl0b3I7XHJcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdmFyIGdldEVkaXRvck9iamVjdCA9IHRoaXMuZ2V0RWRpdG9yT2JqZWN0O1xyXG4gICAgICAgIHRoaXMuZ2V0RWRpdG9yT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgZHRvID0gZ2V0RWRpdG9yT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJldHVybiAkLmV4dGVuZChkdG8sIHtcclxuICAgICAgICAgICAgICAgIENvbnRlbnQ6IHRoaXMuaHRtbFxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMuc2V0SHRtbChodG1sKTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkh0bWwuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkh0bWwoXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZSxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVMYWJlbCxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbCxcclxuICAgICAgICAgICAgdmFsdWUuaGFzRWRpdG9yLFxyXG4gICAgICAgICAgICB2YWx1ZS5ydWxlKTtcclxuXHJcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLnJlZ2lzdGVyRmFjdG9yeShcIkh0bWxcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5IdG1sLmZyb20odmFsdWUpOyB9KTtcclxuXHJcbn0pKGpRdWVyeSwgTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==