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