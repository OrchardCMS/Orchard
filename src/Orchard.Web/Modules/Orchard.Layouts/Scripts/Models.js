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

    LayoutEditor.Element = function (type, data, htmlId, htmlClass, htmlStyle, isTemplated) {
        if (!type)
            throw new Error("Parameter 'type' is required.");

        this.type = type;
        this.data = data;
        this.htmlId = htmlId;
        this.htmlClass = htmlClass;
        this.htmlStyle = htmlStyle;
        this.isTemplated = isTemplated;

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
                isTemplated: this.isTemplated
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

    LayoutEditor.Canvas = function (data, htmlId, htmlClass, htmlStyle, isTemplated, children) {
        LayoutEditor.Element.call(this, "Canvas", data, htmlId, htmlClass, htmlStyle, isTemplated);
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
            LayoutEditor.childrenFrom(value.children));
    };

})(LayoutEditor || (LayoutEditor = {}));

var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Grid = function (data, htmlId, htmlClass, htmlStyle, isTemplated, children) {
        LayoutEditor.Element.call(this, "Grid", data, htmlId, htmlClass, htmlStyle, isTemplated);
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
            LayoutEditor.childrenFrom(value.children));
        result.toolboxIcon = value.toolboxIcon;
        result.toolboxLabel = value.toolboxLabel;
        result.toolboxDescription = value.toolboxDescription;
        return result;
    };

})(LayoutEditor || (LayoutEditor = {}));
var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Row = function (data, htmlId, htmlClass, htmlStyle, isTemplated, children) {
        LayoutEditor.Element.call(this, "Row", data, htmlId, htmlClass, htmlStyle, isTemplated);
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
            LayoutEditor.childrenFrom(value.children));
        result.toolboxIcon = value.toolboxIcon;
        result.toolboxLabel = value.toolboxLabel;
        result.toolboxDescription = value.toolboxDescription;
        return result;
    };

})(LayoutEditor || (LayoutEditor = {}));
var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Column = function (data, htmlId, htmlClass, htmlStyle, isTemplated, width, offset, children) {
        LayoutEditor.Element.call(this, "Column", data, htmlId, htmlClass, htmlStyle, isTemplated);
        LayoutEditor.Container.call(this, ["Grid", "Content"], children);

        this.width = width;
        this.offset = offset;

        var _hasPendingChange = false;
        var _origWidth = 0;
        var _origOffset = 0;

        this.beginChange = function () {
            if (!!_hasPendingChange)
                throw new Error("Column already has a pending change.")
            _hasPendingChange = true;
            _origWidth = this.width;
            _origOffset = this.offset;
        };

        this.commitChange = function () {
            if (!_hasPendingChange)
                throw new Error("Column has no pending change.")
            _origWidth = 0;
            _origOffset = 0;
            _hasPendingChange = false;
        };

        this.rollbackChange = function () {
            if (!_hasPendingChange)
                throw new Error("Column has no pending change.")
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
                children: []
            });
        });
    };

})(LayoutEditor || (LayoutEditor = {}));
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
var LayoutEditor;
(function ($, LayoutEditor) {

    LayoutEditor.Html = function (data, htmlId, htmlClass, htmlStyle, isTemplated, contentType, contentTypeLabel, contentTypeClass, html, hasEditor) {
        LayoutEditor.Element.call(this, "Html", data, htmlId, htmlClass, htmlStyle, isTemplated);

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
            value.hasEditor);

        return result;
    };

    LayoutEditor.registerFactory("Html", function(value) { return LayoutEditor.Html.from(value); });

})(jQuery, LayoutEditor || (LayoutEditor = {}));
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIkhlbHBlcnMuanMiLCJFZGl0b3IuanMiLCJFbGVtZW50LmpzIiwiQ29udGFpbmVyLmpzIiwiQ2FudmFzLmpzIiwiR3JpZC5qcyIsIlJvdy5qcyIsIkNvbHVtbi5qcyIsIkNvbnRlbnQuanMiLCJIdG1sLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3pDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNoQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDdExBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDdklBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN6QkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzVCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM1UkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN2SUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN6REE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6Ik1vZGVscy5qcyIsInNvdXJjZXNDb250ZW50IjpbInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XHJcblxyXG4gICAgQXJyYXkucHJvdG90eXBlLm1vdmUgPSBmdW5jdGlvbiAoZnJvbSwgdG8pIHtcclxuICAgICAgICB0aGlzLnNwbGljZSh0bywgMCwgdGhpcy5zcGxpY2UoZnJvbSwgMSlbMF0pO1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tID0gZnVuY3Rpb24odmFsdWVzKSB7XHJcbiAgICAgICAgcmV0dXJuIF8odmFsdWVzKS5tYXAoZnVuY3Rpb24odmFsdWUpIHtcclxuICAgICAgICAgICAgcmV0dXJuIExheW91dEVkaXRvci5lbGVtZW50RnJvbSh2YWx1ZSk7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9O1xyXG5cclxuICAgIHZhciByZWdpc3RlckZhY3RvcnkgPSBMYXlvdXRFZGl0b3IucmVnaXN0ZXJGYWN0b3J5ID0gZnVuY3Rpb24odHlwZSwgZmFjdG9yeSkge1xyXG4gICAgICAgIHZhciBmYWN0b3JpZXMgPSBMYXlvdXRFZGl0b3IuZmFjdG9yaWVzID0gTGF5b3V0RWRpdG9yLmZhY3RvcmllcyB8fCB7fTtcclxuICAgICAgICBmYWN0b3JpZXNbdHlwZV0gPSBmYWN0b3J5O1xyXG4gICAgfTtcclxuXHJcbiAgICByZWdpc3RlckZhY3RvcnkoXCJHcmlkXCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuR3JpZC5mcm9tKHZhbHVlKTsgfSk7XHJcbiAgICByZWdpc3RlckZhY3RvcnkoXCJSb3dcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5Sb3cuZnJvbSh2YWx1ZSk7IH0pO1xyXG4gICAgcmVnaXN0ZXJGYWN0b3J5KFwiQ29sdW1uXCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuQ29sdW1uLmZyb20odmFsdWUpOyB9KTtcclxuICAgIHJlZ2lzdGVyRmFjdG9yeShcIkNvbnRlbnRcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5Db250ZW50LmZyb20odmFsdWUpOyB9KTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuZWxlbWVudEZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICB2YXIgZmFjdG9yeSA9IExheW91dEVkaXRvci5mYWN0b3JpZXNbdmFsdWUudHlwZV07XHJcblxyXG4gICAgICAgIGlmICghZmFjdG9yeSlcclxuICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiTm8gZWxlbWVudCB3aXRoIHR5cGUgXFxcIlwiICsgdmFsdWUudHlwZSArIFwiXFxcIiB3YXMgZm91bmQuXCIpO1xyXG5cclxuICAgICAgICB2YXIgZWxlbWVudCA9IGZhY3RvcnkodmFsdWUpO1xyXG4gICAgICAgIHJldHVybiBlbGVtZW50O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3Iuc2V0TW9kZWwgPSBmdW5jdGlvbiAoZWxlbWVudFNlbGVjdG9yLCBtb2RlbCkge1xyXG4gICAgICAgICQoZWxlbWVudFNlbGVjdG9yKS5zY29wZSgpLmVsZW1lbnQgPSBtb2RlbDtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLmdldE1vZGVsID0gZnVuY3Rpb24gKGVsZW1lbnRTZWxlY3Rvcikge1xyXG4gICAgICAgIHJldHVybiAkKGVsZW1lbnRTZWxlY3Rvcikuc2NvcGUoKS5lbGVtZW50O1xyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcblxuICAgIExheW91dEVkaXRvci5FZGl0b3IgPSBmdW5jdGlvbiAoY29uZmlnLCBjYW52YXNEYXRhKSB7XG4gICAgICAgIHRoaXMuY29uZmlnID0gY29uZmlnO1xuICAgICAgICB0aGlzLmNhbnZhcyA9IExheW91dEVkaXRvci5DYW52YXMuZnJvbShjYW52YXNEYXRhKTtcbiAgICAgICAgdGhpcy5pbml0aWFsU3RhdGUgPSBKU09OLnN0cmluZ2lmeSh0aGlzLmNhbnZhcy50b09iamVjdCgpKTtcbiAgICAgICAgdGhpcy5hY3RpdmVFbGVtZW50ID0gbnVsbDtcbiAgICAgICAgdGhpcy5mb2N1c2VkRWxlbWVudCA9IG51bGw7XG4gICAgICAgIHRoaXMuZHJvcFRhcmdldEVsZW1lbnQgPSBudWxsO1xuICAgICAgICB0aGlzLmlzRHJhZ2dpbmcgPSBmYWxzZTtcbiAgICAgICAgdGhpcy5pbmxpbmVFZGl0aW5nSXNBY3RpdmUgPSBmYWxzZTtcbiAgICAgICAgdGhpcy5pc1Jlc2l6aW5nID0gZmFsc2U7XG5cbiAgICAgICAgdGhpcy5yZXNldFRvb2xib3hFbGVtZW50cyA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHRoaXMudG9vbGJveEVsZW1lbnRzID0gW1xuICAgICAgICAgICAgICAgIExheW91dEVkaXRvci5Sb3cuZnJvbSh7XG4gICAgICAgICAgICAgICAgICAgIGNoaWxkcmVuOiBbXVxuICAgICAgICAgICAgICAgIH0pXG4gICAgICAgICAgICBdO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuaXNEaXJ0eSA9IGZ1bmN0aW9uKCkge1xuICAgICAgICAgICAgdmFyIGN1cnJlbnRTdGF0ZSA9IEpTT04uc3RyaW5naWZ5KHRoaXMuY2FudmFzLnRvT2JqZWN0KCkpO1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMuaW5pdGlhbFN0YXRlICE9IGN1cnJlbnRTdGF0ZTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnJlc2V0VG9vbGJveEVsZW1lbnRzKCk7XG4gICAgICAgIHRoaXMuY2FudmFzLnNldEVkaXRvcih0aGlzKTtcbiAgICB9O1xuXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7XG4iLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5FbGVtZW50ID0gZnVuY3Rpb24gKHR5cGUsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkKSB7XHJcbiAgICAgICAgaWYgKCF0eXBlKVxyXG4gICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJQYXJhbWV0ZXIgJ3R5cGUnIGlzIHJlcXVpcmVkLlwiKTtcclxuXHJcbiAgICAgICAgdGhpcy50eXBlID0gdHlwZTtcclxuICAgICAgICB0aGlzLmRhdGEgPSBkYXRhO1xyXG4gICAgICAgIHRoaXMuaHRtbElkID0gaHRtbElkO1xyXG4gICAgICAgIHRoaXMuaHRtbENsYXNzID0gaHRtbENsYXNzO1xyXG4gICAgICAgIHRoaXMuaHRtbFN0eWxlID0gaHRtbFN0eWxlO1xyXG4gICAgICAgIHRoaXMuaXNUZW1wbGF0ZWQgPSBpc1RlbXBsYXRlZDtcclxuXHJcbiAgICAgICAgdGhpcy5lZGl0b3IgPSBudWxsO1xyXG4gICAgICAgIHRoaXMucGFyZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLnNldElzRm9jdXNlZEV2ZW50SGFuZGxlcnMgPSBbXTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXRFZGl0b3IgPSBmdW5jdGlvbiAoZWRpdG9yKSB7XHJcbiAgICAgICAgICAgIHRoaXMuZWRpdG9yID0gZWRpdG9yO1xyXG4gICAgICAgICAgICBpZiAoISF0aGlzLmNoaWxkcmVuICYmIF8uaXNBcnJheSh0aGlzLmNoaWxkcmVuKSkge1xyXG4gICAgICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcihlZGl0b3IpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNldFBhcmVudCA9IGZ1bmN0aW9uKHBhcmVudEVsZW1lbnQpIHtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQgPSBwYXJlbnRFbGVtZW50O1xyXG4gICAgICAgICAgICB0aGlzLnBhcmVudC5vbkNoaWxkQWRkZWQodGhpcyk7XHJcblxyXG4gICAgICAgICAgICB2YXIgY3VycmVudEFuY2VzdG9yID0gcGFyZW50RWxlbWVudDtcclxuICAgICAgICAgICAgd2hpbGUgKCEhY3VycmVudEFuY2VzdG9yKSB7XHJcbiAgICAgICAgICAgICAgICBjdXJyZW50QW5jZXN0b3Iub25EZXNjZW5kYW50QWRkZWQodGhpcywgcGFyZW50RWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICBjdXJyZW50QW5jZXN0b3IgPSBjdXJyZW50QW5jZXN0b3IucGFyZW50O1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXRJc1RlbXBsYXRlZCA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgICAgICB0aGlzLmlzVGVtcGxhdGVkID0gdmFsdWU7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMuY2hpbGRyZW4gJiYgXy5pc0FycmF5KHRoaXMuY2hpbGRyZW4pKSB7XHJcbiAgICAgICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY2hpbGQuc2V0SXNUZW1wbGF0ZWQodmFsdWUpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmFwcGx5RWxlbWVudEVkaXRvck1vZGVsID0gZnVuY3Rpb24oKSB7IC8qIFZpcnR1YWwgKi8gfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJc0FjdGl2ZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcclxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPT09IHRoaXMgJiYgIXRoaXMuZ2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXRJc0FjdGl2ZSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5lZGl0b3IuaXNEcmFnZ2luZyB8fCB0aGlzLmVkaXRvci5pbmxpbmVFZGl0aW5nSXNBY3RpdmUgfHwgdGhpcy5lZGl0b3IuaXNSZXNpemluZylcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIGlmICh2YWx1ZSlcclxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSB0aGlzO1xyXG4gICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5hY3RpdmVFbGVtZW50ID0gdGhpcy5wYXJlbnQ7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJc0ZvY3VzZWQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmVkaXRvci5mb2N1c2VkRWxlbWVudCA9PT0gdGhpcztcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNldElzRm9jdXNlZCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcclxuICAgICAgICAgICAgXHRyZXR1cm47XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmlzVGVtcGxhdGVkKVxyXG4gICAgICAgICAgICBcdHJldHVybjtcclxuICAgICAgICAgICAgaWYgKHRoaXMuZWRpdG9yLmlzRHJhZ2dpbmcgfHwgdGhpcy5lZGl0b3IuaW5saW5lRWRpdGluZ0lzQWN0aXZlIHx8IHRoaXMuZWRpdG9yLmlzUmVzaXppbmcpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICB0aGlzLmVkaXRvci5mb2N1c2VkRWxlbWVudCA9IHRoaXM7XHJcbiAgICAgICAgICAgIF8odGhpcy5zZXRJc0ZvY3VzZWRFdmVudEhhbmRsZXJzKS5lYWNoKGZ1bmN0aW9uIChpdGVtKSB7XHJcbiAgICAgICAgICAgICAgICB0cnkge1xyXG4gICAgICAgICAgICAgICAgICAgIGl0ZW0oKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGNhdGNoIChleCkge1xyXG4gICAgICAgICAgICAgICAgICAgIC8vIElnbm9yZS5cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJc1NlbGVjdGVkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5nZXRJc0ZvY3VzZWQoKSlcclxuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xyXG5cclxuICAgICAgICAgICAgaWYgKCEhdGhpcy5jaGlsZHJlbiAmJiBfLmlzQXJyYXkodGhpcy5jaGlsZHJlbikpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBfKHRoaXMuY2hpbGRyZW4pLmFueShmdW5jdGlvbihjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjaGlsZC5nZXRJc1NlbGVjdGVkKCk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SXNEcm9wVGFyZ2V0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5lZGl0b3IuZHJvcFRhcmdldEVsZW1lbnQgPT09IHRoaXM7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnNldElzRHJvcFRhcmdldCA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICBpZiAodmFsdWUpXHJcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5kcm9wVGFyZ2V0RWxlbWVudCA9IHRoaXM7XHJcbiAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmRyb3BUYXJnZXRFbGVtZW50ID0gbnVsbDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmRlbGV0ZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCEhdGhpcy5wYXJlbnQpXHJcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5kZWxldGVDaGlsZCh0aGlzKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbk1vdmVVcCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLnBhcmVudClcclxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbk1vdmVDaGlsZFVwKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZVVwID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoISF0aGlzLnBhcmVudClcclxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50Lm1vdmVDaGlsZFVwKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuTW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5wYXJlbnQpXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5Nb3ZlQ2hpbGREb3duKHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQubW92ZUNoaWxkRG93bih0aGlzKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmVsZW1lbnRUb09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgIHR5cGU6IHRoaXMudHlwZSxcclxuICAgICAgICAgICAgICAgIGRhdGE6IHRoaXMuZGF0YSxcclxuICAgICAgICAgICAgICAgIGh0bWxJZDogdGhpcy5odG1sSWQsXHJcbiAgICAgICAgICAgICAgICBodG1sQ2xhc3M6IHRoaXMuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICAgICAgaHRtbFN0eWxlOiB0aGlzLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgICAgIGlzVGVtcGxhdGVkOiB0aGlzLmlzVGVtcGxhdGVkXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRFZGl0b3JPYmplY3QgPSBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHt9O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29weSA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XHJcbiAgICAgICAgICAgIHZhciB0ZXh0ID0gdGhpcy5nZXRJbm5lclRleHQoKTtcclxuICAgICAgICAgICAgY2xpcGJvYXJkRGF0YS5zZXREYXRhKFwidGV4dC9wbGFpblwiLCB0ZXh0KTtcclxuXHJcbiAgICAgICAgICAgIHZhciBkYXRhID0gdGhpcy50b09iamVjdCgpO1xyXG4gICAgICAgICAgICB2YXIganNvbiA9IEpTT04uc3RyaW5naWZ5KGRhdGEsIG51bGwsIFwiXFx0XCIpO1xyXG4gICAgICAgICAgICBjbGlwYm9hcmREYXRhLnNldERhdGEoXCJ0ZXh0L2pzb25cIiwganNvbik7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jdXQgPSBmdW5jdGlvbiAoY2xpcGJvYXJkRGF0YSkge1xyXG4gICAgICAgICAgICB0aGlzLmNvcHkoY2xpcGJvYXJkRGF0YSk7XHJcbiAgICAgICAgICAgIHRoaXMuZGVsZXRlKCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5wYXN0ZSA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XHJcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGUoY2xpcGJvYXJkRGF0YSk7XHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyID0gZnVuY3Rpb24gKGFsbG93ZWRDaGlsZFR5cGVzLCBjaGlsZHJlbikge1xyXG5cclxuICAgICAgICB0aGlzLmFsbG93ZWRDaGlsZFR5cGVzID0gYWxsb3dlZENoaWxkVHlwZXM7XHJcbiAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xyXG4gICAgICAgIHRoaXMuaXNDb250YWluZXIgPSB0cnVlO1xyXG5cclxuICAgICAgICB2YXIgc2VsZiA9IHRoaXM7XHJcblxyXG4gICAgICAgIHRoaXMub25DaGlsZEFkZGVkID0gZnVuY3Rpb24gKGVsZW1lbnQpIHsgLyogVmlydHVhbCAqLyB9O1xyXG4gICAgICAgIHRoaXMub25EZXNjZW5kYW50QWRkZWQgPSBmdW5jdGlvbiAoZWxlbWVudCwgcGFyZW50RWxlbWVudCkgeyAvKiBWaXJ0dWFsICovIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4gPSBmdW5jdGlvbiAoY2hpbGRyZW4pIHtcclxuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5zZXRQYXJlbnQoc2VsZik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4oY2hpbGRyZW4pO1xyXG5cclxuICAgICAgICB0aGlzLmdldElzU2VhbGVkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5hbnkoZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gY2hpbGQuaXNUZW1wbGF0ZWQ7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuYWRkQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgaWYgKCFfKHRoaXMuY2hpbGRyZW4pLmNvbnRhaW5zKGNoaWxkKSAmJiAoXyh0aGlzLmFsbG93ZWRDaGlsZFR5cGVzKS5jb250YWlucyhjaGlsZC50eXBlKSB8fCBjaGlsZC5pc0NvbnRhaW5hYmxlKSlcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4ucHVzaChjaGlsZCk7XHJcbiAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XHJcbiAgICAgICAgICAgIGNoaWxkLnNldElzVGVtcGxhdGVkKGZhbHNlKTtcclxuICAgICAgICAgICAgY2hpbGQuc2V0UGFyZW50KHRoaXMpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZGVsZXRlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4uc3BsaWNlKGluZGV4LCAxKTtcclxuICAgICAgICAgICAgICAgIGlmIChjaGlsZC5nZXRJc0FjdGl2ZSgpKVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzRm9jdXNlZCgpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gSWYgdGhlIGRlbGV0ZWQgY2hpbGQgd2FzIGZvY3VzZWQsIHRyeSB0byBzZXQgbmV3IGZvY3VzIHRvIHRoZSBtb3N0IGFwcHJvcHJpYXRlIHNpYmxpbmcgb3IgcGFyZW50LlxyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuW2luZGV4XS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlIGlmIChpbmRleCA+IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuc2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLm1vdmVGb2N1c1ByZXZDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPCAyKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPiAwKVxyXG4gICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdLnNldElzRm9jdXNlZCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMubW92ZUZvY3VzTmV4dENoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA8IDIpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA8IHRoaXMuY2hpbGRyZW4ubGVuZ3RoIC0gMSlcclxuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW5baW5kZXggKyAxXS5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmluc2VydENoaWxkID0gZnVuY3Rpb24gKGNoaWxkLCBhZnRlckNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICghXyh0aGlzLmNoaWxkcmVuKS5jb250YWlucyhjaGlsZCkpIHtcclxuICAgICAgICAgICAgICAgIHZhciBpbmRleCA9IE1hdGgubWF4KF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihhZnRlckNoaWxkKSwgMCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuLnNwbGljZShpbmRleCArIDEsIDAsIGNoaWxkKTtcclxuICAgICAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5wYXJlbnQgPSB0aGlzO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5tb3ZlQ2hpbGRVcCA9IGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuTW92ZUNoaWxkVXAoY2hpbGQpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xyXG4gICAgICAgICAgICB0aGlzLmNoaWxkcmVuLm1vdmUoaW5kZXgsIGluZGV4IC0gMSk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5tb3ZlQ2hpbGREb3duID0gZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Nb3ZlQ2hpbGREb3duKGNoaWxkKSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbi5tb3ZlKGluZGV4LCBpbmRleCArIDEpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuTW92ZUNoaWxkVXAgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgcmV0dXJuIGluZGV4ID4gMDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbk1vdmVDaGlsZERvd24gPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcclxuICAgICAgICAgICAgcmV0dXJuIGluZGV4IDwgdGhpcy5jaGlsZHJlbi5sZW5ndGggLSAxO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2hpbGRyZW5Ub09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikubWFwKGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNoaWxkLnRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SW5uZXJUZXh0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5yZWR1Y2UoZnVuY3Rpb24gKG1lbW8sIGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gbWVtbyArIFwiXFxuXCIgKyBjaGlsZC5nZXRJbm5lclRleHQoKTtcclxuICAgICAgICAgICAgfSwgXCJcIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnBhc3RlID0gZnVuY3Rpb24gKGNsaXBib2FyZERhdGEpIHtcclxuICAgICAgICAgICAgdmFyIGpzb24gPSBjbGlwYm9hcmREYXRhLmdldERhdGEoXCJ0ZXh0L2pzb25cIik7XHJcbiAgICAgICAgICAgIGlmICghIWpzb24pIHtcclxuICAgICAgICAgICAgICAgIHZhciBkYXRhID0gSlNPTi5wYXJzZShqc29uKTtcclxuICAgICAgICAgICAgICAgIHZhciBjaGlsZCA9IExheW91dEVkaXRvci5lbGVtZW50RnJvbShkYXRhKTtcclxuICAgICAgICAgICAgICAgIHRoaXMucGFzdGVDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnBhc3RlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgaWYgKF8odGhpcy5hbGxvd2VkQ2hpbGRUeXBlcykuY29udGFpbnMoY2hpbGQudHlwZSkgfHwgY2hpbGQuaXNDb250YWluYWJsZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5hZGRDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGVDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5DYW52YXMgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNoaWxkcmVuKSB7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkNhbnZhc1wiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIkdyaWRcIiwgXCJDb250ZW50XCJdLCBjaGlsZHJlbik7XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuQ2FudmFzLmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICByZXR1cm4gbmV3IExheW91dEVkaXRvci5DYW52YXMoXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcbiAgICB9O1xyXG5cclxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpO1xyXG4iLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5HcmlkID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBjaGlsZHJlbikge1xyXG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJHcmlkXCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkKTtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiUm93XCJdLCBjaGlsZHJlbik7XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuR3JpZC5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuR3JpZChcclxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXHJcbiAgICAgICAgICAgIExheW91dEVkaXRvci5jaGlsZHJlbkZyb20odmFsdWUuY2hpbGRyZW4pKTtcclxuICAgICAgICByZXN1bHQudG9vbGJveEljb24gPSB2YWx1ZS50b29sYm94SWNvbjtcclxuICAgICAgICByZXN1bHQudG9vbGJveExhYmVsID0gdmFsdWUudG9vbGJveExhYmVsO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94RGVzY3JpcHRpb24gPSB2YWx1ZS50b29sYm94RGVzY3JpcHRpb247XHJcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuUm93ID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBjaGlsZHJlbikge1xyXG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJSb3dcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpO1xyXG4gICAgICAgIExheW91dEVkaXRvci5Db250YWluZXIuY2FsbCh0aGlzLCBbXCJDb2x1bW5cIl0sIGNoaWxkcmVuKTtcclxuXHJcbiAgICAgICAgdmFyIF9zZWxmID0gdGhpcztcclxuXHJcbiAgICAgICAgZnVuY3Rpb24gX2dldFRvdGFsQ29sdW1uc1dpZHRoKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gXyhfc2VsZi5jaGlsZHJlbikucmVkdWNlKGZ1bmN0aW9uIChtZW1vLCBjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIG1lbW8gKyBjaGlsZC5vZmZzZXQgKyBjaGlsZC53aWR0aDtcclxuICAgICAgICAgICAgfSwgMCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAvLyBJbXBsZW1lbnRzIGEgc2ltcGxlIGFsZ29yaXRobSB0byBkaXN0cmlidXRlIHNwYWNlIChlaXRoZXIgcG9zaXRpdmUgb3IgbmVnYXRpdmUpXHJcbiAgICAgICAgLy8gYmV0d2VlbiB0aGUgY2hpbGQgY29sdW1ucyBvZiB0aGUgcm93LiBOZWdhdGl2ZSBzcGFjZSBpcyBkaXN0cmlidXRlZCB3aGVuIG1ha2luZ1xyXG4gICAgICAgIC8vIHJvb20gZm9yIGEgbmV3IGNvbHVtbiAoZS5jLiBjbGlwYm9hcmQgcGFzdGUgb3IgZHJvcHBpbmcgZnJvbSB0aGUgdG9vbGJveCkgd2hpbGVcclxuICAgICAgICAvLyBwb3NpdGl2ZSBzcGFjZSBpcyBkaXN0cmlidXRlZCB3aGVuIGZpbGxpbmcgdGhlIGdyYXAgb2YgYSByZW1vdmVkIGNvbHVtbi5cclxuICAgICAgICBmdW5jdGlvbiBfZGlzdHJpYnV0ZVNwYWNlKHNwYWNlKSB7XHJcbiAgICAgICAgICAgIGlmIChzcGFjZSA9PSAwKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICAgICAgICAgICBcclxuICAgICAgICAgICAgdmFyIHVuZGlzdHJpYnV0ZWRTcGFjZSA9IHNwYWNlO1xyXG5cclxuICAgICAgICAgICAgaWYgKHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDApIHtcclxuICAgICAgICAgICAgICAgIHZhciB2YWNhbnRTcGFjZSA9IDEyIC0gX2dldFRvdGFsQ29sdW1uc1dpZHRoKCk7XHJcbiAgICAgICAgICAgICAgICB1bmRpc3RyaWJ1dGVkU3BhY2UgKz0gdmFjYW50U3BhY2U7XHJcbiAgICAgICAgICAgICAgICBpZiAodW5kaXN0cmlidXRlZFNwYWNlID4gMClcclxuICAgICAgICAgICAgICAgICAgICB1bmRpc3RyaWJ1dGVkU3BhY2UgPSAwO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAvLyBJZiBzcGFjZSBpcyBuZWdhdGl2ZSwgdHJ5IHRvIGRlY3JlYXNlIG9mZnNldHMgZmlyc3QuXHJcbiAgICAgICAgICAgIHdoaWxlICh1bmRpc3RyaWJ1dGVkU3BhY2UgPCAwICYmIF8oX3NlbGYuY2hpbGRyZW4pLmFueShmdW5jdGlvbiAoY29sdW1uKSB7IHJldHVybiBjb2x1bW4ub2Zmc2V0ID4gMDsgfSkpIHsgLy8gV2hpbGUgdGhlcmUgaXMgc3RpbGwgb2Zmc2V0IGxlZnQgdG8gcmVtb3ZlLlxyXG4gICAgICAgICAgICAgICAgZm9yIChpID0gMDsgaSA8IF9zZWxmLmNoaWxkcmVuLmxlbmd0aCAmJiB1bmRpc3RyaWJ1dGVkU3BhY2UgPCAwOyBpKyspIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgY29sdW1uID0gX3NlbGYuY2hpbGRyZW5baV07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGNvbHVtbi5vZmZzZXQgPiAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQtLTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdW5kaXN0cmlidXRlZFNwYWNlKys7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBmdW5jdGlvbiBoYXNXaWR0aChjb2x1bW4pIHtcclxuICAgICAgICAgICAgICAgIGlmICh1bmRpc3RyaWJ1dGVkU3BhY2UgPiAwKVxyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ud2lkdGggPCAxMjtcclxuICAgICAgICAgICAgICAgIGVsc2UgaWYgKHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDApXHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA+IDE7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIC8vIFRyeSB0byBkaXN0cmlidXRlIHJlbWFpbmluZyBzcGFjZSAoY291bGQgYmUgbmVnYXRpdmUgb3IgcG9zaXRpdmUpIHVzaW5nIHdpZHRocy5cclxuICAgICAgICAgICAgd2hpbGUgKHVuZGlzdHJpYnV0ZWRTcGFjZSAhPSAwKSB7XHJcbiAgICAgICAgICAgICAgICAvLyBBbnkgbW9yZSBjb2x1bW4gd2lkdGggYXZhaWxhYmxlIGZvciBkaXN0cmlidXRpb24/XHJcbiAgICAgICAgICAgICAgICBpZiAoIV8oX3NlbGYuY2hpbGRyZW4pLmFueShoYXNXaWR0aCkpXHJcbiAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICBmb3IgKGkgPSAwOyBpIDwgX3NlbGYuY2hpbGRyZW4ubGVuZ3RoICYmIHVuZGlzdHJpYnV0ZWRTcGFjZSAhPSAwOyBpKyspIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgY29sdW1uID0gX3NlbGYuY2hpbGRyZW5baSAlIF9zZWxmLmNoaWxkcmVuLmxlbmd0aF07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGhhc1dpZHRoKGNvbHVtbikpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGRlbHRhID0gdW5kaXN0cmlidXRlZFNwYWNlIC8gTWF0aC5hYnModW5kaXN0cmlidXRlZFNwYWNlKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY29sdW1uLndpZHRoICs9IGRlbHRhO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB1bmRpc3RyaWJ1dGVkU3BhY2UgLT0gZGVsdGE7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSAgICAgICAgICAgICAgICBcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHVuZGlzdHJpYnV0ZWRTcGFjZSA9PSAwO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIF9pc0FkZGluZ0NvbHVtbiA9IGZhbHNlO1xyXG5cclxuICAgICAgICB0aGlzLmNhbkFkZENvbHVtbiA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuY2hpbGRyZW4ubGVuZ3RoIDwgMTI7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5iZWdpbkFkZENvbHVtbiA9IGZ1bmN0aW9uIChuZXdDb2x1bW5XaWR0aCkge1xyXG4gICAgICAgICAgICBpZiAoISFfaXNBZGRpbmdDb2x1bW4pXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gYWRkIG9wZXJhdGlvbiBpcyBhbHJlYWR5IGluIHByb2dyZXNzLlwiKVxyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLmJlZ2luQ2hhbmdlKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBpZiAoX2Rpc3RyaWJ1dGVTcGFjZSgtbmV3Q29sdW1uV2lkdGgpKSB7XHJcbiAgICAgICAgICAgICAgICBfaXNBZGRpbmdDb2x1bW4gPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcclxuICAgICAgICAgICAgICAgIGNvbHVtbi5yb2xsYmFja0NoYW5nZSgpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29tbWl0QWRkQ29sdW1uID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAoIV9pc0FkZGluZ0NvbHVtbilcclxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIk5vIGNvbHVtbiBhZGQgb3BlcmF0aW9uIGluIHByb2dyZXNzLlwiKVxyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLmNvbW1pdENoYW5nZSgpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgX2lzQWRkaW5nQ29sdW1uID0gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5yb2xsYmFja0FkZENvbHVtbiA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCFfaXNBZGRpbmdDb2x1bW4pXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJObyBjb2x1bW4gYWRkIG9wZXJhdGlvbiBpbiBwcm9ncmVzcy5cIilcclxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcclxuICAgICAgICAgICAgICAgIGNvbHVtbi5yb2xsYmFja0NoYW5nZSgpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgX2lzQWRkaW5nQ29sdW1uID0gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdmFyIF9iYXNlRGVsZXRlQ2hpbGQgPSB0aGlzLmRlbGV0ZUNoaWxkO1xyXG4gICAgICAgIHRoaXMuZGVsZXRlQ2hpbGQgPSBmdW5jdGlvbiAoY29sdW1uKSB7IFxyXG4gICAgICAgICAgICB2YXIgd2lkdGggPSBjb2x1bW4ud2lkdGg7XHJcbiAgICAgICAgICAgIF9iYXNlRGVsZXRlQ2hpbGQuY2FsbCh0aGlzLCBjb2x1bW4pO1xyXG4gICAgICAgICAgICBfZGlzdHJpYnV0ZVNwYWNlKHdpZHRoKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkNvbnRyYWN0Q29sdW1uUmlnaHQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA+IDE7XHJcbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNvbnRyYWN0Q29sdW1uUmlnaHQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmNhbkNvbnRyYWN0Q29sdW1uUmlnaHQoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG5cclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoY29sdW1uLndpZHRoID4gMSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aC0tO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4ICsgMSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB2YXIgbmV4dENvbHVtbiA9IHRoaXMuY2hpbGRyZW5baW5kZXggKyAxXTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBuZXh0Q29sdW1uLm9mZnNldCA9PSAwKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbmV4dENvbHVtbi53aWR0aCsrO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBuZXh0Q29sdW1uLm9mZnNldCsrO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuRXhwYW5kQ29sdW1uUmlnaHQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoY29sdW1uLndpZHRoID49IDEyKVxyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4ICsgMSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBuZXh0Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCArIDFdO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgbmV4dENvbHVtbi5vZmZzZXQgPT0gMClcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIG5leHRDb2x1bW4ud2lkdGggPiAxO1xyXG4gICAgICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIG5leHRDb2x1bW4ub2Zmc2V0ID4gMDtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIHJldHVybiBfZ2V0VG90YWxDb2x1bW5zV2lkdGgoKSA8IDEyO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmV4cGFuZENvbHVtblJpZ2h0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5FeHBhbmRDb2x1bW5SaWdodChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4ICsgMSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBuZXh0Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCArIDFdO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgbmV4dENvbHVtbi5vZmZzZXQgPT0gMClcclxuICAgICAgICAgICAgICAgICAgICAgICAgbmV4dENvbHVtbi53aWR0aC0tO1xyXG4gICAgICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICAgICAgbmV4dENvbHVtbi5vZmZzZXQtLTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aCsrO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jYW5FeHBhbmRDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGNvbHVtbi53aWR0aCA+PSAxMilcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICBpZiAoaW5kZXggPiAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHByZXZDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4IC0gMV07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBjb2x1bW4ub2Zmc2V0ID09IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBwcmV2Q29sdW1uLndpZHRoID4gMTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIHJldHVybiBjb2x1bW4ub2Zmc2V0ID4gMDtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5leHBhbmRDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5FeHBhbmRDb2x1bW5MZWZ0KGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSlcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xyXG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGluZGV4ID4gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBwcmV2Q29sdW1uID0gdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgY29sdW1uLm9mZnNldCA9PSAwKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBwcmV2Q29sdW1uLndpZHRoLS07XHJcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0LS07XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldC0tO1xyXG4gICAgICAgICAgICAgICAgY29sdW1uLndpZHRoKys7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkNvbnRyYWN0Q29sdW1uTGVmdCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcclxuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gY29sdW1uLndpZHRoID4gMTtcclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29udHJhY3RDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Db250cmFjdENvbHVtbkxlZnQoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG5cclxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XHJcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoaW5kZXggPiAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHByZXZDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4IC0gMV07XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBjb2x1bW4ub2Zmc2V0ID09IDApXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHByZXZDb2x1bW4ud2lkdGgrKztcclxuICAgICAgICAgICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQrKztcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0Kys7XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGgtLTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuZXZlbkNvbHVtbnMgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA9PSAwKVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG5cclxuICAgICAgICAgICAgdmFyIGV2ZW5XaWR0aCA9IE1hdGguZmxvb3IoMTIgLyB0aGlzLmNoaWxkcmVuLmxlbmd0aCk7XHJcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGggPSBldmVuV2lkdGg7XHJcbiAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0ID0gMDtcclxuICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICB2YXIgcmVzdCA9IDEyICUgdGhpcy5jaGlsZHJlbi5sZW5ndGg7XHJcbiAgICAgICAgICAgIGlmIChyZXN0ID4gMClcclxuICAgICAgICAgICAgICAgIF9kaXN0cmlidXRlU3BhY2UocmVzdCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdmFyIF9iYXNlUGFzdGVDaGlsZCA9IHRoaXMucGFzdGVDaGlsZDtcclxuICAgICAgICB0aGlzLnBhc3RlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgaWYgKGNoaWxkLnR5cGUgPT0gXCJDb2x1bW5cIikge1xyXG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuYmVnaW5BZGRDb2x1bW4oY2hpbGQud2lkdGgpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5jb21taXRBZGRDb2x1bW4oKTtcclxuICAgICAgICAgICAgICAgICAgICBfYmFzZVBhc3RlQ2hpbGQuY2FsbCh0aGlzLCBjaGlsZClcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIGlmICghIXRoaXMucGFyZW50KVxyXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGVDaGlsZChjaGlsZCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmVzdWx0LmNoaWxkcmVuID0gdGhpcy5jaGlsZHJlblRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLlJvdy5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuUm93KFxyXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxyXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcclxuICAgICAgICAgICAgTGF5b3V0RWRpdG9yLmNoaWxkcmVuRnJvbSh2YWx1ZS5jaGlsZHJlbikpO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94SWNvbiA9IHZhbHVlLnRvb2xib3hJY29uO1xyXG4gICAgICAgIHJlc3VsdC50b29sYm94TGFiZWwgPSB2YWx1ZS50b29sYm94TGFiZWw7XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hEZXNjcmlwdGlvbiA9IHZhbHVlLnRvb2xib3hEZXNjcmlwdGlvbjtcclxuICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgfTtcclxuXHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5Db2x1bW4gPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHdpZHRoLCBvZmZzZXQsIGNoaWxkcmVuKSB7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkNvbHVtblwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIkdyaWRcIiwgXCJDb250ZW50XCJdLCBjaGlsZHJlbik7XHJcblxyXG4gICAgICAgIHRoaXMud2lkdGggPSB3aWR0aDtcclxuICAgICAgICB0aGlzLm9mZnNldCA9IG9mZnNldDtcclxuXHJcbiAgICAgICAgdmFyIF9oYXNQZW5kaW5nQ2hhbmdlID0gZmFsc2U7XHJcbiAgICAgICAgdmFyIF9vcmlnV2lkdGggPSAwO1xyXG4gICAgICAgIHZhciBfb3JpZ09mZnNldCA9IDA7XHJcblxyXG4gICAgICAgIHRoaXMuYmVnaW5DaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghIV9oYXNQZW5kaW5nQ2hhbmdlKVxyXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiQ29sdW1uIGFscmVhZHkgaGFzIGEgcGVuZGluZyBjaGFuZ2UuXCIpXHJcbiAgICAgICAgICAgIF9oYXNQZW5kaW5nQ2hhbmdlID0gdHJ1ZTtcclxuICAgICAgICAgICAgX29yaWdXaWR0aCA9IHRoaXMud2lkdGg7XHJcbiAgICAgICAgICAgIF9vcmlnT2Zmc2V0ID0gdGhpcy5vZmZzZXQ7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5jb21taXRDaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghX2hhc1BlbmRpbmdDaGFuZ2UpXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gaGFzIG5vIHBlbmRpbmcgY2hhbmdlLlwiKVxyXG4gICAgICAgICAgICBfb3JpZ1dpZHRoID0gMDtcclxuICAgICAgICAgICAgX29yaWdPZmZzZXQgPSAwO1xyXG4gICAgICAgICAgICBfaGFzUGVuZGluZ0NoYW5nZSA9IGZhbHNlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMucm9sbGJhY2tDaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmICghX2hhc1BlbmRpbmdDaGFuZ2UpXHJcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gaGFzIG5vIHBlbmRpbmcgY2hhbmdlLlwiKVxyXG4gICAgICAgICAgICB0aGlzLndpZHRoID0gX29yaWdXaWR0aDtcclxuICAgICAgICAgICAgdGhpcy5vZmZzZXQgPSBfb3JpZ09mZnNldDtcclxuICAgICAgICAgICAgX29yaWdXaWR0aCA9IDA7XHJcbiAgICAgICAgICAgIF9vcmlnT2Zmc2V0ID0gMDtcclxuICAgICAgICAgICAgX2hhc1BlbmRpbmdDaGFuZ2UgPSBmYWxzZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhblNwbGl0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy53aWR0aCA+IDE7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5zcGxpdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLmNhblNwbGl0KCkpXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICB2YXIgbmV3Q29sdW1uV2lkdGggPSBNYXRoLmZsb29yKHRoaXMud2lkdGggLyAyKTtcclxuICAgICAgICAgICAgdmFyIG5ld0NvbHVtbiA9IExheW91dEVkaXRvci5Db2x1bW4uZnJvbSh7XHJcbiAgICAgICAgICAgICAgICBkYXRhOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaHRtbElkOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaHRtbENsYXNzOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaHRtbFN0eWxlOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgd2lkdGg6IG5ld0NvbHVtbldpZHRoLFxyXG4gICAgICAgICAgICAgICAgb2Zmc2V0OiAwLFxyXG4gICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBcclxuICAgICAgICAgICAgdGhpcy53aWR0aCA9IHRoaXMud2lkdGggLSBuZXdDb2x1bW5XaWR0aDtcclxuICAgICAgICAgICAgdGhpcy5wYXJlbnQuaW5zZXJ0Q2hpbGQobmV3Q29sdW1uLCB0aGlzKTtcclxuICAgICAgICAgICAgbmV3Q29sdW1uLnNldElzRm9jdXNlZCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuQ29udHJhY3RSaWdodCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkNvbnRyYWN0Q29sdW1uUmlnaHQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNvbnRyYWN0UmlnaHQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHRoaXMucGFyZW50LmNvbnRyYWN0Q29sdW1uUmlnaHQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkV4cGFuZFJpZ2h0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5wYXJlbnQuY2FuRXhwYW5kQ29sdW1uUmlnaHQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmV4cGFuZFJpZ2h0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB0aGlzLnBhcmVudC5leHBhbmRDb2x1bW5SaWdodCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY2FuRXhwYW5kTGVmdCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkV4cGFuZENvbHVtbkxlZnQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmV4cGFuZExlZnQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XHJcbiAgICAgICAgICAgIHRoaXMucGFyZW50LmV4cGFuZENvbHVtbkxlZnQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmNhbkNvbnRyYWN0TGVmdCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkNvbnRyYWN0Q29sdW1uTGVmdCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuY29udHJhY3RMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xyXG4gICAgICAgICAgICB0aGlzLnBhcmVudC5jb250cmFjdENvbHVtbkxlZnQodGhpcywgY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmVzdWx0LndpZHRoID0gdGhpcy53aWR0aDtcclxuICAgICAgICAgICAgcmVzdWx0Lm9mZnNldCA9IHRoaXMub2Zmc2V0O1xyXG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuQ29sdW1uLmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICB2YXIgcmVzdWx0ID0gbmV3IExheW91dEVkaXRvci5Db2x1bW4oXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS53aWR0aCxcclxuICAgICAgICAgICAgdmFsdWUub2Zmc2V0LFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hJY29uID0gdmFsdWUudG9vbGJveEljb247XHJcbiAgICAgICAgcmVzdWx0LnRvb2xib3hMYWJlbCA9IHZhbHVlLnRvb2xib3hMYWJlbDtcclxuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xyXG4gICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5Db2x1bW4udGltZXMgPSBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICByZXR1cm4gXy50aW1lcyh2YWx1ZSwgZnVuY3Rpb24gKG4pIHtcclxuICAgICAgICAgICAgcmV0dXJuIExheW91dEVkaXRvci5Db2x1bW4uZnJvbSh7XHJcbiAgICAgICAgICAgICAgICBkYXRhOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaHRtbElkOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaHRtbENsYXNzOiBudWxsLFxyXG4gICAgICAgICAgICAgICAgaXNUZW1wbGF0ZWQ6IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgd2lkdGg6IDEyIC8gdmFsdWUsXHJcbiAgICAgICAgICAgICAgICBvZmZzZXQ6IDAsXHJcbiAgICAgICAgICAgICAgICBjaGlsZHJlbjogW11cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9O1xyXG5cclxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbiAoTGF5b3V0RWRpdG9yKSB7XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkNvbnRlbnQgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNvbnRlbnRUeXBlLCBjb250ZW50VHlwZUxhYmVsLCBjb250ZW50VHlwZUNsYXNzLCBodG1sLCBoYXNFZGl0b3IpIHtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiQ29udGVudFwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XHJcblxyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGUgPSBjb250ZW50VHlwZTtcclxuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlTGFiZWwgPSBjb250ZW50VHlwZUxhYmVsO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGVDbGFzcyA9IGNvbnRlbnRUeXBlQ2xhc3M7XHJcbiAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcclxuICAgICAgICB0aGlzLmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcclxuXHJcbiAgICAgICAgdGhpcy5nZXRJbm5lclRleHQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiAkKCQucGFyc2VIVE1MKFwiPGRpdj5cIiArIHRoaXMuaHRtbCArIFwiPC9kaXY+XCIpKS50ZXh0KCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgLy8gVGhpcyBmdW5jdGlvbiB3aWxsIGJlIG92ZXJ3cml0dGVuIGJ5IHRoZSBDb250ZW50IGRpcmVjdGl2ZS5cclxuICAgICAgICB0aGlzLnNldEh0bWwgPSBmdW5jdGlvbiAoaHRtbCkge1xyXG4gICAgICAgICAgICB0aGlzLmh0bWwgPSBodG1sO1xyXG4gICAgICAgICAgICB0aGlzLmh0bWxVbnNhZmUgPSBodG1sO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgIFwidHlwZVwiOiBcIkNvbnRlbnRcIlxyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciByZXN1bHQgPSB0aGlzLmVsZW1lbnRUb09iamVjdCgpO1xyXG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGUgPSB0aGlzLmNvbnRlbnRUeXBlO1xyXG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGVMYWJlbCA9IHRoaXMuY29udGVudFR5cGVMYWJlbDtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlQ2xhc3MgPSB0aGlzLmNvbnRlbnRUeXBlQ2xhc3M7XHJcbiAgICAgICAgICAgIHJlc3VsdC5odG1sID0gdGhpcy5odG1sO1xyXG4gICAgICAgICAgICByZXN1bHQuaGFzRWRpdG9yID0gaGFzRWRpdG9yO1xyXG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0SHRtbChodG1sKTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkNvbnRlbnQuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkNvbnRlbnQoXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZSxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVMYWJlbCxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbCxcclxuICAgICAgICAgICAgdmFsdWUuaGFzRWRpdG9yKTtcclxuXHJcbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgIH07XHJcblxyXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcclxuKGZ1bmN0aW9uICgkLCBMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuSHRtbCA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgY29udGVudFR5cGUsIGNvbnRlbnRUeXBlTGFiZWwsIGNvbnRlbnRUeXBlQ2xhc3MsIGh0bWwsIGhhc0VkaXRvcikge1xyXG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJIdG1sXCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkKTtcclxuXHJcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZSA9IGNvbnRlbnRUeXBlO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGVMYWJlbCA9IGNvbnRlbnRUeXBlTGFiZWw7XHJcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUNsYXNzID0gY29udGVudFR5cGVDbGFzcztcclxuICAgICAgICB0aGlzLmh0bWwgPSBodG1sO1xyXG4gICAgICAgIHRoaXMuaGFzRWRpdG9yID0gaGFzRWRpdG9yO1xyXG4gICAgICAgIHRoaXMuaXNDb250YWluYWJsZSA9IHRydWU7XHJcblxyXG4gICAgICAgIHRoaXMuZ2V0SW5uZXJUZXh0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gJCgkLnBhcnNlSFRNTChcIjxkaXY+XCIgKyB0aGlzLmh0bWwgKyBcIjwvZGl2PlwiKSkudGV4dCgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIC8vIFRoaXMgZnVuY3Rpb24gd2lsbCBiZSBvdmVyd3JpdHRlbiBieSB0aGUgQ29udGVudCBkaXJlY3RpdmUuXHJcbiAgICAgICAgdGhpcy5zZXRIdG1sID0gZnVuY3Rpb24gKGh0bWwpIHtcclxuICAgICAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcclxuICAgICAgICAgICAgdGhpcy5odG1sVW5zYWZlID0gaHRtbDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICBcInR5cGVcIjogXCJIdG1sXCJcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlID0gdGhpcy5jb250ZW50VHlwZTtcclxuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlTGFiZWwgPSB0aGlzLmNvbnRlbnRUeXBlTGFiZWw7XHJcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZUNsYXNzID0gdGhpcy5jb250ZW50VHlwZUNsYXNzO1xyXG4gICAgICAgICAgICByZXN1bHQuaHRtbCA9IHRoaXMuaHRtbDtcclxuICAgICAgICAgICAgcmVzdWx0Lmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB2YXIgZ2V0RWRpdG9yT2JqZWN0ID0gdGhpcy5nZXRFZGl0b3JPYmplY3Q7XHJcbiAgICAgICAgdGhpcy5nZXRFZGl0b3JPYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciBkdG8gPSBnZXRFZGl0b3JPYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuICQuZXh0ZW5kKGR0bywge1xyXG4gICAgICAgICAgICAgICAgQ29udGVudDogdGhpcy5odG1sXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy5zZXRIdG1sKGh0bWwpO1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuSHRtbC5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuSHRtbChcclxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcclxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXHJcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZUxhYmVsLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZUNsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sLFxyXG4gICAgICAgICAgICB2YWx1ZS5oYXNFZGl0b3IpO1xyXG5cclxuICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IucmVnaXN0ZXJGYWN0b3J5KFwiSHRtbFwiLCBmdW5jdGlvbih2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkh0bWwuZnJvbSh2YWx1ZSk7IH0pO1xyXG5cclxufSkoalF1ZXJ5LCBMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9