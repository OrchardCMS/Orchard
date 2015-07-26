/*
 * jQuery File Upload Processing Plugin 1.3.0
 * https://github.com/blueimp/jQuery-File-Upload
 *
 * Copyright 2012, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * http://www.opensource.org/licenses/MIT
 */

/* jshint nomen:false */
/* global define, window */

(function (factory) {
    'use strict';
    if (typeof define === 'function' && define.amd) {
        // Register as an anonymous AMD module:
        define([
            'jquery',
            './jquery.fileupload'
        ], factory);
    } else {
        // Browser globals:
        factory(
            window.jQuery
        );
    }
}(function ($) {
    'use strict';

    var originalAdd = $.blueimp.fileupload.prototype.options.add;

    // The File Upload Processing plugin extends the fileupload widget
    // with file processing functionality:
    $.widget('blueimp.fileupload', $.blueimp.fileupload, {

        options: {
            // The list of processing actions:
            processQueue: [
                /*
                {
                    action: 'log',
                    type: 'debug'
                }
                */
            ],
            add: function (e, data) {
                var $this = $(this);
                data.process(function () {
                    return $this.fileupload('process', data);
                });
                originalAdd.call(this, e, data);
            }
        },

        processActions: {
            /*
            log: function (data, options) {
                console[options.type](
                    'Processing "' + data.files[data.index].name + '"'
                );
            }
            */
        },

        _processFile: function (data, originalData) {
            var that = this,
                dfd = $.Deferred().resolveWith(that, [data]),
                chain = dfd.promise();
            this._trigger('process', null, data);
            $.each(data.processQueue, function (i, settings) {
                var func = function (data) {
                    if (originalData.errorThrown) {
                        return $.Deferred()
                                .rejectWith(that, [originalData]).promise();
                    }
                    return that.processActions[settings.action].call(
                        that,
                        data,
                        settings
                    );
                };
                chain = chain.pipe(func, settings.always && func);
            });
            chain
                .done(function () {
                    that._trigger('processdone', null, data);
                    that._trigger('processalways', null, data);
                })
                .fail(function () {
                    that._trigger('processfail', null, data);
                    that._trigger('processalways', null, data);
                });
            return chain;
        },

        // Replaces the settings of each processQueue item that
        // are strings starting with an "@", using the remaining
        // substring as key for the option map,
        // e.g. "@autoUpload" is replaced with options.autoUpload:
        _transformProcessQueue: function (options) {
            var processQueue = [];
            $.each(options.processQueue, function () {
                var settings = {},
                    action = this.action,
                    prefix = this.prefix === true ? action : this.prefix;
                $.each(this, function (key, value) {
                    if ($.type(value) === 'string' &&
                            value.charAt(0) === '@') {
                        settings[key] = options[
                            value.slice(1) || (prefix ? prefix +
                                key.charAt(0).toUpperCase() + key.slice(1) : key)
                        ];
                    } else {
                        settings[key] = value;
                    }

                });
                processQueue.push(settings);
            });
            options.processQueue = processQueue;
        },

        // Returns the number of files currently in the processsing queue:
        processing: function () {
            return this._processing;
        },

        // Processes the files given as files property of the data parameter,
        // returns a Promise object that allows to bind callbacks:
        process: function (data) {
            var that = this,
                options = $.extend({}, this.options, data);
            if (options.processQueue && options.processQueue.length) {
                this._transformProcessQueue(options);
                if (this._processing === 0) {
                    this._trigger('processstart');
                }
                $.each(data.files, function (index) {
                    var opts = index ? $.extend({}, options) : options,
                        func = function () {
                            if (data.errorThrown) {
                                return $.Deferred()
                                        .rejectWith(that, [data]).promise();
                            }
                            return that._processFile(opts, data);
                        };
                    opts.index = index;
                    that._processing += 1;
                    that._processingQueue = that._processingQueue.pipe(func, func)
                        .always(function () {
                            that._processing -= 1;
                            if (that._processing === 0) {
                                that._trigger('processstop');
                            }
                        });
                });
            }
            return this._processingQueue;
        },

        _create: function () {
            this._super();
            this._processing = 0;
            this._processingQueue = $.Deferred().resolveWith(this)
                .promise();
        }

    });

}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqcXVlcnkuZmlsZXVwbG9hZC1wcm9jZXNzLmpzIl0sInNvdXJjZXNDb250ZW50IjpbIi8qXG4gKiBqUXVlcnkgRmlsZSBVcGxvYWQgUHJvY2Vzc2luZyBQbHVnaW4gMS4zLjBcbiAqIGh0dHBzOi8vZ2l0aHViLmNvbS9ibHVlaW1wL2pRdWVyeS1GaWxlLVVwbG9hZFxuICpcbiAqIENvcHlyaWdodCAyMDEyLCBTZWJhc3RpYW4gVHNjaGFuXG4gKiBodHRwczovL2JsdWVpbXAubmV0XG4gKlxuICogTGljZW5zZWQgdW5kZXIgdGhlIE1JVCBsaWNlbnNlOlxuICogaHR0cDovL3d3dy5vcGVuc291cmNlLm9yZy9saWNlbnNlcy9NSVRcbiAqL1xuXG4vKiBqc2hpbnQgbm9tZW46ZmFsc2UgKi9cbi8qIGdsb2JhbCBkZWZpbmUsIHdpbmRvdyAqL1xuXG4oZnVuY3Rpb24gKGZhY3RvcnkpIHtcbiAgICAndXNlIHN0cmljdCc7XG4gICAgaWYgKHR5cGVvZiBkZWZpbmUgPT09ICdmdW5jdGlvbicgJiYgZGVmaW5lLmFtZCkge1xuICAgICAgICAvLyBSZWdpc3RlciBhcyBhbiBhbm9ueW1vdXMgQU1EIG1vZHVsZTpcbiAgICAgICAgZGVmaW5lKFtcbiAgICAgICAgICAgICdqcXVlcnknLFxuICAgICAgICAgICAgJy4vanF1ZXJ5LmZpbGV1cGxvYWQnXG4gICAgICAgIF0sIGZhY3RvcnkpO1xuICAgIH0gZWxzZSB7XG4gICAgICAgIC8vIEJyb3dzZXIgZ2xvYmFsczpcbiAgICAgICAgZmFjdG9yeShcbiAgICAgICAgICAgIHdpbmRvdy5qUXVlcnlcbiAgICAgICAgKTtcbiAgICB9XG59KGZ1bmN0aW9uICgkKSB7XG4gICAgJ3VzZSBzdHJpY3QnO1xuXG4gICAgdmFyIG9yaWdpbmFsQWRkID0gJC5ibHVlaW1wLmZpbGV1cGxvYWQucHJvdG90eXBlLm9wdGlvbnMuYWRkO1xuXG4gICAgLy8gVGhlIEZpbGUgVXBsb2FkIFByb2Nlc3NpbmcgcGx1Z2luIGV4dGVuZHMgdGhlIGZpbGV1cGxvYWQgd2lkZ2V0XG4gICAgLy8gd2l0aCBmaWxlIHByb2Nlc3NpbmcgZnVuY3Rpb25hbGl0eTpcbiAgICAkLndpZGdldCgnYmx1ZWltcC5maWxldXBsb2FkJywgJC5ibHVlaW1wLmZpbGV1cGxvYWQsIHtcblxuICAgICAgICBvcHRpb25zOiB7XG4gICAgICAgICAgICAvLyBUaGUgbGlzdCBvZiBwcm9jZXNzaW5nIGFjdGlvbnM6XG4gICAgICAgICAgICBwcm9jZXNzUXVldWU6IFtcbiAgICAgICAgICAgICAgICAvKlxuICAgICAgICAgICAgICAgIHtcbiAgICAgICAgICAgICAgICAgICAgYWN0aW9uOiAnbG9nJyxcbiAgICAgICAgICAgICAgICAgICAgdHlwZTogJ2RlYnVnJ1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAqL1xuICAgICAgICAgICAgXSxcbiAgICAgICAgICAgIGFkZDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcbiAgICAgICAgICAgICAgICB2YXIgJHRoaXMgPSAkKHRoaXMpO1xuICAgICAgICAgICAgICAgIGRhdGEucHJvY2VzcyhmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgIHJldHVybiAkdGhpcy5maWxldXBsb2FkKCdwcm9jZXNzJywgZGF0YSk7XG4gICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgb3JpZ2luYWxBZGQuY2FsbCh0aGlzLCBlLCBkYXRhKTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfSxcblxuICAgICAgICBwcm9jZXNzQWN0aW9uczoge1xuICAgICAgICAgICAgLypcbiAgICAgICAgICAgIGxvZzogZnVuY3Rpb24gKGRhdGEsIG9wdGlvbnMpIHtcbiAgICAgICAgICAgICAgICBjb25zb2xlW29wdGlvbnMudHlwZV0oXG4gICAgICAgICAgICAgICAgICAgICdQcm9jZXNzaW5nIFwiJyArIGRhdGEuZmlsZXNbZGF0YS5pbmRleF0ubmFtZSArICdcIidcbiAgICAgICAgICAgICAgICApO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgKi9cbiAgICAgICAgfSxcblxuICAgICAgICBfcHJvY2Vzc0ZpbGU6IGZ1bmN0aW9uIChkYXRhLCBvcmlnaW5hbERhdGEpIHtcbiAgICAgICAgICAgIHZhciB0aGF0ID0gdGhpcyxcbiAgICAgICAgICAgICAgICBkZmQgPSAkLkRlZmVycmVkKCkucmVzb2x2ZVdpdGgodGhhdCwgW2RhdGFdKSxcbiAgICAgICAgICAgICAgICBjaGFpbiA9IGRmZC5wcm9taXNlKCk7XG4gICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdwcm9jZXNzJywgbnVsbCwgZGF0YSk7XG4gICAgICAgICAgICAkLmVhY2goZGF0YS5wcm9jZXNzUXVldWUsIGZ1bmN0aW9uIChpLCBzZXR0aW5ncykge1xuICAgICAgICAgICAgICAgIHZhciBmdW5jID0gZnVuY3Rpb24gKGRhdGEpIHtcbiAgICAgICAgICAgICAgICAgICAgaWYgKG9yaWdpbmFsRGF0YS5lcnJvclRocm93bikge1xuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuICQuRGVmZXJyZWQoKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAucmVqZWN0V2l0aCh0aGF0LCBbb3JpZ2luYWxEYXRhXSkucHJvbWlzZSgpO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgICAgIHJldHVybiB0aGF0LnByb2Nlc3NBY3Rpb25zW3NldHRpbmdzLmFjdGlvbl0uY2FsbChcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQsXG4gICAgICAgICAgICAgICAgICAgICAgICBkYXRhLFxuICAgICAgICAgICAgICAgICAgICAgICAgc2V0dGluZ3NcbiAgICAgICAgICAgICAgICAgICAgKTtcbiAgICAgICAgICAgICAgICB9O1xuICAgICAgICAgICAgICAgIGNoYWluID0gY2hhaW4ucGlwZShmdW5jLCBzZXR0aW5ncy5hbHdheXMgJiYgZnVuYyk7XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgICAgIGNoYWluXG4gICAgICAgICAgICAgICAgLmRvbmUoZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICB0aGF0Ll90cmlnZ2VyKCdwcm9jZXNzZG9uZScsIG51bGwsIGRhdGEpO1xuICAgICAgICAgICAgICAgICAgICB0aGF0Ll90cmlnZ2VyKCdwcm9jZXNzYWx3YXlzJywgbnVsbCwgZGF0YSk7XG4gICAgICAgICAgICAgICAgfSlcbiAgICAgICAgICAgICAgICAuZmFpbChmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgIHRoYXQuX3RyaWdnZXIoJ3Byb2Nlc3NmYWlsJywgbnVsbCwgZGF0YSk7XG4gICAgICAgICAgICAgICAgICAgIHRoYXQuX3RyaWdnZXIoJ3Byb2Nlc3NhbHdheXMnLCBudWxsLCBkYXRhKTtcbiAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgIHJldHVybiBjaGFpbjtcbiAgICAgICAgfSxcblxuICAgICAgICAvLyBSZXBsYWNlcyB0aGUgc2V0dGluZ3Mgb2YgZWFjaCBwcm9jZXNzUXVldWUgaXRlbSB0aGF0XG4gICAgICAgIC8vIGFyZSBzdHJpbmdzIHN0YXJ0aW5nIHdpdGggYW4gXCJAXCIsIHVzaW5nIHRoZSByZW1haW5pbmdcbiAgICAgICAgLy8gc3Vic3RyaW5nIGFzIGtleSBmb3IgdGhlIG9wdGlvbiBtYXAsXG4gICAgICAgIC8vIGUuZy4gXCJAYXV0b1VwbG9hZFwiIGlzIHJlcGxhY2VkIHdpdGggb3B0aW9ucy5hdXRvVXBsb2FkOlxuICAgICAgICBfdHJhbnNmb3JtUHJvY2Vzc1F1ZXVlOiBmdW5jdGlvbiAob3B0aW9ucykge1xuICAgICAgICAgICAgdmFyIHByb2Nlc3NRdWV1ZSA9IFtdO1xuICAgICAgICAgICAgJC5lYWNoKG9wdGlvbnMucHJvY2Vzc1F1ZXVlLCBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgdmFyIHNldHRpbmdzID0ge30sXG4gICAgICAgICAgICAgICAgICAgIGFjdGlvbiA9IHRoaXMuYWN0aW9uLFxuICAgICAgICAgICAgICAgICAgICBwcmVmaXggPSB0aGlzLnByZWZpeCA9PT0gdHJ1ZSA/IGFjdGlvbiA6IHRoaXMucHJlZml4O1xuICAgICAgICAgICAgICAgICQuZWFjaCh0aGlzLCBmdW5jdGlvbiAoa2V5LCB2YWx1ZSkge1xuICAgICAgICAgICAgICAgICAgICBpZiAoJC50eXBlKHZhbHVlKSA9PT0gJ3N0cmluZycgJiZcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YWx1ZS5jaGFyQXQoMCkgPT09ICdAJykge1xuICAgICAgICAgICAgICAgICAgICAgICAgc2V0dGluZ3Nba2V5XSA9IG9wdGlvbnNbXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFsdWUuc2xpY2UoMSkgfHwgKHByZWZpeCA/IHByZWZpeCArXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGtleS5jaGFyQXQoMCkudG9VcHBlckNhc2UoKSArIGtleS5zbGljZSgxKSA6IGtleSlcbiAgICAgICAgICAgICAgICAgICAgICAgIF07XG4gICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBzZXR0aW5nc1trZXldID0gdmFsdWU7XG4gICAgICAgICAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgICAgIHByb2Nlc3NRdWV1ZS5wdXNoKHNldHRpbmdzKTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgb3B0aW9ucy5wcm9jZXNzUXVldWUgPSBwcm9jZXNzUXVldWU7XG4gICAgICAgIH0sXG5cbiAgICAgICAgLy8gUmV0dXJucyB0aGUgbnVtYmVyIG9mIGZpbGVzIGN1cnJlbnRseSBpbiB0aGUgcHJvY2Vzc3NpbmcgcXVldWU6XG4gICAgICAgIHByb2Nlc3Npbmc6IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9wcm9jZXNzaW5nO1xuICAgICAgICB9LFxuXG4gICAgICAgIC8vIFByb2Nlc3NlcyB0aGUgZmlsZXMgZ2l2ZW4gYXMgZmlsZXMgcHJvcGVydHkgb2YgdGhlIGRhdGEgcGFyYW1ldGVyLFxuICAgICAgICAvLyByZXR1cm5zIGEgUHJvbWlzZSBvYmplY3QgdGhhdCBhbGxvd3MgdG8gYmluZCBjYWxsYmFja3M6XG4gICAgICAgIHByb2Nlc3M6IGZ1bmN0aW9uIChkYXRhKSB7XG4gICAgICAgICAgICB2YXIgdGhhdCA9IHRoaXMsXG4gICAgICAgICAgICAgICAgb3B0aW9ucyA9ICQuZXh0ZW5kKHt9LCB0aGlzLm9wdGlvbnMsIGRhdGEpO1xuICAgICAgICAgICAgaWYgKG9wdGlvbnMucHJvY2Vzc1F1ZXVlICYmIG9wdGlvbnMucHJvY2Vzc1F1ZXVlLmxlbmd0aCkge1xuICAgICAgICAgICAgICAgIHRoaXMuX3RyYW5zZm9ybVByb2Nlc3NRdWV1ZShvcHRpb25zKTtcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5fcHJvY2Vzc2luZyA9PT0gMCkge1xuICAgICAgICAgICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdwcm9jZXNzc3RhcnQnKTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgJC5lYWNoKGRhdGEuZmlsZXMsIGZ1bmN0aW9uIChpbmRleCkge1xuICAgICAgICAgICAgICAgICAgICB2YXIgb3B0cyA9IGluZGV4ID8gJC5leHRlbmQoe30sIG9wdGlvbnMpIDogb3B0aW9ucyxcbiAgICAgICAgICAgICAgICAgICAgICAgIGZ1bmMgPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGRhdGEuZXJyb3JUaHJvd24pIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuICQuRGVmZXJyZWQoKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5yZWplY3RXaXRoKHRoYXQsIFtkYXRhXSkucHJvbWlzZSgpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhhdC5fcHJvY2Vzc0ZpbGUob3B0cywgZGF0YSk7XG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xuICAgICAgICAgICAgICAgICAgICBvcHRzLmluZGV4ID0gaW5kZXg7XG4gICAgICAgICAgICAgICAgICAgIHRoYXQuX3Byb2Nlc3NpbmcgKz0gMTtcbiAgICAgICAgICAgICAgICAgICAgdGhhdC5fcHJvY2Vzc2luZ1F1ZXVlID0gdGhhdC5fcHJvY2Vzc2luZ1F1ZXVlLnBpcGUoZnVuYywgZnVuYylcbiAgICAgICAgICAgICAgICAgICAgICAgIC5hbHdheXMoZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQuX3Byb2Nlc3NpbmcgLT0gMTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAodGhhdC5fcHJvY2Vzc2luZyA9PT0gMCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGF0Ll90cmlnZ2VyKCdwcm9jZXNzc3RvcCcpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX3Byb2Nlc3NpbmdRdWV1ZTtcbiAgICAgICAgfSxcblxuICAgICAgICBfY3JlYXRlOiBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICB0aGlzLl9zdXBlcigpO1xuICAgICAgICAgICAgdGhpcy5fcHJvY2Vzc2luZyA9IDA7XG4gICAgICAgICAgICB0aGlzLl9wcm9jZXNzaW5nUXVldWUgPSAkLkRlZmVycmVkKCkucmVzb2x2ZVdpdGgodGhpcylcbiAgICAgICAgICAgICAgICAucHJvbWlzZSgpO1xuICAgICAgICB9XG5cbiAgICB9KTtcblxufSkpO1xuIl0sImZpbGUiOiJqcXVlcnkuZmlsZXVwbG9hZC1wcm9jZXNzLmpzIiwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=