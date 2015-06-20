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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIkhlbHBlcnMuanMiLCJFZGl0b3IuanMiLCJFbGVtZW50LmpzIiwiQ29udGFpbmVyLmpzIiwiQ2FudmFzLmpzIiwiR3JpZC5qcyIsIlJvdy5qcyIsIkNvbHVtbi5qcyIsIkNvbnRlbnQuanMiLCJIdG1sLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3pDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNoQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDaExBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDcElBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN6QkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzVCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM1UkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN2SUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUN6REE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6Ik1vZGVscy5qcyIsInNvdXJjZXNDb250ZW50IjpbInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xuXG4gICAgQXJyYXkucHJvdG90eXBlLm1vdmUgPSBmdW5jdGlvbiAoZnJvbSwgdG8pIHtcbiAgICAgICAgdGhpcy5zcGxpY2UodG8sIDAsIHRoaXMuc3BsaWNlKGZyb20sIDEpWzBdKTtcbiAgICB9O1xuXG4gICAgTGF5b3V0RWRpdG9yLmNoaWxkcmVuRnJvbSA9IGZ1bmN0aW9uKHZhbHVlcykge1xuICAgICAgICByZXR1cm4gXyh2YWx1ZXMpLm1hcChmdW5jdGlvbih2YWx1ZSkge1xuICAgICAgICAgICAgcmV0dXJuIExheW91dEVkaXRvci5lbGVtZW50RnJvbSh2YWx1ZSk7XG4gICAgICAgIH0pO1xuICAgIH07XG5cbiAgICB2YXIgcmVnaXN0ZXJGYWN0b3J5ID0gTGF5b3V0RWRpdG9yLnJlZ2lzdGVyRmFjdG9yeSA9IGZ1bmN0aW9uKHR5cGUsIGZhY3RvcnkpIHtcbiAgICAgICAgdmFyIGZhY3RvcmllcyA9IExheW91dEVkaXRvci5mYWN0b3JpZXMgPSBMYXlvdXRFZGl0b3IuZmFjdG9yaWVzIHx8IHt9O1xuICAgICAgICBmYWN0b3JpZXNbdHlwZV0gPSBmYWN0b3J5O1xuICAgIH07XG5cbiAgICByZWdpc3RlckZhY3RvcnkoXCJHcmlkXCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuR3JpZC5mcm9tKHZhbHVlKTsgfSk7XG4gICAgcmVnaXN0ZXJGYWN0b3J5KFwiUm93XCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuUm93LmZyb20odmFsdWUpOyB9KTtcbiAgICByZWdpc3RlckZhY3RvcnkoXCJDb2x1bW5cIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5Db2x1bW4uZnJvbSh2YWx1ZSk7IH0pO1xuICAgIHJlZ2lzdGVyRmFjdG9yeShcIkNvbnRlbnRcIiwgZnVuY3Rpb24odmFsdWUpIHsgcmV0dXJuIExheW91dEVkaXRvci5Db250ZW50LmZyb20odmFsdWUpOyB9KTtcblxuICAgIExheW91dEVkaXRvci5lbGVtZW50RnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xuICAgICAgICB2YXIgZmFjdG9yeSA9IExheW91dEVkaXRvci5mYWN0b3JpZXNbdmFsdWUudHlwZV07XG5cbiAgICAgICAgaWYgKCFmYWN0b3J5KVxuICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiTm8gZWxlbWVudCB3aXRoIHR5cGUgXFxcIlwiICsgdmFsdWUudHlwZSArIFwiXFxcIiB3YXMgZm91bmQuXCIpO1xuXG4gICAgICAgIHZhciBlbGVtZW50ID0gZmFjdG9yeSh2YWx1ZSk7XG4gICAgICAgIHJldHVybiBlbGVtZW50O1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3Iuc2V0TW9kZWwgPSBmdW5jdGlvbiAoZWxlbWVudFNlbGVjdG9yLCBtb2RlbCkge1xuICAgICAgICAkKGVsZW1lbnRTZWxlY3Rvcikuc2NvcGUoKS5lbGVtZW50ID0gbW9kZWw7XG4gICAgfTtcblxuICAgIExheW91dEVkaXRvci5nZXRNb2RlbCA9IGZ1bmN0aW9uIChlbGVtZW50U2VsZWN0b3IpIHtcbiAgICAgICAgcmV0dXJuICQoZWxlbWVudFNlbGVjdG9yKS5zY29wZSgpLmVsZW1lbnQ7XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xuXG4gICAgTGF5b3V0RWRpdG9yLkVkaXRvciA9IGZ1bmN0aW9uIChjb25maWcsIGNhbnZhc0RhdGEpIHtcbiAgICAgICAgdGhpcy5jb25maWcgPSBjb25maWc7XG4gICAgICAgIHRoaXMuY2FudmFzID0gTGF5b3V0RWRpdG9yLkNhbnZhcy5mcm9tKGNhbnZhc0RhdGEpO1xuICAgICAgICB0aGlzLmluaXRpYWxTdGF0ZSA9IEpTT04uc3RyaW5naWZ5KHRoaXMuY2FudmFzLnRvT2JqZWN0KCkpO1xuICAgICAgICB0aGlzLmFjdGl2ZUVsZW1lbnQgPSBudWxsO1xuICAgICAgICB0aGlzLmZvY3VzZWRFbGVtZW50ID0gbnVsbDtcbiAgICAgICAgdGhpcy5kcm9wVGFyZ2V0RWxlbWVudCA9IG51bGw7XG4gICAgICAgIHRoaXMuaXNEcmFnZ2luZyA9IGZhbHNlO1xuICAgICAgICB0aGlzLmlubGluZUVkaXRpbmdJc0FjdGl2ZSA9IGZhbHNlO1xuICAgICAgICB0aGlzLmlzUmVzaXppbmcgPSBmYWxzZTtcblxuICAgICAgICB0aGlzLnJlc2V0VG9vbGJveEVsZW1lbnRzID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdGhpcy50b29sYm94RWxlbWVudHMgPSBbXG4gICAgICAgICAgICAgICAgTGF5b3V0RWRpdG9yLlJvdy5mcm9tKHtcbiAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXG4gICAgICAgICAgICAgICAgfSlcbiAgICAgICAgICAgIF07XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5pc0RpcnR5ID0gZnVuY3Rpb24oKSB7XG4gICAgICAgICAgICB2YXIgY3VycmVudFN0YXRlID0gSlNPTi5zdHJpbmdpZnkodGhpcy5jYW52YXMudG9PYmplY3QoKSk7XG4gICAgICAgICAgICByZXR1cm4gdGhpcy5pbml0aWFsU3RhdGUgIT0gY3VycmVudFN0YXRlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMucmVzZXRUb29sYm94RWxlbWVudHMoKTtcbiAgICAgICAgdGhpcy5jYW52YXMuc2V0RWRpdG9yKHRoaXMpO1xuICAgIH07XG5cbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTtcbiIsInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xuXG4gICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQgPSBmdW5jdGlvbiAodHlwZSwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpIHtcbiAgICAgICAgaWYgKCF0eXBlKVxuICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiUGFyYW1ldGVyICd0eXBlJyBpcyByZXF1aXJlZC5cIik7XG5cbiAgICAgICAgdGhpcy50eXBlID0gdHlwZTtcbiAgICAgICAgdGhpcy5kYXRhID0gZGF0YTtcbiAgICAgICAgdGhpcy5odG1sSWQgPSBodG1sSWQ7XG4gICAgICAgIHRoaXMuaHRtbENsYXNzID0gaHRtbENsYXNzO1xuICAgICAgICB0aGlzLmh0bWxTdHlsZSA9IGh0bWxTdHlsZTtcbiAgICAgICAgdGhpcy5pc1RlbXBsYXRlZCA9IGlzVGVtcGxhdGVkO1xuXG4gICAgICAgIHRoaXMuZWRpdG9yID0gbnVsbDtcbiAgICAgICAgdGhpcy5wYXJlbnQgPSBudWxsO1xuICAgICAgICB0aGlzLnNldElzRm9jdXNlZEV2ZW50SGFuZGxlcnMgPSBbXTtcblxuICAgICAgICB0aGlzLnNldEVkaXRvciA9IGZ1bmN0aW9uIChlZGl0b3IpIHtcbiAgICAgICAgICAgIHRoaXMuZWRpdG9yID0gZWRpdG9yO1xuICAgICAgICAgICAgaWYgKCEhdGhpcy5jaGlsZHJlbiAmJiBfLmlzQXJyYXkodGhpcy5jaGlsZHJlbikpIHtcbiAgICAgICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcihlZGl0b3IpO1xuICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgfVxuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuc2V0UGFyZW50ID0gZnVuY3Rpb24ocGFyZW50RWxlbWVudCkge1xuICAgICAgICAgICAgdGhpcy5wYXJlbnQgPSBwYXJlbnRFbGVtZW50O1xuXG4gICAgICAgICAgICBpZiAoISF0aGlzLnBhcmVudC5saW5rQ2hpbGQpXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQubGlua0NoaWxkKHRoaXMpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuc2V0SXNUZW1wbGF0ZWQgPSBmdW5jdGlvbiAodmFsdWUpIHtcbiAgICAgICAgICAgIHRoaXMuaXNUZW1wbGF0ZWQgPSB2YWx1ZTtcbiAgICAgICAgICAgIGlmICghIXRoaXMuY2hpbGRyZW4gJiYgXy5pc0FycmF5KHRoaXMuY2hpbGRyZW4pKSB7XG4gICAgICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgICAgICAgICBjaGlsZC5zZXRJc1RlbXBsYXRlZCh2YWx1ZSk7XG4gICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5nZXRJc0FjdGl2ZSA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZWRpdG9yLmFjdGl2ZUVsZW1lbnQgPT09IHRoaXMgJiYgIXRoaXMuZ2V0SXNGb2N1c2VkKCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5zZXRJc0FjdGl2ZSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xuICAgICAgICAgICAgaWYgKCF0aGlzLmVkaXRvcilcbiAgICAgICAgICAgICAgICByZXR1cm47XG4gICAgICAgICAgICBpZiAodGhpcy5lZGl0b3IuaXNEcmFnZ2luZyB8fCB0aGlzLmVkaXRvci5pbmxpbmVFZGl0aW5nSXNBY3RpdmUgfHwgdGhpcy5lZGl0b3IuaXNSZXNpemluZylcbiAgICAgICAgICAgICAgICByZXR1cm47XG5cbiAgICAgICAgICAgIGlmICh2YWx1ZSlcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5hY3RpdmVFbGVtZW50ID0gdGhpcztcbiAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5hY3RpdmVFbGVtZW50ID0gdGhpcy5wYXJlbnQ7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5nZXRJc0ZvY3VzZWQgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmVkaXRvci5mb2N1c2VkRWxlbWVudCA9PT0gdGhpcztcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnNldElzRm9jdXNlZCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5lZGl0b3IpXG4gICAgICAgICAgICBcdHJldHVybjtcbiAgICAgICAgICAgIGlmICh0aGlzLmlzVGVtcGxhdGVkKVxuICAgICAgICAgICAgXHRyZXR1cm47XG4gICAgICAgICAgICBpZiAodGhpcy5lZGl0b3IuaXNEcmFnZ2luZyB8fCB0aGlzLmVkaXRvci5pbmxpbmVFZGl0aW5nSXNBY3RpdmUgfHwgdGhpcy5lZGl0b3IuaXNSZXNpemluZylcbiAgICAgICAgICAgICAgICByZXR1cm47XG5cbiAgICAgICAgICAgIHRoaXMuZWRpdG9yLmZvY3VzZWRFbGVtZW50ID0gdGhpcztcbiAgICAgICAgICAgIF8odGhpcy5zZXRJc0ZvY3VzZWRFdmVudEhhbmRsZXJzKS5lYWNoKGZ1bmN0aW9uIChpdGVtKSB7XG4gICAgICAgICAgICAgICAgdHJ5IHtcbiAgICAgICAgICAgICAgICAgICAgaXRlbSgpO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICBjYXRjaCAoZXgpIHtcbiAgICAgICAgICAgICAgICAgICAgLy8gSWdub3JlLlxuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH0pO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZ2V0SXNTZWxlY3RlZCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICh0aGlzLmdldElzRm9jdXNlZCgpKVxuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xuXG4gICAgICAgICAgICBpZiAoISF0aGlzLmNoaWxkcmVuICYmIF8uaXNBcnJheSh0aGlzLmNoaWxkcmVuKSkge1xuICAgICAgICAgICAgICAgIHJldHVybiBfKHRoaXMuY2hpbGRyZW4pLmFueShmdW5jdGlvbihjaGlsZCkge1xuICAgICAgICAgICAgICAgICAgICByZXR1cm4gY2hpbGQuZ2V0SXNTZWxlY3RlZCgpO1xuICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5nZXRJc0Ryb3BUYXJnZXQgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmVkaXRvci5kcm9wVGFyZ2V0RWxlbWVudCA9PT0gdGhpcztcbiAgICAgICAgfVxuXG4gICAgICAgIHRoaXMuc2V0SXNEcm9wVGFyZ2V0ID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuZWRpdG9yKVxuICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgICAgIGlmICh2YWx1ZSlcbiAgICAgICAgICAgICAgICB0aGlzLmVkaXRvci5kcm9wVGFyZ2V0RWxlbWVudCA9IHRoaXM7XG4gICAgICAgICAgICBlbHNlXG4gICAgICAgICAgICAgICAgdGhpcy5lZGl0b3IuZHJvcFRhcmdldEVsZW1lbnQgPSBudWxsO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZGVsZXRlID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCEhdGhpcy5wYXJlbnQpXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQuZGVsZXRlQ2hpbGQodGhpcyk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5Nb3ZlVXAgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMucGFyZW50KVxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5Nb3ZlQ2hpbGRVcCh0aGlzKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLm1vdmVVcCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghIXRoaXMucGFyZW50KVxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50Lm1vdmVDaGlsZFVwKHRoaXMpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuTW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMucGFyZW50KVxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnBhcmVudC5jYW5Nb3ZlQ2hpbGREb3duKHRoaXMpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMubW92ZURvd24gPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoISF0aGlzLnBhcmVudClcbiAgICAgICAgICAgICAgICB0aGlzLnBhcmVudC5tb3ZlQ2hpbGREb3duKHRoaXMpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZWxlbWVudFRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgICAgICB0eXBlOiB0aGlzLnR5cGUsXG4gICAgICAgICAgICAgICAgZGF0YTogdGhpcy5kYXRhLFxuICAgICAgICAgICAgICAgIGh0bWxJZDogdGhpcy5odG1sSWQsXG4gICAgICAgICAgICAgICAgaHRtbENsYXNzOiB0aGlzLmh0bWxDbGFzcyxcbiAgICAgICAgICAgICAgICBodG1sU3R5bGU6IHRoaXMuaHRtbFN0eWxlLFxuICAgICAgICAgICAgICAgIGlzVGVtcGxhdGVkOiB0aGlzLmlzVGVtcGxhdGVkXG4gICAgICAgICAgICB9O1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZ2V0RWRpdG9yT2JqZWN0ID0gZnVuY3Rpb24oKSB7XG4gICAgICAgICAgICByZXR1cm4ge307XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jb3B5ID0gZnVuY3Rpb24gKGNsaXBib2FyZERhdGEpIHtcbiAgICAgICAgICAgIHZhciB0ZXh0ID0gdGhpcy5nZXRJbm5lclRleHQoKTtcbiAgICAgICAgICAgIGNsaXBib2FyZERhdGEuc2V0RGF0YShcInRleHQvcGxhaW5cIiwgdGV4dCk7XG5cbiAgICAgICAgICAgIHZhciBkYXRhID0gdGhpcy50b09iamVjdCgpO1xuICAgICAgICAgICAgdmFyIGpzb24gPSBKU09OLnN0cmluZ2lmeShkYXRhLCBudWxsLCBcIlxcdFwiKTtcbiAgICAgICAgICAgIGNsaXBib2FyZERhdGEuc2V0RGF0YShcInRleHQvanNvblwiLCBqc29uKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmN1dCA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XG4gICAgICAgICAgICB0aGlzLmNvcHkoY2xpcGJvYXJkRGF0YSk7XG4gICAgICAgICAgICB0aGlzLmRlbGV0ZSgpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMucGFzdGUgPSBmdW5jdGlvbiAoY2xpcGJvYXJkRGF0YSkge1xuICAgICAgICAgICAgaWYgKCEhdGhpcy5wYXJlbnQpXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGUoY2xpcGJvYXJkRGF0YSk7XG4gICAgICAgIH07XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xuXG4gICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lciA9IGZ1bmN0aW9uIChhbGxvd2VkQ2hpbGRUeXBlcywgY2hpbGRyZW4pIHtcblxuICAgICAgICB0aGlzLmFsbG93ZWRDaGlsZFR5cGVzID0gYWxsb3dlZENoaWxkVHlwZXM7XG4gICAgICAgIHRoaXMuY2hpbGRyZW4gPSBjaGlsZHJlbjtcbiAgICAgICAgdGhpcy5pc0NvbnRhaW5lciA9IHRydWU7XG5cbiAgICAgICAgdmFyIF9zZWxmID0gdGhpcztcblxuICAgICAgICB0aGlzLnNldENoaWxkcmVuID0gZnVuY3Rpb24gKGNoaWxkcmVuKSB7XG4gICAgICAgICAgICB0aGlzLmNoaWxkcmVuID0gY2hpbGRyZW47XG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICAgICAgY2hpbGQucGFyZW50ID0gX3NlbGY7XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnNldENoaWxkcmVuKGNoaWxkcmVuKTtcblxuICAgICAgICB0aGlzLmdldElzU2VhbGVkID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgcmV0dXJuIF8odGhpcy5jaGlsZHJlbikuYW55KGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgICAgIHJldHVybiBjaGlsZC5pc1RlbXBsYXRlZDtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuYWRkQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIGlmICghXyh0aGlzLmNoaWxkcmVuKS5jb250YWlucyhjaGlsZCkgJiYgKF8odGhpcy5hbGxvd2VkQ2hpbGRUeXBlcykuY29udGFpbnMoY2hpbGQudHlwZSkgfHwgY2hpbGQuaXNDb250YWluYWJsZSkpXG4gICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbi5wdXNoKGNoaWxkKTtcbiAgICAgICAgICAgIGNoaWxkLnNldEVkaXRvcih0aGlzLmVkaXRvcik7XG4gICAgICAgICAgICBjaGlsZC5zZXRJc1RlbXBsYXRlZChmYWxzZSk7XG4gICAgICAgICAgICBjaGlsZC5wYXJlbnQgPSB0aGlzO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZGVsZXRlQ2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xuICAgICAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4uc3BsaWNlKGluZGV4LCAxKTtcbiAgICAgICAgICAgICAgICBpZiAoY2hpbGQuZ2V0SXNBY3RpdmUoKSlcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5lZGl0b3IuYWN0aXZlRWxlbWVudCA9IG51bGw7XG4gICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzRm9jdXNlZCgpKSB7XG4gICAgICAgICAgICAgICAgICAgIC8vIElmIHRoZSBkZWxldGVkIGNoaWxkIHdhcyBmb2N1c2VkLCB0cnkgdG8gc2V0IG5ldyBmb2N1cyB0byB0aGUgbW9zdCBhcHByb3ByaWF0ZSBzaWJsaW5nIG9yIHBhcmVudC5cbiAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoID4gaW5kZXgpXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuW2luZGV4XS5zZXRJc0ZvY3VzZWQoKTtcbiAgICAgICAgICAgICAgICAgICAgZWxzZSBpZiAoaW5kZXggPiAwKVxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbltpbmRleCAtIDFdLnNldElzRm9jdXNlZCgpO1xuICAgICAgICAgICAgICAgICAgICBlbHNlXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLnNldElzRm9jdXNlZCgpO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLm1vdmVGb2N1c1ByZXZDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgaWYgKHRoaXMuY2hpbGRyZW4ubGVuZ3RoIDwgMilcbiAgICAgICAgICAgICAgICByZXR1cm47XG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xuICAgICAgICAgICAgaWYgKGluZGV4ID4gMClcbiAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuW2luZGV4IC0gMV0uc2V0SXNGb2N1c2VkKCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5tb3ZlRm9jdXNOZXh0Q2hpbGQgPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA8IDIpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA8IHRoaXMuY2hpbGRyZW4ubGVuZ3RoIC0gMSlcbiAgICAgICAgICAgICAgICB0aGlzLmNoaWxkcmVuW2luZGV4ICsgMV0uc2V0SXNGb2N1c2VkKCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5pbnNlcnRDaGlsZCA9IGZ1bmN0aW9uIChjaGlsZCwgYWZ0ZXJDaGlsZCkge1xuICAgICAgICAgICAgaWYgKCFfKHRoaXMuY2hpbGRyZW4pLmNvbnRhaW5zKGNoaWxkKSkge1xuICAgICAgICAgICAgICAgIHZhciBpbmRleCA9IE1hdGgubWF4KF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihhZnRlckNoaWxkKSwgMCk7XG4gICAgICAgICAgICAgICAgdGhpcy5jaGlsZHJlbi5zcGxpY2UoaW5kZXggKyAxLCAwLCBjaGlsZCk7XG4gICAgICAgICAgICAgICAgY2hpbGQuc2V0RWRpdG9yKHRoaXMuZWRpdG9yKTtcbiAgICAgICAgICAgICAgICBjaGlsZC5wYXJlbnQgPSB0aGlzO1xuICAgICAgICAgICAgfVxuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMubW92ZUNoaWxkVXAgPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Nb3ZlQ2hpbGRVcChjaGlsZCkpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcbiAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4ubW92ZShpbmRleCwgaW5kZXggLSAxKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLm1vdmVDaGlsZERvd24gPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Nb3ZlQ2hpbGREb3duKGNoaWxkKSlcbiAgICAgICAgICAgICAgICByZXR1cm47XG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY2hpbGQpO1xuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbi5tb3ZlKGluZGV4LCBpbmRleCArIDEpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuTW92ZUNoaWxkVXAgPSBmdW5jdGlvbiAoY2hpbGQpIHtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjaGlsZCk7XG4gICAgICAgICAgICByZXR1cm4gaW5kZXggPiAwO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY2FuTW92ZUNoaWxkRG93biA9IGZ1bmN0aW9uIChjaGlsZCkge1xuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNoaWxkKTtcbiAgICAgICAgICAgIHJldHVybiBpbmRleCA8IHRoaXMuY2hpbGRyZW4ubGVuZ3RoIC0gMTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNoaWxkcmVuVG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4gXyh0aGlzLmNoaWxkcmVuKS5tYXAoZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIGNoaWxkLnRvT2JqZWN0KCk7XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmdldElubmVyVGV4dCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHJldHVybiBfKHRoaXMuY2hpbGRyZW4pLnJlZHVjZShmdW5jdGlvbiAobWVtbywgY2hpbGQpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gbWVtbyArIFwiXFxuXCIgKyBjaGlsZC5nZXRJbm5lclRleHQoKTtcbiAgICAgICAgICAgIH0sIFwiXCIpO1xuICAgICAgICB9XG5cbiAgICAgICAgdGhpcy5wYXN0ZSA9IGZ1bmN0aW9uIChjbGlwYm9hcmREYXRhKSB7XG4gICAgICAgICAgICB2YXIganNvbiA9IGNsaXBib2FyZERhdGEuZ2V0RGF0YShcInRleHQvanNvblwiKTtcbiAgICAgICAgICAgIGlmICghIWpzb24pIHtcbiAgICAgICAgICAgICAgICB2YXIgZGF0YSA9IEpTT04ucGFyc2UoanNvbik7XG4gICAgICAgICAgICAgICAgdmFyIGNoaWxkID0gTGF5b3V0RWRpdG9yLmVsZW1lbnRGcm9tKGRhdGEpO1xuICAgICAgICAgICAgICAgIHRoaXMucGFzdGVDaGlsZChjaGlsZCk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5wYXN0ZUNoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICBpZiAoXyh0aGlzLmFsbG93ZWRDaGlsZFR5cGVzKS5jb250YWlucyhjaGlsZC50eXBlKSB8fCBjaGlsZC5pc0NvbnRhaW5hYmxlKSB7XG4gICAgICAgICAgICAgICAgdGhpcy5hZGRDaGlsZChjaGlsZCk7XG4gICAgICAgICAgICAgICAgY2hpbGQuc2V0SXNGb2N1c2VkKCk7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBlbHNlIGlmICghIXRoaXMucGFyZW50KVxuICAgICAgICAgICAgICAgIHRoaXMucGFyZW50LnBhc3RlQ2hpbGQoY2hpbGQpO1xuICAgICAgICB9XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xuXG4gICAgTGF5b3V0RWRpdG9yLkNhbnZhcyA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgY2hpbGRyZW4pIHtcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkNhbnZhc1wiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XG4gICAgICAgIExheW91dEVkaXRvci5Db250YWluZXIuY2FsbCh0aGlzLCBbXCJHcmlkXCIsIFwiQ29udGVudFwiXSwgY2hpbGRyZW4pO1xuXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcbiAgICAgICAgICAgIHJlc3VsdC5jaGlsZHJlbiA9IHRoaXMuY2hpbGRyZW5Ub09iamVjdCgpO1xuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcbiAgICAgICAgfTtcbiAgICB9O1xuXG4gICAgTGF5b3V0RWRpdG9yLkNhbnZhcy5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgIHJldHVybiBuZXcgTGF5b3V0RWRpdG9yLkNhbnZhcyhcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcbiAgICAgICAgICAgIExheW91dEVkaXRvci5jaGlsZHJlbkZyb20odmFsdWUuY2hpbGRyZW4pKTtcbiAgICB9O1xuXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7XG4iLCJ2YXIgTGF5b3V0RWRpdG9yO1xuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcblxuICAgIExheW91dEVkaXRvci5HcmlkID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBjaGlsZHJlbikge1xuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiR3JpZFwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XG4gICAgICAgIExheW91dEVkaXRvci5Db250YWluZXIuY2FsbCh0aGlzLCBbXCJSb3dcIl0sIGNoaWxkcmVuKTtcblxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgICAgIH07XG4gICAgfTtcblxuICAgIExheW91dEVkaXRvci5HcmlkLmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuR3JpZChcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcbiAgICAgICAgICAgIExheW91dEVkaXRvci5jaGlsZHJlbkZyb20odmFsdWUuY2hpbGRyZW4pKTtcbiAgICAgICAgcmVzdWx0LnRvb2xib3hJY29uID0gdmFsdWUudG9vbGJveEljb247XG4gICAgICAgIHJlc3VsdC50b29sYm94TGFiZWwgPSB2YWx1ZS50b29sYm94TGFiZWw7XG4gICAgICAgIHJlc3VsdC50b29sYm94RGVzY3JpcHRpb24gPSB2YWx1ZS50b29sYm94RGVzY3JpcHRpb247XG4gICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xuXG4gICAgTGF5b3V0RWRpdG9yLlJvdyA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgY2hpbGRyZW4pIHtcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIlJvd1wiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XG4gICAgICAgIExheW91dEVkaXRvci5Db250YWluZXIuY2FsbCh0aGlzLCBbXCJDb2x1bW5cIl0sIGNoaWxkcmVuKTtcblxuICAgICAgICB2YXIgX3NlbGYgPSB0aGlzO1xuXG4gICAgICAgIGZ1bmN0aW9uIF9nZXRUb3RhbENvbHVtbnNXaWR0aCgpIHtcbiAgICAgICAgICAgIHJldHVybiBfKF9zZWxmLmNoaWxkcmVuKS5yZWR1Y2UoZnVuY3Rpb24gKG1lbW8sIGNoaWxkKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIG1lbW8gKyBjaGlsZC5vZmZzZXQgKyBjaGlsZC53aWR0aDtcbiAgICAgICAgICAgIH0sIDApO1xuICAgICAgICB9XG5cbiAgICAgICAgLy8gSW1wbGVtZW50cyBhIHNpbXBsZSBhbGdvcml0aG0gdG8gZGlzdHJpYnV0ZSBzcGFjZSAoZWl0aGVyIHBvc2l0aXZlIG9yIG5lZ2F0aXZlKVxuICAgICAgICAvLyBiZXR3ZWVuIHRoZSBjaGlsZCBjb2x1bW5zIG9mIHRoZSByb3cuIE5lZ2F0aXZlIHNwYWNlIGlzIGRpc3RyaWJ1dGVkIHdoZW4gbWFraW5nXG4gICAgICAgIC8vIHJvb20gZm9yIGEgbmV3IGNvbHVtbiAoZS5jLiBjbGlwYm9hcmQgcGFzdGUgb3IgZHJvcHBpbmcgZnJvbSB0aGUgdG9vbGJveCkgd2hpbGVcbiAgICAgICAgLy8gcG9zaXRpdmUgc3BhY2UgaXMgZGlzdHJpYnV0ZWQgd2hlbiBmaWxsaW5nIHRoZSBncmFwIG9mIGEgcmVtb3ZlZCBjb2x1bW4uXG4gICAgICAgIGZ1bmN0aW9uIF9kaXN0cmlidXRlU3BhY2Uoc3BhY2UpIHtcbiAgICAgICAgICAgIGlmIChzcGFjZSA9PSAwKVxuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xuICAgICAgICAgICAgIFxuICAgICAgICAgICAgdmFyIHVuZGlzdHJpYnV0ZWRTcGFjZSA9IHNwYWNlO1xuXG4gICAgICAgICAgICBpZiAodW5kaXN0cmlidXRlZFNwYWNlIDwgMCkge1xuICAgICAgICAgICAgICAgIHZhciB2YWNhbnRTcGFjZSA9IDEyIC0gX2dldFRvdGFsQ29sdW1uc1dpZHRoKCk7XG4gICAgICAgICAgICAgICAgdW5kaXN0cmlidXRlZFNwYWNlICs9IHZhY2FudFNwYWNlO1xuICAgICAgICAgICAgICAgIGlmICh1bmRpc3RyaWJ1dGVkU3BhY2UgPiAwKVxuICAgICAgICAgICAgICAgICAgICB1bmRpc3RyaWJ1dGVkU3BhY2UgPSAwO1xuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICAvLyBJZiBzcGFjZSBpcyBuZWdhdGl2ZSwgdHJ5IHRvIGRlY3JlYXNlIG9mZnNldHMgZmlyc3QuXG4gICAgICAgICAgICB3aGlsZSAodW5kaXN0cmlidXRlZFNwYWNlIDwgMCAmJiBfKF9zZWxmLmNoaWxkcmVuKS5hbnkoZnVuY3Rpb24gKGNvbHVtbikgeyByZXR1cm4gY29sdW1uLm9mZnNldCA+IDA7IH0pKSB7IC8vIFdoaWxlIHRoZXJlIGlzIHN0aWxsIG9mZnNldCBsZWZ0IHRvIHJlbW92ZS5cbiAgICAgICAgICAgICAgICBmb3IgKGkgPSAwOyBpIDwgX3NlbGYuY2hpbGRyZW4ubGVuZ3RoICYmIHVuZGlzdHJpYnV0ZWRTcGFjZSA8IDA7IGkrKykge1xuICAgICAgICAgICAgICAgICAgICB2YXIgY29sdW1uID0gX3NlbGYuY2hpbGRyZW5baV07XG4gICAgICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ub2Zmc2V0ID4gMCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldC0tO1xuICAgICAgICAgICAgICAgICAgICAgICAgdW5kaXN0cmlidXRlZFNwYWNlKys7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG5cbiAgICAgICAgICAgIGZ1bmN0aW9uIGhhc1dpZHRoKGNvbHVtbikge1xuICAgICAgICAgICAgICAgIGlmICh1bmRpc3RyaWJ1dGVkU3BhY2UgPiAwKVxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gY29sdW1uLndpZHRoIDwgMTI7XG4gICAgICAgICAgICAgICAgZWxzZSBpZiAodW5kaXN0cmlidXRlZFNwYWNlIDwgMClcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA+IDE7XG4gICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICAvLyBUcnkgdG8gZGlzdHJpYnV0ZSByZW1haW5pbmcgc3BhY2UgKGNvdWxkIGJlIG5lZ2F0aXZlIG9yIHBvc2l0aXZlKSB1c2luZyB3aWR0aHMuXG4gICAgICAgICAgICB3aGlsZSAodW5kaXN0cmlidXRlZFNwYWNlICE9IDApIHtcbiAgICAgICAgICAgICAgICAvLyBBbnkgbW9yZSBjb2x1bW4gd2lkdGggYXZhaWxhYmxlIGZvciBkaXN0cmlidXRpb24/XG4gICAgICAgICAgICAgICAgaWYgKCFfKF9zZWxmLmNoaWxkcmVuKS5hbnkoaGFzV2lkdGgpKVxuICAgICAgICAgICAgICAgICAgICBicmVhaztcbiAgICAgICAgICAgICAgICBmb3IgKGkgPSAwOyBpIDwgX3NlbGYuY2hpbGRyZW4ubGVuZ3RoICYmIHVuZGlzdHJpYnV0ZWRTcGFjZSAhPSAwOyBpKyspIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIGNvbHVtbiA9IF9zZWxmLmNoaWxkcmVuW2kgJSBfc2VsZi5jaGlsZHJlbi5sZW5ndGhdO1xuICAgICAgICAgICAgICAgICAgICBpZiAoaGFzV2lkdGgoY29sdW1uKSkge1xuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGRlbHRhID0gdW5kaXN0cmlidXRlZFNwYWNlIC8gTWF0aC5hYnModW5kaXN0cmlidXRlZFNwYWNlKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aCArPSBkZWx0YTtcbiAgICAgICAgICAgICAgICAgICAgICAgIHVuZGlzdHJpYnV0ZWRTcGFjZSAtPSBkZWx0YTtcbiAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIH0gICAgICAgICAgICAgICAgXG4gICAgICAgICAgICB9XG5cbiAgICAgICAgICAgIHJldHVybiB1bmRpc3RyaWJ1dGVkU3BhY2UgPT0gMDtcbiAgICAgICAgfVxuXG4gICAgICAgIHZhciBfaXNBZGRpbmdDb2x1bW4gPSBmYWxzZTtcblxuICAgICAgICB0aGlzLmNhbkFkZENvbHVtbiA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmNoaWxkcmVuLmxlbmd0aCA8IDEyO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuYmVnaW5BZGRDb2x1bW4gPSBmdW5jdGlvbiAobmV3Q29sdW1uV2lkdGgpIHtcbiAgICAgICAgICAgIGlmICghIV9pc0FkZGluZ0NvbHVtbilcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gYWRkIG9wZXJhdGlvbiBpcyBhbHJlYWR5IGluIHByb2dyZXNzLlwiKVxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcbiAgICAgICAgICAgICAgICBjb2x1bW4uYmVnaW5DaGFuZ2UoKTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgaWYgKF9kaXN0cmlidXRlU3BhY2UoLW5ld0NvbHVtbldpZHRoKSkge1xuICAgICAgICAgICAgICAgIF9pc0FkZGluZ0NvbHVtbiA9IHRydWU7XG4gICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xuICAgICAgICAgICAgICAgIGNvbHVtbi5yb2xsYmFja0NoYW5nZSgpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jb21taXRBZGRDb2x1bW4gPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoIV9pc0FkZGluZ0NvbHVtbilcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJObyBjb2x1bW4gYWRkIG9wZXJhdGlvbiBpbiBwcm9ncmVzcy5cIilcbiAgICAgICAgICAgIF8odGhpcy5jaGlsZHJlbikuZWFjaChmdW5jdGlvbiAoY29sdW1uKSB7XG4gICAgICAgICAgICAgICAgY29sdW1uLmNvbW1pdENoYW5nZSgpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICBfaXNBZGRpbmdDb2x1bW4gPSBmYWxzZTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnJvbGxiYWNrQWRkQ29sdW1uID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCFfaXNBZGRpbmdDb2x1bW4pXG4gICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiTm8gY29sdW1uIGFkZCBvcGVyYXRpb24gaW4gcHJvZ3Jlc3MuXCIpXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNvbHVtbikge1xuICAgICAgICAgICAgICAgIGNvbHVtbi5yb2xsYmFja0NoYW5nZSgpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICBfaXNBZGRpbmdDb2x1bW4gPSBmYWxzZTtcbiAgICAgICAgfTtcblxuICAgICAgICB2YXIgX2Jhc2VEZWxldGVDaGlsZCA9IHRoaXMuZGVsZXRlQ2hpbGQ7XG4gICAgICAgIHRoaXMuZGVsZXRlQ2hpbGQgPSBmdW5jdGlvbiAoY29sdW1uKSB7IFxuICAgICAgICAgICAgdmFyIHdpZHRoID0gY29sdW1uLndpZHRoO1xuICAgICAgICAgICAgX2Jhc2VEZWxldGVDaGlsZC5jYWxsKHRoaXMsIGNvbHVtbik7XG4gICAgICAgICAgICBfZGlzdHJpYnV0ZVNwYWNlKHdpZHRoKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNhbkNvbnRyYWN0Q29sdW1uUmlnaHQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA+IDE7XG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jb250cmFjdENvbHVtblJpZ2h0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuQ29udHJhY3RDb2x1bW5SaWdodChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XG4gICAgICAgICAgICAgICAgaWYgKGNvbHVtbi53aWR0aCA+IDEpIHtcbiAgICAgICAgICAgICAgICAgICAgY29sdW1uLndpZHRoLS07XG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmNoaWxkcmVuLmxlbmd0aCA+IGluZGV4ICsgMSkge1xuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIG5leHRDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4ICsgMV07XG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoY29ubmVjdEFkamFjZW50ICYmIG5leHRDb2x1bW4ub2Zmc2V0ID09IDApXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbmV4dENvbHVtbi53aWR0aCsrO1xuICAgICAgICAgICAgICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ub2Zmc2V0Kys7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5FeHBhbmRDb2x1bW5SaWdodCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xuICAgICAgICAgICAgICAgIGlmIChjb2x1bW4ud2lkdGggPj0gMTIpXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPiBpbmRleCArIDEpIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIG5leHRDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4ICsgMV07XG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgbmV4dENvbHVtbi5vZmZzZXQgPT0gMClcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBuZXh0Q29sdW1uLndpZHRoID4gMTtcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIG5leHRDb2x1bW4ub2Zmc2V0ID4gMDtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgcmV0dXJuIF9nZXRUb3RhbENvbHVtbnNXaWR0aCgpIDwgMTI7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5leHBhbmRDb2x1bW5SaWdodCA9IGZ1bmN0aW9uIChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgaWYgKCF0aGlzLmNhbkV4cGFuZENvbHVtblJpZ2h0KGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSlcbiAgICAgICAgICAgICAgICByZXR1cm47XG5cbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApIHtcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPiBpbmRleCArIDEpIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIG5leHRDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4ICsgMV07XG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgbmV4dENvbHVtbi5vZmZzZXQgPT0gMClcbiAgICAgICAgICAgICAgICAgICAgICAgIG5leHRDb2x1bW4ud2lkdGgtLTtcbiAgICAgICAgICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgICAgICAgICAgbmV4dENvbHVtbi5vZmZzZXQtLTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgY29sdW1uLndpZHRoKys7XG4gICAgICAgICAgICB9XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5FeHBhbmRDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XG4gICAgICAgICAgICAgICAgaWYgKGNvbHVtbi53aWR0aCA+PSAxMilcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICAgICAgICAgIGlmIChpbmRleCA+IDApIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIHByZXZDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4IC0gMV07XG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgY29sdW1uLm9mZnNldCA9PSAwKVxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHByZXZDb2x1bW4ud2lkdGggPiAxO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICByZXR1cm4gY29sdW1uLm9mZnNldCA+IDA7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5leHBhbmRDb2x1bW5MZWZ0ID0gZnVuY3Rpb24gKGNvbHVtbiwgY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICBpZiAoIXRoaXMuY2FuRXhwYW5kQ29sdW1uTGVmdChjb2x1bW4sIGNvbm5lY3RBZGphY2VudCkpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuXG4gICAgICAgICAgICB2YXIgaW5kZXggPSBfKHRoaXMuY2hpbGRyZW4pLmluZGV4T2YoY29sdW1uKTtcbiAgICAgICAgICAgIGlmIChpbmRleCA+PSAwKSB7XG4gICAgICAgICAgICAgICAgaWYgKGluZGV4ID4gMCkge1xuICAgICAgICAgICAgICAgICAgICB2YXIgcHJldkNvbHVtbiA9IHRoaXMuY2hpbGRyZW5baW5kZXggLSAxXTtcbiAgICAgICAgICAgICAgICAgICAgaWYgKGNvbm5lY3RBZGphY2VudCAmJiBjb2x1bW4ub2Zmc2V0ID09IDApXG4gICAgICAgICAgICAgICAgICAgICAgICBwcmV2Q29sdW1uLndpZHRoLS07XG4gICAgICAgICAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbHVtbi5vZmZzZXQtLTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0LS07XG4gICAgICAgICAgICAgICAgY29sdW1uLndpZHRoKys7XG4gICAgICAgICAgICB9XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5Db250cmFjdENvbHVtbkxlZnQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIHZhciBpbmRleCA9IF8odGhpcy5jaGlsZHJlbikuaW5kZXhPZihjb2x1bW4pO1xuICAgICAgICAgICAgaWYgKGluZGV4ID49IDApXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNvbHVtbi53aWR0aCA+IDE7XG4gICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jb250cmFjdENvbHVtbkxlZnQgPSBmdW5jdGlvbiAoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIGlmICghdGhpcy5jYW5Db250cmFjdENvbHVtbkxlZnQoY29sdW1uLCBjb25uZWN0QWRqYWNlbnQpKVxuICAgICAgICAgICAgICAgIHJldHVybjtcblxuICAgICAgICAgICAgdmFyIGluZGV4ID0gXyh0aGlzLmNoaWxkcmVuKS5pbmRleE9mKGNvbHVtbik7XG4gICAgICAgICAgICBpZiAoaW5kZXggPj0gMCkge1xuICAgICAgICAgICAgICAgIGlmIChpbmRleCA+IDApIHtcbiAgICAgICAgICAgICAgICAgICAgdmFyIHByZXZDb2x1bW4gPSB0aGlzLmNoaWxkcmVuW2luZGV4IC0gMV07XG4gICAgICAgICAgICAgICAgICAgIGlmIChjb25uZWN0QWRqYWNlbnQgJiYgY29sdW1uLm9mZnNldCA9PSAwKVxuICAgICAgICAgICAgICAgICAgICAgICAgcHJldkNvbHVtbi53aWR0aCsrO1xuICAgICAgICAgICAgICAgICAgICBlbHNlXG4gICAgICAgICAgICAgICAgICAgICAgICBjb2x1bW4ub2Zmc2V0Kys7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIGVsc2VcbiAgICAgICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldCsrO1xuICAgICAgICAgICAgICAgIGNvbHVtbi53aWR0aC0tO1xuICAgICAgICAgICAgfVxuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuZXZlbkNvbHVtbnMgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAodGhpcy5jaGlsZHJlbi5sZW5ndGggPT0gMClcbiAgICAgICAgICAgICAgICByZXR1cm47XG5cbiAgICAgICAgICAgIHZhciBldmVuV2lkdGggPSBNYXRoLmZsb29yKDEyIC8gdGhpcy5jaGlsZHJlbi5sZW5ndGgpO1xuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjb2x1bW4pIHtcbiAgICAgICAgICAgICAgICBjb2x1bW4ud2lkdGggPSBldmVuV2lkdGg7XG4gICAgICAgICAgICAgICAgY29sdW1uLm9mZnNldCA9IDA7XG4gICAgICAgICAgICB9KTtcblxuICAgICAgICAgICAgdmFyIHJlc3QgPSAxMiAlIHRoaXMuY2hpbGRyZW4ubGVuZ3RoO1xuICAgICAgICAgICAgaWYgKHJlc3QgPiAwKVxuICAgICAgICAgICAgICAgIF9kaXN0cmlidXRlU3BhY2UocmVzdCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdmFyIF9iYXNlUGFzdGVDaGlsZCA9IHRoaXMucGFzdGVDaGlsZDtcbiAgICAgICAgdGhpcy5wYXN0ZUNoaWxkID0gZnVuY3Rpb24gKGNoaWxkKSB7XG4gICAgICAgICAgICBpZiAoY2hpbGQudHlwZSA9PSBcIkNvbHVtblwiKSB7XG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuYmVnaW5BZGRDb2x1bW4oY2hpbGQud2lkdGgpKSB7XG4gICAgICAgICAgICAgICAgICAgIHRoaXMuY29tbWl0QWRkQ29sdW1uKCk7XG4gICAgICAgICAgICAgICAgICAgIF9iYXNlUGFzdGVDaGlsZC5jYWxsKHRoaXMsIGNoaWxkKVxuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIGVsc2UgaWYgKCEhdGhpcy5wYXJlbnQpXG4gICAgICAgICAgICAgICAgdGhpcy5wYXJlbnQucGFzdGVDaGlsZChjaGlsZCk7XG4gICAgICAgIH1cblxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgICAgIH07XG4gICAgfTtcblxuICAgIExheW91dEVkaXRvci5Sb3cuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xuICAgICAgICB2YXIgcmVzdWx0ID0gbmV3IExheW91dEVkaXRvci5Sb3coXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XG4gICAgICAgIHJlc3VsdC50b29sYm94SWNvbiA9IHZhbHVlLnRvb2xib3hJY29uO1xuICAgICAgICByZXN1bHQudG9vbGJveExhYmVsID0gdmFsdWUudG9vbGJveExhYmVsO1xuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xuICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgIH07XG5cbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xuKGZ1bmN0aW9uIChMYXlvdXRFZGl0b3IpIHtcblxuICAgIExheW91dEVkaXRvci5Db2x1bW4gPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIHdpZHRoLCBvZmZzZXQsIGNoaWxkcmVuKSB7XG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJDb2x1bW5cIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpO1xuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiR3JpZFwiLCBcIkNvbnRlbnRcIl0sIGNoaWxkcmVuKTtcblxuICAgICAgICB0aGlzLndpZHRoID0gd2lkdGg7XG4gICAgICAgIHRoaXMub2Zmc2V0ID0gb2Zmc2V0O1xuXG4gICAgICAgIHZhciBfaGFzUGVuZGluZ0NoYW5nZSA9IGZhbHNlO1xuICAgICAgICB2YXIgX29yaWdXaWR0aCA9IDA7XG4gICAgICAgIHZhciBfb3JpZ09mZnNldCA9IDA7XG5cbiAgICAgICAgdGhpcy5iZWdpbkNoYW5nZSA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIGlmICghIV9oYXNQZW5kaW5nQ2hhbmdlKVxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIkNvbHVtbiBhbHJlYWR5IGhhcyBhIHBlbmRpbmcgY2hhbmdlLlwiKVxuICAgICAgICAgICAgX2hhc1BlbmRpbmdDaGFuZ2UgPSB0cnVlO1xuICAgICAgICAgICAgX29yaWdXaWR0aCA9IHRoaXMud2lkdGg7XG4gICAgICAgICAgICBfb3JpZ09mZnNldCA9IHRoaXMub2Zmc2V0O1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY29tbWl0Q2hhbmdlID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCFfaGFzUGVuZGluZ0NoYW5nZSlcbiAgICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoXCJDb2x1bW4gaGFzIG5vIHBlbmRpbmcgY2hhbmdlLlwiKVxuICAgICAgICAgICAgX29yaWdXaWR0aCA9IDA7XG4gICAgICAgICAgICBfb3JpZ09mZnNldCA9IDA7XG4gICAgICAgICAgICBfaGFzUGVuZGluZ0NoYW5nZSA9IGZhbHNlO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMucm9sbGJhY2tDaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICBpZiAoIV9oYXNQZW5kaW5nQ2hhbmdlKVxuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcIkNvbHVtbiBoYXMgbm8gcGVuZGluZyBjaGFuZ2UuXCIpXG4gICAgICAgICAgICB0aGlzLndpZHRoID0gX29yaWdXaWR0aDtcbiAgICAgICAgICAgIHRoaXMub2Zmc2V0ID0gX29yaWdPZmZzZXQ7XG4gICAgICAgICAgICBfb3JpZ1dpZHRoID0gMDtcbiAgICAgICAgICAgIF9vcmlnT2Zmc2V0ID0gMDtcbiAgICAgICAgICAgIF9oYXNQZW5kaW5nQ2hhbmdlID0gZmFsc2U7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5TcGxpdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLndpZHRoID4gMTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnNwbGl0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgaWYgKCF0aGlzLmNhblNwbGl0KCkpXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xuXG4gICAgICAgICAgICB2YXIgbmV3Q29sdW1uV2lkdGggPSBNYXRoLmZsb29yKHRoaXMud2lkdGggLyAyKTtcbiAgICAgICAgICAgIHZhciBuZXdDb2x1bW4gPSBMYXlvdXRFZGl0b3IuQ29sdW1uLmZyb20oe1xuICAgICAgICAgICAgICAgIGRhdGE6IG51bGwsXG4gICAgICAgICAgICAgICAgaHRtbElkOiBudWxsLFxuICAgICAgICAgICAgICAgIGh0bWxDbGFzczogbnVsbCxcbiAgICAgICAgICAgICAgICBodG1sU3R5bGU6IG51bGwsXG4gICAgICAgICAgICAgICAgd2lkdGg6IG5ld0NvbHVtbldpZHRoLFxuICAgICAgICAgICAgICAgIG9mZnNldDogMCxcbiAgICAgICAgICAgICAgICBjaGlsZHJlbjogW11cbiAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgXG4gICAgICAgICAgICB0aGlzLndpZHRoID0gdGhpcy53aWR0aCAtIG5ld0NvbHVtbldpZHRoO1xuICAgICAgICAgICAgdGhpcy5wYXJlbnQuaW5zZXJ0Q2hpbGQobmV3Q29sdW1uLCB0aGlzKTtcbiAgICAgICAgICAgIG5ld0NvbHVtbi5zZXRJc0ZvY3VzZWQoKTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNhbkNvbnRyYWN0UmlnaHQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICByZXR1cm4gdGhpcy5wYXJlbnQuY2FuQ29udHJhY3RDb2x1bW5SaWdodCh0aGlzLCBjb25uZWN0QWRqYWNlbnQpO1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMuY29udHJhY3RSaWdodCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIHRoaXMucGFyZW50LmNvbnRyYWN0Q29sdW1uUmlnaHQodGhpcywgY29ubmVjdEFkamFjZW50KTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmNhbkV4cGFuZFJpZ2h0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkV4cGFuZENvbHVtblJpZ2h0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5leHBhbmRSaWdodCA9IGZ1bmN0aW9uIChjb25uZWN0QWRqYWNlbnQpIHtcbiAgICAgICAgICAgIHRoaXMucGFyZW50LmV4cGFuZENvbHVtblJpZ2h0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5FeHBhbmRMZWZ0ID0gZnVuY3Rpb24gKGNvbm5lY3RBZGphY2VudCkge1xuICAgICAgICAgICAgcmV0dXJuIHRoaXMucGFyZW50LmNhbkV4cGFuZENvbHVtbkxlZnQodGhpcywgY29ubmVjdEFkamFjZW50KTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLmV4cGFuZExlZnQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICB0aGlzLnBhcmVudC5leHBhbmRDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jYW5Db250cmFjdExlZnQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICByZXR1cm4gdGhpcy5wYXJlbnQuY2FuQ29udHJhY3RDb2x1bW5MZWZ0KHRoaXMsIGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5jb250cmFjdExlZnQgPSBmdW5jdGlvbiAoY29ubmVjdEFkamFjZW50KSB7XG4gICAgICAgICAgICB0aGlzLnBhcmVudC5jb250cmFjdENvbHVtbkxlZnQodGhpcywgY29ubmVjdEFkamFjZW50KTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXN1bHQud2lkdGggPSB0aGlzLndpZHRoO1xuICAgICAgICAgICAgcmVzdWx0Lm9mZnNldCA9IHRoaXMub2Zmc2V0O1xuICAgICAgICAgICAgcmVzdWx0LmNoaWxkcmVuID0gdGhpcy5jaGlsZHJlblRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgICAgICB9O1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IuQ29sdW1uLmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuQ29sdW1uKFxuICAgICAgICAgICAgdmFsdWUuZGF0YSxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcbiAgICAgICAgICAgIHZhbHVlLmh0bWxTdHlsZSxcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxuICAgICAgICAgICAgdmFsdWUud2lkdGgsXG4gICAgICAgICAgICB2YWx1ZS5vZmZzZXQsXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XG4gICAgICAgIHJlc3VsdC50b29sYm94SWNvbiA9IHZhbHVlLnRvb2xib3hJY29uO1xuICAgICAgICByZXN1bHQudG9vbGJveExhYmVsID0gdmFsdWUudG9vbGJveExhYmVsO1xuICAgICAgICByZXN1bHQudG9vbGJveERlc2NyaXB0aW9uID0gdmFsdWUudG9vbGJveERlc2NyaXB0aW9uO1xuICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IuQ29sdW1uLnRpbWVzID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgIHJldHVybiBfLnRpbWVzKHZhbHVlLCBmdW5jdGlvbiAobikge1xuICAgICAgICAgICAgcmV0dXJuIExheW91dEVkaXRvci5Db2x1bW4uZnJvbSh7XG4gICAgICAgICAgICAgICAgZGF0YTogbnVsbCxcbiAgICAgICAgICAgICAgICBodG1sSWQ6IG51bGwsXG4gICAgICAgICAgICAgICAgaHRtbENsYXNzOiBudWxsLFxuICAgICAgICAgICAgICAgIGlzVGVtcGxhdGVkOiBmYWxzZSxcbiAgICAgICAgICAgICAgICB3aWR0aDogMTIgLyB2YWx1ZSxcbiAgICAgICAgICAgICAgICBvZmZzZXQ6IDAsXG4gICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXG4gICAgICAgICAgICB9KTtcbiAgICAgICAgfSk7XG4gICAgfTtcblxufSkoTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpOyIsInZhciBMYXlvdXRFZGl0b3I7XG4oZnVuY3Rpb24gKExheW91dEVkaXRvcikge1xuXG4gICAgTGF5b3V0RWRpdG9yLkNvbnRlbnQgPSBmdW5jdGlvbiAoZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQsIGNvbnRlbnRUeXBlLCBjb250ZW50VHlwZUxhYmVsLCBjb250ZW50VHlwZUNsYXNzLCBodG1sLCBoYXNFZGl0b3IpIHtcbiAgICAgICAgTGF5b3V0RWRpdG9yLkVsZW1lbnQuY2FsbCh0aGlzLCBcIkNvbnRlbnRcIiwgZGF0YSwgaHRtbElkLCBodG1sQ2xhc3MsIGh0bWxTdHlsZSwgaXNUZW1wbGF0ZWQpO1xuXG4gICAgICAgIHRoaXMuY29udGVudFR5cGUgPSBjb250ZW50VHlwZTtcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUxhYmVsID0gY29udGVudFR5cGVMYWJlbDtcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUNsYXNzID0gY29udGVudFR5cGVDbGFzcztcbiAgICAgICAgdGhpcy5odG1sID0gaHRtbDtcbiAgICAgICAgdGhpcy5oYXNFZGl0b3IgPSBoYXNFZGl0b3I7XG5cbiAgICAgICAgdGhpcy5nZXRJbm5lclRleHQgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4gJCgkLnBhcnNlSFRNTChcIjxkaXY+XCIgKyB0aGlzLmh0bWwgKyBcIjwvZGl2PlwiKSkudGV4dCgpO1xuICAgICAgICB9O1xuXG4gICAgICAgIC8vIFRoaXMgZnVuY3Rpb24gd2lsbCBiZSBvdmVyd3JpdHRlbiBieSB0aGUgQ29udGVudCBkaXJlY3RpdmUuXG4gICAgICAgIHRoaXMuc2V0SHRtbCA9IGZ1bmN0aW9uIChodG1sKSB7XG4gICAgICAgICAgICB0aGlzLmh0bWwgPSBodG1sO1xuICAgICAgICAgICAgdGhpcy5odG1sVW5zYWZlID0gaHRtbDtcbiAgICAgICAgfVxuXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICByZXR1cm4ge1xuICAgICAgICAgICAgICAgIFwidHlwZVwiOiBcIkNvbnRlbnRcIlxuICAgICAgICAgICAgfTtcbiAgICAgICAgfTtcblxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGUgPSB0aGlzLmNvbnRlbnRUeXBlO1xuICAgICAgICAgICAgcmVzdWx0LmNvbnRlbnRUeXBlTGFiZWwgPSB0aGlzLmNvbnRlbnRUeXBlTGFiZWw7XG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGVDbGFzcyA9IHRoaXMuY29udGVudFR5cGVDbGFzcztcbiAgICAgICAgICAgIHJlc3VsdC5odG1sID0gdGhpcy5odG1sO1xuICAgICAgICAgICAgcmVzdWx0Lmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgICAgIH07XG5cbiAgICAgICAgdGhpcy5zZXRIdG1sKGh0bWwpO1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IuQ29udGVudC5mcm9tID0gZnVuY3Rpb24gKHZhbHVlKSB7XG4gICAgICAgIHZhciByZXN1bHQgPSBuZXcgTGF5b3V0RWRpdG9yLkNvbnRlbnQoXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxuICAgICAgICAgICAgdmFsdWUuaHRtbElkLFxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxuICAgICAgICAgICAgdmFsdWUuaXNUZW1wbGF0ZWQsXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZSxcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlTGFiZWwsXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZUNsYXNzLFxuICAgICAgICAgICAgdmFsdWUuaHRtbCxcbiAgICAgICAgICAgIHZhbHVlLmhhc0VkaXRvcik7XG5cbiAgICAgICAgcmV0dXJuIHJlc3VsdDtcbiAgICB9O1xuXG59KShMYXlvdXRFZGl0b3IgfHwgKExheW91dEVkaXRvciA9IHt9KSk7IiwidmFyIExheW91dEVkaXRvcjtcbihmdW5jdGlvbiAoJCwgTGF5b3V0RWRpdG9yKSB7XG5cbiAgICBMYXlvdXRFZGl0b3IuSHRtbCA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgY29udGVudFR5cGUsIGNvbnRlbnRUeXBlTGFiZWwsIGNvbnRlbnRUeXBlQ2xhc3MsIGh0bWwsIGhhc0VkaXRvcikge1xuICAgICAgICBMYXlvdXRFZGl0b3IuRWxlbWVudC5jYWxsKHRoaXMsIFwiSHRtbFwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XG5cbiAgICAgICAgdGhpcy5jb250ZW50VHlwZSA9IGNvbnRlbnRUeXBlO1xuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlTGFiZWwgPSBjb250ZW50VHlwZUxhYmVsO1xuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlQ2xhc3MgPSBjb250ZW50VHlwZUNsYXNzO1xuICAgICAgICB0aGlzLmh0bWwgPSBodG1sO1xuICAgICAgICB0aGlzLmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcbiAgICAgICAgdGhpcy5pc0NvbnRhaW5hYmxlID0gdHJ1ZTtcblxuICAgICAgICB0aGlzLmdldElubmVyVGV4dCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHJldHVybiAkKCQucGFyc2VIVE1MKFwiPGRpdj5cIiArIHRoaXMuaHRtbCArIFwiPC9kaXY+XCIpKS50ZXh0KCk7XG4gICAgICAgIH07XG5cbiAgICAgICAgLy8gVGhpcyBmdW5jdGlvbiB3aWxsIGJlIG92ZXJ3cml0dGVuIGJ5IHRoZSBDb250ZW50IGRpcmVjdGl2ZS5cbiAgICAgICAgdGhpcy5zZXRIdG1sID0gZnVuY3Rpb24gKGh0bWwpIHtcbiAgICAgICAgICAgIHRoaXMuaHRtbCA9IGh0bWw7XG4gICAgICAgICAgICB0aGlzLmh0bWxVbnNhZmUgPSBodG1sO1xuICAgICAgICB9XG5cbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHJldHVybiB7XG4gICAgICAgICAgICAgICAgXCJ0eXBlXCI6IFwiSHRtbFwiXG4gICAgICAgICAgICB9O1xuICAgICAgICB9O1xuXG4gICAgICAgIHRoaXMudG9PYmplY3QgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZSA9IHRoaXMuY29udGVudFR5cGU7XG4gICAgICAgICAgICByZXN1bHQuY29udGVudFR5cGVMYWJlbCA9IHRoaXMuY29udGVudFR5cGVMYWJlbDtcbiAgICAgICAgICAgIHJlc3VsdC5jb250ZW50VHlwZUNsYXNzID0gdGhpcy5jb250ZW50VHlwZUNsYXNzO1xuICAgICAgICAgICAgcmVzdWx0Lmh0bWwgPSB0aGlzLmh0bWw7XG4gICAgICAgICAgICByZXN1bHQuaGFzRWRpdG9yID0gaGFzRWRpdG9yO1xuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcbiAgICAgICAgfTtcblxuICAgICAgICB2YXIgZ2V0RWRpdG9yT2JqZWN0ID0gdGhpcy5nZXRFZGl0b3JPYmplY3Q7XG4gICAgICAgIHRoaXMuZ2V0RWRpdG9yT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgdmFyIGR0byA9IGdldEVkaXRvck9iamVjdCgpO1xuICAgICAgICAgICAgcmV0dXJuICQuZXh0ZW5kKGR0bywge1xuICAgICAgICAgICAgICAgIENvbnRlbnQ6IHRoaXMuaHRtbFxuICAgICAgICAgICAgfSk7XG4gICAgICAgIH1cblxuICAgICAgICB0aGlzLnNldEh0bWwoaHRtbCk7XG4gICAgfTtcblxuICAgIExheW91dEVkaXRvci5IdG1sLmZyb20gPSBmdW5jdGlvbiAodmFsdWUpIHtcbiAgICAgICAgdmFyIHJlc3VsdCA9IG5ldyBMYXlvdXRFZGl0b3IuSHRtbChcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXG4gICAgICAgICAgICB2YWx1ZS5odG1sQ2xhc3MsXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlLFxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVMYWJlbCxcbiAgICAgICAgICAgIHZhbHVlLmNvbnRlbnRUeXBlQ2xhc3MsXG4gICAgICAgICAgICB2YWx1ZS5odG1sLFxuICAgICAgICAgICAgdmFsdWUuaGFzRWRpdG9yKTtcblxuICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgIH07XG5cbiAgICBMYXlvdXRFZGl0b3IucmVnaXN0ZXJGYWN0b3J5KFwiSHRtbFwiLCBmdW5jdGlvbih2YWx1ZSkgeyByZXR1cm4gTGF5b3V0RWRpdG9yLkh0bWwuZnJvbSh2YWx1ZSk7IH0pO1xuXG59KShqUXVlcnksIExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiXSwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=