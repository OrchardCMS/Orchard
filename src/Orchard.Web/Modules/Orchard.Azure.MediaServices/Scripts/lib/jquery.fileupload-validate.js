/*
 * jQuery File Upload Validation Plugin 1.1.2
 * https://github.com/blueimp/jQuery-File-Upload
 *
 * Copyright 2013, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * http://www.opensource.org/licenses/MIT
 */

/* global define, window */

(function (factory) {
    'use strict';
    if (typeof define === 'function' && define.amd) {
        // Register as an anonymous AMD module:
        define([
            'jquery',
            './jquery.fileupload-process'
        ], factory);
    } else {
        // Browser globals:
        factory(
            window.jQuery
        );
    }
}(function ($) {
    'use strict';

    // Append to the default processQueue:
    $.blueimp.fileupload.prototype.options.processQueue.push(
        {
            action: 'validate',
            // Always trigger this action,
            // even if the previous action was rejected: 
            always: true,
            // Options taken from the global options map:
            acceptFileTypes: '@',
            maxFileSize: '@',
            minFileSize: '@',
            maxNumberOfFiles: '@',
            disabled: '@disableValidation'
        }
    );

    // The File Upload Validation plugin extends the fileupload widget
    // with file validation functionality:
    $.widget('blueimp.fileupload', $.blueimp.fileupload, {

        options: {
            /*
            // The regular expression for allowed file types, matches
            // against either file type or file name:
            acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
            // The maximum allowed file size in bytes:
            maxFileSize: 10000000, // 10 MB
            // The minimum allowed file size in bytes:
            minFileSize: undefined, // No minimal file size
            // The limit of files to be uploaded:
            maxNumberOfFiles: 10,
            */

            // Function returning the current number of files,
            // has to be overriden for maxNumberOfFiles validation:
            getNumberOfFiles: $.noop,

            // Error and info messages:
            messages: {
                maxNumberOfFiles: 'Maximum number of files exceeded',
                acceptFileTypes: 'File type not allowed',
                maxFileSize: 'File is too large',
                minFileSize: 'File is too small'
            }
        },

        processActions: {

            validate: function (data, options) {
                if (options.disabled) {
                    return data;
                }
                var dfd = $.Deferred(),
                    settings = this.options,
                    file = data.files[data.index],
                    fileSize;
                if (options.minFileSize || options.maxFileSize) {
                    fileSize = file.size;
                }
                if ($.type(options.maxNumberOfFiles) === 'number' &&
                        (settings.getNumberOfFiles() || 0) + data.files.length >
                            options.maxNumberOfFiles) {
                    file.error = settings.i18n('maxNumberOfFiles');
                } else if (options.acceptFileTypes &&
                        !(options.acceptFileTypes.test(file.type) ||
                        options.acceptFileTypes.test(file.name))) {
                    file.error = settings.i18n('acceptFileTypes');
                } else if (fileSize > options.maxFileSize) {
                    file.error = settings.i18n('maxFileSize');
                } else if ($.type(fileSize) === 'number' &&
                        fileSize < options.minFileSize) {
                    file.error = settings.i18n('minFileSize');
                } else {
                    delete file.error;
                }
                if (file.error || data.files.error) {
                    data.files.error = true;
                    dfd.rejectWith(this, [data]);
                } else {
                    dfd.resolveWith(this, [data]);
                }
                return dfd.promise();
            }

        }

    });

}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqcXVlcnkuZmlsZXVwbG9hZC12YWxpZGF0ZS5qcyJdLCJzb3VyY2VzQ29udGVudCI6WyIvKlxyXG4gKiBqUXVlcnkgRmlsZSBVcGxvYWQgVmFsaWRhdGlvbiBQbHVnaW4gMS4xLjJcclxuICogaHR0cHM6Ly9naXRodWIuY29tL2JsdWVpbXAvalF1ZXJ5LUZpbGUtVXBsb2FkXHJcbiAqXHJcbiAqIENvcHlyaWdodCAyMDEzLCBTZWJhc3RpYW4gVHNjaGFuXHJcbiAqIGh0dHBzOi8vYmx1ZWltcC5uZXRcclxuICpcclxuICogTGljZW5zZWQgdW5kZXIgdGhlIE1JVCBsaWNlbnNlOlxyXG4gKiBodHRwOi8vd3d3Lm9wZW5zb3VyY2Uub3JnL2xpY2Vuc2VzL01JVFxyXG4gKi9cclxuXHJcbi8qIGdsb2JhbCBkZWZpbmUsIHdpbmRvdyAqL1xyXG5cclxuKGZ1bmN0aW9uIChmYWN0b3J5KSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcbiAgICBpZiAodHlwZW9mIGRlZmluZSA9PT0gJ2Z1bmN0aW9uJyAmJiBkZWZpbmUuYW1kKSB7XHJcbiAgICAgICAgLy8gUmVnaXN0ZXIgYXMgYW4gYW5vbnltb3VzIEFNRCBtb2R1bGU6XHJcbiAgICAgICAgZGVmaW5lKFtcclxuICAgICAgICAgICAgJ2pxdWVyeScsXHJcbiAgICAgICAgICAgICcuL2pxdWVyeS5maWxldXBsb2FkLXByb2Nlc3MnXHJcbiAgICAgICAgXSwgZmFjdG9yeSk7XHJcbiAgICB9IGVsc2Uge1xyXG4gICAgICAgIC8vIEJyb3dzZXIgZ2xvYmFsczpcclxuICAgICAgICBmYWN0b3J5KFxyXG4gICAgICAgICAgICB3aW5kb3cualF1ZXJ5XHJcbiAgICAgICAgKTtcclxuICAgIH1cclxufShmdW5jdGlvbiAoJCkge1xyXG4gICAgJ3VzZSBzdHJpY3QnO1xyXG5cclxuICAgIC8vIEFwcGVuZCB0byB0aGUgZGVmYXVsdCBwcm9jZXNzUXVldWU6XHJcbiAgICAkLmJsdWVpbXAuZmlsZXVwbG9hZC5wcm90b3R5cGUub3B0aW9ucy5wcm9jZXNzUXVldWUucHVzaChcclxuICAgICAgICB7XHJcbiAgICAgICAgICAgIGFjdGlvbjogJ3ZhbGlkYXRlJyxcclxuICAgICAgICAgICAgLy8gQWx3YXlzIHRyaWdnZXIgdGhpcyBhY3Rpb24sXHJcbiAgICAgICAgICAgIC8vIGV2ZW4gaWYgdGhlIHByZXZpb3VzIGFjdGlvbiB3YXMgcmVqZWN0ZWQ6IFxyXG4gICAgICAgICAgICBhbHdheXM6IHRydWUsXHJcbiAgICAgICAgICAgIC8vIE9wdGlvbnMgdGFrZW4gZnJvbSB0aGUgZ2xvYmFsIG9wdGlvbnMgbWFwOlxyXG4gICAgICAgICAgICBhY2NlcHRGaWxlVHlwZXM6ICdAJyxcclxuICAgICAgICAgICAgbWF4RmlsZVNpemU6ICdAJyxcclxuICAgICAgICAgICAgbWluRmlsZVNpemU6ICdAJyxcclxuICAgICAgICAgICAgbWF4TnVtYmVyT2ZGaWxlczogJ0AnLFxyXG4gICAgICAgICAgICBkaXNhYmxlZDogJ0BkaXNhYmxlVmFsaWRhdGlvbidcclxuICAgICAgICB9XHJcbiAgICApO1xyXG5cclxuICAgIC8vIFRoZSBGaWxlIFVwbG9hZCBWYWxpZGF0aW9uIHBsdWdpbiBleHRlbmRzIHRoZSBmaWxldXBsb2FkIHdpZGdldFxyXG4gICAgLy8gd2l0aCBmaWxlIHZhbGlkYXRpb24gZnVuY3Rpb25hbGl0eTpcclxuICAgICQud2lkZ2V0KCdibHVlaW1wLmZpbGV1cGxvYWQnLCAkLmJsdWVpbXAuZmlsZXVwbG9hZCwge1xyXG5cclxuICAgICAgICBvcHRpb25zOiB7XHJcbiAgICAgICAgICAgIC8qXHJcbiAgICAgICAgICAgIC8vIFRoZSByZWd1bGFyIGV4cHJlc3Npb24gZm9yIGFsbG93ZWQgZmlsZSB0eXBlcywgbWF0Y2hlc1xyXG4gICAgICAgICAgICAvLyBhZ2FpbnN0IGVpdGhlciBmaWxlIHR5cGUgb3IgZmlsZSBuYW1lOlxyXG4gICAgICAgICAgICBhY2NlcHRGaWxlVHlwZXM6IC8oXFwufFxcLykoZ2lmfGpwZT9nfHBuZykkL2ksXHJcbiAgICAgICAgICAgIC8vIFRoZSBtYXhpbXVtIGFsbG93ZWQgZmlsZSBzaXplIGluIGJ5dGVzOlxyXG4gICAgICAgICAgICBtYXhGaWxlU2l6ZTogMTAwMDAwMDAsIC8vIDEwIE1CXHJcbiAgICAgICAgICAgIC8vIFRoZSBtaW5pbXVtIGFsbG93ZWQgZmlsZSBzaXplIGluIGJ5dGVzOlxyXG4gICAgICAgICAgICBtaW5GaWxlU2l6ZTogdW5kZWZpbmVkLCAvLyBObyBtaW5pbWFsIGZpbGUgc2l6ZVxyXG4gICAgICAgICAgICAvLyBUaGUgbGltaXQgb2YgZmlsZXMgdG8gYmUgdXBsb2FkZWQ6XHJcbiAgICAgICAgICAgIG1heE51bWJlck9mRmlsZXM6IDEwLFxyXG4gICAgICAgICAgICAqL1xyXG5cclxuICAgICAgICAgICAgLy8gRnVuY3Rpb24gcmV0dXJuaW5nIHRoZSBjdXJyZW50IG51bWJlciBvZiBmaWxlcyxcclxuICAgICAgICAgICAgLy8gaGFzIHRvIGJlIG92ZXJyaWRlbiBmb3IgbWF4TnVtYmVyT2ZGaWxlcyB2YWxpZGF0aW9uOlxyXG4gICAgICAgICAgICBnZXROdW1iZXJPZkZpbGVzOiAkLm5vb3AsXHJcblxyXG4gICAgICAgICAgICAvLyBFcnJvciBhbmQgaW5mbyBtZXNzYWdlczpcclxuICAgICAgICAgICAgbWVzc2FnZXM6IHtcclxuICAgICAgICAgICAgICAgIG1heE51bWJlck9mRmlsZXM6ICdNYXhpbXVtIG51bWJlciBvZiBmaWxlcyBleGNlZWRlZCcsXHJcbiAgICAgICAgICAgICAgICBhY2NlcHRGaWxlVHlwZXM6ICdGaWxlIHR5cGUgbm90IGFsbG93ZWQnLFxyXG4gICAgICAgICAgICAgICAgbWF4RmlsZVNpemU6ICdGaWxlIGlzIHRvbyBsYXJnZScsXHJcbiAgICAgICAgICAgICAgICBtaW5GaWxlU2l6ZTogJ0ZpbGUgaXMgdG9vIHNtYWxsJ1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJvY2Vzc0FjdGlvbnM6IHtcclxuXHJcbiAgICAgICAgICAgIHZhbGlkYXRlOiBmdW5jdGlvbiAoZGF0YSwgb3B0aW9ucykge1xyXG4gICAgICAgICAgICAgICAgaWYgKG9wdGlvbnMuZGlzYWJsZWQpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZGF0YTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIHZhciBkZmQgPSAkLkRlZmVycmVkKCksXHJcbiAgICAgICAgICAgICAgICAgICAgc2V0dGluZ3MgPSB0aGlzLm9wdGlvbnMsXHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZSA9IGRhdGEuZmlsZXNbZGF0YS5pbmRleF0sXHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZVNpemU7XHJcbiAgICAgICAgICAgICAgICBpZiAob3B0aW9ucy5taW5GaWxlU2l6ZSB8fCBvcHRpb25zLm1heEZpbGVTaXplKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZVNpemUgPSBmaWxlLnNpemU7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBpZiAoJC50eXBlKG9wdGlvbnMubWF4TnVtYmVyT2ZGaWxlcykgPT09ICdudW1iZXInICYmXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIChzZXR0aW5ncy5nZXROdW1iZXJPZkZpbGVzKCkgfHwgMCkgKyBkYXRhLmZpbGVzLmxlbmd0aCA+XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBvcHRpb25zLm1heE51bWJlck9mRmlsZXMpIHtcclxuICAgICAgICAgICAgICAgICAgICBmaWxlLmVycm9yID0gc2V0dGluZ3MuaTE4bignbWF4TnVtYmVyT2ZGaWxlcycpO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIGlmIChvcHRpb25zLmFjY2VwdEZpbGVUeXBlcyAmJlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAhKG9wdGlvbnMuYWNjZXB0RmlsZVR5cGVzLnRlc3QoZmlsZS50eXBlKSB8fFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBvcHRpb25zLmFjY2VwdEZpbGVUeXBlcy50ZXN0KGZpbGUubmFtZSkpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZS5lcnJvciA9IHNldHRpbmdzLmkxOG4oJ2FjY2VwdEZpbGVUeXBlcycpO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIGlmIChmaWxlU2l6ZSA+IG9wdGlvbnMubWF4RmlsZVNpemUpIHtcclxuICAgICAgICAgICAgICAgICAgICBmaWxlLmVycm9yID0gc2V0dGluZ3MuaTE4bignbWF4RmlsZVNpemUnKTtcclxuICAgICAgICAgICAgICAgIH0gZWxzZSBpZiAoJC50eXBlKGZpbGVTaXplKSA9PT0gJ251bWJlcicgJiZcclxuICAgICAgICAgICAgICAgICAgICAgICAgZmlsZVNpemUgPCBvcHRpb25zLm1pbkZpbGVTaXplKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZS5lcnJvciA9IHNldHRpbmdzLmkxOG4oJ21pbkZpbGVTaXplJyk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIGRlbGV0ZSBmaWxlLmVycm9yO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgaWYgKGZpbGUuZXJyb3IgfHwgZGF0YS5maWxlcy5lcnJvcikge1xyXG4gICAgICAgICAgICAgICAgICAgIGRhdGEuZmlsZXMuZXJyb3IgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgIGRmZC5yZWplY3RXaXRoKHRoaXMsIFtkYXRhXSk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIGRmZC5yZXNvbHZlV2l0aCh0aGlzLCBbZGF0YV0pO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGRmZC5wcm9taXNlKCk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgfVxyXG5cclxuICAgIH0pO1xyXG5cclxufSkpO1xyXG4iXSwiZmlsZSI6ImpxdWVyeS5maWxldXBsb2FkLXZhbGlkYXRlLmpzIiwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=