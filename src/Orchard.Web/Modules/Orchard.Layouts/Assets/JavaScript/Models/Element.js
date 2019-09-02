var LayoutEditor;
(function (LayoutEditor) {

    LayoutEditor.Element = function (type, data, htmlId, htmlClass, htmlStyle, isTemplated, rule) {
        if (!type)
            throw new Error("Parameter 'type' is required.");

        var self = this;
        this.type = type;
        this.data = data;
        this.htmlId = htmlId;
        this.htmlClass = htmlClass;
        this.htmlStyle = htmlStyle;
        this.isTemplated = isTemplated;
        this.rule = rule;

        this.templateStyles = {};
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
            if (this.editor.isDragging || this.editor.isResizing)
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
            if (!this.children && this.isTemplated)
            	return;
            if (this.editor.isDragging || this.editor.isResizing)
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
            if (this.isTemplated || !this.parent)
                return false;
            return true;
        };

        this.delete = function () {
            if (!this.canDelete())
                return;
            this.parent.deleteChild(this);
        };

        this.canMoveUp = function () {
            if (this.isTemplated || !this.parent)
                return false;
            return this.parent.canMoveChildUp(this);
        };

        this.moveUp = function () {
            if (!this.canMoveUp())
                return;
            this.parent.moveChildUp(this);
        };

        this.canMoveDown = function () {
            if (this.isTemplated || !this.parent)
                return false;
            return this.parent.canMoveChildDown(this);
        };

        this.moveDown = function () {
            if (!this.canMoveDown())
                return;
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
                rule: this.rule,
                contentType: this.contentType,
                hasEditor: this.hasEditor
            };
        };

        this.getEditorObject = function() {
            return {};
        };

        this.toObject = function () {
            return self.elementToObject();
        };

        this.copy = function (clipboardData) {
            var text = this.getInnerText();
            clipboardData.setData("text/plain", text);

            var data = this.toObject();
            var json = JSON.stringify(data, null, "\t");
            clipboardData.setData("text/json", json);
        };

        this.cut = function (clipboardData) {
            if (this.canDelete()) {
                this.copy(clipboardData);
                this.delete();
            }
        };

        this.paste = function (clipboardData) {
            if (!!this.parent)
                this.parent.paste(clipboardData);
        };

        this.getTemplateStyles = function () {
            var styles = this.templateStyles || {};
            var css = "";

            for (var property in styles) {
                css += property + ":" + styles[property] + ";";
            }

            return css;
        }
    };

})(LayoutEditor || (LayoutEditor = {}));