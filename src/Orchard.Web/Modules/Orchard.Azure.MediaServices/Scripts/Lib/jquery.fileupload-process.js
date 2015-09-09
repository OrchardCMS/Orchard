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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqcXVlcnkuZmlsZXVwbG9hZC1wcm9jZXNzLmpzIl0sInNvdXJjZXNDb250ZW50IjpbIi8qXHJcbiAqIGpRdWVyeSBGaWxlIFVwbG9hZCBQcm9jZXNzaW5nIFBsdWdpbiAxLjMuMFxyXG4gKiBodHRwczovL2dpdGh1Yi5jb20vYmx1ZWltcC9qUXVlcnktRmlsZS1VcGxvYWRcclxuICpcclxuICogQ29weXJpZ2h0IDIwMTIsIFNlYmFzdGlhbiBUc2NoYW5cclxuICogaHR0cHM6Ly9ibHVlaW1wLm5ldFxyXG4gKlxyXG4gKiBMaWNlbnNlZCB1bmRlciB0aGUgTUlUIGxpY2Vuc2U6XHJcbiAqIGh0dHA6Ly93d3cub3BlbnNvdXJjZS5vcmcvbGljZW5zZXMvTUlUXHJcbiAqL1xyXG5cclxuLyoganNoaW50IG5vbWVuOmZhbHNlICovXHJcbi8qIGdsb2JhbCBkZWZpbmUsIHdpbmRvdyAqL1xyXG5cclxuKGZ1bmN0aW9uIChmYWN0b3J5KSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcbiAgICBpZiAodHlwZW9mIGRlZmluZSA9PT0gJ2Z1bmN0aW9uJyAmJiBkZWZpbmUuYW1kKSB7XHJcbiAgICAgICAgLy8gUmVnaXN0ZXIgYXMgYW4gYW5vbnltb3VzIEFNRCBtb2R1bGU6XHJcbiAgICAgICAgZGVmaW5lKFtcclxuICAgICAgICAgICAgJ2pxdWVyeScsXHJcbiAgICAgICAgICAgICcuL2pxdWVyeS5maWxldXBsb2FkJ1xyXG4gICAgICAgIF0sIGZhY3RvcnkpO1xyXG4gICAgfSBlbHNlIHtcclxuICAgICAgICAvLyBCcm93c2VyIGdsb2JhbHM6XHJcbiAgICAgICAgZmFjdG9yeShcclxuICAgICAgICAgICAgd2luZG93LmpRdWVyeVxyXG4gICAgICAgICk7XHJcbiAgICB9XHJcbn0oZnVuY3Rpb24gKCQpIHtcclxuICAgICd1c2Ugc3RyaWN0JztcclxuXHJcbiAgICB2YXIgb3JpZ2luYWxBZGQgPSAkLmJsdWVpbXAuZmlsZXVwbG9hZC5wcm90b3R5cGUub3B0aW9ucy5hZGQ7XHJcblxyXG4gICAgLy8gVGhlIEZpbGUgVXBsb2FkIFByb2Nlc3NpbmcgcGx1Z2luIGV4dGVuZHMgdGhlIGZpbGV1cGxvYWQgd2lkZ2V0XHJcbiAgICAvLyB3aXRoIGZpbGUgcHJvY2Vzc2luZyBmdW5jdGlvbmFsaXR5OlxyXG4gICAgJC53aWRnZXQoJ2JsdWVpbXAuZmlsZXVwbG9hZCcsICQuYmx1ZWltcC5maWxldXBsb2FkLCB7XHJcblxyXG4gICAgICAgIG9wdGlvbnM6IHtcclxuICAgICAgICAgICAgLy8gVGhlIGxpc3Qgb2YgcHJvY2Vzc2luZyBhY3Rpb25zOlxyXG4gICAgICAgICAgICBwcm9jZXNzUXVldWU6IFtcclxuICAgICAgICAgICAgICAgIC8qXHJcbiAgICAgICAgICAgICAgICB7XHJcbiAgICAgICAgICAgICAgICAgICAgYWN0aW9uOiAnbG9nJyxcclxuICAgICAgICAgICAgICAgICAgICB0eXBlOiAnZGVidWcnXHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAqL1xyXG4gICAgICAgICAgICBdLFxyXG4gICAgICAgICAgICBhZGQ6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgJHRoaXMgPSAkKHRoaXMpO1xyXG4gICAgICAgICAgICAgICAgZGF0YS5wcm9jZXNzKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gJHRoaXMuZmlsZXVwbG9hZCgncHJvY2VzcycsIGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICBvcmlnaW5hbEFkZC5jYWxsKHRoaXMsIGUsIGRhdGEpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJvY2Vzc0FjdGlvbnM6IHtcclxuICAgICAgICAgICAgLypcclxuICAgICAgICAgICAgbG9nOiBmdW5jdGlvbiAoZGF0YSwgb3B0aW9ucykge1xyXG4gICAgICAgICAgICAgICAgY29uc29sZVtvcHRpb25zLnR5cGVdKFxyXG4gICAgICAgICAgICAgICAgICAgICdQcm9jZXNzaW5nIFwiJyArIGRhdGEuZmlsZXNbZGF0YS5pbmRleF0ubmFtZSArICdcIidcclxuICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgKi9cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfcHJvY2Vzc0ZpbGU6IGZ1bmN0aW9uIChkYXRhLCBvcmlnaW5hbERhdGEpIHtcclxuICAgICAgICAgICAgdmFyIHRoYXQgPSB0aGlzLFxyXG4gICAgICAgICAgICAgICAgZGZkID0gJC5EZWZlcnJlZCgpLnJlc29sdmVXaXRoKHRoYXQsIFtkYXRhXSksXHJcbiAgICAgICAgICAgICAgICBjaGFpbiA9IGRmZC5wcm9taXNlKCk7XHJcbiAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ3Byb2Nlc3MnLCBudWxsLCBkYXRhKTtcclxuICAgICAgICAgICAgJC5lYWNoKGRhdGEucHJvY2Vzc1F1ZXVlLCBmdW5jdGlvbiAoaSwgc2V0dGluZ3MpIHtcclxuICAgICAgICAgICAgICAgIHZhciBmdW5jID0gZnVuY3Rpb24gKGRhdGEpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAob3JpZ2luYWxEYXRhLmVycm9yVGhyb3duKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAkLkRlZmVycmVkKClcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAucmVqZWN0V2l0aCh0aGF0LCBbb3JpZ2luYWxEYXRhXSkucHJvbWlzZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhhdC5wcm9jZXNzQWN0aW9uc1tzZXR0aW5ncy5hY3Rpb25dLmNhbGwoXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRhdGEsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHNldHRpbmdzXHJcbiAgICAgICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgICAgICBjaGFpbiA9IGNoYWluLnBpcGUoZnVuYywgc2V0dGluZ3MuYWx3YXlzICYmIGZ1bmMpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY2hhaW5cclxuICAgICAgICAgICAgICAgIC5kb25lKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGF0Ll90cmlnZ2VyKCdwcm9jZXNzZG9uZScsIG51bGwsIGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoYXQuX3RyaWdnZXIoJ3Byb2Nlc3NhbHdheXMnLCBudWxsLCBkYXRhKTtcclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAuZmFpbChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhhdC5fdHJpZ2dlcigncHJvY2Vzc2ZhaWwnLCBudWxsLCBkYXRhKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGF0Ll90cmlnZ2VyKCdwcm9jZXNzYWx3YXlzJywgbnVsbCwgZGF0YSk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgcmV0dXJuIGNoYWluO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIC8vIFJlcGxhY2VzIHRoZSBzZXR0aW5ncyBvZiBlYWNoIHByb2Nlc3NRdWV1ZSBpdGVtIHRoYXRcclxuICAgICAgICAvLyBhcmUgc3RyaW5ncyBzdGFydGluZyB3aXRoIGFuIFwiQFwiLCB1c2luZyB0aGUgcmVtYWluaW5nXHJcbiAgICAgICAgLy8gc3Vic3RyaW5nIGFzIGtleSBmb3IgdGhlIG9wdGlvbiBtYXAsXHJcbiAgICAgICAgLy8gZS5nLiBcIkBhdXRvVXBsb2FkXCIgaXMgcmVwbGFjZWQgd2l0aCBvcHRpb25zLmF1dG9VcGxvYWQ6XHJcbiAgICAgICAgX3RyYW5zZm9ybVByb2Nlc3NRdWV1ZTogZnVuY3Rpb24gKG9wdGlvbnMpIHtcclxuICAgICAgICAgICAgdmFyIHByb2Nlc3NRdWV1ZSA9IFtdO1xyXG4gICAgICAgICAgICAkLmVhY2gob3B0aW9ucy5wcm9jZXNzUXVldWUsIGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHZhciBzZXR0aW5ncyA9IHt9LFxyXG4gICAgICAgICAgICAgICAgICAgIGFjdGlvbiA9IHRoaXMuYWN0aW9uLFxyXG4gICAgICAgICAgICAgICAgICAgIHByZWZpeCA9IHRoaXMucHJlZml4ID09PSB0cnVlID8gYWN0aW9uIDogdGhpcy5wcmVmaXg7XHJcbiAgICAgICAgICAgICAgICAkLmVhY2godGhpcywgZnVuY3Rpb24gKGtleSwgdmFsdWUpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoJC50eXBlKHZhbHVlKSA9PT0gJ3N0cmluZycgJiZcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhbHVlLmNoYXJBdCgwKSA9PT0gJ0AnKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHNldHRpbmdzW2tleV0gPSBvcHRpb25zW1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFsdWUuc2xpY2UoMSkgfHwgKHByZWZpeCA/IHByZWZpeCArXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAga2V5LmNoYXJBdCgwKS50b1VwcGVyQ2FzZSgpICsga2V5LnNsaWNlKDEpIDoga2V5KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBdO1xyXG4gICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHNldHRpbmdzW2tleV0gPSB2YWx1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICBwcm9jZXNzUXVldWUucHVzaChzZXR0aW5ncyk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBvcHRpb25zLnByb2Nlc3NRdWV1ZSA9IHByb2Nlc3NRdWV1ZTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICAvLyBSZXR1cm5zIHRoZSBudW1iZXIgb2YgZmlsZXMgY3VycmVudGx5IGluIHRoZSBwcm9jZXNzc2luZyBxdWV1ZTpcclxuICAgICAgICBwcm9jZXNzaW5nOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9wcm9jZXNzaW5nO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIC8vIFByb2Nlc3NlcyB0aGUgZmlsZXMgZ2l2ZW4gYXMgZmlsZXMgcHJvcGVydHkgb2YgdGhlIGRhdGEgcGFyYW1ldGVyLFxyXG4gICAgICAgIC8vIHJldHVybnMgYSBQcm9taXNlIG9iamVjdCB0aGF0IGFsbG93cyB0byBiaW5kIGNhbGxiYWNrczpcclxuICAgICAgICBwcm9jZXNzOiBmdW5jdGlvbiAoZGF0YSkge1xyXG4gICAgICAgICAgICB2YXIgdGhhdCA9IHRoaXMsXHJcbiAgICAgICAgICAgICAgICBvcHRpb25zID0gJC5leHRlbmQoe30sIHRoaXMub3B0aW9ucywgZGF0YSk7XHJcbiAgICAgICAgICAgIGlmIChvcHRpb25zLnByb2Nlc3NRdWV1ZSAmJiBvcHRpb25zLnByb2Nlc3NRdWV1ZS5sZW5ndGgpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3RyYW5zZm9ybVByb2Nlc3NRdWV1ZShvcHRpb25zKTtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLl9wcm9jZXNzaW5nID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcigncHJvY2Vzc3N0YXJ0Jyk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAkLmVhY2goZGF0YS5maWxlcywgZnVuY3Rpb24gKGluZGV4KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG9wdHMgPSBpbmRleCA/ICQuZXh0ZW5kKHt9LCBvcHRpb25zKSA6IG9wdGlvbnMsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGZ1bmMgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoZGF0YS5lcnJvclRocm93bikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAkLkRlZmVycmVkKClcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5yZWplY3RXaXRoKHRoYXQsIFtkYXRhXSkucHJvbWlzZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHRoYXQuX3Byb2Nlc3NGaWxlKG9wdHMsIGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICAgICAgICAgIG9wdHMuaW5kZXggPSBpbmRleDtcclxuICAgICAgICAgICAgICAgICAgICB0aGF0Ll9wcm9jZXNzaW5nICs9IDE7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhhdC5fcHJvY2Vzc2luZ1F1ZXVlID0gdGhhdC5fcHJvY2Vzc2luZ1F1ZXVlLnBpcGUoZnVuYywgZnVuYylcclxuICAgICAgICAgICAgICAgICAgICAgICAgLmFsd2F5cyhmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGF0Ll9wcm9jZXNzaW5nIC09IDE7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAodGhhdC5fcHJvY2Vzc2luZyA9PT0gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQuX3RyaWdnZXIoJ3Byb2Nlc3NzdG9wJyk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX3Byb2Nlc3NpbmdRdWV1ZTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfY3JlYXRlOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHRoaXMuX3N1cGVyKCk7XHJcbiAgICAgICAgICAgIHRoaXMuX3Byb2Nlc3NpbmcgPSAwO1xyXG4gICAgICAgICAgICB0aGlzLl9wcm9jZXNzaW5nUXVldWUgPSAkLkRlZmVycmVkKCkucmVzb2x2ZVdpdGgodGhpcylcclxuICAgICAgICAgICAgICAgIC5wcm9taXNlKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgIH0pO1xyXG5cclxufSkpO1xyXG4iXSwiZmlsZSI6ImpxdWVyeS5maWxldXBsb2FkLXByb2Nlc3MuanMiLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==