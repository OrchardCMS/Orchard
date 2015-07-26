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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqcXVlcnkuZmlsZXVwbG9hZC12YWxpZGF0ZS5qcyJdLCJzb3VyY2VzQ29udGVudCI6WyIvKlxuICogalF1ZXJ5IEZpbGUgVXBsb2FkIFZhbGlkYXRpb24gUGx1Z2luIDEuMS4yXG4gKiBodHRwczovL2dpdGh1Yi5jb20vYmx1ZWltcC9qUXVlcnktRmlsZS1VcGxvYWRcbiAqXG4gKiBDb3B5cmlnaHQgMjAxMywgU2ViYXN0aWFuIFRzY2hhblxuICogaHR0cHM6Ly9ibHVlaW1wLm5ldFxuICpcbiAqIExpY2Vuc2VkIHVuZGVyIHRoZSBNSVQgbGljZW5zZTpcbiAqIGh0dHA6Ly93d3cub3BlbnNvdXJjZS5vcmcvbGljZW5zZXMvTUlUXG4gKi9cblxuLyogZ2xvYmFsIGRlZmluZSwgd2luZG93ICovXG5cbihmdW5jdGlvbiAoZmFjdG9yeSkge1xuICAgICd1c2Ugc3RyaWN0JztcbiAgICBpZiAodHlwZW9mIGRlZmluZSA9PT0gJ2Z1bmN0aW9uJyAmJiBkZWZpbmUuYW1kKSB7XG4gICAgICAgIC8vIFJlZ2lzdGVyIGFzIGFuIGFub255bW91cyBBTUQgbW9kdWxlOlxuICAgICAgICBkZWZpbmUoW1xuICAgICAgICAgICAgJ2pxdWVyeScsXG4gICAgICAgICAgICAnLi9qcXVlcnkuZmlsZXVwbG9hZC1wcm9jZXNzJ1xuICAgICAgICBdLCBmYWN0b3J5KTtcbiAgICB9IGVsc2Uge1xuICAgICAgICAvLyBCcm93c2VyIGdsb2JhbHM6XG4gICAgICAgIGZhY3RvcnkoXG4gICAgICAgICAgICB3aW5kb3cualF1ZXJ5XG4gICAgICAgICk7XG4gICAgfVxufShmdW5jdGlvbiAoJCkge1xuICAgICd1c2Ugc3RyaWN0JztcblxuICAgIC8vIEFwcGVuZCB0byB0aGUgZGVmYXVsdCBwcm9jZXNzUXVldWU6XG4gICAgJC5ibHVlaW1wLmZpbGV1cGxvYWQucHJvdG90eXBlLm9wdGlvbnMucHJvY2Vzc1F1ZXVlLnB1c2goXG4gICAgICAgIHtcbiAgICAgICAgICAgIGFjdGlvbjogJ3ZhbGlkYXRlJyxcbiAgICAgICAgICAgIC8vIEFsd2F5cyB0cmlnZ2VyIHRoaXMgYWN0aW9uLFxuICAgICAgICAgICAgLy8gZXZlbiBpZiB0aGUgcHJldmlvdXMgYWN0aW9uIHdhcyByZWplY3RlZDogXG4gICAgICAgICAgICBhbHdheXM6IHRydWUsXG4gICAgICAgICAgICAvLyBPcHRpb25zIHRha2VuIGZyb20gdGhlIGdsb2JhbCBvcHRpb25zIG1hcDpcbiAgICAgICAgICAgIGFjY2VwdEZpbGVUeXBlczogJ0AnLFxuICAgICAgICAgICAgbWF4RmlsZVNpemU6ICdAJyxcbiAgICAgICAgICAgIG1pbkZpbGVTaXplOiAnQCcsXG4gICAgICAgICAgICBtYXhOdW1iZXJPZkZpbGVzOiAnQCcsXG4gICAgICAgICAgICBkaXNhYmxlZDogJ0BkaXNhYmxlVmFsaWRhdGlvbidcbiAgICAgICAgfVxuICAgICk7XG5cbiAgICAvLyBUaGUgRmlsZSBVcGxvYWQgVmFsaWRhdGlvbiBwbHVnaW4gZXh0ZW5kcyB0aGUgZmlsZXVwbG9hZCB3aWRnZXRcbiAgICAvLyB3aXRoIGZpbGUgdmFsaWRhdGlvbiBmdW5jdGlvbmFsaXR5OlxuICAgICQud2lkZ2V0KCdibHVlaW1wLmZpbGV1cGxvYWQnLCAkLmJsdWVpbXAuZmlsZXVwbG9hZCwge1xuXG4gICAgICAgIG9wdGlvbnM6IHtcbiAgICAgICAgICAgIC8qXG4gICAgICAgICAgICAvLyBUaGUgcmVndWxhciBleHByZXNzaW9uIGZvciBhbGxvd2VkIGZpbGUgdHlwZXMsIG1hdGNoZXNcbiAgICAgICAgICAgIC8vIGFnYWluc3QgZWl0aGVyIGZpbGUgdHlwZSBvciBmaWxlIG5hbWU6XG4gICAgICAgICAgICBhY2NlcHRGaWxlVHlwZXM6IC8oXFwufFxcLykoZ2lmfGpwZT9nfHBuZykkL2ksXG4gICAgICAgICAgICAvLyBUaGUgbWF4aW11bSBhbGxvd2VkIGZpbGUgc2l6ZSBpbiBieXRlczpcbiAgICAgICAgICAgIG1heEZpbGVTaXplOiAxMDAwMDAwMCwgLy8gMTAgTUJcbiAgICAgICAgICAgIC8vIFRoZSBtaW5pbXVtIGFsbG93ZWQgZmlsZSBzaXplIGluIGJ5dGVzOlxuICAgICAgICAgICAgbWluRmlsZVNpemU6IHVuZGVmaW5lZCwgLy8gTm8gbWluaW1hbCBmaWxlIHNpemVcbiAgICAgICAgICAgIC8vIFRoZSBsaW1pdCBvZiBmaWxlcyB0byBiZSB1cGxvYWRlZDpcbiAgICAgICAgICAgIG1heE51bWJlck9mRmlsZXM6IDEwLFxuICAgICAgICAgICAgKi9cblxuICAgICAgICAgICAgLy8gRnVuY3Rpb24gcmV0dXJuaW5nIHRoZSBjdXJyZW50IG51bWJlciBvZiBmaWxlcyxcbiAgICAgICAgICAgIC8vIGhhcyB0byBiZSBvdmVycmlkZW4gZm9yIG1heE51bWJlck9mRmlsZXMgdmFsaWRhdGlvbjpcbiAgICAgICAgICAgIGdldE51bWJlck9mRmlsZXM6ICQubm9vcCxcblxuICAgICAgICAgICAgLy8gRXJyb3IgYW5kIGluZm8gbWVzc2FnZXM6XG4gICAgICAgICAgICBtZXNzYWdlczoge1xuICAgICAgICAgICAgICAgIG1heE51bWJlck9mRmlsZXM6ICdNYXhpbXVtIG51bWJlciBvZiBmaWxlcyBleGNlZWRlZCcsXG4gICAgICAgICAgICAgICAgYWNjZXB0RmlsZVR5cGVzOiAnRmlsZSB0eXBlIG5vdCBhbGxvd2VkJyxcbiAgICAgICAgICAgICAgICBtYXhGaWxlU2l6ZTogJ0ZpbGUgaXMgdG9vIGxhcmdlJyxcbiAgICAgICAgICAgICAgICBtaW5GaWxlU2l6ZTogJ0ZpbGUgaXMgdG9vIHNtYWxsJ1xuICAgICAgICAgICAgfVxuICAgICAgICB9LFxuXG4gICAgICAgIHByb2Nlc3NBY3Rpb25zOiB7XG5cbiAgICAgICAgICAgIHZhbGlkYXRlOiBmdW5jdGlvbiAoZGF0YSwgb3B0aW9ucykge1xuICAgICAgICAgICAgICAgIGlmIChvcHRpb25zLmRpc2FibGVkKSB7XG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBkYXRhO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICB2YXIgZGZkID0gJC5EZWZlcnJlZCgpLFxuICAgICAgICAgICAgICAgICAgICBzZXR0aW5ncyA9IHRoaXMub3B0aW9ucyxcbiAgICAgICAgICAgICAgICAgICAgZmlsZSA9IGRhdGEuZmlsZXNbZGF0YS5pbmRleF0sXG4gICAgICAgICAgICAgICAgICAgIGZpbGVTaXplO1xuICAgICAgICAgICAgICAgIGlmIChvcHRpb25zLm1pbkZpbGVTaXplIHx8IG9wdGlvbnMubWF4RmlsZVNpemUpIHtcbiAgICAgICAgICAgICAgICAgICAgZmlsZVNpemUgPSBmaWxlLnNpemU7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIGlmICgkLnR5cGUob3B0aW9ucy5tYXhOdW1iZXJPZkZpbGVzKSA9PT0gJ251bWJlcicgJiZcbiAgICAgICAgICAgICAgICAgICAgICAgIChzZXR0aW5ncy5nZXROdW1iZXJPZkZpbGVzKCkgfHwgMCkgKyBkYXRhLmZpbGVzLmxlbmd0aCA+XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgb3B0aW9ucy5tYXhOdW1iZXJPZkZpbGVzKSB7XG4gICAgICAgICAgICAgICAgICAgIGZpbGUuZXJyb3IgPSBzZXR0aW5ncy5pMThuKCdtYXhOdW1iZXJPZkZpbGVzJyk7XG4gICAgICAgICAgICAgICAgfSBlbHNlIGlmIChvcHRpb25zLmFjY2VwdEZpbGVUeXBlcyAmJlxuICAgICAgICAgICAgICAgICAgICAgICAgIShvcHRpb25zLmFjY2VwdEZpbGVUeXBlcy50ZXN0KGZpbGUudHlwZSkgfHxcbiAgICAgICAgICAgICAgICAgICAgICAgIG9wdGlvbnMuYWNjZXB0RmlsZVR5cGVzLnRlc3QoZmlsZS5uYW1lKSkpIHtcbiAgICAgICAgICAgICAgICAgICAgZmlsZS5lcnJvciA9IHNldHRpbmdzLmkxOG4oJ2FjY2VwdEZpbGVUeXBlcycpO1xuICAgICAgICAgICAgICAgIH0gZWxzZSBpZiAoZmlsZVNpemUgPiBvcHRpb25zLm1heEZpbGVTaXplKSB7XG4gICAgICAgICAgICAgICAgICAgIGZpbGUuZXJyb3IgPSBzZXR0aW5ncy5pMThuKCdtYXhGaWxlU2l6ZScpO1xuICAgICAgICAgICAgICAgIH0gZWxzZSBpZiAoJC50eXBlKGZpbGVTaXplKSA9PT0gJ251bWJlcicgJiZcbiAgICAgICAgICAgICAgICAgICAgICAgIGZpbGVTaXplIDwgb3B0aW9ucy5taW5GaWxlU2l6ZSkge1xuICAgICAgICAgICAgICAgICAgICBmaWxlLmVycm9yID0gc2V0dGluZ3MuaTE4bignbWluRmlsZVNpemUnKTtcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgICAgICAgICBkZWxldGUgZmlsZS5lcnJvcjtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgaWYgKGZpbGUuZXJyb3IgfHwgZGF0YS5maWxlcy5lcnJvcikge1xuICAgICAgICAgICAgICAgICAgICBkYXRhLmZpbGVzLmVycm9yID0gdHJ1ZTtcbiAgICAgICAgICAgICAgICAgICAgZGZkLnJlamVjdFdpdGgodGhpcywgW2RhdGFdKTtcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgICAgICAgICBkZmQucmVzb2x2ZVdpdGgodGhpcywgW2RhdGFdKTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgcmV0dXJuIGRmZC5wcm9taXNlKCk7XG4gICAgICAgICAgICB9XG5cbiAgICAgICAgfVxuXG4gICAgfSk7XG5cbn0pKTtcbiJdLCJmaWxlIjoianF1ZXJ5LmZpbGV1cGxvYWQtdmFsaWRhdGUuanMiLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==