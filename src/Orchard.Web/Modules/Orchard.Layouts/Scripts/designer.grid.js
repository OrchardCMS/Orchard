(function ($) {
    var Column = function (element) {
        if (element.data("column"))
            return element.data("column");

        var self = this;
        this.element = element;
        this.row = new Row(element.closest(".x-row"));
        this.canvas = this.row.canvas;
        this.element.data("column", this);
        this.state = $.deserialize(this.element.data("element").state);
        this.originalSpan = parseInt(this.state["ColumnSpan"]);

        this.getSpan = function () {
            return parseInt(self.state["ColumnSpan"]);
        };

        this.setSpan = function (value) {
            if (isNaN(value))
                return;
            updateColumn(function (column, row) {
                updateSpan(column, row, function (originalValue) {
                    self.element.removeClass("span-" + originalValue);
                    self.element.addClass("span-" + value);
                    return value;
                });
            }, "span-changed");
        };

        this.setOffset = function (delta) {
            if (isNaN(delta))
                return;
            updateColumn(function (column, row) {
                updateOffset(column, row, function (originalValue) {
                    originalValue = isNaN(originalValue) ? 0 : originalValue;
                    var newValue = originalValue + delta;
                    self.element.removeClass("offset-" + originalValue);
                    self.element.addClass("offset-" + newValue);
                    return newValue;
                });
            }, "offset-changed");
        };

        var calculateSpanWidth = function () {
            var totalSpan = 12;
            var totalWidth = self.row.element.width();
            return Math.round(totalWidth / totalSpan);
        };

        this.element.find(".resizable").resizable({
            grid: [calculateSpanWidth(), 0],
            handles: "e",
            containment: self.row.element,
            stop: function (e, ui) {
                var totalSpan = 12;
                var totalWidth = self.row.element.width();
                var columnWidth = ui.size.width;
                var newSpan = Math.round(totalSpan / (totalWidth / columnWidth));

                ui.element.removeAttr("style");
                self.setSpan(newSpan);
            }
        });

        this.element.find(".draggable").draggable({
            grid: [calculateSpanWidth(), 0],
            handle: ".column-toolbar",
            containment: self.row.element,
            stop: function (e, ui) {
                var spanWidth = calculateSpanWidth();
                var spansMoved = Math.round((parseFloat(ui.position.left) / parseFloat(spanWidth)));
                var newValue = spansMoved;

                ui.helper.removeAttr("style");
                self.setOffset(newValue);
            }
        });

        this.element.on("click", ".split-column:first", function (e) {
            e.preventDefault();
            updateColumn(function (column, row) {
                var columnState = $.deserialize(column.state);
                var span = parseInt(columnState["ColumnSpan"]);

                if (isNaN(span))
                    span = 12;

                // Divide the span.
                var isEven = span % 2 == 0;
                var span1 = isEven ? span / 2 : Math.round((span / 2) + 0.5);
                var span2 = isEven ? span1 : Math.round((span / 2) - 0.5);
                columnState["ColumnSpan"] = span1;
                column.state = $.param(columnState);

                // Push a copy of the column.
                var newColumn = {
                    typeName: "Orchard.Layouts.Elements.Column",
                    state: "ColumnSpan=" + span2
                };

                var columns = row.elements;
                columns.splice(column.index, 1, column, newColumn);
                row.elements = columns;
            }, "split-column");
        });

        this.element.on("click", ".increase-offset:first", function (e) {
            e.preventDefault();

            updateColumn(function (column, row) {
                updateOffset(column, row, function (offset) {
                    if (isNaN(offset))
                        return 1;
                    return offset < 11 ? offset + 1 : 11;
                });
            }, "offset-changed");
        });

        this.element.on("click", ".decrease-offset:first", function (e) {
            e.preventDefault();

            updateColumn(function (column, row) {
                updateOffset(column, row, function (offset) {
                    if (isNaN(offset))
                        return 0;
                    return offset > 0 ? offset - 1 : 0;
                });
            }, "offset-changed");
        });

        var updateSpan = function (column, row, updateCallback) {
            updateProperty(column, row, "ColumnSpan", function (value) {
                return updateCallback(parseInt(value || 0));
            });
        };

        var updateOffset = function (column, row, updateCallback) {
            updateProperty(column, row, "ColumnOffset", function (value) {
                return updateCallback(parseInt(value || 0));
            });
        };

        var updateProperty = function (column, row, propertyName, updateCallback) {
            var columnState = $.deserialize(column.state);
            var value = columnState[propertyName];

            // Update the property.
            value = updateCallback(value);
            columnState[propertyName] = value;
            column.state = $.param(columnState);

            // Replace the column in the row with the updated column.
            var columns = row.elements;
            columns.splice(column.index, 1, column);
            row.elements = columns;
        };

        var updateColumn = function (updateColumnCallback, renderEventName) {
            // Serialize the row and column.
            var columnGraph = {};
            var rowGraph = {};

            self.canvas.serialize(columnGraph, self.element);
            self.canvas.serialize(rowGraph, self.row.element);

            // Update the column.
            var column = columnGraph.elements[0];
            var row = rowGraph.elements[0];

            if (updateColumnCallback)
                updateColumnCallback(column, row);

            // Render the updated row.
            self.canvas.renderGraph(self.row.element, rowGraph, self.canvas.settings.domOperations.replace, { renderEventName: renderEventName, renderEventArgs: column.index });
        };

        return this;
    };

    var Row = function (element) {
        if (element.data("row"))
            return element.data("row");

        var self = this;
        this.grid = new Grid(element.closest(".x-grid"));
        this.canvas = this.grid.canvas;
        this.element = element;
        this.element.data("row", this);

        this.element.on("click", ".add-column:first", function (e) {
            e.preventDefault();

            // Serialize the row.
            var graph = {};
            self.canvas.serialize(graph, self.element);

            // Add a column element to the row.
            graph.elements[0].elements.push({
                typeName: "Orchard.Layouts.Elements.Column",
                state: "ColumnSpan=12"
            });

            // Render the updated row.
            self.canvas.renderGraph(self.element, graph, self.canvas.settings.domOperations.replace);
        });

        return this;
    };

    var Grid = function (element) {
        if (element.data("grid"))
            return element.data("grid");

        var self = this;
        this.canvasElement = element.closest(".layout-editor");
        this.canvas = this.canvasElement.data("layout-editor");;
        this.element = element;
        this.element.data("grid", this);

        this.element.on("click", ".add-row:first", function (e) {
            e.preventDefault();

            self.addRow();
        });

        this.addRow = function () {
            // Serialize the grid.
            var graph = {};
            self.canvas.serialize(graph, self.element);

            // Add a row to the grid right after the sender row.
            var gridData = graph.elements[0];
            var rows = gridData.elements || [];
            var newRow = {
                typeName: "Orchard.Layouts.Elements.Row",
                elements: [
                    {
                        typeName: "Orchard.Layouts.Elements.Column",
                        state: "ColumnSpan=12"
                    }
                ]
            };

            rows.push(newRow);
            gridData.elements = rows;

            // Render the updated grid.
            self.canvas.renderGraph(self.element, graph, self.canvas.settings.domOperations.replace);
        };

        return this;
    };

    $(function () {
        var initDragDrop = function () {
            $(".canvas").find(".x-grid").sortable({
                revert: false,
                zIndex: 100,
                handle: ".row-toolbar.sortable",
                placeholder: "sortable-placeholder",
                helper: "clone",
                connectWith: ".x-grid"
            });
        };

        var initialize = function () {
            initDragDrop();
            $(".canvas").find(".x-column:not(.templated)").each(function () {
                var column = new Column($(this));
            });
        };

        $(".layout-editor").on("elementupdated", function (e, data) {
            initialize();
        });

        initialize();
    });
})(jQuery);