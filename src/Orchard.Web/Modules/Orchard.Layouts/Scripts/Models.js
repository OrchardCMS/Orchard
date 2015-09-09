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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIkhlbHBlcnMuanMiLCJFZGl0b3IuanMiLCJFbGVtZW50LmpzIiwiQ29udGFpbmVyLmpzIiwiQ2FudmFzLmpzIiwiR3JpZC5qcyIsIlJvdy5qcyIsIkNvbHVtbi5qcyIsIkNvbnRlbnQuanMiLCJIdG1sLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDMUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2hDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2hNQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3ZJQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN0Q0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDN0JBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDN1JBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDN0lBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUMxREE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EiLCJmaWxlIjoiTW9kZWxzLmpzIiwic291cmNlc0NvbnRlbnQiOlsidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBBcnJheS5wcm90b3R5cGUubW92ZSA9IGZ1bmN0aW9uIChmcm9tLCB0bykge1xyXG4gICAgICAgIHRoaXMuc3BsaWNlKHRvLCAwLCB0aGlzLnNwbGljZShmcm9tLCAxKVswXSk7XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5jaGlsZHJlbkZyb20gPSBmdW5jdGlvbih2YWx1ZXMpIHtcclxuICAgICAgICByZXR1cm4gXyh2YWx1ZXMpLm1hcChmdW5jdGlvbih2YWx1ZSkge1xyXG4gICAgICAgICAgICByZXR1cm4gTGF5b3V0RWRpdG9yLmVsZW1lbnRGcm9tKHZhbHVlKTtcclxuICAgICAgICB9KTtcclxuICAgIH07XHJcblxyXG4gICAgdmFyIHJlZ2lzdGVyRmFjdG9yeSA9IExheW91dEVkaXRvci5yZWdpc3RlckZhY3RvcnkgPSBmdW5jdGlvbih0eXBlLCBmYWN0b3J5KSB7XHJcbiAgICAgICAgdmFyIGZhY3RvcmllcyA9IExheW91dEVkaXRvci5mYWN0b3JpZXMgPSBMYXlvdXRFZGl0b3IuZmFjdG9yaWVzIHx8IHt9O1xyXG4gICAgICAgIGZhY3Rvcmllc1t0eXBlXSA9IGZhY3Rvcnk7XHJcbiAgICB9O1xyXG5cclxuICAgIHJlZ2lzdGVyRmFjdG9yeShcIkNhbnZhc1wiLCBmdW5jdGlvbiAodmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5DYW52YXMuZnJvbSh2YWx1ZSk7IH0pO1xyXG4gICAgcmVnaXN0ZXJGYWN0b3J5KFwiR3JpZFwiLCBmdW5jdGlvbih2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkdyaWQuZnJvbSh2YWx1ZSk7IH0pO1xyXG4gICAgcmVnaXN0ZXJGYWN0b3J5KFwiUm93XCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuUm93LmZyb20odmFsdWUpOyB9KTtcclxuICAgIHJlZ2lzdGVyRmFjdG9yeShcIkNvbHVtblwiLCBmdW5jdGlvbih2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkNvbHVtbi5mcm9tKHZhbHVlKTsgfSk7XHJcbiAgICByZWdpc3RlckZhY3RvcnkoXCJDb250ZW50XCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuQ29udGVudC5mcm9tKHZhbHVlKTsgfSk7XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLmVsZW1lbnRGcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgdmFyIGZhY3RvcnkgPSBMYXlvdXRFZGl0b3IuZmFjdG9yaWVzW3ZhbHVlLnR5cGVdO1xyXG5cclxuICAgICAgICBpZiAoIWZhY3RvcnkpXHJcbiAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIk5vIGVsZW1lbnQgd2l0aCB0eXBlIFxcXCJcIiArIHZhbHVlLnR5cGUgKyBcIlxcXCIgd2FzIGZvdW5kLlwiKTtcclxuXHJcbiAgICAgICAgdmFyIGVsZW1lbnQgPSBmYWN0b3J5KHZhbHVlKTtcclxuICAgICAgICByZXR1cm4gZWxlbWVudDtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLnNldE1vZGVsID0gZnVuY3Rpb24gKGVsZW1lbnRTZWxlY3RvciwgbW9kZWwpIHtcclxuICAgICAgICAkKGVsZW1lbnRTZWxlY3Rvcikuc2NvcGUoKS5lbGVtZW50ID0gbW9kZWw7XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5nZXRNb2RlbCA9IGZ1bmN0aW9uIChlbGVtZW50U2VsZWN0b3IpIHtcclxuICAgICAgICByZXR1cm4gJChlbGVtZW50U2VsZWN0b3IpLnNjb3BlKCkuZWxlbWVudDtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuRWRpdG9yID0gZnVuY3Rpb24gKGNvbmZpZywgY2FudmFzRGF0YSkge1xyXG4gICAgICAgIHRoaXMuY29uZmlnID0gY29uZmlnO1xyXG4gICAgICAgIHRoaXMuY2FudmFzID0gTGF5b3V0RWRpdG9yLkNhbnZhcy5mcm9tKGNhbnZhc0RhdGEpO1xyXG4gICAgICAgIHRoaXMuaW5pdGlhbFN0YXRlID0gSlNPTi5zdHJpbmdpZnkodGhpcy5jYW52YXMudG9PYmplY3QoKSk7XHJcbiAgICAgICAgdGhpcy5hY3RpdmVFbGVtZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLmZvY3VzZWRFbGVtZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLmRyb3BUYXJnZXRFbGVtZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLmlzRHJhZ2dpbmcgPSBmYWxzZTtcclxuICAgICAgICB0aGlzLmlubGluZUVkaXRpbmdJc0FjdGl2ZSA9IGZhbHNlO1xyXG4gICAgICAgIHRoaXMuaXNSZXNpemluZyA9IGZhbHNlO1xyXG5cclxuICAgICAgICB0aGlzLnJlc2V0VG9vbGJveEVsZW1lbnRzID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB0aGlzLnRvb2xib3hFbGVtZW50cyA9IFtcclxuICAgICAgICAgICAgICAgIExheW91dEVkaXRvci5Sb3cuZnJvbSh7XHJcbiAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXHJcbiAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICBdO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuaXNEaXJ0eSA9IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgY3VycmVudFN0YXRlID0gSlNPTi5zdHJpbmdpZnkodGhpcy5jYW52YXMudG9PYmplY3QoKSk7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmluaXRpYWxTdGF0ZSAhPSBjdXJyZW50U3RhdGU7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5yZXNldFRvb2xib3hFbGVtZW50cygpO1xyXG4gICAgICAgIHRoaXMuY2FudmFzLnNldEVkaXRvcih0aGlzKTtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7XHJcbiIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQgPSBmdW5jdGlvbiAodHlwZSwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUpIHtcclxuICAgICAgICBpZiAoIXR5cGUpXHJcbiAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIlBhcmFtZXRlciAndHlwZScgaXMgcmVxdWlyZWQuXCIpO1xyXG5cclxuICAgICAgICB0aGlzLnR5cGUgPSB0eXBlO1xyXG4gICAgICAgIHRoaXMuZGF0YSA9IGRhdGE7XHJcbiAgICAgICAgdGhpcy5odG1sSWQgPSBodG1sSWQ7XHJcbiAgICAgICAgdGhpcy5odG1sQ2xhc3MgPSBodG1sQ2xhc3M7XHJcbiAgICAgICAgdGhpcy5odG1sU3R5bGUgPSBodG1sU3R5bGU7XHJcbiAgICAgICAgdGhpcy5pc1RlbXBsYXRlZCA9IGlzVGVtcGxhdGVkO1xyXG4gICAgICAgIHRoaXMucnVsZSA9IHJ1bGU7XHJcblxyXG4gICAgICAgIHRoaXMuZWRpdG9yID0gbnVsbDtcclxuICAgICAgICB0aGlzLnBhcmVudCA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5zZXRJc0ZvY3VzZWRFdmVudEhhbmRsZXJzID0gW107XHJcblxyXG4gICAgICAgIHRoaXMuc2V0RWRpdG9yID0gZnVuY3Rpb24gKGVkaXRvcikge1xyXG4gICAgICAgICAgICB0aGlzLmVkaXRvciA9IGVkaXRvcjtcclxuICAgICAgICAgICAgaWYgKCEhdGhpcy5jaGlsZHJlbiAmJiBfLmlzQXJyYXkodGhpcy5jaGlsZHJlbikpIHtcclxuICAgICAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgICAgICAgICBjaGlsZC5zZXRFZGl0b3IoZWRpdG9yKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXRQYXJlbnQgPSBmdW5jdGlvbihwYXJlbnRFbGVtZW50KSB7XHJcbiAgICAgICAgICAgIHRoaXMucGFyZW50ID0gcGFyZW50RWxlbWVudDtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQub25DaGlsZEFkZGVkKHRoaXMpO1xyXG5cclxuICAgICAgICAgICAgdmFyIGN1cnJlbnRBbmNlc3RvciA9IHBhcmVudEVsZW1lbnQ7XHJcbiAgICAgICAgICAgIHdoaWxlICghIWN1cnJlbnRBbmNlc3Rvcikge1xyXG4gICAgICAgICAgICAgICAgY3VycmVudEFuY2VzdG9yLm9uRGVzY2VuZGFudEFkZGVkKHRoaXMsIHBhcmVudEVsZW1lbnQpO1xyXG4gICAgICAgICAgICAgICAgY3VycmVudEFuY2VzdG9yID0gY3VycmVudEFuY2VzdG9yLnBhcmVudDtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0SXNUZW1wbGF0ZWQgPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICAgICAgdGhpcy5pc1RlbXBsYXRlZCA9IHZhbHVlO1xyXG4gICAgICAgICAgICBpZiAoISF0aGlzLmNoaWxkcmVuICYmIF8uaXNBcnJheSh0aGlzLmNoaWxkcmVuKSkge1xyXG4gICAgICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNoaWxkLnNldElzVGVtcGxhdGVkKHZhbHVlKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5hcHBseUVsZW1lbnRFZGl0b3JNb2RlbCA9IGZ1bmN0aW9uKCkgeyAvKiBWaXJ0dWFsICovIH07XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SXNBY3RpdmUgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmVkaXRvci5hY3RpdmVFbGVtZW50ID09PSB0aGlzICYmICF0aGlzLmdldElzRm9jdXNlZCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0SXNBY3RpdmUgPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgaWYgKHRoaXMuZWRpdG9yLmlzRHJhZ2dpbmcgfHwgdGhpcy5lZGl0b3IuaW5saW5lRWRpdGluZ0lzQWN0aXZlIHx8IHRoaXMuZWRpdG9yLmlzUmVzaXppbmcpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICBpZiAodmFsdWUpXHJcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5hY3RpdmVFbGVtZW50ID0gdGhpcztcclxuICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgdGhpcy5lZGl0b3IuYWN0aXZlRWxlbWVudCA9IHRoaXMucGFyZW50O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SXNGb2N1c2VkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5lZGl0b3IuZm9jdXNlZEVsZW1lbnQgPT09IHRoaXM7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5hbGxvd1NlYWxlZEZvY3VzID0gZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNldElzRm9jdXNlZCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgaWYgKHRoaXMuaXNUZW1wbGF0ZWQgJiYgIXRoaXMuYWxsb3dTZWFsZWRGb2N1cygpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5lZGl0b3IuaXNEcmFnZ2luZyB8fCB0aGlzLmVkaXRvci5pbmxpbmVFZGl0aW5nSXNBY3RpdmUgfHwgdGhpcy5lZGl0b3IuaXNSZXNpemluZylcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuZWRpdG9yLmZvY3VzZWRFbGVtZW50ID0gdGhpcztcclxuICAgICAgICAgICAgXyh0aGlzLnNldElzRm9jdXNlZEV2ZW50SGFuZGxlcnMpLmVhY2goZnVuY3Rpb24gKGl0ZW0pIHtcclxuICAgICAgICAgICAgICAgIHRyeSB7XHJcbiAgICAgICAgICAgICAgICAgICAgaXRlbSgpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgY2F0Y2ggKGV4KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gSWdub3JlLlxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmdldElzU2VsZWN0ZWQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmdldElzRm9jdXNlZCgpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcblxyXG4gICAgICAgICAgICBpZiAoISF0aGlzLmNoaWxkcmVuICYmIF8uaXNBcnJheSh0aGlzLmNoaWxkcmVuKSkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikuYW55KGZ1bmN0aW9uKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNoaWxkLmdldElzU2VsZWN0ZWQoKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJc0Ryb3BUYXJnZXQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmVkaXRvci5kcm9wVGFyZ2V0RWxlbWVudCA9PT0gdGhpcztcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMuc2V0SXNEcm9wVGFyZ2V0ID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIGlmICh2YWx1ZSlcclxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmRyb3BUYXJnZXRFbGVtZW50ID0gdGhpcztcclxuICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgdGhpcy5lZGl0b3IuZHJvcFRhcmdldEVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuRGVsZXRlID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gISF0aGlzLnBhcmVudDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmRlbGV0ZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCEhdGhpcy5wYXJlbnQpXHJcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5kZWxldGVDaGlsZCh0aGlzKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbk1vdmVVcCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLnBhcmVudClcclxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbk1vdmVDaGlsZFVwKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZVVwID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoISF0aGlzLnBhcmVudClcclxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50Lm1vdmVDaGlsZFVwKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuTW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5wYXJlbnQpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5Nb3ZlQ2hpbGREb3duKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQubW92ZUNoaWxkRG93bih0aGlzKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmVsZW1lbnRUb09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgIHR5cGU6IHRoaXMudHlwZSxcclxuICAgICAgICAgICAgICAgIGRhdGE6IHRoaXMuZGF0YSxcclxuICAgICAgICAgICAgICAgIGh0bWxJZDogdGhpcy5odG1sSWQsXHJcbiAgICAgICAgICAgICAgICBodG1sQ2xhc3M6IHRoaXMuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICAgICAgaHRtbFN0eWxlOiB0aGlzLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgICAgIGlzVGVtcGxhdGVkOiB0aGlzLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICAgICAgcnVsZTogdGhpcy5ydWxlXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRFZGl0b3JPYmplY3QgPSBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHt9O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29weSA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XHJcbiAgICAgICAgICAgIHZhciB0ZXh0ID0gdGhpcy5nZXRJbm5lclRleHQoKTtcclxuICAgICAgICAgICAgY2xpcGJvYXJkRGF0YS5zZXREYXRhKFwidGV4dC9wbGFpblwiLCB0ZXh0KTtcclxuXHJcbiAgICAgICAgICAgIHZhciBkYXRhID0gdGhpcy50b09iamVjdCgpO1xyXG4gICAgICAgICAgICB2YXIganNvbiA9IEpTT04uc3RyaW5naWZ5KGRhdGEsIG51bGwsIFwiXFx0XCIpO1xyXG4gICAgICAgICAgICBjbGlwYm9hcmREYXRhLnNldERhdGEoXCJ0ZXh0L2pzb25cIiwganNvbik7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jdXQgPSBmdW5jdGlvbiAoY2xpcGJvYXJkRGF0YSkge1xyXG4gICAgICAgICAgICB0aGlzLmNvcHkoY2xpcGJvYXJkRGF0YSk7XHJcbiAgICAgICAgICAgIHRoaXMuZGVsZXRlKCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5wYXN0ZSA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGUoY2xpcGJvYXJkRGF0YSk7XHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyID0gZnVuY3Rpb24gKGFsbG93ZWRDaGlsZFR5cGVzLCBjaGlsZHJlbikge1xyXG5cclxuICAgICAgICB0aGlzLmFsbG93ZWRDaGlsZFR5cGVzID0gYWxsb3dlZENoaWxkVHlwZXM7XHJcbiAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xyXG4gICAgICAgIHRoaXMuaXNDb250YWluZXIgPSB0cnVlO1xyXG5cclxuICAgICAgICB2YXIgc2VsZiA9IHRoaXM7XHJcblxyXG4gICAgICAgIHRoaXMub25DaGlsZEFkZGVkID0gZnVuY3Rpb24gKGVsZW1lbnQpIHsgLyogVmlydHVhbCAqLyB9O1xyXG4gICAgICAgIHRoaXMub25EZXNjZW5kYW50QWRkZWQgPSBmdW5jdGlvbiAoZWxlbWVudCwgcGFyZW50RWxlbWVudCkgeyAvKiBWaXJ0dWFsICovIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4gPSBmdW5jdGlvbiAoY2hpbGRyZW4pIHtcclxuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5zZXRQYXJlbnQoc2VsZik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4oY2hpbGRyZW4pO1xyXG5cclxuICAgICAgICB0aGlzLmdldElzU2VhbGVkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5hbnkoZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gY2hpbGQuaXNUZW1wbGF0ZWQ7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuYWRkQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgaWYgKCFfKHRoaXMuY2hpbGRyZW4pLmNvbnRhaW5zKGNoaWxkKSAmJiAoXyh0aGlzLmFsbG93ZWRDaGlsZFR5cGVzKS5jb250YWlucyhjaGlsZC50eXBlKSB8fCBjaGlsZC5pc0NvbnRhaW5hYmxlKSlcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4ucHVzaChjaGlsZCk7XHJcbiAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XHJcbiAgICAgICAgICAgIGNoaWxkLnNldElzVGVtcGxhdGVkKGZhbHNlKTtcclxuICAgICAgICAgICAgY2hpbGQuc2V0UGFyZW50KHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZGVsZXRlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4uc3BsaWNlKGluZGV4LCAxKTtcclxuICAgICAgICAgICAgICAgIGlmIChjaGlsZC5nZXRJc0FjdGl2ZSgpKVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzRm9jdXNlZCgpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gSWYgdGhlIGRlbGV0ZWQgY2hpbGQgd2FzIGZvY3VzZWQsIHRyeSB0byBzZXQgbmV3IGZvY3VzIHRvIHRoZSBtb3N0IGFwcHJvcHJpYXRlIHNpYmxpbmcgb3IgcGFyZW50LlxyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuW2luZGV4XS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlIGlmIChpbmRleCA+IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuc2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLm1vdmVGb2N1c1ByZXZDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPCAyKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPiAwKVxyXG4gICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdLnNldElzRm9jdXNlZCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZUZvY3VzTmV4dENoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA8IDIpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA8IHRoaXMuY2hpbGRyZW4ubGVuZ3RoIC0gMSlcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW5baW5kZXggKyAxXS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmluc2VydENoaWxkID0gZnVuY3Rpb24gKGNoaWxkLCBhZnRlckNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICghXyh0aGlzLmNoaWxkcmVuKS5jb250YWlucyhjaGlsZCkpIHtcclxuICAgICAgICAgICAgICAgIHZhciBpbmRleCA9IE1hdGgubWF4KF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihhZnRlckNoaWxkKSwgMCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuLnNwbGljZShpbmRleCArIDEsIDAsIGNoaWxkKTtcclxuICAgICAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5wYXJlbnQgPSB0aGlzO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5tb3ZlQ2hpbGRVcCA9IGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuTW92ZUNoaWxkVXAoY2hpbGQpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xyXG4gICAgICAgICAgICB0aGlzLmNoaWxkcmVuLm1vdmUoaW5kZXgsIGluZGV4IC0gMSk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5tb3ZlQ2hpbGREb3duID0gZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Nb3ZlQ2hpbGREb3duKGNoaWxkKSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbi5tb3ZlKGluZGV4LCBpbmRleCArIDEpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuTW92ZUNoaWxkVXAgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgcmV0dXJuIGluZGV4ID4gMDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbk1vdmVDaGlsZERvd24gPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgcmV0dXJuIGluZGV4IDwgdGhpcy5jaGlsZHJlbi5sZW5ndGggLSAxO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2hpbGRyZW5Ub09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikubWFwKGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNoaWxkLnRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SW5uZXJUZXh0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5yZWR1Y2UoZnVuY3Rpb24gKG1lbW8sIGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gbWVtbyArIFwiXFxuXCIgKyBjaGlsZC5nZXRJbm5lclRleHQoKTtcclxuICAgICAgICAgICAgfSwgXCJcIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnBhc3RlID0gZnVuY3Rpb24gKGNsaXBib2FyZERhdGEpIHtcclxuICAgICAgICAgICAgdmFyIGpzb24gPSBjbGlwYm9hcmREYXRhLmdldERhdGEoXCJ0ZXh0L2pzb25cIik7XHJcbiAgICAgICAgICAgIGlmICghIWpzb24pIHtcclxuICAgICAgICAgICAgICAgIHZhciBkYXRhID0gSlNPTi5wYXJzZShqc29uKTtcclxuICAgICAgICAgICAgICAgIHZhciBjaGlsZCA9IExheW91dEVkaXRvci5lbGVtZW50RnJvbShkYXRhKTtcclxuICAgICAgICAgICAgICAgIHRoaXMucGFzdGVDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnBhc3RlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgaWYgKF8odGhpcy5hbGxvd2VkQ2hpbGRUeXBlcykuY29udGFpbnMoY2hpbGQudHlwZSkgfHwgY2hpbGQuaXNDb250YWluYWJsZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5hZGRDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGVDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5DYW52YXMgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUsIGNoaWxkcmVuKSB7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkNhbnZhc1wiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSk7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIkNhbnZhc1wiLCBcIkdyaWRcIiwgXCJDb250ZW50XCJdLCBjaGlsZHJlbik7XHJcblxyXG4gICAgICAgIHRoaXMuaXNDb250YWluYWJsZSA9IHRydWU7XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmFsbG93U2VhbGVkRm9jdXMgPSBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuY2hpbGRyZW4ubGVuZ3RoID09PSAwO1xyXG4gICAgICAgIH07XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5DYW52YXMuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkNhbnZhcyhcclxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXHJcbiAgICAgICAgICAgIHZhbHVlLnJ1bGUsXHJcbiAgICAgICAgICAgIExheW91dEVkaXRvci5jaGlsZHJlbkZyb20odmFsdWUuY2hpbGRyZW4pKTtcclxuXHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hJY29uID0gdmFsdWUudG9vbGJveEljb247XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hMYWJlbCA9IHZhbHVlLnRvb2xib3hMYWJlbDtcclxuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xyXG5cclxuICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTtcclxuIiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuR3JpZCA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSwgY2hpbGRyZW4pIHtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiR3JpZFwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSk7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIlJvd1wiXSwgY2hpbGRyZW4pO1xyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmVzdWx0LmNoaWxkcmVuID0gdGhpcy5jaGlsZHJlblRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkdyaWQuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkdyaWQoXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS5ydWxlLFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hJY29uID0gdmFsdWUudG9vbGJveEljb247XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hMYWJlbCA9IHZhbHVlLnRvb2xib3hMYWJlbDtcclxuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xyXG4gICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICB9O1xyXG5cclxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLlJvdyA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSwgY2hpbGRyZW4pIHtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiUm93XCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBydWxlKTtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiQ29sdW1uXCJdLCBjaGlsZHJlbik7XHJcblxyXG4gICAgICAgIHZhciBfc2VsZiA9IHRoaXM7XHJcblxyXG4gICAgICAgIGZ1bmN0aW9uIF9nZXRUb3RhbENvbHVtbnNXaWR0aCgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIF8oX3NlbGYuY2hpbGRyZW4pLnJlZHVjZShmdW5jdGlvbiAobWVtbywgY2hpbGQpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBtZW1vICsgY2hpbGQub2Zmc2V0ICsgY2hpbGQud2lkdGg7XHJcbiAgICAgICAgICAgIH0sIDApO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgLy8gSW1wbGVtZW50cyBhIHNpbXBsZSBhbGdvcml0aG0gdG8gZGlzdHJpYnV0ZSBzcGFjZSAoZWl0aGVyIHBvc2l0aXZlIG9yIG5lZ2F0aXZlKVxyXG4gICAgICAgIC8vIGJldHdlZW4gdGhlIGNoaWxkIGNvbHVtbnMgb2YgdGhlIHJvdy4gTmVnYXRpdmUgc3BhY2UgaXMgZGlzdHJpYnV0ZWQgd2hlbiBtYWtpbmdcclxuICAgICAgICAvLyByb29tIGZvciBhIG5ldyBjb2x1bW4gKGUuYy4gY2xpcGJvYXJkIHBhc3RlIG9yIGRyb3BwaW5nIGZyb20gdGhlIHRvb2xib3gpIHdoaWxlXHJcbiAgICAgICAgLy8gcG9zaXRpdmUgc3BhY2UgaXMgZGlzdHJpYnV0ZWQgd2hlbiBmaWxsaW5nIHRoZSBncmFwIG9mIGEgcmVtb3ZlZCBjb2x1bW4uXHJcbiAgICAgICAgZnVuY3Rpb24gX2Rpc3RyaWJ1dGVTcGFjZShzcGFjZSkge1xyXG4gICAgICAgICAgICBpZiAoc3BhY2UgPT0gMClcclxuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xyXG4gICAgICAgICAgICAgXHJcbiAgICAgICAgICAgIHZhciB1bmRpc3RyaWJ1dGVkU3BhY2UgPSBzcGFjZTtcclxuXHJcbiAgICAgICAgICAgIGlmICh1bmRpc3RyaWJ1dGVkU3BhY2UgPCAwKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgdmFjYW50U3BhY2UgPSAxMiAtIF9nZXRUb3RhbENvbHVtbnNXaWR0aCgpO1xyXG4gICAgICAgICAgICAgICAgdW5kaXN0cmlidXRlZFNwYWNlICs9IHZhY2FudFNwYWNlO1xyXG4gICAgICAgICAgICAgICAgaWYgKHVuZGlzdHJpYnV0ZWRTcGFjZSA+IDApXHJcbiAgICAgICAgICAgICAgICAgICAgdW5kaXN0cmlidXRlZFNwYWNlID0gMDtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgLy8gSWYgc3BhY2UgaXMgbmVnYXRpdmUsIHRyeSB0byBkZWNyZWFzZSBvZmZzZXRzIGZpcnN0LlxyXG4gICAgICAgICAgICB3aGlsZSAodW5kaXN0cmlidXRlZFNwYWNlIDwgMCAmJiBfKF9zZWxmLmNoaWxkcmVuKS5hbnkoZnVuY3Rpb24gKGNvbHVtbikgeyByZXR1cm4gY29sdW1uLm9mZnNldCA+IDA7IH0pKSB7IC8vIFdoaWxlIHRoZXJlIGlzIHN0aWxsIG9mZnNldCBsZWZ0IHRvIHJlbW92ZS5cclxuICAgICAgICAgICAgICAgIGZvciAoaSA9IDA7IGkgPCBfc2VsZi5jaGlsZHJlbi5sZW5ndGggJiYgdW5kaXN0cmlidXRlZFNwYWNlIDwgMDsgaSsrKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGNvbHVtbiA9IF9zZWxmLmNoaWxkcmVuW2ldO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ub2Zmc2V0ID4gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0LS07XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHVuZGlzdHJpYnV0ZWRTcGFjZSsrO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgZnVuY3Rpb24gaGFzV2lkdGgoY29sdW1uKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAodW5kaXN0cmlidXRlZFNwYWNlID4gMClcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gY29sdW1uLndpZHRoIDwgMTI7XHJcbiAgICAgICAgICAgICAgICBlbHNlIGlmICh1bmRpc3RyaWJ1dGVkU3BhY2UgPCAwKVxyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ud2lkdGggPiAxO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAvLyBUcnkgdG8gZGlzdHJpYnV0ZSByZW1haW5pbmcgc3BhY2UgKGNvdWxkIGJlIG5lZ2F0aXZlIG9yIHBvc2l0aXZlKSB1c2luZyB3aWR0aHMuXHJcbiAgICAgICAgICAgIHdoaWxlICh1bmRpc3RyaWJ1dGVkU3BhY2UgIT0gMCkge1xyXG4gICAgICAgICAgICAgICAgLy8gQW55IG1vcmUgY29sdW1uIHdpZHRoIGF2YWlsYWJsZSBmb3IgZGlzdHJpYnV0aW9uP1xyXG4gICAgICAgICAgICAgICAgaWYgKCFfKF9zZWxmLmNoaWxkcmVuKS5hbnkoaGFzV2lkdGgpKVxyXG4gICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgZm9yIChpID0gMDsgaSA8IF9zZWxmLmNoaWxkcmVuLmxlbmd0aCAmJiB1bmRpc3RyaWJ1dGVkU3BhY2UgIT0gMDsgaSsrKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGNvbHVtbiA9IF9zZWxmLmNoaWxkcmVuW2kgJSBfc2VsZi5jaGlsZHJlbi5sZW5ndGhdO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChoYXNXaWR0aChjb2x1bW4pKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBkZWx0YSA9IHVuZGlzdHJpYnV0ZWRTcGFjZSAvIE1hdGguYWJzKHVuZGlzdHJpYnV0ZWRTcGFjZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aCArPSBkZWx0YTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdW5kaXN0cmlidXRlZFNwYWNlIC09IGRlbHRhO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0gICAgICAgICAgICAgICAgXHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB1bmRpc3RyaWJ1dGVkU3BhY2UgPT0gMDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBfaXNBZGRpbmdDb2x1bW4gPSBmYWxzZTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5BZGRDb2x1bW4gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmNoaWxkcmVuLmxlbmd0aCA8IDEyO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuYmVnaW5BZGRDb2x1bW4gPSBmdW5jdGlvbiAobmV3Q29sdW1uV2lkdGgpIHtcclxuICAgICAgICAgICAgaWYgKCEhX2lzQWRkaW5nQ29sdW1uKVxyXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiQ29sdW1uIGFkZCBvcGVyYXRpb24gaXMgYWxyZWFkeSBpbiBwcm9ncmVzcy5cIilcclxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcclxuICAgICAgICAgICAgICAgIGNvbHVtbi5iZWdpbkNoYW5nZSgpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgaWYgKF9kaXN0cmlidXRlU3BhY2UoLW5ld0NvbHVtbldpZHRoKSkge1xyXG4gICAgICAgICAgICAgICAgX2lzQWRkaW5nQ29sdW1uID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4ucm9sbGJhY2tDaGFuZ2UoKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNvbW1pdEFkZENvbHVtbiA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCFfaXNBZGRpbmdDb2x1bW4pXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJObyBjb2x1bW4gYWRkIG9wZXJhdGlvbiBpbiBwcm9ncmVzcy5cIilcclxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcclxuICAgICAgICAgICAgICAgIGNvbHVtbi5jb21taXRDaGFuZ2UoKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIF9pc0FkZGluZ0NvbHVtbiA9IGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMucm9sbGJhY2tBZGRDb2x1bW4gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghX2lzQWRkaW5nQ29sdW1uKVxyXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiTm8gY29sdW1uIGFkZCBvcGVyYXRpb24gaW4gcHJvZ3Jlc3MuXCIpXHJcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4ucm9sbGJhY2tDaGFuZ2UoKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIF9pc0FkZGluZ0NvbHVtbiA9IGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHZhciBfYmFzZURlbGV0ZUNoaWxkID0gdGhpcy5kZWxldGVDaGlsZDtcclxuICAgICAgICB0aGlzLmRlbGV0ZUNoaWxkID0gZnVuY3Rpb24gKGNvbHVtbikgeyBcclxuICAgICAgICAgICAgdmFyIHdpZHRoID0gY29sdW1uLndpZHRoO1xyXG4gICAgICAgICAgICBfYmFzZURlbGV0ZUNoaWxkLmNhbGwodGhpcywgY29sdW1uKTtcclxuICAgICAgICAgICAgX2Rpc3RyaWJ1dGVTcGFjZSh3aWR0aCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5Db250cmFjdENvbHVtblJpZ2h0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMClcclxuICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ud2lkdGggPiAxO1xyXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jb250cmFjdENvbHVtblJpZ2h0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Db250cmFjdENvbHVtblJpZ2h0KGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGNvbHVtbi53aWR0aCA+IDEpIHtcclxuICAgICAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGgtLTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPiBpbmRleCArIDEpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIG5leHRDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4ICsgMV07XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgbmV4dENvbHVtbi5vZmZzZXQgPT0gMClcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ud2lkdGgrKztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbmV4dENvbHVtbi5vZmZzZXQrKztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkV4cGFuZENvbHVtblJpZ2h0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGNvbHVtbi53aWR0aCA+PSAxMilcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPiBpbmRleCArIDEpIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgbmV4dENvbHVtbiA9IHRoaXMuY2hpbGRyZW5baW5kZXggKyAxXTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIG5leHRDb2x1bW4ub2Zmc2V0ID09IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBuZXh0Q29sdW1uLndpZHRoID4gMTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBuZXh0Q29sdW1uLm9mZnNldCA+IDA7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gX2dldFRvdGFsQ29sdW1uc1dpZHRoKCkgPCAxMjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5leHBhbmRDb2x1bW5SaWdodCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuRXhwYW5kQ29sdW1uUmlnaHQoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG5cclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPiBpbmRleCArIDEpIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgbmV4dENvbHVtbiA9IHRoaXMuY2hpbGRyZW5baW5kZXggKyAxXTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIG5leHRDb2x1bW4ub2Zmc2V0ID09IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ud2lkdGgtLTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ub2Zmc2V0LS07XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGgrKztcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuRXhwYW5kQ29sdW1uTGVmdCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ud2lkdGggPj0gMTIpXHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgaWYgKGluZGV4ID4gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBwcmV2Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgY29sdW1uLm9mZnNldCA9PSAwKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gcHJldkNvbHVtbi53aWR0aCA+IDE7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gY29sdW1uLm9mZnNldCA+IDA7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZXhwYW5kQ29sdW1uTGVmdCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuRXhwYW5kQ29sdW1uTGVmdChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIGlmIChpbmRleCA+IDApIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgcHJldkNvbHVtbiA9IHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIGNvbHVtbi5vZmZzZXQgPT0gMClcclxuICAgICAgICAgICAgICAgICAgICAgICAgcHJldkNvbHVtbi53aWR0aC0tO1xyXG4gICAgICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldC0tO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQtLTtcclxuICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aCsrO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5Db250cmFjdENvbHVtbkxlZnQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA+IDE7XHJcbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNvbnRyYWN0Q29sdW1uTGVmdCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuQ29udHJhY3RDb2x1bW5MZWZ0KGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGluZGV4ID4gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBwcmV2Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgY29sdW1uLm9mZnNldCA9PSAwKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBwcmV2Q29sdW1uLndpZHRoKys7XHJcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0Kys7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldCsrO1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLndpZHRoLS07XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmV2ZW5Db2x1bW5zID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPT0gMClcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHZhciBldmVuV2lkdGggPSBNYXRoLmZsb29yKDEyIC8gdGhpcy5jaGlsZHJlbi5sZW5ndGgpO1xyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLndpZHRoID0gZXZlbldpZHRoO1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldCA9IDA7XHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgdmFyIHJlc3QgPSAxMiAlIHRoaXMuY2hpbGRyZW4ubGVuZ3RoO1xyXG4gICAgICAgICAgICBpZiAocmVzdCA+IDApXHJcbiAgICAgICAgICAgICAgICBfZGlzdHJpYnV0ZVNwYWNlKHJlc3QpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHZhciBfYmFzZVBhc3RlQ2hpbGQgPSB0aGlzLnBhc3RlQ2hpbGQ7XHJcbiAgICAgICAgdGhpcy5wYXN0ZUNoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmIChjaGlsZC50eXBlID09IFwiQ29sdW1uXCIpIHtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLmJlZ2luQWRkQ29sdW1uKGNoaWxkLndpZHRoKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuY29tbWl0QWRkQ29sdW1uKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgX2Jhc2VQYXN0ZUNoaWxkLmNhbGwodGhpcywgY2hpbGQpXHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZWxzZSBpZiAoISF0aGlzLnBhcmVudClcclxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50LnBhc3RlQ2hpbGQoY2hpbGQpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jaGlsZHJlbiA9IHRoaXMuY2hpbGRyZW5Ub09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgICAgIH07XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5Sb3cuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLlJvdyhcclxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXHJcbiAgICAgICAgICAgIHZhbHVlLnJ1bGUsXHJcbiAgICAgICAgICAgIExheW91dEVkaXRvci5jaGlsZHJlbkZyb20odmFsdWUuY2hpbGRyZW4pKTtcclxuICAgICAgICByZXN1bHQudG9vbGJveEljb24gPSB2YWx1ZS50b29sYm94SWNvbjtcclxuICAgICAgICByZXN1bHQudG9vbGJveExhYmVsID0gdmFsdWUudG9vbGJveExhYmVsO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94RGVzY3JpcHRpb24gPSB2YWx1ZS50b29sYm94RGVzY3JpcHRpb247XHJcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuICAgIExheW91dEVkaXRvci5Db2x1bW4gPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHdpZHRoLCBvZmZzZXQsIGNvbGxhcHNpYmxlLCBydWxlLCBjaGlsZHJlbikge1xyXG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJDb2x1bW5cIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHJ1bGUpO1xyXG4gICAgICAgIExheW91dEVkaXRvci5Db250YWluZXIuY2FsbCh0aGlzLCBbXCJHcmlkXCIsIFwiQ29udGVudFwiXSwgY2hpbGRyZW4pO1xyXG4gICAgICAgIHRoaXMud2lkdGggPSB3aWR0aDtcclxuICAgICAgICB0aGlzLm9mZnNldCA9IG9mZnNldDtcclxuICAgICAgICB0aGlzLmNvbGxhcHNpYmxlID0gY29sbGFwc2libGU7XHJcblxyXG4gICAgICAgIHZhciBfaGFzUGVuZGluZ0NoYW5nZSA9IGZhbHNlO1xyXG4gICAgICAgIHZhciBfb3JpZ1dpZHRoID0gMDtcclxuICAgICAgICB2YXIgX29yaWdPZmZzZXQgPSAwO1xyXG5cclxuICAgICAgICB0aGlzLmFsbG93U2VhbGVkRm9jdXMgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmNoaWxkcmVuLmxlbmd0aCA9PT0gMDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmJlZ2luQ2hhbmdlID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoISFfaGFzUGVuZGluZ0NoYW5nZSlcclxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIkNvbHVtbiBhbHJlYWR5IGhhcyBhIHBlbmRpbmcgY2hhbmdlLlwiKTtcclxuICAgICAgICAgICAgX2hhc1BlbmRpbmdDaGFuZ2UgPSB0cnVlO1xyXG4gICAgICAgICAgICBfb3JpZ1dpZHRoID0gdGhpcy53aWR0aDtcclxuICAgICAgICAgICAgX29yaWdPZmZzZXQgPSB0aGlzLm9mZnNldDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNvbW1pdENoYW5nZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCFfaGFzUGVuZGluZ0NoYW5nZSlcclxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIkNvbHVtbiBoYXMgbm8gcGVuZGluZyBjaGFuZ2UuXCIpO1xyXG4gICAgICAgICAgICBfb3JpZ1dpZHRoID0gMDtcclxuICAgICAgICAgICAgX29yaWdPZmZzZXQgPSAwO1xyXG4gICAgICAgICAgICBfaGFzUGVuZGluZ0NoYW5nZSA9IGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMucm9sbGJhY2tDaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghX2hhc1BlbmRpbmdDaGFuZ2UpXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gaGFzIG5vIHBlbmRpbmcgY2hhbmdlLlwiKTtcclxuICAgICAgICAgICAgdGhpcy53aWR0aCA9IF9vcmlnV2lkdGg7XHJcbiAgICAgICAgICAgIHRoaXMub2Zmc2V0ID0gX29yaWdPZmZzZXQ7XHJcbiAgICAgICAgICAgIF9vcmlnV2lkdGggPSAwO1xyXG4gICAgICAgICAgICBfb3JpZ09mZnNldCA9IDA7XHJcbiAgICAgICAgICAgIF9oYXNQZW5kaW5nQ2hhbmdlID0gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5TcGxpdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMud2lkdGggPiAxO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc3BsaXQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5TcGxpdCgpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG5cclxuICAgICAgICAgICAgdmFyIG5ld0NvbHVtbldpZHRoID0gTWF0aC5mbG9vcih0aGlzLndpZHRoIC8gMik7XHJcbiAgICAgICAgICAgIHZhciBuZXdDb2x1bW4gPSBMYXlvdXRFZGl0b3IuQ29sdW1uLmZyb20oe1xyXG4gICAgICAgICAgICAgICAgZGF0YTogbnVsbCxcclxuICAgICAgICAgICAgICAgIGh0bWxJZDogbnVsbCxcclxuICAgICAgICAgICAgICAgIGh0bWxDbGFzczogbnVsbCxcclxuICAgICAgICAgICAgICAgIGh0bWxTdHlsZTogbnVsbCxcclxuICAgICAgICAgICAgICAgIHdpZHRoOiBuZXdDb2x1bW5XaWR0aCxcclxuICAgICAgICAgICAgICAgIG9mZnNldDogMCxcclxuICAgICAgICAgICAgICAgIGNoaWxkcmVuOiBbXVxyXG4gICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMud2lkdGggPSB0aGlzLndpZHRoIC0gbmV3Q29sdW1uV2lkdGg7XHJcbiAgICAgICAgICAgIHRoaXMucGFyZW50Lmluc2VydENoaWxkKG5ld0NvbHVtbiwgdGhpcyk7XHJcbiAgICAgICAgICAgIG5ld0NvbHVtbi5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkNvbnRyYWN0UmlnaHQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5Db250cmFjdENvbHVtblJpZ2h0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jb250cmFjdFJpZ2h0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB0aGlzLnBhcmVudC5jb250cmFjdENvbHVtblJpZ2h0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5FeHBhbmRSaWdodCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkV4cGFuZENvbHVtblJpZ2h0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5leHBhbmRSaWdodCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQuZXhwYW5kQ29sdW1uUmlnaHQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkV4cGFuZExlZnQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5FeHBhbmRDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5leHBhbmRMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB0aGlzLnBhcmVudC5leHBhbmRDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5Db250cmFjdExlZnQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5Db250cmFjdENvbHVtbkxlZnQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNvbnRyYWN0TGVmdCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQuY29udHJhY3RDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJlc3VsdC53aWR0aCA9IHRoaXMud2lkdGg7XHJcbiAgICAgICAgICAgIHJlc3VsdC5vZmZzZXQgPSB0aGlzLm9mZnNldDtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbGxhcHNpYmxlID0gdGhpcy5jb2xsYXBzaWJsZTtcclxuICAgICAgICAgICAgcmVzdWx0LmNoaWxkcmVuID0gdGhpcy5jaGlsZHJlblRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkNvbHVtbi5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuQ29sdW1uKFxyXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxyXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcclxuICAgICAgICAgICAgdmFsdWUud2lkdGgsXHJcbiAgICAgICAgICAgIHZhbHVlLm9mZnNldCxcclxuICAgICAgICAgICAgdmFsdWUuY29sbGFwc2libGUsXHJcbiAgICAgICAgICAgIHZhbHVlLnJ1bGUsXHJcbiAgICAgICAgICAgIExheW91dEVkaXRvci5jaGlsZHJlbkZyb20odmFsdWUuY2hpbGRyZW4pKTtcclxuICAgICAgICByZXN1bHQudG9vbGJveEljb24gPSB2YWx1ZS50b29sYm94SWNvbjtcclxuICAgICAgICByZXN1bHQudG9vbGJveExhYmVsID0gdmFsdWUudG9vbGJveExhYmVsO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94RGVzY3JpcHRpb24gPSB2YWx1ZS50b29sYm94RGVzY3JpcHRpb247XHJcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkNvbHVtbi50aW1lcyA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHJldHVybiBfLnRpbWVzKHZhbHVlLCBmdW5jdGlvbiAobikge1xyXG4gICAgICAgICAgICByZXR1cm4gTGF5b3V0RWRpdG9yLkNvbHVtbi5mcm9tKHtcclxuICAgICAgICAgICAgICAgIGRhdGE6IG51bGwsXHJcbiAgICAgICAgICAgICAgICBodG1sSWQ6IG51bGwsXHJcbiAgICAgICAgICAgICAgICBodG1sQ2xhc3M6IG51bGwsXHJcbiAgICAgICAgICAgICAgICBpc1RlbXBsYXRlZDogZmFsc2UsXHJcbiAgICAgICAgICAgICAgICB3aWR0aDogMTIgLyB2YWx1ZSxcclxuICAgICAgICAgICAgICAgIG9mZnNldDogMCxcclxuICAgICAgICAgICAgICAgIGNvbGxhcHNpYmxlOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfTtcclxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkNvbnRlbnQgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNvbnRlbnRUeXBlLCBjb250ZW50VHlwZUxhYmVsLCBjb250ZW50VHlwZUNsYXNzLCBodG1sLCBoYXNFZGl0b3IsIHJ1bGUpIHtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiQ29udGVudFwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgcnVsZSk7XHJcblxyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGUgPSBjb250ZW50VHlwZTtcclxuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlTGFiZWwgPSBjb250ZW50VHlwZUxhYmVsO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGVDbGFzcyA9IGNvbnRlbnRUeXBlQ2xhc3M7XHJcbiAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcclxuICAgICAgICB0aGlzLmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJbm5lclRleHQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiAkKCQucGFyc2VIVE1MKFwiPGRpdj5cIiArIHRoaXMuaHRtbCArIFwiPC9kaXY+XCIpKS50ZXh0KCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgLy8gVGhpcyBmdW5jdGlvbiB3aWxsIGJlIG92ZXJ3cml0dGVuIGJ5IHRoZSBDb250ZW50IGRpcmVjdGl2ZS5cclxuICAgICAgICB0aGlzLnNldEh0bWwgPSBmdW5jdGlvbiAoaHRtbCkge1xyXG4gICAgICAgICAgICB0aGlzLmh0bWwgPSBodG1sO1xyXG4gICAgICAgICAgICB0aGlzLmh0bWxVbnNhZmUgPSBodG1sO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgIFwidHlwZVwiOiBcIkNvbnRlbnRcIlxyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGUgPSB0aGlzLmNvbnRlbnRUeXBlO1xyXG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGVMYWJlbCA9IHRoaXMuY29udGVudFR5cGVMYWJlbDtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlQ2xhc3MgPSB0aGlzLmNvbnRlbnRUeXBlQ2xhc3M7XHJcbiAgICAgICAgICAgIHJlc3VsdC5odG1sID0gdGhpcy5odG1sO1xyXG4gICAgICAgICAgICByZXN1bHQuaGFzRWRpdG9yID0gaGFzRWRpdG9yO1xyXG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0SHRtbChodG1sKTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkNvbnRlbnQuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkNvbnRlbnQoXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZSxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVMYWJlbCxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbCxcclxuICAgICAgICAgICAgdmFsdWUuaGFzRWRpdG9yLFxyXG4gICAgICAgICAgICB2YWx1ZS5ydWxlKTtcclxuXHJcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uICgkLCBMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuSHRtbCA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgY29udGVudFR5cGUsIGNvbnRlbnRUeXBlTGFiZWwsIGNvbnRlbnRUeXBlQ2xhc3MsIGh0bWwsIGhhc0VkaXRvciwgcnVsZSkge1xyXG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJIdG1sXCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBydWxlKTtcclxuXHJcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZSA9IGNvbnRlbnRUeXBlO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGVMYWJlbCA9IGNvbnRlbnRUeXBlTGFiZWw7XHJcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUNsYXNzID0gY29udGVudFR5cGVDbGFzcztcclxuICAgICAgICB0aGlzLmh0bWwgPSBodG1sO1xyXG4gICAgICAgIHRoaXMuaGFzRWRpdG9yID0gaGFzRWRpdG9yO1xyXG4gICAgICAgIHRoaXMuaXNDb250YWluYWJsZSA9IHRydWU7XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SW5uZXJUZXh0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gJCgkLnBhcnNlSFRNTChcIjxkaXY+XCIgKyB0aGlzLmh0bWwgKyBcIjwvZGl2PlwiKSkudGV4dCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIC8vIFRoaXMgZnVuY3Rpb24gd2lsbCBiZSBvdmVyd3JpdHRlbiBieSB0aGUgQ29udGVudCBkaXJlY3RpdmUuXHJcbiAgICAgICAgdGhpcy5zZXRIdG1sID0gZnVuY3Rpb24gKGh0bWwpIHtcclxuICAgICAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcclxuICAgICAgICAgICAgdGhpcy5odG1sVW5zYWZlID0gaHRtbDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICBcInR5cGVcIjogXCJIdG1sXCJcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlID0gdGhpcy5jb250ZW50VHlwZTtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlTGFiZWwgPSB0aGlzLmNvbnRlbnRUeXBlTGFiZWw7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZUNsYXNzID0gdGhpcy5jb250ZW50VHlwZUNsYXNzO1xyXG4gICAgICAgICAgICByZXN1bHQuaHRtbCA9IHRoaXMuaHRtbDtcclxuICAgICAgICAgICAgcmVzdWx0Lmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB2YXIgZ2V0RWRpdG9yT2JqZWN0ID0gdGhpcy5nZXRFZGl0b3JPYmplY3Q7XHJcbiAgICAgICAgdGhpcy5nZXRFZGl0b3JPYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciBkdG8gPSBnZXRFZGl0b3JPYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuICQuZXh0ZW5kKGR0bywge1xyXG4gICAgICAgICAgICAgICAgQ29udGVudDogdGhpcy5odG1sXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy5zZXRIdG1sKGh0bWwpO1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuSHRtbC5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuSHRtbChcclxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXHJcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZUxhYmVsLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZUNsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sLFxyXG4gICAgICAgICAgICB2YWx1ZS5oYXNFZGl0b3IsXHJcbiAgICAgICAgICAgIHZhbHVlLnJ1bGUpO1xyXG5cclxuICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IucmVnaXN0ZXJGYWN0b3J5KFwiSHRtbFwiLCBmdW5jdGlvbih2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkh0bWwuZnJvbSh2YWx1ZSk7IH0pO1xyXG5cclxufSkoalF1ZXJ5LCBMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9