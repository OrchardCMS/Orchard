/*
 * jQuery File Upload Plugin 5.40.1
 * https://github.com/blueimp/jQuery-File-Upload
 *
 * Copyright 2010, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * http://www.opensource.org/licenses/MIT
 */

/* jshint nomen:false */
/* global define, window, document, location, Blob, FormData */

(function (factory) {
    'use strict';
    if (typeof define === 'function' && define.amd) {
        // Register as an anonymous AMD module:
        define([
            'jquery',
            'jquery.ui.widget'
        ], factory);
    } else {
        // Browser globals:
        factory(window.jQuery);
    }
}(function ($) {
    'use strict';

    // Detect file input support, based on
    // http://viljamis.com/blog/2012/file-upload-support-on-mobile/
    $.support.fileInput = !(new RegExp(
        // Handle devices which give false positives for the feature detection:
        '(Android (1\\.[0156]|2\\.[01]))' +
            '|(Windows Phone (OS 7|8\\.0))|(XBLWP)|(ZuneWP)|(WPDesktop)' +
            '|(w(eb)?OSBrowser)|(webOS)' +
            '|(Kindle/(1\\.0|2\\.[05]|3\\.0))'
    ).test(window.navigator.userAgent) ||
        // Feature detection for all other devices:
        $('<input type="file">').prop('disabled'));

    // The FileReader API is not actually used, but works as feature detection,
    // as some Safari versions (5?) support XHR file uploads via the FormData API,
    // but not non-multipart XHR file uploads.
    // window.XMLHttpRequestUpload is not available on IE10, so we check for
    // window.ProgressEvent instead to detect XHR2 file upload capability:
    $.support.xhrFileUpload = !!(window.ProgressEvent && window.FileReader);
    $.support.xhrFormDataFileUpload = !!window.FormData;

    // Detect support for Blob slicing (required for chunked uploads):
    $.support.blobSlice = window.Blob && (Blob.prototype.slice ||
        Blob.prototype.webkitSlice || Blob.prototype.mozSlice);

    // The fileupload widget listens for change events on file input fields defined
    // via fileInput setting and paste or drop events of the given dropZone.
    // In addition to the default jQuery Widget methods, the fileupload widget
    // exposes the "add" and "send" methods, to add or directly send files using
    // the fileupload API.
    // By default, files added via file input selection, paste, drag & drop or
    // "add" method are uploaded immediately, but it is possible to override
    // the "add" callback option to queue file uploads.
    $.widget('blueimp.fileupload', {

        options: {
            // The drop target element(s), by the default the complete document.
            // Set to null to disable drag & drop support:
            dropZone: $(document),
            // The paste target element(s), by the default the complete document.
            // Set to null to disable paste support:
            pasteZone: $(document),
            // The file input field(s), that are listened to for change events.
            // If undefined, it is set to the file input fields inside
            // of the widget element on plugin initialization.
            // Set to null to disable the change listener.
            fileInput: undefined,
            // By default, the file input field is replaced with a clone after
            // each input field change event. This is required for iframe transport
            // queues and allows change events to be fired for the same file
            // selection, but can be disabled by setting the following option to false:
            replaceFileInput: true,
            // The parameter name for the file form data (the request argument name).
            // If undefined or empty, the name property of the file input field is
            // used, or "files[]" if the file input name property is also empty,
            // can be a string or an array of strings:
            paramName: undefined,
            // By default, each file of a selection is uploaded using an individual
            // request for XHR type uploads. Set to false to upload file
            // selections in one request each:
            singleFileUploads: true,
            // To limit the number of files uploaded with one XHR request,
            // set the following option to an integer greater than 0:
            limitMultiFileUploads: undefined,
            // The following option limits the number of files uploaded with one
            // XHR request to keep the request size under or equal to the defined
            // limit in bytes:
            limitMultiFileUploadSize: undefined,
            // Multipart file uploads add a number of bytes to each uploaded file,
            // therefore the following option adds an overhead for each file used
            // in the limitMultiFileUploadSize configuration:
            limitMultiFileUploadSizeOverhead: 512,
            // Set the following option to true to issue all file upload requests
            // in a sequential order:
            sequentialUploads: false,
            // To limit the number of concurrent uploads,
            // set the following option to an integer greater than 0:
            limitConcurrentUploads: undefined,
            // Set the following option to true to force iframe transport uploads:
            forceIframeTransport: false,
            // Set the following option to the location of a redirect url on the
            // origin server, for cross-domain iframe transport uploads:
            redirect: undefined,
            // The parameter name for the redirect url, sent as part of the form
            // data and set to 'redirect' if this option is empty:
            redirectParamName: undefined,
            // Set the following option to the location of a postMessage window,
            // to enable postMessage transport uploads:
            postMessage: undefined,
            // By default, XHR file uploads are sent as multipart/form-data.
            // The iframe transport is always using multipart/form-data.
            // Set to false to enable non-multipart XHR uploads:
            multipart: true,
            // To upload large files in smaller chunks, set the following option
            // to a preferred maximum chunk size. If set to 0, null or undefined,
            // or the browser does not support the required Blob API, files will
            // be uploaded as a whole.
            maxChunkSize: undefined,
            // When a non-multipart upload or a chunked multipart upload has been
            // aborted, this option can be used to resume the upload by setting
            // it to the size of the already uploaded bytes. This option is most
            // useful when modifying the options object inside of the "add" or
            // "send" callbacks, as the options are cloned for each file upload.
            uploadedBytes: undefined,
            // By default, failed (abort or error) file uploads are removed from the
            // global progress calculation. Set the following option to false to
            // prevent recalculating the global progress data:
            recalculateProgress: true,
            // Interval in milliseconds to calculate and trigger progress events:
            progressInterval: 100,
            // Interval in milliseconds to calculate progress bitrate:
            bitrateInterval: 500,
            // By default, uploads are started automatically when adding files:
            autoUpload: true,

            // Error and info messages:
            messages: {
                uploadedBytes: 'Uploaded bytes exceed file size'
            },

            // Translation function, gets the message key to be translated
            // and an object with context specific data as arguments:
            i18n: function (message, context) {
                message = this.messages[message] || message.toString();
                if (context) {
                    $.each(context, function (key, value) {
                        message = message.replace('{' + key + '}', value);
                    });
                }
                return message;
            },

            // Additional form data to be sent along with the file uploads can be set
            // using this option, which accepts an array of objects with name and
            // value properties, a function returning such an array, a FormData
            // object (for XHR file uploads), or a simple object.
            // The form of the first fileInput is given as parameter to the function:
            formData: function (form) {
                return form.serializeArray();
            },

            // The add callback is invoked as soon as files are added to the fileupload
            // widget (via file input selection, drag & drop, paste or add API call).
            // If the singleFileUploads option is enabled, this callback will be
            // called once for each file in the selection for XHR file uploads, else
            // once for each file selection.
            //
            // The upload starts when the submit method is invoked on the data parameter.
            // The data object contains a files property holding the added files
            // and allows you to override plugin options as well as define ajax settings.
            //
            // Listeners for this callback can also be bound the following way:
            // .bind('fileuploadadd', func);
            //
            // data.submit() returns a Promise object and allows to attach additional
            // handlers using jQuery's Deferred callbacks:
            // data.submit().done(func).fail(func).always(func);
            add: function (e, data) {
                if (e.isDefaultPrevented()) {
                    return false;
                }
                if (data.autoUpload || (data.autoUpload !== false &&
                        $(this).fileupload('option', 'autoUpload'))) {
                    data.process().done(function () {
                        data.submit();
                    });
                }
            },

            // Other callbacks:

            // Callback for the submit event of each file upload:
            // submit: function (e, data) {}, // .bind('fileuploadsubmit', func);

            // Callback for the start of each file upload request:
            // send: function (e, data) {}, // .bind('fileuploadsend', func);

            // Callback for successful uploads:
            // done: function (e, data) {}, // .bind('fileuploaddone', func);

            // Callback for failed (abort or error) uploads:
            // fail: function (e, data) {}, // .bind('fileuploadfail', func);

            // Callback for completed (success, abort or error) requests:
            // always: function (e, data) {}, // .bind('fileuploadalways', func);

            // Callback for upload progress events:
            // progress: function (e, data) {}, // .bind('fileuploadprogress', func);

            // Callback for global upload progress events:
            // progressall: function (e, data) {}, // .bind('fileuploadprogressall', func);

            // Callback for uploads start, equivalent to the global ajaxStart event:
            // start: function (e) {}, // .bind('fileuploadstart', func);

            // Callback for uploads stop, equivalent to the global ajaxStop event:
            // stop: function (e) {}, // .bind('fileuploadstop', func);

            // Callback for change events of the fileInput(s):
            // change: function (e, data) {}, // .bind('fileuploadchange', func);

            // Callback for paste events to the pasteZone(s):
            // paste: function (e, data) {}, // .bind('fileuploadpaste', func);

            // Callback for drop events of the dropZone(s):
            // drop: function (e, data) {}, // .bind('fileuploaddrop', func);

            // Callback for dragover events of the dropZone(s):
            // dragover: function (e) {}, // .bind('fileuploaddragover', func);

            // Callback for the start of each chunk upload request:
            // chunksend: function (e, data) {}, // .bind('fileuploadchunksend', func);

            // Callback for successful chunk uploads:
            // chunkdone: function (e, data) {}, // .bind('fileuploadchunkdone', func);

            // Callback for failed (abort or error) chunk uploads:
            // chunkfail: function (e, data) {}, // .bind('fileuploadchunkfail', func);

            // Callback for completed (success, abort or error) chunk upload requests:
            // chunkalways: function (e, data) {}, // .bind('fileuploadchunkalways', func);

            // The plugin options are used as settings object for the ajax calls.
            // The following are jQuery ajax settings required for the file uploads:
            processData: false,
            contentType: false,
            cache: false
        },

        // A list of options that require reinitializing event listeners and/or
        // special initialization code:
        _specialOptions: [
            'fileInput',
            'dropZone',
            'pasteZone',
            'multipart',
            'forceIframeTransport'
        ],

        _blobSlice: $.support.blobSlice && function () {
            var slice = this.slice || this.webkitSlice || this.mozSlice;
            return slice.apply(this, arguments);
        },

        _BitrateTimer: function () {
            this.timestamp = ((Date.now) ? Date.now() : (new Date()).getTime());
            this.loaded = 0;
            this.bitrate = 0;
            this.getBitrate = function (now, loaded, interval) {
                var timeDiff = now - this.timestamp;
                if (!this.bitrate || !interval || timeDiff > interval) {
                    this.bitrate = (loaded - this.loaded) * (1000 / timeDiff) * 8;
                    this.loaded = loaded;
                    this.timestamp = now;
                }
                return this.bitrate;
            };
        },

        _isXHRUpload: function (options) {
            return !options.forceIframeTransport &&
                ((!options.multipart && $.support.xhrFileUpload) ||
                $.support.xhrFormDataFileUpload);
        },

        _getFormData: function (options) {
            var formData;
            if ($.type(options.formData) === 'function') {
                return options.formData(options.form);
            }
            if ($.isArray(options.formData)) {
                return options.formData;
            }
            if ($.type(options.formData) === 'object') {
                formData = [];
                $.each(options.formData, function (name, value) {
                    formData.push({name: name, value: value});
                });
                return formData;
            }
            return [];
        },

        _getTotal: function (files) {
            var total = 0;
            $.each(files, function (index, file) {
                total += file.size || 1;
            });
            return total;
        },

        _initProgressObject: function (obj) {
            var progress = {
                loaded: 0,
                total: 0,
                bitrate: 0
            };
            if (obj._progress) {
                $.extend(obj._progress, progress);
            } else {
                obj._progress = progress;
            }
        },

        _initResponseObject: function (obj) {
            var prop;
            if (obj._response) {
                for (prop in obj._response) {
                    if (obj._response.hasOwnProperty(prop)) {
                        delete obj._response[prop];
                    }
                }
            } else {
                obj._response = {};
            }
        },

        _onProgress: function (e, data) {
            if (e.lengthComputable) {
                var now = ((Date.now) ? Date.now() : (new Date()).getTime()),
                    loaded;
                if (data._time && data.progressInterval &&
                        (now - data._time < data.progressInterval) &&
                        e.loaded !== e.total) {
                    return;
                }
                data._time = now;
                loaded = Math.floor(
                    e.loaded / e.total * (data.chunkSize || data._progress.total)
                ) + (data.uploadedBytes || 0);
                // Add the difference from the previously loaded state
                // to the global loaded counter:
                this._progress.loaded += (loaded - data._progress.loaded);
                this._progress.bitrate = this._bitrateTimer.getBitrate(
                    now,
                    this._progress.loaded,
                    data.bitrateInterval
                );
                data._progress.loaded = data.loaded = loaded;
                data._progress.bitrate = data.bitrate = data._bitrateTimer.getBitrate(
                    now,
                    loaded,
                    data.bitrateInterval
                );
                // Trigger a custom progress event with a total data property set
                // to the file size(s) of the current upload and a loaded data
                // property calculated accordingly:
                this._trigger(
                    'progress',
                    $.Event('progress', {delegatedEvent: e}),
                    data
                );
                // Trigger a global progress event for all current file uploads,
                // including ajax calls queued for sequential file uploads:
                this._trigger(
                    'progressall',
                    $.Event('progressall', {delegatedEvent: e}),
                    this._progress
                );
            }
        },

        _initProgressListener: function (options) {
            var that = this,
                xhr = options.xhr ? options.xhr() : $.ajaxSettings.xhr();
            // Accesss to the native XHR object is required to add event listeners
            // for the upload progress event:
            if (xhr.upload) {
                $(xhr.upload).bind('progress', function (e) {
                    var oe = e.originalEvent;
                    // Make sure the progress event properties get copied over:
                    e.lengthComputable = oe.lengthComputable;
                    e.loaded = oe.loaded;
                    e.total = oe.total;
                    that._onProgress(e, options);
                });
                options.xhr = function () {
                    return xhr;
                };
            }
        },

        _isInstanceOf: function (type, obj) {
            // Cross-frame instanceof check
            return Object.prototype.toString.call(obj) === '[object ' + type + ']';
        },

        _initXHRData: function (options) {
            var that = this,
                formData,
                file = options.files[0],
                // Ignore non-multipart setting if not supported:
                multipart = options.multipart || !$.support.xhrFileUpload,
                paramName = $.type(options.paramName) === 'array' ?
                    options.paramName[0] : options.paramName;
            options.headers = $.extend({}, options.headers);
            if (options.contentRange) {
                options.headers['Content-Range'] = options.contentRange;
            }
            if (!multipart || options.blob || !this._isInstanceOf('File', file)) {
                options.headers['Content-Disposition'] = 'attachment; filename="' +
                    encodeURI(file.name) + '"';
            }
            if (!multipart) {
                options.contentType = file.type || 'application/octet-stream';
                options.data = options.blob || file;
            } else if ($.support.xhrFormDataFileUpload) {
                if (options.postMessage) {
                    // window.postMessage does not allow sending FormData
                    // objects, so we just add the File/Blob objects to
                    // the formData array and let the postMessage window
                    // create the FormData object out of this array:
                    formData = this._getFormData(options);
                    if (options.blob) {
                        formData.push({
                            name: paramName,
                            value: options.blob
                        });
                    } else {
                        $.each(options.files, function (index, file) {
                            formData.push({
                                name: ($.type(options.paramName) === 'array' &&
                                    options.paramName[index]) || paramName,
                                value: file
                            });
                        });
                    }
                } else {
                    if (that._isInstanceOf('FormData', options.formData)) {
                        formData = options.formData;
                    } else {
                        formData = new FormData();
                        $.each(this._getFormData(options), function (index, field) {
                            formData.append(field.name, field.value);
                        });
                    }
                    if (options.blob) {
                        formData.append(paramName, options.blob, file.name);
                    } else {
                        $.each(options.files, function (index, file) {
                            // This check allows the tests to run with
                            // dummy objects:
                            if (that._isInstanceOf('File', file) ||
                                    that._isInstanceOf('Blob', file)) {
                                formData.append(
                                    ($.type(options.paramName) === 'array' &&
                                        options.paramName[index]) || paramName,
                                    file,
                                    file.uploadName || file.name
                                );
                            }
                        });
                    }
                }
                options.data = formData;
            }
            // Blob reference is not needed anymore, free memory:
            options.blob = null;
        },

        _initIframeSettings: function (options) {
            var targetHost = $('<a></a>').prop('href', options.url).prop('host');
            // Setting the dataType to iframe enables the iframe transport:
            options.dataType = 'iframe ' + (options.dataType || '');
            // The iframe transport accepts a serialized array as form data:
            options.formData = this._getFormData(options);
            // Add redirect url to form data on cross-domain uploads:
            if (options.redirect && targetHost && targetHost !== location.host) {
                options.formData.push({
                    name: options.redirectParamName || 'redirect',
                    value: options.redirect
                });
            }
        },

        _initDataSettings: function (options) {
            if (this._isXHRUpload(options)) {
                if (!this._chunkedUpload(options, true)) {
                    if (!options.data) {
                        this._initXHRData(options);
                    }
                    this._initProgressListener(options);
                }
                if (options.postMessage) {
                    // Setting the dataType to postmessage enables the
                    // postMessage transport:
                    options.dataType = 'postmessage ' + (options.dataType || '');
                }
            } else {
                this._initIframeSettings(options);
            }
        },

        _getParamName: function (options) {
            var fileInput = $(options.fileInput),
                paramName = options.paramName;
            if (!paramName) {
                paramName = [];
                fileInput.each(function () {
                    var input = $(this),
                        name = input.prop('name') || 'files[]',
                        i = (input.prop('files') || [1]).length;
                    while (i) {
                        paramName.push(name);
                        i -= 1;
                    }
                });
                if (!paramName.length) {
                    paramName = [fileInput.prop('name') || 'files[]'];
                }
            } else if (!$.isArray(paramName)) {
                paramName = [paramName];
            }
            return paramName;
        },

        _initFormSettings: function (options) {
            // Retrieve missing options from the input field and the
            // associated form, if available:
            if (!options.form || !options.form.length) {
                options.form = $(options.fileInput.prop('form'));
                // If the given file input doesn't have an associated form,
                // use the default widget file input's form:
                if (!options.form.length) {
                    options.form = $(this.options.fileInput.prop('form'));
                }
            }
            options.paramName = this._getParamName(options);
            if (!options.url) {
                options.url = options.form.prop('action') || location.href;
            }
            // The HTTP request method must be "POST" or "PUT":
            options.type = (options.type ||
                ($.type(options.form.prop('method')) === 'string' &&
                    options.form.prop('method')) || ''
                ).toUpperCase();
            if (options.type !== 'POST' && options.type !== 'PUT' &&
                    options.type !== 'PATCH') {
                options.type = 'POST';
            }
            if (!options.formAcceptCharset) {
                options.formAcceptCharset = options.form.attr('accept-charset');
            }
        },

        _getAJAXSettings: function (data) {
            var options = $.extend({}, this.options, data);
            this._initFormSettings(options);
            this._initDataSettings(options);
            return options;
        },

        // jQuery 1.6 doesn't provide .state(),
        // while jQuery 1.8+ removed .isRejected() and .isResolved():
        _getDeferredState: function (deferred) {
            if (deferred.state) {
                return deferred.state();
            }
            if (deferred.isResolved()) {
                return 'resolved';
            }
            if (deferred.isRejected()) {
                return 'rejected';
            }
            return 'pending';
        },

        // Maps jqXHR callbacks to the equivalent
        // methods of the given Promise object:
        _enhancePromise: function (promise) {
            promise.success = promise.done;
            promise.error = promise.fail;
            promise.complete = promise.always;
            return promise;
        },

        // Creates and returns a Promise object enhanced with
        // the jqXHR methods abort, success, error and complete:
        _getXHRPromise: function (resolveOrReject, context, args) {
            var dfd = $.Deferred(),
                promise = dfd.promise();
            context = context || this.options.context || promise;
            if (resolveOrReject === true) {
                dfd.resolveWith(context, args);
            } else if (resolveOrReject === false) {
                dfd.rejectWith(context, args);
            }
            promise.abort = dfd.promise;
            return this._enhancePromise(promise);
        },

        // Adds convenience methods to the data callback argument:
        _addConvenienceMethods: function (e, data) {
            var that = this,
                getPromise = function (args) {
                    return $.Deferred().resolveWith(that, args).promise();
                };
            data.process = function (resolveFunc, rejectFunc) {
                if (resolveFunc || rejectFunc) {
                    data._processQueue = this._processQueue =
                        (this._processQueue || getPromise([this])).pipe(
                            function () {
                                if (data.errorThrown) {
                                    return $.Deferred()
                                        .rejectWith(that, [data]).promise();
                                }
                                return getPromise(arguments);
                            }
                        ).pipe(resolveFunc, rejectFunc);
                }
                return this._processQueue || getPromise([this]);
            };
            data.submit = function () {
                if (this.state() !== 'pending') {
                    data.jqXHR = this.jqXHR =
                        (that._trigger(
                            'submit',
                            $.Event('submit', {delegatedEvent: e}),
                            this
                        ) !== false) && that._onSend(e, this);
                }
                return this.jqXHR || that._getXHRPromise();
            };
            data.abort = function () {
                if (this.jqXHR) {
                    return this.jqXHR.abort();
                }
                this.errorThrown = 'abort';
                that._trigger('fail', null, this);
                return that._getXHRPromise(false);
            };
            data.state = function () {
                if (this.jqXHR) {
                    return that._getDeferredState(this.jqXHR);
                }
                if (this._processQueue) {
                    return that._getDeferredState(this._processQueue);
                }
            };
            data.processing = function () {
                return !this.jqXHR && this._processQueue && that
                    ._getDeferredState(this._processQueue) === 'pending';
            };
            data.progress = function () {
                return this._progress;
            };
            data.response = function () {
                return this._response;
            };
        },

        // Parses the Range header from the server response
        // and returns the uploaded bytes:
        _getUploadedBytes: function (jqXHR) {
            var range = jqXHR.getResponseHeader('Range'),
                parts = range && range.split('-'),
                upperBytesPos = parts && parts.length > 1 &&
                    parseInt(parts[1], 10);
            return upperBytesPos && upperBytesPos + 1;
        },

        // Uploads a file in multiple, sequential requests
        // by splitting the file up in multiple blob chunks.
        // If the second parameter is true, only tests if the file
        // should be uploaded in chunks, but does not invoke any
        // upload requests:
        _chunkedUpload: function (options, testOnly) {
            options.uploadedBytes = options.uploadedBytes || 0;
            var that = this,
                file = options.files[0],
                fs = file.size,
                ub = options.uploadedBytes,
                mcs = options.maxChunkSize || fs,
                slice = this._blobSlice,
                dfd = $.Deferred(),
                promise = dfd.promise(),
                jqXHR,
                upload;
            if (!(this._isXHRUpload(options) && slice && (ub || mcs < fs)) ||
                    options.data) {
                return false;
            }
            if (testOnly) {
                return true;
            }
            if (ub >= fs) {
                file.error = options.i18n('uploadedBytes');
                return this._getXHRPromise(
                    false,
                    options.context,
                    [null, 'error', file.error]
                );
            }
            // The chunk upload method:
            upload = function () {
                // Clone the options object for each chunk upload:
                var o = $.extend({}, options),
                    currentLoaded = o._progress.loaded;
                o.blob = slice.call(
                    file,
                    ub,
                    ub + mcs,
                    file.type
                );
                // Store the current chunk size, as the blob itself
                // will be dereferenced after data processing:
                o.chunkSize = o.blob.size;
                // Expose the chunk bytes position range:
                o.contentRange = 'bytes ' + ub + '-' +
                    (ub + o.chunkSize - 1) + '/' + fs;
                // Process the upload data (the blob and potential form data):
                that._initXHRData(o);
                // Add progress listeners for this chunk upload:
                that._initProgressListener(o);
                jqXHR = ((that._trigger('chunksend', null, o) !== false && $.ajax(o)) ||
                        that._getXHRPromise(false, o.context))
                    .done(function (result, textStatus, jqXHR) {
                        ub = that._getUploadedBytes(jqXHR) ||
                            (ub + o.chunkSize);
                        // Create a progress event if no final progress event
                        // with loaded equaling total has been triggered
                        // for this chunk:
                        if (currentLoaded + o.chunkSize - o._progress.loaded) {
                            that._onProgress($.Event('progress', {
                                lengthComputable: true,
                                loaded: ub - o.uploadedBytes,
                                total: ub - o.uploadedBytes
                            }), o);
                        }
                        options.uploadedBytes = o.uploadedBytes = ub;
                        o.result = result;
                        o.textStatus = textStatus;
                        o.jqXHR = jqXHR;
                        that._trigger('chunkdone', null, o);
                        that._trigger('chunkalways', null, o);
                        if (ub < fs) {
                            // File upload not yet complete,
                            // continue with the next chunk:
                            upload();
                        } else {
                            dfd.resolveWith(
                                o.context,
                                [result, textStatus, jqXHR]
                            );
                        }
                    })
                    .fail(function (jqXHR, textStatus, errorThrown) {
                        o.jqXHR = jqXHR;
                        o.textStatus = textStatus;
                        o.errorThrown = errorThrown;
                        that._trigger('chunkfail', null, o);
                        that._trigger('chunkalways', null, o);
                        dfd.rejectWith(
                            o.context,
                            [jqXHR, textStatus, errorThrown]
                        );
                    });
            };
            this._enhancePromise(promise);
            promise.abort = function () {
                return jqXHR.abort();
            };
            upload();
            return promise;
        },

        _beforeSend: function (e, data) {
            if (this._active === 0) {
                // the start callback is triggered when an upload starts
                // and no other uploads are currently running,
                // equivalent to the global ajaxStart event:
                this._trigger('start');
                // Set timer for global bitrate progress calculation:
                this._bitrateTimer = new this._BitrateTimer();
                // Reset the global progress values:
                this._progress.loaded = this._progress.total = 0;
                this._progress.bitrate = 0;
            }
            // Make sure the container objects for the .response() and
            // .progress() methods on the data object are available
            // and reset to their initial state:
            this._initResponseObject(data);
            this._initProgressObject(data);
            data._progress.loaded = data.loaded = data.uploadedBytes || 0;
            data._progress.total = data.total = this._getTotal(data.files) || 1;
            data._progress.bitrate = data.bitrate = 0;
            this._active += 1;
            // Initialize the global progress values:
            this._progress.loaded += data.loaded;
            this._progress.total += data.total;
        },

        _onDone: function (result, textStatus, jqXHR, options) {
            var total = options._progress.total,
                response = options._response;
            if (options._progress.loaded < total) {
                // Create a progress event if no final progress event
                // with loaded equaling total has been triggered:
                this._onProgress($.Event('progress', {
                    lengthComputable: true,
                    loaded: total,
                    total: total
                }), options);
            }
            response.result = options.result = result;
            response.textStatus = options.textStatus = textStatus;
            response.jqXHR = options.jqXHR = jqXHR;
            this._trigger('done', null, options);
        },

        _onFail: function (jqXHR, textStatus, errorThrown, options) {
            var response = options._response;
            if (options.recalculateProgress) {
                // Remove the failed (error or abort) file upload from
                // the global progress calculation:
                this._progress.loaded -= options._progress.loaded;
                this._progress.total -= options._progress.total;
            }
            response.jqXHR = options.jqXHR = jqXHR;
            response.textStatus = options.textStatus = textStatus;
            response.errorThrown = options.errorThrown = errorThrown;
            this._trigger('fail', null, options);
        },

        _onAlways: function (jqXHRorResult, textStatus, jqXHRorError, options) {
            // jqXHRorResult, textStatus and jqXHRorError are added to the
            // options object via done and fail callbacks
            this._trigger('always', null, options);
        },

        _onSend: function (e, data) {
            if (!data.submit) {
                this._addConvenienceMethods(e, data);
            }
            var that = this,
                jqXHR,
                aborted,
                slot,
                pipe,
                options = that._getAJAXSettings(data),
                send = function () {
                    that._sending += 1;
                    // Set timer for bitrate progress calculation:
                    options._bitrateTimer = new that._BitrateTimer();
                    jqXHR = jqXHR || (
                        ((aborted || that._trigger(
                            'send',
                            $.Event('send', {delegatedEvent: e}),
                            options
                        ) === false) &&
                        that._getXHRPromise(false, options.context, aborted)) ||
                        that._chunkedUpload(options) || $.ajax(options)
                    ).done(function (result, textStatus, jqXHR) {
                        that._onDone(result, textStatus, jqXHR, options);
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        that._onFail(jqXHR, textStatus, errorThrown, options);
                    }).always(function (jqXHRorResult, textStatus, jqXHRorError) {
                        that._onAlways(
                            jqXHRorResult,
                            textStatus,
                            jqXHRorError,
                            options
                        );
                        that._sending -= 1;
                        that._active -= 1;
                        if (options.limitConcurrentUploads &&
                                options.limitConcurrentUploads > that._sending) {
                            // Start the next queued upload,
                            // that has not been aborted:
                            var nextSlot = that._slots.shift();
                            while (nextSlot) {
                                if (that._getDeferredState(nextSlot) === 'pending') {
                                    nextSlot.resolve();
                                    break;
                                }
                                nextSlot = that._slots.shift();
                            }
                        }
                        if (that._active === 0) {
                            // The stop callback is triggered when all uploads have
                            // been completed, equivalent to the global ajaxStop event:
                            that._trigger('stop');
                        }
                    });
                    return jqXHR;
                };
            this._beforeSend(e, options);
            if (this.options.sequentialUploads ||
                    (this.options.limitConcurrentUploads &&
                    this.options.limitConcurrentUploads <= this._sending)) {
                if (this.options.limitConcurrentUploads > 1) {
                    slot = $.Deferred();
                    this._slots.push(slot);
                    pipe = slot.pipe(send);
                } else {
                    this._sequence = this._sequence.pipe(send, send);
                    pipe = this._sequence;
                }
                // Return the piped Promise object, enhanced with an abort method,
                // which is delegated to the jqXHR object of the current upload,
                // and jqXHR callbacks mapped to the equivalent Promise methods:
                pipe.abort = function () {
                    aborted = [undefined, 'abort', 'abort'];
                    if (!jqXHR) {
                        if (slot) {
                            slot.rejectWith(options.context, aborted);
                        }
                        return send();
                    }
                    return jqXHR.abort();
                };
                return this._enhancePromise(pipe);
            }
            return send();
        },

        _onAdd: function (e, data) {
            var that = this,
                result = true,
                options = $.extend({}, this.options, data),
                files = data.files,
                filesLength = files.length,
                limit = options.limitMultiFileUploads,
                limitSize = options.limitMultiFileUploadSize,
                overhead = options.limitMultiFileUploadSizeOverhead,
                batchSize = 0,
                paramName = this._getParamName(options),
                paramNameSet,
                paramNameSlice,
                fileSet,
                i,
                j = 0;
            if (limitSize && (!filesLength || files[0].size === undefined)) {
                limitSize = undefined;
            }
            if (!(options.singleFileUploads || limit || limitSize) ||
                    !this._isXHRUpload(options)) {
                fileSet = [files];
                paramNameSet = [paramName];
            } else if (!(options.singleFileUploads || limitSize) && limit) {
                fileSet = [];
                paramNameSet = [];
                for (i = 0; i < filesLength; i += limit) {
                    fileSet.push(files.slice(i, i + limit));
                    paramNameSlice = paramName.slice(i, i + limit);
                    if (!paramNameSlice.length) {
                        paramNameSlice = paramName;
                    }
                    paramNameSet.push(paramNameSlice);
                }
            } else if (!options.singleFileUploads && limitSize) {
                fileSet = [];
                paramNameSet = [];
                for (i = 0; i < filesLength; i = i + 1) {
                    batchSize += files[i].size + overhead;
                    if (i + 1 === filesLength ||
                            ((batchSize + files[i + 1].size + overhead) > limitSize) ||
                            (limit && i + 1 - j >= limit)) {
                        fileSet.push(files.slice(j, i + 1));
                        paramNameSlice = paramName.slice(j, i + 1);
                        if (!paramNameSlice.length) {
                            paramNameSlice = paramName;
                        }
                        paramNameSet.push(paramNameSlice);
                        j = i + 1;
                        batchSize = 0;
                    }
                }
            } else {
                paramNameSet = paramName;
            }
            data.originalFiles = files;
            $.each(fileSet || files, function (index, element) {
                var newData = $.extend({}, data);
                newData.files = fileSet ? element : [element];
                newData.paramName = paramNameSet[index];
                that._initResponseObject(newData);
                that._initProgressObject(newData);
                that._addConvenienceMethods(e, newData);
                result = that._trigger(
                    'add',
                    $.Event('add', {delegatedEvent: e}),
                    newData
                );
                return result;
            });
            return result;
        },

        _replaceFileInput: function (input) {
            var inputClone = input.clone(true);
            $('<form></form>').append(inputClone)[0].reset();
            // Detaching allows to insert the fileInput on another form
            // without loosing the file input value:
            input.after(inputClone).detach();
            // Avoid memory leaks with the detached file input:
            $.cleanData(input.unbind('remove'));
            // Replace the original file input element in the fileInput
            // elements set with the clone, which has been copied including
            // event handlers:
            this.options.fileInput = this.options.fileInput.map(function (i, el) {
                if (el === input[0]) {
                    return inputClone[0];
                }
                return el;
            });
            // If the widget has been initialized on the file input itself,
            // override this.element with the file input clone:
            if (input[0] === this.element[0]) {
                this.element = inputClone;
            }
        },

        _handleFileTreeEntry: function (entry, path) {
            var that = this,
                dfd = $.Deferred(),
                errorHandler = function (e) {
                    if (e && !e.entry) {
                        e.entry = entry;
                    }
                    // Since $.when returns immediately if one
                    // Deferred is rejected, we use resolve instead.
                    // This allows valid files and invalid items
                    // to be returned together in one set:
                    dfd.resolve([e]);
                },
                dirReader;
            path = path || '';
            if (entry.isFile) {
                if (entry._file) {
                    // Workaround for Chrome bug #149735
                    entry._file.relativePath = path;
                    dfd.resolve(entry._file);
                } else {
                    entry.file(function (file) {
                        file.relativePath = path;
                        dfd.resolve(file);
                    }, errorHandler);
                }
            } else if (entry.isDirectory) {
                dirReader = entry.createReader();
                dirReader.readEntries(function (entries) {
                    that._handleFileTreeEntries(
                        entries,
                        path + entry.name + '/'
                    ).done(function (files) {
                        dfd.resolve(files);
                    }).fail(errorHandler);
                }, errorHandler);
            } else {
                // Return an empy list for file system items
                // other than files or directories:
                dfd.resolve([]);
            }
            return dfd.promise();
        },

        _handleFileTreeEntries: function (entries, path) {
            var that = this;
            return $.when.apply(
                $,
                $.map(entries, function (entry) {
                    return that._handleFileTreeEntry(entry, path);
                })
            ).pipe(function () {
                return Array.prototype.concat.apply(
                    [],
                    arguments
                );
            });
        },

        _getDroppedFiles: function (dataTransfer) {
            dataTransfer = dataTransfer || {};
            var items = dataTransfer.items;
            if (items && items.length && (items[0].webkitGetAsEntry ||
                    items[0].getAsEntry)) {
                return this._handleFileTreeEntries(
                    $.map(items, function (item) {
                        var entry;
                        if (item.webkitGetAsEntry) {
                            entry = item.webkitGetAsEntry();
                            if (entry) {
                                // Workaround for Chrome bug #149735:
                                entry._file = item.getAsFile();
                            }
                            return entry;
                        }
                        return item.getAsEntry();
                    })
                );
            }
            return $.Deferred().resolve(
                $.makeArray(dataTransfer.files)
            ).promise();
        },

        _getSingleFileInputFiles: function (fileInput) {
            fileInput = $(fileInput);
            var entries = fileInput.prop('webkitEntries') ||
                    fileInput.prop('entries'),
                files,
                value;
            if (entries && entries.length) {
                return this._handleFileTreeEntries(entries);
            }
            files = $.makeArray(fileInput.prop('files'));
            if (!files.length) {
                value = fileInput.prop('value');
                if (!value) {
                    return $.Deferred().resolve([]).promise();
                }
                // If the files property is not available, the browser does not
                // support the File API and we add a pseudo File object with
                // the input value as name with path information removed:
                files = [{name: value.replace(/^.*\\/, '')}];
            } else if (files[0].name === undefined && files[0].fileName) {
                // File normalization for Safari 4 and Firefox 3:
                $.each(files, function (index, file) {
                    file.name = file.fileName;
                    file.size = file.fileSize;
                });
            }
            return $.Deferred().resolve(files).promise();
        },

        _getFileInputFiles: function (fileInput) {
            if (!(fileInput instanceof $) || fileInput.length === 1) {
                return this._getSingleFileInputFiles(fileInput);
            }
            return $.when.apply(
                $,
                $.map(fileInput, this._getSingleFileInputFiles)
            ).pipe(function () {
                return Array.prototype.concat.apply(
                    [],
                    arguments
                );
            });
        },

        _onChange: function (e) {
            var that = this,
                data = {
                    fileInput: $(e.target),
                    form: $(e.target.form)
                };
            this._getFileInputFiles(data.fileInput).always(function (files) {
                data.files = files;
                if (that.options.replaceFileInput) {
                    that._replaceFileInput(data.fileInput);
                }
                if (that._trigger(
                        'change',
                        $.Event('change', {delegatedEvent: e}),
                        data
                    ) !== false) {
                    that._onAdd(e, data);
                }
            });
        },

        _onPaste: function (e) {
            var items = e.originalEvent && e.originalEvent.clipboardData &&
                    e.originalEvent.clipboardData.items,
                data = {files: []};
            if (items && items.length) {
                $.each(items, function (index, item) {
                    var file = item.getAsFile && item.getAsFile();
                    if (file) {
                        data.files.push(file);
                    }
                });
                if (this._trigger(
                        'paste',
                        $.Event('paste', {delegatedEvent: e}),
                        data
                    ) !== false) {
                    this._onAdd(e, data);
                }
            }
        },

        _onDrop: function (e) {
            e.dataTransfer = e.originalEvent && e.originalEvent.dataTransfer;
            var that = this,
                dataTransfer = e.dataTransfer,
                data = {};
            if (dataTransfer && dataTransfer.files && dataTransfer.files.length) {
                e.preventDefault();
                this._getDroppedFiles(dataTransfer).always(function (files) {
                    data.files = files;
                    if (that._trigger(
                            'drop',
                            $.Event('drop', {delegatedEvent: e}),
                            data
                        ) !== false) {
                        that._onAdd(e, data);
                    }
                });
            }
        },

        _onDragOver: function (e) {
            e.dataTransfer = e.originalEvent && e.originalEvent.dataTransfer;
            var dataTransfer = e.dataTransfer;
            if (dataTransfer && $.inArray('Files', dataTransfer.types) !== -1 &&
                    this._trigger(
                        'dragover',
                        $.Event('dragover', {delegatedEvent: e})
                    ) !== false) {
                e.preventDefault();
                dataTransfer.dropEffect = 'copy';
            }
        },

        _initEventHandlers: function () {
            if (this._isXHRUpload(this.options)) {
                this._on(this.options.dropZone, {
                    dragover: this._onDragOver,
                    drop: this._onDrop
                });
                this._on(this.options.pasteZone, {
                    paste: this._onPaste
                });
            }
            if ($.support.fileInput) {
                this._on(this.options.fileInput, {
                    change: this._onChange
                });
            }
        },

        _destroyEventHandlers: function () {
            this._off(this.options.dropZone, 'dragover drop');
            this._off(this.options.pasteZone, 'paste');
            this._off(this.options.fileInput, 'change');
        },

        _setOption: function (key, value) {
            var reinit = $.inArray(key, this._specialOptions) !== -1;
            if (reinit) {
                this._destroyEventHandlers();
            }
            this._super(key, value);
            if (reinit) {
                this._initSpecialOptions();
                this._initEventHandlers();
            }
        },

        _initSpecialOptions: function () {
            var options = this.options;
            if (options.fileInput === undefined) {
                options.fileInput = this.element.is('input[type="file"]') ?
                        this.element : this.element.find('input[type="file"]');
            } else if (!(options.fileInput instanceof $)) {
                options.fileInput = $(options.fileInput);
            }
            if (!(options.dropZone instanceof $)) {
                options.dropZone = $(options.dropZone);
            }
            if (!(options.pasteZone instanceof $)) {
                options.pasteZone = $(options.pasteZone);
            }
        },

        _getRegExp: function (str) {
            var parts = str.split('/'),
                modifiers = parts.pop();
            parts.shift();
            return new RegExp(parts.join('/'), modifiers);
        },

        _isRegExpOption: function (key, value) {
            return key !== 'url' && $.type(value) === 'string' &&
                /^\/.*\/[igm]{0,3}$/.test(value);
        },

        _initDataAttributes: function () {
            var that = this,
                options = this.options,
                clone = $(this.element[0].cloneNode(false));
            // Initialize options set via HTML5 data-attributes:
            $.each(
                clone.data(),
                function (key, value) {
                    var dataAttributeName = 'data-' +
                        // Convert camelCase to hyphen-ated key:
                        key.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
                    if (clone.attr(dataAttributeName)) {
                        if (that._isRegExpOption(key, value)) {
                            value = that._getRegExp(value);
                        }
                        options[key] = value;
                    }
                }
            );
        },

        _create: function () {
            this._initDataAttributes();
            this._initSpecialOptions();
            this._slots = [];
            this._sequence = this._getXHRPromise(true);
            this._sending = this._active = 0;
            this._initProgressObject(this);
            this._initEventHandlers();
        },

        // This method is exposed to the widget API and allows to query
        // the number of active uploads:
        active: function () {
            return this._active;
        },

        // This method is exposed to the widget API and allows to query
        // the widget upload progress.
        // It returns an object with loaded, total and bitrate properties
        // for the running uploads:
        progress: function () {
            return this._progress;
        },

        // This method is exposed to the widget API and allows adding files
        // using the fileupload API. The data parameter accepts an object which
        // must have a files property and can contain additional options:
        // .fileupload('add', {files: filesList});
        add: function (data) {
            var that = this;
            if (!data || this.options.disabled) {
                return;
            }
            if (data.fileInput && !data.files) {
                this._getFileInputFiles(data.fileInput).always(function (files) {
                    data.files = files;
                    that._onAdd(null, data);
                });
            } else {
                data.files = $.makeArray(data.files);
                this._onAdd(null, data);
            }
        },

        // This method is exposed to the widget API and allows sending files
        // using the fileupload API. The data parameter accepts an object which
        // must have a files or fileInput property and can contain additional options:
        // .fileupload('send', {files: filesList});
        // The method returns a Promise object for the file upload call.
        send: function (data) {
            if (data && !this.options.disabled) {
                if (data.fileInput && !data.files) {
                    var that = this,
                        dfd = $.Deferred(),
                        promise = dfd.promise(),
                        jqXHR,
                        aborted;
                    promise.abort = function () {
                        aborted = true;
                        if (jqXHR) {
                            return jqXHR.abort();
                        }
                        dfd.reject(null, 'abort', 'abort');
                        return promise;
                    };
                    this._getFileInputFiles(data.fileInput).always(
                        function (files) {
                            if (aborted) {
                                return;
                            }
                            if (!files.length) {
                                dfd.reject();
                                return;
                            }
                            data.files = files;
                            jqXHR = that._onSend(null, data).then(
                                function (result, textStatus, jqXHR) {
                                    dfd.resolve(result, textStatus, jqXHR);
                                },
                                function (jqXHR, textStatus, errorThrown) {
                                    dfd.reject(jqXHR, textStatus, errorThrown);
                                }
                            );
                        }
                    );
                    return this._enhancePromise(promise);
                }
                data.files = $.makeArray(data.files);
                if (data.files.length) {
                    return this._onSend(null, data);
                }
            }
            return this._getXHRPromise(false, data && data.context);
        }

    });

}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqcXVlcnkuZmlsZXVwbG9hZC5qcyJdLCJzb3VyY2VzQ29udGVudCI6WyIvKlxyXG4gKiBqUXVlcnkgRmlsZSBVcGxvYWQgUGx1Z2luIDUuNDAuMVxyXG4gKiBodHRwczovL2dpdGh1Yi5jb20vYmx1ZWltcC9qUXVlcnktRmlsZS1VcGxvYWRcclxuICpcclxuICogQ29weXJpZ2h0IDIwMTAsIFNlYmFzdGlhbiBUc2NoYW5cclxuICogaHR0cHM6Ly9ibHVlaW1wLm5ldFxyXG4gKlxyXG4gKiBMaWNlbnNlZCB1bmRlciB0aGUgTUlUIGxpY2Vuc2U6XHJcbiAqIGh0dHA6Ly93d3cub3BlbnNvdXJjZS5vcmcvbGljZW5zZXMvTUlUXHJcbiAqL1xyXG5cclxuLyoganNoaW50IG5vbWVuOmZhbHNlICovXHJcbi8qIGdsb2JhbCBkZWZpbmUsIHdpbmRvdywgZG9jdW1lbnQsIGxvY2F0aW9uLCBCbG9iLCBGb3JtRGF0YSAqL1xyXG5cclxuKGZ1bmN0aW9uIChmYWN0b3J5KSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcbiAgICBpZiAodHlwZW9mIGRlZmluZSA9PT0gJ2Z1bmN0aW9uJyAmJiBkZWZpbmUuYW1kKSB7XHJcbiAgICAgICAgLy8gUmVnaXN0ZXIgYXMgYW4gYW5vbnltb3VzIEFNRCBtb2R1bGU6XHJcbiAgICAgICAgZGVmaW5lKFtcclxuICAgICAgICAgICAgJ2pxdWVyeScsXHJcbiAgICAgICAgICAgICdqcXVlcnkudWkud2lkZ2V0J1xyXG4gICAgICAgIF0sIGZhY3RvcnkpO1xyXG4gICAgfSBlbHNlIHtcclxuICAgICAgICAvLyBCcm93c2VyIGdsb2JhbHM6XHJcbiAgICAgICAgZmFjdG9yeSh3aW5kb3cualF1ZXJ5KTtcclxuICAgIH1cclxufShmdW5jdGlvbiAoJCkge1xyXG4gICAgJ3VzZSBzdHJpY3QnO1xyXG5cclxuICAgIC8vIERldGVjdCBmaWxlIGlucHV0IHN1cHBvcnQsIGJhc2VkIG9uXHJcbiAgICAvLyBodHRwOi8vdmlsamFtaXMuY29tL2Jsb2cvMjAxMi9maWxlLXVwbG9hZC1zdXBwb3J0LW9uLW1vYmlsZS9cclxuICAgICQuc3VwcG9ydC5maWxlSW5wdXQgPSAhKG5ldyBSZWdFeHAoXHJcbiAgICAgICAgLy8gSGFuZGxlIGRldmljZXMgd2hpY2ggZ2l2ZSBmYWxzZSBwb3NpdGl2ZXMgZm9yIHRoZSBmZWF0dXJlIGRldGVjdGlvbjpcclxuICAgICAgICAnKEFuZHJvaWQgKDFcXFxcLlswMTU2XXwyXFxcXC5bMDFdKSknICtcclxuICAgICAgICAgICAgJ3woV2luZG93cyBQaG9uZSAoT1MgN3w4XFxcXC4wKSl8KFhCTFdQKXwoWnVuZVdQKXwoV1BEZXNrdG9wKScgK1xyXG4gICAgICAgICAgICAnfCh3KGViKT9PU0Jyb3dzZXIpfCh3ZWJPUyknICtcclxuICAgICAgICAgICAgJ3woS2luZGxlLygxXFxcXC4wfDJcXFxcLlswNV18M1xcXFwuMCkpJ1xyXG4gICAgKS50ZXN0KHdpbmRvdy5uYXZpZ2F0b3IudXNlckFnZW50KSB8fFxyXG4gICAgICAgIC8vIEZlYXR1cmUgZGV0ZWN0aW9uIGZvciBhbGwgb3RoZXIgZGV2aWNlczpcclxuICAgICAgICAkKCc8aW5wdXQgdHlwZT1cImZpbGVcIj4nKS5wcm9wKCdkaXNhYmxlZCcpKTtcclxuXHJcbiAgICAvLyBUaGUgRmlsZVJlYWRlciBBUEkgaXMgbm90IGFjdHVhbGx5IHVzZWQsIGJ1dCB3b3JrcyBhcyBmZWF0dXJlIGRldGVjdGlvbixcclxuICAgIC8vIGFzIHNvbWUgU2FmYXJpIHZlcnNpb25zICg1Pykgc3VwcG9ydCBYSFIgZmlsZSB1cGxvYWRzIHZpYSB0aGUgRm9ybURhdGEgQVBJLFxyXG4gICAgLy8gYnV0IG5vdCBub24tbXVsdGlwYXJ0IFhIUiBmaWxlIHVwbG9hZHMuXHJcbiAgICAvLyB3aW5kb3cuWE1MSHR0cFJlcXVlc3RVcGxvYWQgaXMgbm90IGF2YWlsYWJsZSBvbiBJRTEwLCBzbyB3ZSBjaGVjayBmb3JcclxuICAgIC8vIHdpbmRvdy5Qcm9ncmVzc0V2ZW50IGluc3RlYWQgdG8gZGV0ZWN0IFhIUjIgZmlsZSB1cGxvYWQgY2FwYWJpbGl0eTpcclxuICAgICQuc3VwcG9ydC54aHJGaWxlVXBsb2FkID0gISEod2luZG93LlByb2dyZXNzRXZlbnQgJiYgd2luZG93LkZpbGVSZWFkZXIpO1xyXG4gICAgJC5zdXBwb3J0LnhockZvcm1EYXRhRmlsZVVwbG9hZCA9ICEhd2luZG93LkZvcm1EYXRhO1xyXG5cclxuICAgIC8vIERldGVjdCBzdXBwb3J0IGZvciBCbG9iIHNsaWNpbmcgKHJlcXVpcmVkIGZvciBjaHVua2VkIHVwbG9hZHMpOlxyXG4gICAgJC5zdXBwb3J0LmJsb2JTbGljZSA9IHdpbmRvdy5CbG9iICYmIChCbG9iLnByb3RvdHlwZS5zbGljZSB8fFxyXG4gICAgICAgIEJsb2IucHJvdG90eXBlLndlYmtpdFNsaWNlIHx8IEJsb2IucHJvdG90eXBlLm1velNsaWNlKTtcclxuXHJcbiAgICAvLyBUaGUgZmlsZXVwbG9hZCB3aWRnZXQgbGlzdGVucyBmb3IgY2hhbmdlIGV2ZW50cyBvbiBmaWxlIGlucHV0IGZpZWxkcyBkZWZpbmVkXHJcbiAgICAvLyB2aWEgZmlsZUlucHV0IHNldHRpbmcgYW5kIHBhc3RlIG9yIGRyb3AgZXZlbnRzIG9mIHRoZSBnaXZlbiBkcm9wWm9uZS5cclxuICAgIC8vIEluIGFkZGl0aW9uIHRvIHRoZSBkZWZhdWx0IGpRdWVyeSBXaWRnZXQgbWV0aG9kcywgdGhlIGZpbGV1cGxvYWQgd2lkZ2V0XHJcbiAgICAvLyBleHBvc2VzIHRoZSBcImFkZFwiIGFuZCBcInNlbmRcIiBtZXRob2RzLCB0byBhZGQgb3IgZGlyZWN0bHkgc2VuZCBmaWxlcyB1c2luZ1xyXG4gICAgLy8gdGhlIGZpbGV1cGxvYWQgQVBJLlxyXG4gICAgLy8gQnkgZGVmYXVsdCwgZmlsZXMgYWRkZWQgdmlhIGZpbGUgaW5wdXQgc2VsZWN0aW9uLCBwYXN0ZSwgZHJhZyAmIGRyb3Agb3JcclxuICAgIC8vIFwiYWRkXCIgbWV0aG9kIGFyZSB1cGxvYWRlZCBpbW1lZGlhdGVseSwgYnV0IGl0IGlzIHBvc3NpYmxlIHRvIG92ZXJyaWRlXHJcbiAgICAvLyB0aGUgXCJhZGRcIiBjYWxsYmFjayBvcHRpb24gdG8gcXVldWUgZmlsZSB1cGxvYWRzLlxyXG4gICAgJC53aWRnZXQoJ2JsdWVpbXAuZmlsZXVwbG9hZCcsIHtcclxuXHJcbiAgICAgICAgb3B0aW9uczoge1xyXG4gICAgICAgICAgICAvLyBUaGUgZHJvcCB0YXJnZXQgZWxlbWVudChzKSwgYnkgdGhlIGRlZmF1bHQgdGhlIGNvbXBsZXRlIGRvY3VtZW50LlxyXG4gICAgICAgICAgICAvLyBTZXQgdG8gbnVsbCB0byBkaXNhYmxlIGRyYWcgJiBkcm9wIHN1cHBvcnQ6XHJcbiAgICAgICAgICAgIGRyb3Bab25lOiAkKGRvY3VtZW50KSxcclxuICAgICAgICAgICAgLy8gVGhlIHBhc3RlIHRhcmdldCBlbGVtZW50KHMpLCBieSB0aGUgZGVmYXVsdCB0aGUgY29tcGxldGUgZG9jdW1lbnQuXHJcbiAgICAgICAgICAgIC8vIFNldCB0byBudWxsIHRvIGRpc2FibGUgcGFzdGUgc3VwcG9ydDpcclxuICAgICAgICAgICAgcGFzdGVab25lOiAkKGRvY3VtZW50KSxcclxuICAgICAgICAgICAgLy8gVGhlIGZpbGUgaW5wdXQgZmllbGQocyksIHRoYXQgYXJlIGxpc3RlbmVkIHRvIGZvciBjaGFuZ2UgZXZlbnRzLlxyXG4gICAgICAgICAgICAvLyBJZiB1bmRlZmluZWQsIGl0IGlzIHNldCB0byB0aGUgZmlsZSBpbnB1dCBmaWVsZHMgaW5zaWRlXHJcbiAgICAgICAgICAgIC8vIG9mIHRoZSB3aWRnZXQgZWxlbWVudCBvbiBwbHVnaW4gaW5pdGlhbGl6YXRpb24uXHJcbiAgICAgICAgICAgIC8vIFNldCB0byBudWxsIHRvIGRpc2FibGUgdGhlIGNoYW5nZSBsaXN0ZW5lci5cclxuICAgICAgICAgICAgZmlsZUlucHV0OiB1bmRlZmluZWQsXHJcbiAgICAgICAgICAgIC8vIEJ5IGRlZmF1bHQsIHRoZSBmaWxlIGlucHV0IGZpZWxkIGlzIHJlcGxhY2VkIHdpdGggYSBjbG9uZSBhZnRlclxyXG4gICAgICAgICAgICAvLyBlYWNoIGlucHV0IGZpZWxkIGNoYW5nZSBldmVudC4gVGhpcyBpcyByZXF1aXJlZCBmb3IgaWZyYW1lIHRyYW5zcG9ydFxyXG4gICAgICAgICAgICAvLyBxdWV1ZXMgYW5kIGFsbG93cyBjaGFuZ2UgZXZlbnRzIHRvIGJlIGZpcmVkIGZvciB0aGUgc2FtZSBmaWxlXHJcbiAgICAgICAgICAgIC8vIHNlbGVjdGlvbiwgYnV0IGNhbiBiZSBkaXNhYmxlZCBieSBzZXR0aW5nIHRoZSBmb2xsb3dpbmcgb3B0aW9uIHRvIGZhbHNlOlxyXG4gICAgICAgICAgICByZXBsYWNlRmlsZUlucHV0OiB0cnVlLFxyXG4gICAgICAgICAgICAvLyBUaGUgcGFyYW1ldGVyIG5hbWUgZm9yIHRoZSBmaWxlIGZvcm0gZGF0YSAodGhlIHJlcXVlc3QgYXJndW1lbnQgbmFtZSkuXHJcbiAgICAgICAgICAgIC8vIElmIHVuZGVmaW5lZCBvciBlbXB0eSwgdGhlIG5hbWUgcHJvcGVydHkgb2YgdGhlIGZpbGUgaW5wdXQgZmllbGQgaXNcclxuICAgICAgICAgICAgLy8gdXNlZCwgb3IgXCJmaWxlc1tdXCIgaWYgdGhlIGZpbGUgaW5wdXQgbmFtZSBwcm9wZXJ0eSBpcyBhbHNvIGVtcHR5LFxyXG4gICAgICAgICAgICAvLyBjYW4gYmUgYSBzdHJpbmcgb3IgYW4gYXJyYXkgb2Ygc3RyaW5nczpcclxuICAgICAgICAgICAgcGFyYW1OYW1lOiB1bmRlZmluZWQsXHJcbiAgICAgICAgICAgIC8vIEJ5IGRlZmF1bHQsIGVhY2ggZmlsZSBvZiBhIHNlbGVjdGlvbiBpcyB1cGxvYWRlZCB1c2luZyBhbiBpbmRpdmlkdWFsXHJcbiAgICAgICAgICAgIC8vIHJlcXVlc3QgZm9yIFhIUiB0eXBlIHVwbG9hZHMuIFNldCB0byBmYWxzZSB0byB1cGxvYWQgZmlsZVxyXG4gICAgICAgICAgICAvLyBzZWxlY3Rpb25zIGluIG9uZSByZXF1ZXN0IGVhY2g6XHJcbiAgICAgICAgICAgIHNpbmdsZUZpbGVVcGxvYWRzOiB0cnVlLFxyXG4gICAgICAgICAgICAvLyBUbyBsaW1pdCB0aGUgbnVtYmVyIG9mIGZpbGVzIHVwbG9hZGVkIHdpdGggb25lIFhIUiByZXF1ZXN0LFxyXG4gICAgICAgICAgICAvLyBzZXQgdGhlIGZvbGxvd2luZyBvcHRpb24gdG8gYW4gaW50ZWdlciBncmVhdGVyIHRoYW4gMDpcclxuICAgICAgICAgICAgbGltaXRNdWx0aUZpbGVVcGxvYWRzOiB1bmRlZmluZWQsXHJcbiAgICAgICAgICAgIC8vIFRoZSBmb2xsb3dpbmcgb3B0aW9uIGxpbWl0cyB0aGUgbnVtYmVyIG9mIGZpbGVzIHVwbG9hZGVkIHdpdGggb25lXHJcbiAgICAgICAgICAgIC8vIFhIUiByZXF1ZXN0IHRvIGtlZXAgdGhlIHJlcXVlc3Qgc2l6ZSB1bmRlciBvciBlcXVhbCB0byB0aGUgZGVmaW5lZFxyXG4gICAgICAgICAgICAvLyBsaW1pdCBpbiBieXRlczpcclxuICAgICAgICAgICAgbGltaXRNdWx0aUZpbGVVcGxvYWRTaXplOiB1bmRlZmluZWQsXHJcbiAgICAgICAgICAgIC8vIE11bHRpcGFydCBmaWxlIHVwbG9hZHMgYWRkIGEgbnVtYmVyIG9mIGJ5dGVzIHRvIGVhY2ggdXBsb2FkZWQgZmlsZSxcclxuICAgICAgICAgICAgLy8gdGhlcmVmb3JlIHRoZSBmb2xsb3dpbmcgb3B0aW9uIGFkZHMgYW4gb3ZlcmhlYWQgZm9yIGVhY2ggZmlsZSB1c2VkXHJcbiAgICAgICAgICAgIC8vIGluIHRoZSBsaW1pdE11bHRpRmlsZVVwbG9hZFNpemUgY29uZmlndXJhdGlvbjpcclxuICAgICAgICAgICAgbGltaXRNdWx0aUZpbGVVcGxvYWRTaXplT3ZlcmhlYWQ6IDUxMixcclxuICAgICAgICAgICAgLy8gU2V0IHRoZSBmb2xsb3dpbmcgb3B0aW9uIHRvIHRydWUgdG8gaXNzdWUgYWxsIGZpbGUgdXBsb2FkIHJlcXVlc3RzXHJcbiAgICAgICAgICAgIC8vIGluIGEgc2VxdWVudGlhbCBvcmRlcjpcclxuICAgICAgICAgICAgc2VxdWVudGlhbFVwbG9hZHM6IGZhbHNlLFxyXG4gICAgICAgICAgICAvLyBUbyBsaW1pdCB0aGUgbnVtYmVyIG9mIGNvbmN1cnJlbnQgdXBsb2FkcyxcclxuICAgICAgICAgICAgLy8gc2V0IHRoZSBmb2xsb3dpbmcgb3B0aW9uIHRvIGFuIGludGVnZXIgZ3JlYXRlciB0aGFuIDA6XHJcbiAgICAgICAgICAgIGxpbWl0Q29uY3VycmVudFVwbG9hZHM6IHVuZGVmaW5lZCxcclxuICAgICAgICAgICAgLy8gU2V0IHRoZSBmb2xsb3dpbmcgb3B0aW9uIHRvIHRydWUgdG8gZm9yY2UgaWZyYW1lIHRyYW5zcG9ydCB1cGxvYWRzOlxyXG4gICAgICAgICAgICBmb3JjZUlmcmFtZVRyYW5zcG9ydDogZmFsc2UsXHJcbiAgICAgICAgICAgIC8vIFNldCB0aGUgZm9sbG93aW5nIG9wdGlvbiB0byB0aGUgbG9jYXRpb24gb2YgYSByZWRpcmVjdCB1cmwgb24gdGhlXHJcbiAgICAgICAgICAgIC8vIG9yaWdpbiBzZXJ2ZXIsIGZvciBjcm9zcy1kb21haW4gaWZyYW1lIHRyYW5zcG9ydCB1cGxvYWRzOlxyXG4gICAgICAgICAgICByZWRpcmVjdDogdW5kZWZpbmVkLFxyXG4gICAgICAgICAgICAvLyBUaGUgcGFyYW1ldGVyIG5hbWUgZm9yIHRoZSByZWRpcmVjdCB1cmwsIHNlbnQgYXMgcGFydCBvZiB0aGUgZm9ybVxyXG4gICAgICAgICAgICAvLyBkYXRhIGFuZCBzZXQgdG8gJ3JlZGlyZWN0JyBpZiB0aGlzIG9wdGlvbiBpcyBlbXB0eTpcclxuICAgICAgICAgICAgcmVkaXJlY3RQYXJhbU5hbWU6IHVuZGVmaW5lZCxcclxuICAgICAgICAgICAgLy8gU2V0IHRoZSBmb2xsb3dpbmcgb3B0aW9uIHRvIHRoZSBsb2NhdGlvbiBvZiBhIHBvc3RNZXNzYWdlIHdpbmRvdyxcclxuICAgICAgICAgICAgLy8gdG8gZW5hYmxlIHBvc3RNZXNzYWdlIHRyYW5zcG9ydCB1cGxvYWRzOlxyXG4gICAgICAgICAgICBwb3N0TWVzc2FnZTogdW5kZWZpbmVkLFxyXG4gICAgICAgICAgICAvLyBCeSBkZWZhdWx0LCBYSFIgZmlsZSB1cGxvYWRzIGFyZSBzZW50IGFzIG11bHRpcGFydC9mb3JtLWRhdGEuXHJcbiAgICAgICAgICAgIC8vIFRoZSBpZnJhbWUgdHJhbnNwb3J0IGlzIGFsd2F5cyB1c2luZyBtdWx0aXBhcnQvZm9ybS1kYXRhLlxyXG4gICAgICAgICAgICAvLyBTZXQgdG8gZmFsc2UgdG8gZW5hYmxlIG5vbi1tdWx0aXBhcnQgWEhSIHVwbG9hZHM6XHJcbiAgICAgICAgICAgIG11bHRpcGFydDogdHJ1ZSxcclxuICAgICAgICAgICAgLy8gVG8gdXBsb2FkIGxhcmdlIGZpbGVzIGluIHNtYWxsZXIgY2h1bmtzLCBzZXQgdGhlIGZvbGxvd2luZyBvcHRpb25cclxuICAgICAgICAgICAgLy8gdG8gYSBwcmVmZXJyZWQgbWF4aW11bSBjaHVuayBzaXplLiBJZiBzZXQgdG8gMCwgbnVsbCBvciB1bmRlZmluZWQsXHJcbiAgICAgICAgICAgIC8vIG9yIHRoZSBicm93c2VyIGRvZXMgbm90IHN1cHBvcnQgdGhlIHJlcXVpcmVkIEJsb2IgQVBJLCBmaWxlcyB3aWxsXHJcbiAgICAgICAgICAgIC8vIGJlIHVwbG9hZGVkIGFzIGEgd2hvbGUuXHJcbiAgICAgICAgICAgIG1heENodW5rU2l6ZTogdW5kZWZpbmVkLFxyXG4gICAgICAgICAgICAvLyBXaGVuIGEgbm9uLW11bHRpcGFydCB1cGxvYWQgb3IgYSBjaHVua2VkIG11bHRpcGFydCB1cGxvYWQgaGFzIGJlZW5cclxuICAgICAgICAgICAgLy8gYWJvcnRlZCwgdGhpcyBvcHRpb24gY2FuIGJlIHVzZWQgdG8gcmVzdW1lIHRoZSB1cGxvYWQgYnkgc2V0dGluZ1xyXG4gICAgICAgICAgICAvLyBpdCB0byB0aGUgc2l6ZSBvZiB0aGUgYWxyZWFkeSB1cGxvYWRlZCBieXRlcy4gVGhpcyBvcHRpb24gaXMgbW9zdFxyXG4gICAgICAgICAgICAvLyB1c2VmdWwgd2hlbiBtb2RpZnlpbmcgdGhlIG9wdGlvbnMgb2JqZWN0IGluc2lkZSBvZiB0aGUgXCJhZGRcIiBvclxyXG4gICAgICAgICAgICAvLyBcInNlbmRcIiBjYWxsYmFja3MsIGFzIHRoZSBvcHRpb25zIGFyZSBjbG9uZWQgZm9yIGVhY2ggZmlsZSB1cGxvYWQuXHJcbiAgICAgICAgICAgIHVwbG9hZGVkQnl0ZXM6IHVuZGVmaW5lZCxcclxuICAgICAgICAgICAgLy8gQnkgZGVmYXVsdCwgZmFpbGVkIChhYm9ydCBvciBlcnJvcikgZmlsZSB1cGxvYWRzIGFyZSByZW1vdmVkIGZyb20gdGhlXHJcbiAgICAgICAgICAgIC8vIGdsb2JhbCBwcm9ncmVzcyBjYWxjdWxhdGlvbi4gU2V0IHRoZSBmb2xsb3dpbmcgb3B0aW9uIHRvIGZhbHNlIHRvXHJcbiAgICAgICAgICAgIC8vIHByZXZlbnQgcmVjYWxjdWxhdGluZyB0aGUgZ2xvYmFsIHByb2dyZXNzIGRhdGE6XHJcbiAgICAgICAgICAgIHJlY2FsY3VsYXRlUHJvZ3Jlc3M6IHRydWUsXHJcbiAgICAgICAgICAgIC8vIEludGVydmFsIGluIG1pbGxpc2Vjb25kcyB0byBjYWxjdWxhdGUgYW5kIHRyaWdnZXIgcHJvZ3Jlc3MgZXZlbnRzOlxyXG4gICAgICAgICAgICBwcm9ncmVzc0ludGVydmFsOiAxMDAsXHJcbiAgICAgICAgICAgIC8vIEludGVydmFsIGluIG1pbGxpc2Vjb25kcyB0byBjYWxjdWxhdGUgcHJvZ3Jlc3MgYml0cmF0ZTpcclxuICAgICAgICAgICAgYml0cmF0ZUludGVydmFsOiA1MDAsXHJcbiAgICAgICAgICAgIC8vIEJ5IGRlZmF1bHQsIHVwbG9hZHMgYXJlIHN0YXJ0ZWQgYXV0b21hdGljYWxseSB3aGVuIGFkZGluZyBmaWxlczpcclxuICAgICAgICAgICAgYXV0b1VwbG9hZDogdHJ1ZSxcclxuXHJcbiAgICAgICAgICAgIC8vIEVycm9yIGFuZCBpbmZvIG1lc3NhZ2VzOlxyXG4gICAgICAgICAgICBtZXNzYWdlczoge1xyXG4gICAgICAgICAgICAgICAgdXBsb2FkZWRCeXRlczogJ1VwbG9hZGVkIGJ5dGVzIGV4Y2VlZCBmaWxlIHNpemUnXHJcbiAgICAgICAgICAgIH0sXHJcblxyXG4gICAgICAgICAgICAvLyBUcmFuc2xhdGlvbiBmdW5jdGlvbiwgZ2V0cyB0aGUgbWVzc2FnZSBrZXkgdG8gYmUgdHJhbnNsYXRlZFxyXG4gICAgICAgICAgICAvLyBhbmQgYW4gb2JqZWN0IHdpdGggY29udGV4dCBzcGVjaWZpYyBkYXRhIGFzIGFyZ3VtZW50czpcclxuICAgICAgICAgICAgaTE4bjogZnVuY3Rpb24gKG1lc3NhZ2UsIGNvbnRleHQpIHtcclxuICAgICAgICAgICAgICAgIG1lc3NhZ2UgPSB0aGlzLm1lc3NhZ2VzW21lc3NhZ2VdIHx8IG1lc3NhZ2UudG9TdHJpbmcoKTtcclxuICAgICAgICAgICAgICAgIGlmIChjb250ZXh0KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgJC5lYWNoKGNvbnRleHQsIGZ1bmN0aW9uIChrZXksIHZhbHVlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG1lc3NhZ2UgPSBtZXNzYWdlLnJlcGxhY2UoJ3snICsga2V5ICsgJ30nLCB2YWx1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gbWVzc2FnZTtcclxuICAgICAgICAgICAgfSxcclxuXHJcbiAgICAgICAgICAgIC8vIEFkZGl0aW9uYWwgZm9ybSBkYXRhIHRvIGJlIHNlbnQgYWxvbmcgd2l0aCB0aGUgZmlsZSB1cGxvYWRzIGNhbiBiZSBzZXRcclxuICAgICAgICAgICAgLy8gdXNpbmcgdGhpcyBvcHRpb24sIHdoaWNoIGFjY2VwdHMgYW4gYXJyYXkgb2Ygb2JqZWN0cyB3aXRoIG5hbWUgYW5kXHJcbiAgICAgICAgICAgIC8vIHZhbHVlIHByb3BlcnRpZXMsIGEgZnVuY3Rpb24gcmV0dXJuaW5nIHN1Y2ggYW4gYXJyYXksIGEgRm9ybURhdGFcclxuICAgICAgICAgICAgLy8gb2JqZWN0IChmb3IgWEhSIGZpbGUgdXBsb2FkcyksIG9yIGEgc2ltcGxlIG9iamVjdC5cclxuICAgICAgICAgICAgLy8gVGhlIGZvcm0gb2YgdGhlIGZpcnN0IGZpbGVJbnB1dCBpcyBnaXZlbiBhcyBwYXJhbWV0ZXIgdG8gdGhlIGZ1bmN0aW9uOlxyXG4gICAgICAgICAgICBmb3JtRGF0YTogZnVuY3Rpb24gKGZvcm0pIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBmb3JtLnNlcmlhbGl6ZUFycmF5KCk7XHJcbiAgICAgICAgICAgIH0sXHJcblxyXG4gICAgICAgICAgICAvLyBUaGUgYWRkIGNhbGxiYWNrIGlzIGludm9rZWQgYXMgc29vbiBhcyBmaWxlcyBhcmUgYWRkZWQgdG8gdGhlIGZpbGV1cGxvYWRcclxuICAgICAgICAgICAgLy8gd2lkZ2V0ICh2aWEgZmlsZSBpbnB1dCBzZWxlY3Rpb24sIGRyYWcgJiBkcm9wLCBwYXN0ZSBvciBhZGQgQVBJIGNhbGwpLlxyXG4gICAgICAgICAgICAvLyBJZiB0aGUgc2luZ2xlRmlsZVVwbG9hZHMgb3B0aW9uIGlzIGVuYWJsZWQsIHRoaXMgY2FsbGJhY2sgd2lsbCBiZVxyXG4gICAgICAgICAgICAvLyBjYWxsZWQgb25jZSBmb3IgZWFjaCBmaWxlIGluIHRoZSBzZWxlY3Rpb24gZm9yIFhIUiBmaWxlIHVwbG9hZHMsIGVsc2VcclxuICAgICAgICAgICAgLy8gb25jZSBmb3IgZWFjaCBmaWxlIHNlbGVjdGlvbi5cclxuICAgICAgICAgICAgLy9cclxuICAgICAgICAgICAgLy8gVGhlIHVwbG9hZCBzdGFydHMgd2hlbiB0aGUgc3VibWl0IG1ldGhvZCBpcyBpbnZva2VkIG9uIHRoZSBkYXRhIHBhcmFtZXRlci5cclxuICAgICAgICAgICAgLy8gVGhlIGRhdGEgb2JqZWN0IGNvbnRhaW5zIGEgZmlsZXMgcHJvcGVydHkgaG9sZGluZyB0aGUgYWRkZWQgZmlsZXNcclxuICAgICAgICAgICAgLy8gYW5kIGFsbG93cyB5b3UgdG8gb3ZlcnJpZGUgcGx1Z2luIG9wdGlvbnMgYXMgd2VsbCBhcyBkZWZpbmUgYWpheCBzZXR0aW5ncy5cclxuICAgICAgICAgICAgLy9cclxuICAgICAgICAgICAgLy8gTGlzdGVuZXJzIGZvciB0aGlzIGNhbGxiYWNrIGNhbiBhbHNvIGJlIGJvdW5kIHRoZSBmb2xsb3dpbmcgd2F5OlxyXG4gICAgICAgICAgICAvLyAuYmluZCgnZmlsZXVwbG9hZGFkZCcsIGZ1bmMpO1xyXG4gICAgICAgICAgICAvL1xyXG4gICAgICAgICAgICAvLyBkYXRhLnN1Ym1pdCgpIHJldHVybnMgYSBQcm9taXNlIG9iamVjdCBhbmQgYWxsb3dzIHRvIGF0dGFjaCBhZGRpdGlvbmFsXHJcbiAgICAgICAgICAgIC8vIGhhbmRsZXJzIHVzaW5nIGpRdWVyeSdzIERlZmVycmVkIGNhbGxiYWNrczpcclxuICAgICAgICAgICAgLy8gZGF0YS5zdWJtaXQoKS5kb25lKGZ1bmMpLmZhaWwoZnVuYykuYWx3YXlzKGZ1bmMpO1xyXG4gICAgICAgICAgICBhZGQ6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoZS5pc0RlZmF1bHRQcmV2ZW50ZWQoKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGlmIChkYXRhLmF1dG9VcGxvYWQgfHwgKGRhdGEuYXV0b1VwbG9hZCAhPT0gZmFsc2UgJiZcclxuICAgICAgICAgICAgICAgICAgICAgICAgJCh0aGlzKS5maWxldXBsb2FkKCdvcHRpb24nLCAnYXV0b1VwbG9hZCcpKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGRhdGEucHJvY2VzcygpLmRvbmUoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkYXRhLnN1Ym1pdCgpO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9LFxyXG5cclxuICAgICAgICAgICAgLy8gT3RoZXIgY2FsbGJhY2tzOlxyXG5cclxuICAgICAgICAgICAgLy8gQ2FsbGJhY2sgZm9yIHRoZSBzdWJtaXQgZXZlbnQgb2YgZWFjaCBmaWxlIHVwbG9hZDpcclxuICAgICAgICAgICAgLy8gc3VibWl0OiBmdW5jdGlvbiAoZSwgZGF0YSkge30sIC8vIC5iaW5kKCdmaWxldXBsb2Fkc3VibWl0JywgZnVuYyk7XHJcblxyXG4gICAgICAgICAgICAvLyBDYWxsYmFjayBmb3IgdGhlIHN0YXJ0IG9mIGVhY2ggZmlsZSB1cGxvYWQgcmVxdWVzdDpcclxuICAgICAgICAgICAgLy8gc2VuZDogZnVuY3Rpb24gKGUsIGRhdGEpIHt9LCAvLyAuYmluZCgnZmlsZXVwbG9hZHNlbmQnLCBmdW5jKTtcclxuXHJcbiAgICAgICAgICAgIC8vIENhbGxiYWNrIGZvciBzdWNjZXNzZnVsIHVwbG9hZHM6XHJcbiAgICAgICAgICAgIC8vIGRvbmU6IGZ1bmN0aW9uIChlLCBkYXRhKSB7fSwgLy8gLmJpbmQoJ2ZpbGV1cGxvYWRkb25lJywgZnVuYyk7XHJcblxyXG4gICAgICAgICAgICAvLyBDYWxsYmFjayBmb3IgZmFpbGVkIChhYm9ydCBvciBlcnJvcikgdXBsb2FkczpcclxuICAgICAgICAgICAgLy8gZmFpbDogZnVuY3Rpb24gKGUsIGRhdGEpIHt9LCAvLyAuYmluZCgnZmlsZXVwbG9hZGZhaWwnLCBmdW5jKTtcclxuXHJcbiAgICAgICAgICAgIC8vIENhbGxiYWNrIGZvciBjb21wbGV0ZWQgKHN1Y2Nlc3MsIGFib3J0IG9yIGVycm9yKSByZXF1ZXN0czpcclxuICAgICAgICAgICAgLy8gYWx3YXlzOiBmdW5jdGlvbiAoZSwgZGF0YSkge30sIC8vIC5iaW5kKCdmaWxldXBsb2FkYWx3YXlzJywgZnVuYyk7XHJcblxyXG4gICAgICAgICAgICAvLyBDYWxsYmFjayBmb3IgdXBsb2FkIHByb2dyZXNzIGV2ZW50czpcclxuICAgICAgICAgICAgLy8gcHJvZ3Jlc3M6IGZ1bmN0aW9uIChlLCBkYXRhKSB7fSwgLy8gLmJpbmQoJ2ZpbGV1cGxvYWRwcm9ncmVzcycsIGZ1bmMpO1xyXG5cclxuICAgICAgICAgICAgLy8gQ2FsbGJhY2sgZm9yIGdsb2JhbCB1cGxvYWQgcHJvZ3Jlc3MgZXZlbnRzOlxyXG4gICAgICAgICAgICAvLyBwcm9ncmVzc2FsbDogZnVuY3Rpb24gKGUsIGRhdGEpIHt9LCAvLyAuYmluZCgnZmlsZXVwbG9hZHByb2dyZXNzYWxsJywgZnVuYyk7XHJcblxyXG4gICAgICAgICAgICAvLyBDYWxsYmFjayBmb3IgdXBsb2FkcyBzdGFydCwgZXF1aXZhbGVudCB0byB0aGUgZ2xvYmFsIGFqYXhTdGFydCBldmVudDpcclxuICAgICAgICAgICAgLy8gc3RhcnQ6IGZ1bmN0aW9uIChlKSB7fSwgLy8gLmJpbmQoJ2ZpbGV1cGxvYWRzdGFydCcsIGZ1bmMpO1xyXG5cclxuICAgICAgICAgICAgLy8gQ2FsbGJhY2sgZm9yIHVwbG9hZHMgc3RvcCwgZXF1aXZhbGVudCB0byB0aGUgZ2xvYmFsIGFqYXhTdG9wIGV2ZW50OlxyXG4gICAgICAgICAgICAvLyBzdG9wOiBmdW5jdGlvbiAoZSkge30sIC8vIC5iaW5kKCdmaWxldXBsb2Fkc3RvcCcsIGZ1bmMpO1xyXG5cclxuICAgICAgICAgICAgLy8gQ2FsbGJhY2sgZm9yIGNoYW5nZSBldmVudHMgb2YgdGhlIGZpbGVJbnB1dChzKTpcclxuICAgICAgICAgICAgLy8gY2hhbmdlOiBmdW5jdGlvbiAoZSwgZGF0YSkge30sIC8vIC5iaW5kKCdmaWxldXBsb2FkY2hhbmdlJywgZnVuYyk7XHJcblxyXG4gICAgICAgICAgICAvLyBDYWxsYmFjayBmb3IgcGFzdGUgZXZlbnRzIHRvIHRoZSBwYXN0ZVpvbmUocyk6XHJcbiAgICAgICAgICAgIC8vIHBhc3RlOiBmdW5jdGlvbiAoZSwgZGF0YSkge30sIC8vIC5iaW5kKCdmaWxldXBsb2FkcGFzdGUnLCBmdW5jKTtcclxuXHJcbiAgICAgICAgICAgIC8vIENhbGxiYWNrIGZvciBkcm9wIGV2ZW50cyBvZiB0aGUgZHJvcFpvbmUocyk6XHJcbiAgICAgICAgICAgIC8vIGRyb3A6IGZ1bmN0aW9uIChlLCBkYXRhKSB7fSwgLy8gLmJpbmQoJ2ZpbGV1cGxvYWRkcm9wJywgZnVuYyk7XHJcblxyXG4gICAgICAgICAgICAvLyBDYWxsYmFjayBmb3IgZHJhZ292ZXIgZXZlbnRzIG9mIHRoZSBkcm9wWm9uZShzKTpcclxuICAgICAgICAgICAgLy8gZHJhZ292ZXI6IGZ1bmN0aW9uIChlKSB7fSwgLy8gLmJpbmQoJ2ZpbGV1cGxvYWRkcmFnb3ZlcicsIGZ1bmMpO1xyXG5cclxuICAgICAgICAgICAgLy8gQ2FsbGJhY2sgZm9yIHRoZSBzdGFydCBvZiBlYWNoIGNodW5rIHVwbG9hZCByZXF1ZXN0OlxyXG4gICAgICAgICAgICAvLyBjaHVua3NlbmQ6IGZ1bmN0aW9uIChlLCBkYXRhKSB7fSwgLy8gLmJpbmQoJ2ZpbGV1cGxvYWRjaHVua3NlbmQnLCBmdW5jKTtcclxuXHJcbiAgICAgICAgICAgIC8vIENhbGxiYWNrIGZvciBzdWNjZXNzZnVsIGNodW5rIHVwbG9hZHM6XHJcbiAgICAgICAgICAgIC8vIGNodW5rZG9uZTogZnVuY3Rpb24gKGUsIGRhdGEpIHt9LCAvLyAuYmluZCgnZmlsZXVwbG9hZGNodW5rZG9uZScsIGZ1bmMpO1xyXG5cclxuICAgICAgICAgICAgLy8gQ2FsbGJhY2sgZm9yIGZhaWxlZCAoYWJvcnQgb3IgZXJyb3IpIGNodW5rIHVwbG9hZHM6XHJcbiAgICAgICAgICAgIC8vIGNodW5rZmFpbDogZnVuY3Rpb24gKGUsIGRhdGEpIHt9LCAvLyAuYmluZCgnZmlsZXVwbG9hZGNodW5rZmFpbCcsIGZ1bmMpO1xyXG5cclxuICAgICAgICAgICAgLy8gQ2FsbGJhY2sgZm9yIGNvbXBsZXRlZCAoc3VjY2VzcywgYWJvcnQgb3IgZXJyb3IpIGNodW5rIHVwbG9hZCByZXF1ZXN0czpcclxuICAgICAgICAgICAgLy8gY2h1bmthbHdheXM6IGZ1bmN0aW9uIChlLCBkYXRhKSB7fSwgLy8gLmJpbmQoJ2ZpbGV1cGxvYWRjaHVua2Fsd2F5cycsIGZ1bmMpO1xyXG5cclxuICAgICAgICAgICAgLy8gVGhlIHBsdWdpbiBvcHRpb25zIGFyZSB1c2VkIGFzIHNldHRpbmdzIG9iamVjdCBmb3IgdGhlIGFqYXggY2FsbHMuXHJcbiAgICAgICAgICAgIC8vIFRoZSBmb2xsb3dpbmcgYXJlIGpRdWVyeSBhamF4IHNldHRpbmdzIHJlcXVpcmVkIGZvciB0aGUgZmlsZSB1cGxvYWRzOlxyXG4gICAgICAgICAgICBwcm9jZXNzRGF0YTogZmFsc2UsXHJcbiAgICAgICAgICAgIGNvbnRlbnRUeXBlOiBmYWxzZSxcclxuICAgICAgICAgICAgY2FjaGU6IGZhbHNlXHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgLy8gQSBsaXN0IG9mIG9wdGlvbnMgdGhhdCByZXF1aXJlIHJlaW5pdGlhbGl6aW5nIGV2ZW50IGxpc3RlbmVycyBhbmQvb3JcclxuICAgICAgICAvLyBzcGVjaWFsIGluaXRpYWxpemF0aW9uIGNvZGU6XHJcbiAgICAgICAgX3NwZWNpYWxPcHRpb25zOiBbXHJcbiAgICAgICAgICAgICdmaWxlSW5wdXQnLFxyXG4gICAgICAgICAgICAnZHJvcFpvbmUnLFxyXG4gICAgICAgICAgICAncGFzdGVab25lJyxcclxuICAgICAgICAgICAgJ211bHRpcGFydCcsXHJcbiAgICAgICAgICAgICdmb3JjZUlmcmFtZVRyYW5zcG9ydCdcclxuICAgICAgICBdLFxyXG5cclxuICAgICAgICBfYmxvYlNsaWNlOiAkLnN1cHBvcnQuYmxvYlNsaWNlICYmIGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHNsaWNlID0gdGhpcy5zbGljZSB8fCB0aGlzLndlYmtpdFNsaWNlIHx8IHRoaXMubW96U2xpY2U7XHJcbiAgICAgICAgICAgIHJldHVybiBzbGljZS5hcHBseSh0aGlzLCBhcmd1bWVudHMpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9CaXRyYXRlVGltZXI6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdGhpcy50aW1lc3RhbXAgPSAoKERhdGUubm93KSA/IERhdGUubm93KCkgOiAobmV3IERhdGUoKSkuZ2V0VGltZSgpKTtcclxuICAgICAgICAgICAgdGhpcy5sb2FkZWQgPSAwO1xyXG4gICAgICAgICAgICB0aGlzLmJpdHJhdGUgPSAwO1xyXG4gICAgICAgICAgICB0aGlzLmdldEJpdHJhdGUgPSBmdW5jdGlvbiAobm93LCBsb2FkZWQsIGludGVydmFsKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgdGltZURpZmYgPSBub3cgLSB0aGlzLnRpbWVzdGFtcDtcclxuICAgICAgICAgICAgICAgIGlmICghdGhpcy5iaXRyYXRlIHx8ICFpbnRlcnZhbCB8fCB0aW1lRGlmZiA+IGludGVydmFsKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5iaXRyYXRlID0gKGxvYWRlZCAtIHRoaXMubG9hZGVkKSAqICgxMDAwIC8gdGltZURpZmYpICogODtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmxvYWRlZCA9IGxvYWRlZDtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnRpbWVzdGFtcCA9IG5vdztcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLmJpdHJhdGU7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX2lzWEhSVXBsb2FkOiBmdW5jdGlvbiAob3B0aW9ucykge1xyXG4gICAgICAgICAgICByZXR1cm4gIW9wdGlvbnMuZm9yY2VJZnJhbWVUcmFuc3BvcnQgJiZcclxuICAgICAgICAgICAgICAgICgoIW9wdGlvbnMubXVsdGlwYXJ0ICYmICQuc3VwcG9ydC54aHJGaWxlVXBsb2FkKSB8fFxyXG4gICAgICAgICAgICAgICAgJC5zdXBwb3J0LnhockZvcm1EYXRhRmlsZVVwbG9hZCk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX2dldEZvcm1EYXRhOiBmdW5jdGlvbiAob3B0aW9ucykge1xyXG4gICAgICAgICAgICB2YXIgZm9ybURhdGE7XHJcbiAgICAgICAgICAgIGlmICgkLnR5cGUob3B0aW9ucy5mb3JtRGF0YSkgPT09ICdmdW5jdGlvbicpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBvcHRpb25zLmZvcm1EYXRhKG9wdGlvbnMuZm9ybSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgaWYgKCQuaXNBcnJheShvcHRpb25zLmZvcm1EYXRhKSkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIG9wdGlvbnMuZm9ybURhdGE7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgaWYgKCQudHlwZShvcHRpb25zLmZvcm1EYXRhKSA9PT0gJ29iamVjdCcpIHtcclxuICAgICAgICAgICAgICAgIGZvcm1EYXRhID0gW107XHJcbiAgICAgICAgICAgICAgICAkLmVhY2gob3B0aW9ucy5mb3JtRGF0YSwgZnVuY3Rpb24gKG5hbWUsIHZhbHVlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZm9ybURhdGEucHVzaCh7bmFtZTogbmFtZSwgdmFsdWU6IHZhbHVlfSk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIHJldHVybiBmb3JtRGF0YTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXR1cm4gW107XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX2dldFRvdGFsOiBmdW5jdGlvbiAoZmlsZXMpIHtcclxuICAgICAgICAgICAgdmFyIHRvdGFsID0gMDtcclxuICAgICAgICAgICAgJC5lYWNoKGZpbGVzLCBmdW5jdGlvbiAoaW5kZXgsIGZpbGUpIHtcclxuICAgICAgICAgICAgICAgIHRvdGFsICs9IGZpbGUuc2l6ZSB8fCAxO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgcmV0dXJuIHRvdGFsO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9pbml0UHJvZ3Jlc3NPYmplY3Q6IGZ1bmN0aW9uIChvYmopIHtcclxuICAgICAgICAgICAgdmFyIHByb2dyZXNzID0ge1xyXG4gICAgICAgICAgICAgICAgbG9hZGVkOiAwLFxyXG4gICAgICAgICAgICAgICAgdG90YWw6IDAsXHJcbiAgICAgICAgICAgICAgICBiaXRyYXRlOiAwXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGlmIChvYmouX3Byb2dyZXNzKSB7XHJcbiAgICAgICAgICAgICAgICAkLmV4dGVuZChvYmouX3Byb2dyZXNzLCBwcm9ncmVzcyk7XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICBvYmouX3Byb2dyZXNzID0gcHJvZ3Jlc3M7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfaW5pdFJlc3BvbnNlT2JqZWN0OiBmdW5jdGlvbiAob2JqKSB7XHJcbiAgICAgICAgICAgIHZhciBwcm9wO1xyXG4gICAgICAgICAgICBpZiAob2JqLl9yZXNwb25zZSkge1xyXG4gICAgICAgICAgICAgICAgZm9yIChwcm9wIGluIG9iai5fcmVzcG9uc2UpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAob2JqLl9yZXNwb25zZS5oYXNPd25Qcm9wZXJ0eShwcm9wKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkZWxldGUgb2JqLl9yZXNwb25zZVtwcm9wXTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICBvYmouX3Jlc3BvbnNlID0ge307XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfb25Qcm9ncmVzczogZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuICAgICAgICAgICAgaWYgKGUubGVuZ3RoQ29tcHV0YWJsZSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIG5vdyA9ICgoRGF0ZS5ub3cpID8gRGF0ZS5ub3coKSA6IChuZXcgRGF0ZSgpKS5nZXRUaW1lKCkpLFxyXG4gICAgICAgICAgICAgICAgICAgIGxvYWRlZDtcclxuICAgICAgICAgICAgICAgIGlmIChkYXRhLl90aW1lICYmIGRhdGEucHJvZ3Jlc3NJbnRlcnZhbCAmJlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAobm93IC0gZGF0YS5fdGltZSA8IGRhdGEucHJvZ3Jlc3NJbnRlcnZhbCkgJiZcclxuICAgICAgICAgICAgICAgICAgICAgICAgZS5sb2FkZWQgIT09IGUudG90YWwpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBkYXRhLl90aW1lID0gbm93O1xyXG4gICAgICAgICAgICAgICAgbG9hZGVkID0gTWF0aC5mbG9vcihcclxuICAgICAgICAgICAgICAgICAgICBlLmxvYWRlZCAvIGUudG90YWwgKiAoZGF0YS5jaHVua1NpemUgfHwgZGF0YS5fcHJvZ3Jlc3MudG90YWwpXHJcbiAgICAgICAgICAgICAgICApICsgKGRhdGEudXBsb2FkZWRCeXRlcyB8fCAwKTtcclxuICAgICAgICAgICAgICAgIC8vIEFkZCB0aGUgZGlmZmVyZW5jZSBmcm9tIHRoZSBwcmV2aW91c2x5IGxvYWRlZCBzdGF0ZVxyXG4gICAgICAgICAgICAgICAgLy8gdG8gdGhlIGdsb2JhbCBsb2FkZWQgY291bnRlcjpcclxuICAgICAgICAgICAgICAgIHRoaXMuX3Byb2dyZXNzLmxvYWRlZCArPSAobG9hZGVkIC0gZGF0YS5fcHJvZ3Jlc3MubG9hZGVkKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3Byb2dyZXNzLmJpdHJhdGUgPSB0aGlzLl9iaXRyYXRlVGltZXIuZ2V0Qml0cmF0ZShcclxuICAgICAgICAgICAgICAgICAgICBub3csXHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5fcHJvZ3Jlc3MubG9hZGVkLFxyXG4gICAgICAgICAgICAgICAgICAgIGRhdGEuYml0cmF0ZUludGVydmFsXHJcbiAgICAgICAgICAgICAgICApO1xyXG4gICAgICAgICAgICAgICAgZGF0YS5fcHJvZ3Jlc3MubG9hZGVkID0gZGF0YS5sb2FkZWQgPSBsb2FkZWQ7XHJcbiAgICAgICAgICAgICAgICBkYXRhLl9wcm9ncmVzcy5iaXRyYXRlID0gZGF0YS5iaXRyYXRlID0gZGF0YS5fYml0cmF0ZVRpbWVyLmdldEJpdHJhdGUoXHJcbiAgICAgICAgICAgICAgICAgICAgbm93LFxyXG4gICAgICAgICAgICAgICAgICAgIGxvYWRlZCxcclxuICAgICAgICAgICAgICAgICAgICBkYXRhLmJpdHJhdGVJbnRlcnZhbFxyXG4gICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgICAgIC8vIFRyaWdnZXIgYSBjdXN0b20gcHJvZ3Jlc3MgZXZlbnQgd2l0aCBhIHRvdGFsIGRhdGEgcHJvcGVydHkgc2V0XHJcbiAgICAgICAgICAgICAgICAvLyB0byB0aGUgZmlsZSBzaXplKHMpIG9mIHRoZSBjdXJyZW50IHVwbG9hZCBhbmQgYSBsb2FkZWQgZGF0YVxyXG4gICAgICAgICAgICAgICAgLy8gcHJvcGVydHkgY2FsY3VsYXRlZCBhY2NvcmRpbmdseTpcclxuICAgICAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoXHJcbiAgICAgICAgICAgICAgICAgICAgJ3Byb2dyZXNzJyxcclxuICAgICAgICAgICAgICAgICAgICAkLkV2ZW50KCdwcm9ncmVzcycsIHtkZWxlZ2F0ZWRFdmVudDogZX0pLFxyXG4gICAgICAgICAgICAgICAgICAgIGRhdGFcclxuICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgICAgICAvLyBUcmlnZ2VyIGEgZ2xvYmFsIHByb2dyZXNzIGV2ZW50IGZvciBhbGwgY3VycmVudCBmaWxlIHVwbG9hZHMsXHJcbiAgICAgICAgICAgICAgICAvLyBpbmNsdWRpbmcgYWpheCBjYWxscyBxdWV1ZWQgZm9yIHNlcXVlbnRpYWwgZmlsZSB1cGxvYWRzOlxyXG4gICAgICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcihcclxuICAgICAgICAgICAgICAgICAgICAncHJvZ3Jlc3NhbGwnLFxyXG4gICAgICAgICAgICAgICAgICAgICQuRXZlbnQoJ3Byb2dyZXNzYWxsJywge2RlbGVnYXRlZEV2ZW50OiBlfSksXHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5fcHJvZ3Jlc3NcclxuICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfaW5pdFByb2dyZXNzTGlzdGVuZXI6IGZ1bmN0aW9uIChvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIHZhciB0aGF0ID0gdGhpcyxcclxuICAgICAgICAgICAgICAgIHhociA9IG9wdGlvbnMueGhyID8gb3B0aW9ucy54aHIoKSA6ICQuYWpheFNldHRpbmdzLnhocigpO1xyXG4gICAgICAgICAgICAvLyBBY2Nlc3NzIHRvIHRoZSBuYXRpdmUgWEhSIG9iamVjdCBpcyByZXF1aXJlZCB0byBhZGQgZXZlbnQgbGlzdGVuZXJzXHJcbiAgICAgICAgICAgIC8vIGZvciB0aGUgdXBsb2FkIHByb2dyZXNzIGV2ZW50OlxyXG4gICAgICAgICAgICBpZiAoeGhyLnVwbG9hZCkge1xyXG4gICAgICAgICAgICAgICAgJCh4aHIudXBsb2FkKS5iaW5kKCdwcm9ncmVzcycsIGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG9lID0gZS5vcmlnaW5hbEV2ZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIC8vIE1ha2Ugc3VyZSB0aGUgcHJvZ3Jlc3MgZXZlbnQgcHJvcGVydGllcyBnZXQgY29waWVkIG92ZXI6XHJcbiAgICAgICAgICAgICAgICAgICAgZS5sZW5ndGhDb21wdXRhYmxlID0gb2UubGVuZ3RoQ29tcHV0YWJsZTtcclxuICAgICAgICAgICAgICAgICAgICBlLmxvYWRlZCA9IG9lLmxvYWRlZDtcclxuICAgICAgICAgICAgICAgICAgICBlLnRvdGFsID0gb2UudG90YWw7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhhdC5fb25Qcm9ncmVzcyhlLCBvcHRpb25zKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgb3B0aW9ucy54aHIgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHhocjtcclxuICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfaXNJbnN0YW5jZU9mOiBmdW5jdGlvbiAodHlwZSwgb2JqKSB7XHJcbiAgICAgICAgICAgIC8vIENyb3NzLWZyYW1lIGluc3RhbmNlb2YgY2hlY2tcclxuICAgICAgICAgICAgcmV0dXJuIE9iamVjdC5wcm90b3R5cGUudG9TdHJpbmcuY2FsbChvYmopID09PSAnW29iamVjdCAnICsgdHlwZSArICddJztcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfaW5pdFhIUkRhdGE6IGZ1bmN0aW9uIChvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIHZhciB0aGF0ID0gdGhpcyxcclxuICAgICAgICAgICAgICAgIGZvcm1EYXRhLFxyXG4gICAgICAgICAgICAgICAgZmlsZSA9IG9wdGlvbnMuZmlsZXNbMF0sXHJcbiAgICAgICAgICAgICAgICAvLyBJZ25vcmUgbm9uLW11bHRpcGFydCBzZXR0aW5nIGlmIG5vdCBzdXBwb3J0ZWQ6XHJcbiAgICAgICAgICAgICAgICBtdWx0aXBhcnQgPSBvcHRpb25zLm11bHRpcGFydCB8fCAhJC5zdXBwb3J0LnhockZpbGVVcGxvYWQsXHJcbiAgICAgICAgICAgICAgICBwYXJhbU5hbWUgPSAkLnR5cGUob3B0aW9ucy5wYXJhbU5hbWUpID09PSAnYXJyYXknID9cclxuICAgICAgICAgICAgICAgICAgICBvcHRpb25zLnBhcmFtTmFtZVswXSA6IG9wdGlvbnMucGFyYW1OYW1lO1xyXG4gICAgICAgICAgICBvcHRpb25zLmhlYWRlcnMgPSAkLmV4dGVuZCh7fSwgb3B0aW9ucy5oZWFkZXJzKTtcclxuICAgICAgICAgICAgaWYgKG9wdGlvbnMuY29udGVudFJhbmdlKSB7XHJcbiAgICAgICAgICAgICAgICBvcHRpb25zLmhlYWRlcnNbJ0NvbnRlbnQtUmFuZ2UnXSA9IG9wdGlvbnMuY29udGVudFJhbmdlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmICghbXVsdGlwYXJ0IHx8IG9wdGlvbnMuYmxvYiB8fCAhdGhpcy5faXNJbnN0YW5jZU9mKCdGaWxlJywgZmlsZSkpIHtcclxuICAgICAgICAgICAgICAgIG9wdGlvbnMuaGVhZGVyc1snQ29udGVudC1EaXNwb3NpdGlvbiddID0gJ2F0dGFjaG1lbnQ7IGZpbGVuYW1lPVwiJyArXHJcbiAgICAgICAgICAgICAgICAgICAgZW5jb2RlVVJJKGZpbGUubmFtZSkgKyAnXCInO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmICghbXVsdGlwYXJ0KSB7XHJcbiAgICAgICAgICAgICAgICBvcHRpb25zLmNvbnRlbnRUeXBlID0gZmlsZS50eXBlIHx8ICdhcHBsaWNhdGlvbi9vY3RldC1zdHJlYW0nO1xyXG4gICAgICAgICAgICAgICAgb3B0aW9ucy5kYXRhID0gb3B0aW9ucy5ibG9iIHx8IGZpbGU7XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAoJC5zdXBwb3J0LnhockZvcm1EYXRhRmlsZVVwbG9hZCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKG9wdGlvbnMucG9zdE1lc3NhZ2UpIHtcclxuICAgICAgICAgICAgICAgICAgICAvLyB3aW5kb3cucG9zdE1lc3NhZ2UgZG9lcyBub3QgYWxsb3cgc2VuZGluZyBGb3JtRGF0YVxyXG4gICAgICAgICAgICAgICAgICAgIC8vIG9iamVjdHMsIHNvIHdlIGp1c3QgYWRkIHRoZSBGaWxlL0Jsb2Igb2JqZWN0cyB0b1xyXG4gICAgICAgICAgICAgICAgICAgIC8vIHRoZSBmb3JtRGF0YSBhcnJheSBhbmQgbGV0IHRoZSBwb3N0TWVzc2FnZSB3aW5kb3dcclxuICAgICAgICAgICAgICAgICAgICAvLyBjcmVhdGUgdGhlIEZvcm1EYXRhIG9iamVjdCBvdXQgb2YgdGhpcyBhcnJheTpcclxuICAgICAgICAgICAgICAgICAgICBmb3JtRGF0YSA9IHRoaXMuX2dldEZvcm1EYXRhKG9wdGlvbnMpO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChvcHRpb25zLmJsb2IpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZm9ybURhdGEucHVzaCh7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBuYW1lOiBwYXJhbU5hbWUsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YWx1ZTogb3B0aW9ucy5ibG9iXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICQuZWFjaChvcHRpb25zLmZpbGVzLCBmdW5jdGlvbiAoaW5kZXgsIGZpbGUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvcm1EYXRhLnB1c2goe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5hbWU6ICgkLnR5cGUob3B0aW9ucy5wYXJhbU5hbWUpID09PSAnYXJyYXknICYmXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG9wdGlvbnMucGFyYW1OYW1lW2luZGV4XSkgfHwgcGFyYW1OYW1lLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhbHVlOiBmaWxlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGhhdC5faXNJbnN0YW5jZU9mKCdGb3JtRGF0YScsIG9wdGlvbnMuZm9ybURhdGEpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGZvcm1EYXRhID0gb3B0aW9ucy5mb3JtRGF0YTtcclxuICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBmb3JtRGF0YSA9IG5ldyBGb3JtRGF0YSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAkLmVhY2godGhpcy5fZ2V0Rm9ybURhdGEob3B0aW9ucyksIGZ1bmN0aW9uIChpbmRleCwgZmllbGQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvcm1EYXRhLmFwcGVuZChmaWVsZC5uYW1lLCBmaWVsZC52YWx1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICBpZiAob3B0aW9ucy5ibG9iKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGZvcm1EYXRhLmFwcGVuZChwYXJhbU5hbWUsIG9wdGlvbnMuYmxvYiwgZmlsZS5uYW1lKTtcclxuICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAkLmVhY2gob3B0aW9ucy5maWxlcywgZnVuY3Rpb24gKGluZGV4LCBmaWxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBUaGlzIGNoZWNrIGFsbG93cyB0aGUgdGVzdHMgdG8gcnVuIHdpdGhcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIGR1bW15IG9iamVjdHM6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAodGhhdC5faXNJbnN0YW5jZU9mKCdGaWxlJywgZmlsZSkgfHxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhhdC5faXNJbnN0YW5jZU9mKCdCbG9iJywgZmlsZSkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmb3JtRGF0YS5hcHBlbmQoXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICgkLnR5cGUob3B0aW9ucy5wYXJhbU5hbWUpID09PSAnYXJyYXknICYmXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBvcHRpb25zLnBhcmFtTmFtZVtpbmRleF0pIHx8IHBhcmFtTmFtZSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZmlsZSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZmlsZS51cGxvYWROYW1lIHx8IGZpbGUubmFtZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIG9wdGlvbnMuZGF0YSA9IGZvcm1EYXRhO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIC8vIEJsb2IgcmVmZXJlbmNlIGlzIG5vdCBuZWVkZWQgYW55bW9yZSwgZnJlZSBtZW1vcnk6XHJcbiAgICAgICAgICAgIG9wdGlvbnMuYmxvYiA9IG51bGw7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX2luaXRJZnJhbWVTZXR0aW5nczogZnVuY3Rpb24gKG9wdGlvbnMpIHtcclxuICAgICAgICAgICAgdmFyIHRhcmdldEhvc3QgPSAkKCc8YT48L2E+JykucHJvcCgnaHJlZicsIG9wdGlvbnMudXJsKS5wcm9wKCdob3N0Jyk7XHJcbiAgICAgICAgICAgIC8vIFNldHRpbmcgdGhlIGRhdGFUeXBlIHRvIGlmcmFtZSBlbmFibGVzIHRoZSBpZnJhbWUgdHJhbnNwb3J0OlxyXG4gICAgICAgICAgICBvcHRpb25zLmRhdGFUeXBlID0gJ2lmcmFtZSAnICsgKG9wdGlvbnMuZGF0YVR5cGUgfHwgJycpO1xyXG4gICAgICAgICAgICAvLyBUaGUgaWZyYW1lIHRyYW5zcG9ydCBhY2NlcHRzIGEgc2VyaWFsaXplZCBhcnJheSBhcyBmb3JtIGRhdGE6XHJcbiAgICAgICAgICAgIG9wdGlvbnMuZm9ybURhdGEgPSB0aGlzLl9nZXRGb3JtRGF0YShvcHRpb25zKTtcclxuICAgICAgICAgICAgLy8gQWRkIHJlZGlyZWN0IHVybCB0byBmb3JtIGRhdGEgb24gY3Jvc3MtZG9tYWluIHVwbG9hZHM6XHJcbiAgICAgICAgICAgIGlmIChvcHRpb25zLnJlZGlyZWN0ICYmIHRhcmdldEhvc3QgJiYgdGFyZ2V0SG9zdCAhPT0gbG9jYXRpb24uaG9zdCkge1xyXG4gICAgICAgICAgICAgICAgb3B0aW9ucy5mb3JtRGF0YS5wdXNoKHtcclxuICAgICAgICAgICAgICAgICAgICBuYW1lOiBvcHRpb25zLnJlZGlyZWN0UGFyYW1OYW1lIHx8ICdyZWRpcmVjdCcsXHJcbiAgICAgICAgICAgICAgICAgICAgdmFsdWU6IG9wdGlvbnMucmVkaXJlY3RcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX2luaXREYXRhU2V0dGluZ3M6IGZ1bmN0aW9uIChvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLl9pc1hIUlVwbG9hZChvcHRpb25zKSkge1xyXG4gICAgICAgICAgICAgICAgaWYgKCF0aGlzLl9jaHVua2VkVXBsb2FkKG9wdGlvbnMsIHRydWUpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCFvcHRpb25zLmRhdGEpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5faW5pdFhIUkRhdGEob3B0aW9ucyk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuX2luaXRQcm9ncmVzc0xpc3RlbmVyKG9wdGlvbnMpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgaWYgKG9wdGlvbnMucG9zdE1lc3NhZ2UpIHtcclxuICAgICAgICAgICAgICAgICAgICAvLyBTZXR0aW5nIHRoZSBkYXRhVHlwZSB0byBwb3N0bWVzc2FnZSBlbmFibGVzIHRoZVxyXG4gICAgICAgICAgICAgICAgICAgIC8vIHBvc3RNZXNzYWdlIHRyYW5zcG9ydDpcclxuICAgICAgICAgICAgICAgICAgICBvcHRpb25zLmRhdGFUeXBlID0gJ3Bvc3RtZXNzYWdlICcgKyAob3B0aW9ucy5kYXRhVHlwZSB8fCAnJyk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9pbml0SWZyYW1lU2V0dGluZ3Mob3B0aW9ucyk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfZ2V0UGFyYW1OYW1lOiBmdW5jdGlvbiAob3B0aW9ucykge1xyXG4gICAgICAgICAgICB2YXIgZmlsZUlucHV0ID0gJChvcHRpb25zLmZpbGVJbnB1dCksXHJcbiAgICAgICAgICAgICAgICBwYXJhbU5hbWUgPSBvcHRpb25zLnBhcmFtTmFtZTtcclxuICAgICAgICAgICAgaWYgKCFwYXJhbU5hbWUpIHtcclxuICAgICAgICAgICAgICAgIHBhcmFtTmFtZSA9IFtdO1xyXG4gICAgICAgICAgICAgICAgZmlsZUlucHV0LmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBpbnB1dCA9ICQodGhpcyksXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG5hbWUgPSBpbnB1dC5wcm9wKCduYW1lJykgfHwgJ2ZpbGVzW10nLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpID0gKGlucHV0LnByb3AoJ2ZpbGVzJykgfHwgWzFdKS5sZW5ndGg7XHJcbiAgICAgICAgICAgICAgICAgICAgd2hpbGUgKGkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcGFyYW1OYW1lLnB1c2gobmFtZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGkgLT0gMTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIGlmICghcGFyYW1OYW1lLmxlbmd0aCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHBhcmFtTmFtZSA9IFtmaWxlSW5wdXQucHJvcCgnbmFtZScpIHx8ICdmaWxlc1tdJ107XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAoISQuaXNBcnJheShwYXJhbU5hbWUpKSB7XHJcbiAgICAgICAgICAgICAgICBwYXJhbU5hbWUgPSBbcGFyYW1OYW1lXTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXR1cm4gcGFyYW1OYW1lO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9pbml0Rm9ybVNldHRpbmdzOiBmdW5jdGlvbiAob3B0aW9ucykge1xyXG4gICAgICAgICAgICAvLyBSZXRyaWV2ZSBtaXNzaW5nIG9wdGlvbnMgZnJvbSB0aGUgaW5wdXQgZmllbGQgYW5kIHRoZVxyXG4gICAgICAgICAgICAvLyBhc3NvY2lhdGVkIGZvcm0sIGlmIGF2YWlsYWJsZTpcclxuICAgICAgICAgICAgaWYgKCFvcHRpb25zLmZvcm0gfHwgIW9wdGlvbnMuZm9ybS5sZW5ndGgpIHtcclxuICAgICAgICAgICAgICAgIG9wdGlvbnMuZm9ybSA9ICQob3B0aW9ucy5maWxlSW5wdXQucHJvcCgnZm9ybScpKTtcclxuICAgICAgICAgICAgICAgIC8vIElmIHRoZSBnaXZlbiBmaWxlIGlucHV0IGRvZXNuJ3QgaGF2ZSBhbiBhc3NvY2lhdGVkIGZvcm0sXHJcbiAgICAgICAgICAgICAgICAvLyB1c2UgdGhlIGRlZmF1bHQgd2lkZ2V0IGZpbGUgaW5wdXQncyBmb3JtOlxyXG4gICAgICAgICAgICAgICAgaWYgKCFvcHRpb25zLmZvcm0ubGVuZ3RoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgb3B0aW9ucy5mb3JtID0gJCh0aGlzLm9wdGlvbnMuZmlsZUlucHV0LnByb3AoJ2Zvcm0nKSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgb3B0aW9ucy5wYXJhbU5hbWUgPSB0aGlzLl9nZXRQYXJhbU5hbWUob3B0aW9ucyk7XHJcbiAgICAgICAgICAgIGlmICghb3B0aW9ucy51cmwpIHtcclxuICAgICAgICAgICAgICAgIG9wdGlvbnMudXJsID0gb3B0aW9ucy5mb3JtLnByb3AoJ2FjdGlvbicpIHx8IGxvY2F0aW9uLmhyZWY7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgLy8gVGhlIEhUVFAgcmVxdWVzdCBtZXRob2QgbXVzdCBiZSBcIlBPU1RcIiBvciBcIlBVVFwiOlxyXG4gICAgICAgICAgICBvcHRpb25zLnR5cGUgPSAob3B0aW9ucy50eXBlIHx8XHJcbiAgICAgICAgICAgICAgICAoJC50eXBlKG9wdGlvbnMuZm9ybS5wcm9wKCdtZXRob2QnKSkgPT09ICdzdHJpbmcnICYmXHJcbiAgICAgICAgICAgICAgICAgICAgb3B0aW9ucy5mb3JtLnByb3AoJ21ldGhvZCcpKSB8fCAnJ1xyXG4gICAgICAgICAgICAgICAgKS50b1VwcGVyQ2FzZSgpO1xyXG4gICAgICAgICAgICBpZiAob3B0aW9ucy50eXBlICE9PSAnUE9TVCcgJiYgb3B0aW9ucy50eXBlICE9PSAnUFVUJyAmJlxyXG4gICAgICAgICAgICAgICAgICAgIG9wdGlvbnMudHlwZSAhPT0gJ1BBVENIJykge1xyXG4gICAgICAgICAgICAgICAgb3B0aW9ucy50eXBlID0gJ1BPU1QnO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmICghb3B0aW9ucy5mb3JtQWNjZXB0Q2hhcnNldCkge1xyXG4gICAgICAgICAgICAgICAgb3B0aW9ucy5mb3JtQWNjZXB0Q2hhcnNldCA9IG9wdGlvbnMuZm9ybS5hdHRyKCdhY2NlcHQtY2hhcnNldCcpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX2dldEFKQVhTZXR0aW5nczogZnVuY3Rpb24gKGRhdGEpIHtcclxuICAgICAgICAgICAgdmFyIG9wdGlvbnMgPSAkLmV4dGVuZCh7fSwgdGhpcy5vcHRpb25zLCBkYXRhKTtcclxuICAgICAgICAgICAgdGhpcy5faW5pdEZvcm1TZXR0aW5ncyhvcHRpb25zKTtcclxuICAgICAgICAgICAgdGhpcy5faW5pdERhdGFTZXR0aW5ncyhvcHRpb25zKTtcclxuICAgICAgICAgICAgcmV0dXJuIG9wdGlvbnM7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgLy8galF1ZXJ5IDEuNiBkb2Vzbid0IHByb3ZpZGUgLnN0YXRlKCksXHJcbiAgICAgICAgLy8gd2hpbGUgalF1ZXJ5IDEuOCsgcmVtb3ZlZCAuaXNSZWplY3RlZCgpIGFuZCAuaXNSZXNvbHZlZCgpOlxyXG4gICAgICAgIF9nZXREZWZlcnJlZFN0YXRlOiBmdW5jdGlvbiAoZGVmZXJyZWQpIHtcclxuICAgICAgICAgICAgaWYgKGRlZmVycmVkLnN0YXRlKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZGVmZXJyZWQuc3RhdGUoKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBpZiAoZGVmZXJyZWQuaXNSZXNvbHZlZCgpKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gJ3Jlc29sdmVkJztcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBpZiAoZGVmZXJyZWQuaXNSZWplY3RlZCgpKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gJ3JlamVjdGVkJztcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXR1cm4gJ3BlbmRpbmcnO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIC8vIE1hcHMganFYSFIgY2FsbGJhY2tzIHRvIHRoZSBlcXVpdmFsZW50XHJcbiAgICAgICAgLy8gbWV0aG9kcyBvZiB0aGUgZ2l2ZW4gUHJvbWlzZSBvYmplY3Q6XHJcbiAgICAgICAgX2VuaGFuY2VQcm9taXNlOiBmdW5jdGlvbiAocHJvbWlzZSkge1xyXG4gICAgICAgICAgICBwcm9taXNlLnN1Y2Nlc3MgPSBwcm9taXNlLmRvbmU7XHJcbiAgICAgICAgICAgIHByb21pc2UuZXJyb3IgPSBwcm9taXNlLmZhaWw7XHJcbiAgICAgICAgICAgIHByb21pc2UuY29tcGxldGUgPSBwcm9taXNlLmFsd2F5cztcclxuICAgICAgICAgICAgcmV0dXJuIHByb21pc2U7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgLy8gQ3JlYXRlcyBhbmQgcmV0dXJucyBhIFByb21pc2Ugb2JqZWN0IGVuaGFuY2VkIHdpdGhcclxuICAgICAgICAvLyB0aGUganFYSFIgbWV0aG9kcyBhYm9ydCwgc3VjY2VzcywgZXJyb3IgYW5kIGNvbXBsZXRlOlxyXG4gICAgICAgIF9nZXRYSFJQcm9taXNlOiBmdW5jdGlvbiAocmVzb2x2ZU9yUmVqZWN0LCBjb250ZXh0LCBhcmdzKSB7XHJcbiAgICAgICAgICAgIHZhciBkZmQgPSAkLkRlZmVycmVkKCksXHJcbiAgICAgICAgICAgICAgICBwcm9taXNlID0gZGZkLnByb21pc2UoKTtcclxuICAgICAgICAgICAgY29udGV4dCA9IGNvbnRleHQgfHwgdGhpcy5vcHRpb25zLmNvbnRleHQgfHwgcHJvbWlzZTtcclxuICAgICAgICAgICAgaWYgKHJlc29sdmVPclJlamVjdCA9PT0gdHJ1ZSkge1xyXG4gICAgICAgICAgICAgICAgZGZkLnJlc29sdmVXaXRoKGNvbnRleHQsIGFyZ3MpO1xyXG4gICAgICAgICAgICB9IGVsc2UgaWYgKHJlc29sdmVPclJlamVjdCA9PT0gZmFsc2UpIHtcclxuICAgICAgICAgICAgICAgIGRmZC5yZWplY3RXaXRoKGNvbnRleHQsIGFyZ3MpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHByb21pc2UuYWJvcnQgPSBkZmQucHJvbWlzZTtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2VuaGFuY2VQcm9taXNlKHByb21pc2UpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIC8vIEFkZHMgY29udmVuaWVuY2UgbWV0aG9kcyB0byB0aGUgZGF0YSBjYWxsYmFjayBhcmd1bWVudDpcclxuICAgICAgICBfYWRkQ29udmVuaWVuY2VNZXRob2RzOiBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG4gICAgICAgICAgICB2YXIgdGhhdCA9IHRoaXMsXHJcbiAgICAgICAgICAgICAgICBnZXRQcm9taXNlID0gZnVuY3Rpb24gKGFyZ3MpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gJC5EZWZlcnJlZCgpLnJlc29sdmVXaXRoKHRoYXQsIGFyZ3MpLnByb21pc2UoKTtcclxuICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGRhdGEucHJvY2VzcyA9IGZ1bmN0aW9uIChyZXNvbHZlRnVuYywgcmVqZWN0RnVuYykge1xyXG4gICAgICAgICAgICAgICAgaWYgKHJlc29sdmVGdW5jIHx8IHJlamVjdEZ1bmMpIHtcclxuICAgICAgICAgICAgICAgICAgICBkYXRhLl9wcm9jZXNzUXVldWUgPSB0aGlzLl9wcm9jZXNzUXVldWUgPVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAodGhpcy5fcHJvY2Vzc1F1ZXVlIHx8IGdldFByb21pc2UoW3RoaXNdKSkucGlwZShcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoZGF0YS5lcnJvclRocm93bikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gJC5EZWZlcnJlZCgpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAucmVqZWN0V2l0aCh0aGF0LCBbZGF0YV0pLnByb21pc2UoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGdldFByb21pc2UoYXJndW1lbnRzKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgKS5waXBlKHJlc29sdmVGdW5jLCByZWplY3RGdW5jKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLl9wcm9jZXNzUXVldWUgfHwgZ2V0UHJvbWlzZShbdGhpc10pO1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICBkYXRhLnN1Ym1pdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLnN0YXRlKCkgIT09ICdwZW5kaW5nJykge1xyXG4gICAgICAgICAgICAgICAgICAgIGRhdGEuanFYSFIgPSB0aGlzLmpxWEhSID1cclxuICAgICAgICAgICAgICAgICAgICAgICAgKHRoYXQuX3RyaWdnZXIoXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAnc3VibWl0JyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICQuRXZlbnQoJ3N1Ym1pdCcsIHtkZWxlZ2F0ZWRFdmVudDogZX0pLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpc1xyXG4gICAgICAgICAgICAgICAgICAgICAgICApICE9PSBmYWxzZSkgJiYgdGhhdC5fb25TZW5kKGUsIHRoaXMpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMuanFYSFIgfHwgdGhhdC5fZ2V0WEhSUHJvbWlzZSgpO1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICBkYXRhLmFib3J0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuanFYSFIpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5qcVhIUi5hYm9ydCgpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgdGhpcy5lcnJvclRocm93biA9ICdhYm9ydCc7XHJcbiAgICAgICAgICAgICAgICB0aGF0Ll90cmlnZ2VyKCdmYWlsJywgbnVsbCwgdGhpcyk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhhdC5fZ2V0WEhSUHJvbWlzZShmYWxzZSk7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGRhdGEuc3RhdGUgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5qcVhIUikge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiB0aGF0Ll9nZXREZWZlcnJlZFN0YXRlKHRoaXMuanFYSFIpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgaWYgKHRoaXMuX3Byb2Nlc3NRdWV1ZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiB0aGF0Ll9nZXREZWZlcnJlZFN0YXRlKHRoaXMuX3Byb2Nlc3NRdWV1ZSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGRhdGEucHJvY2Vzc2luZyA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiAhdGhpcy5qcVhIUiAmJiB0aGlzLl9wcm9jZXNzUXVldWUgJiYgdGhhdFxyXG4gICAgICAgICAgICAgICAgICAgIC5fZ2V0RGVmZXJyZWRTdGF0ZSh0aGlzLl9wcm9jZXNzUXVldWUpID09PSAncGVuZGluZyc7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGRhdGEucHJvZ3Jlc3MgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5fcHJvZ3Jlc3M7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGRhdGEucmVzcG9uc2UgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5fcmVzcG9uc2U7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgLy8gUGFyc2VzIHRoZSBSYW5nZSBoZWFkZXIgZnJvbSB0aGUgc2VydmVyIHJlc3BvbnNlXHJcbiAgICAgICAgLy8gYW5kIHJldHVybnMgdGhlIHVwbG9hZGVkIGJ5dGVzOlxyXG4gICAgICAgIF9nZXRVcGxvYWRlZEJ5dGVzOiBmdW5jdGlvbiAoanFYSFIpIHtcclxuICAgICAgICAgICAgdmFyIHJhbmdlID0ganFYSFIuZ2V0UmVzcG9uc2VIZWFkZXIoJ1JhbmdlJyksXHJcbiAgICAgICAgICAgICAgICBwYXJ0cyA9IHJhbmdlICYmIHJhbmdlLnNwbGl0KCctJyksXHJcbiAgICAgICAgICAgICAgICB1cHBlckJ5dGVzUG9zID0gcGFydHMgJiYgcGFydHMubGVuZ3RoID4gMSAmJlxyXG4gICAgICAgICAgICAgICAgICAgIHBhcnNlSW50KHBhcnRzWzFdLCAxMCk7XHJcbiAgICAgICAgICAgIHJldHVybiB1cHBlckJ5dGVzUG9zICYmIHVwcGVyQnl0ZXNQb3MgKyAxO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIC8vIFVwbG9hZHMgYSBmaWxlIGluIG11bHRpcGxlLCBzZXF1ZW50aWFsIHJlcXVlc3RzXHJcbiAgICAgICAgLy8gYnkgc3BsaXR0aW5nIHRoZSBmaWxlIHVwIGluIG11bHRpcGxlIGJsb2IgY2h1bmtzLlxyXG4gICAgICAgIC8vIElmIHRoZSBzZWNvbmQgcGFyYW1ldGVyIGlzIHRydWUsIG9ubHkgdGVzdHMgaWYgdGhlIGZpbGVcclxuICAgICAgICAvLyBzaG91bGQgYmUgdXBsb2FkZWQgaW4gY2h1bmtzLCBidXQgZG9lcyBub3QgaW52b2tlIGFueVxyXG4gICAgICAgIC8vIHVwbG9hZCByZXF1ZXN0czpcclxuICAgICAgICBfY2h1bmtlZFVwbG9hZDogZnVuY3Rpb24gKG9wdGlvbnMsIHRlc3RPbmx5KSB7XHJcbiAgICAgICAgICAgIG9wdGlvbnMudXBsb2FkZWRCeXRlcyA9IG9wdGlvbnMudXBsb2FkZWRCeXRlcyB8fCAwO1xyXG4gICAgICAgICAgICB2YXIgdGhhdCA9IHRoaXMsXHJcbiAgICAgICAgICAgICAgICBmaWxlID0gb3B0aW9ucy5maWxlc1swXSxcclxuICAgICAgICAgICAgICAgIGZzID0gZmlsZS5zaXplLFxyXG4gICAgICAgICAgICAgICAgdWIgPSBvcHRpb25zLnVwbG9hZGVkQnl0ZXMsXHJcbiAgICAgICAgICAgICAgICBtY3MgPSBvcHRpb25zLm1heENodW5rU2l6ZSB8fCBmcyxcclxuICAgICAgICAgICAgICAgIHNsaWNlID0gdGhpcy5fYmxvYlNsaWNlLFxyXG4gICAgICAgICAgICAgICAgZGZkID0gJC5EZWZlcnJlZCgpLFxyXG4gICAgICAgICAgICAgICAgcHJvbWlzZSA9IGRmZC5wcm9taXNlKCksXHJcbiAgICAgICAgICAgICAgICBqcVhIUixcclxuICAgICAgICAgICAgICAgIHVwbG9hZDtcclxuICAgICAgICAgICAgaWYgKCEodGhpcy5faXNYSFJVcGxvYWQob3B0aW9ucykgJiYgc2xpY2UgJiYgKHViIHx8IG1jcyA8IGZzKSkgfHxcclxuICAgICAgICAgICAgICAgICAgICBvcHRpb25zLmRhdGEpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBpZiAodGVzdE9ubHkpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmICh1YiA+PSBmcykge1xyXG4gICAgICAgICAgICAgICAgZmlsZS5lcnJvciA9IG9wdGlvbnMuaTE4bigndXBsb2FkZWRCeXRlcycpO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2dldFhIUlByb21pc2UoXHJcbiAgICAgICAgICAgICAgICAgICAgZmFsc2UsXHJcbiAgICAgICAgICAgICAgICAgICAgb3B0aW9ucy5jb250ZXh0LFxyXG4gICAgICAgICAgICAgICAgICAgIFtudWxsLCAnZXJyb3InLCBmaWxlLmVycm9yXVxyXG4gICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAvLyBUaGUgY2h1bmsgdXBsb2FkIG1ldGhvZDpcclxuICAgICAgICAgICAgdXBsb2FkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgLy8gQ2xvbmUgdGhlIG9wdGlvbnMgb2JqZWN0IGZvciBlYWNoIGNodW5rIHVwbG9hZDpcclxuICAgICAgICAgICAgICAgIHZhciBvID0gJC5leHRlbmQoe30sIG9wdGlvbnMpLFxyXG4gICAgICAgICAgICAgICAgICAgIGN1cnJlbnRMb2FkZWQgPSBvLl9wcm9ncmVzcy5sb2FkZWQ7XHJcbiAgICAgICAgICAgICAgICBvLmJsb2IgPSBzbGljZS5jYWxsKFxyXG4gICAgICAgICAgICAgICAgICAgIGZpbGUsXHJcbiAgICAgICAgICAgICAgICAgICAgdWIsXHJcbiAgICAgICAgICAgICAgICAgICAgdWIgKyBtY3MsXHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZS50eXBlXHJcbiAgICAgICAgICAgICAgICApO1xyXG4gICAgICAgICAgICAgICAgLy8gU3RvcmUgdGhlIGN1cnJlbnQgY2h1bmsgc2l6ZSwgYXMgdGhlIGJsb2IgaXRzZWxmXHJcbiAgICAgICAgICAgICAgICAvLyB3aWxsIGJlIGRlcmVmZXJlbmNlZCBhZnRlciBkYXRhIHByb2Nlc3Npbmc6XHJcbiAgICAgICAgICAgICAgICBvLmNodW5rU2l6ZSA9IG8uYmxvYi5zaXplO1xyXG4gICAgICAgICAgICAgICAgLy8gRXhwb3NlIHRoZSBjaHVuayBieXRlcyBwb3NpdGlvbiByYW5nZTpcclxuICAgICAgICAgICAgICAgIG8uY29udGVudFJhbmdlID0gJ2J5dGVzICcgKyB1YiArICctJyArXHJcbiAgICAgICAgICAgICAgICAgICAgKHViICsgby5jaHVua1NpemUgLSAxKSArICcvJyArIGZzO1xyXG4gICAgICAgICAgICAgICAgLy8gUHJvY2VzcyB0aGUgdXBsb2FkIGRhdGEgKHRoZSBibG9iIGFuZCBwb3RlbnRpYWwgZm9ybSBkYXRhKTpcclxuICAgICAgICAgICAgICAgIHRoYXQuX2luaXRYSFJEYXRhKG8pO1xyXG4gICAgICAgICAgICAgICAgLy8gQWRkIHByb2dyZXNzIGxpc3RlbmVycyBmb3IgdGhpcyBjaHVuayB1cGxvYWQ6XHJcbiAgICAgICAgICAgICAgICB0aGF0Ll9pbml0UHJvZ3Jlc3NMaXN0ZW5lcihvKTtcclxuICAgICAgICAgICAgICAgIGpxWEhSID0gKCh0aGF0Ll90cmlnZ2VyKCdjaHVua3NlbmQnLCBudWxsLCBvKSAhPT0gZmFsc2UgJiYgJC5hamF4KG8pKSB8fFxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGF0Ll9nZXRYSFJQcm9taXNlKGZhbHNlLCBvLmNvbnRleHQpKVxyXG4gICAgICAgICAgICAgICAgICAgIC5kb25lKGZ1bmN0aW9uIChyZXN1bHQsIHRleHRTdGF0dXMsIGpxWEhSKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHViID0gdGhhdC5fZ2V0VXBsb2FkZWRCeXRlcyhqcVhIUikgfHxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICh1YiArIG8uY2h1bmtTaXplKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgLy8gQ3JlYXRlIGEgcHJvZ3Jlc3MgZXZlbnQgaWYgbm8gZmluYWwgcHJvZ3Jlc3MgZXZlbnRcclxuICAgICAgICAgICAgICAgICAgICAgICAgLy8gd2l0aCBsb2FkZWQgZXF1YWxpbmcgdG90YWwgaGFzIGJlZW4gdHJpZ2dlcmVkXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC8vIGZvciB0aGlzIGNodW5rOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoY3VycmVudExvYWRlZCArIG8uY2h1bmtTaXplIC0gby5fcHJvZ3Jlc3MubG9hZGVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGF0Ll9vblByb2dyZXNzKCQuRXZlbnQoJ3Byb2dyZXNzJywge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxlbmd0aENvbXB1dGFibGU6IHRydWUsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbG9hZGVkOiB1YiAtIG8udXBsb2FkZWRCeXRlcyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b3RhbDogdWIgLSBvLnVwbG9hZGVkQnl0ZXNcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pLCBvKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBvcHRpb25zLnVwbG9hZGVkQnl0ZXMgPSBvLnVwbG9hZGVkQnl0ZXMgPSB1YjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgby5yZXN1bHQgPSByZXN1bHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG8udGV4dFN0YXR1cyA9IHRleHRTdGF0dXM7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG8uanFYSFIgPSBqcVhIUjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhhdC5fdHJpZ2dlcignY2h1bmtkb25lJywgbnVsbCwgbyk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQuX3RyaWdnZXIoJ2NodW5rYWx3YXlzJywgbnVsbCwgbyk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICh1YiA8IGZzKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBGaWxlIHVwbG9hZCBub3QgeWV0IGNvbXBsZXRlLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gY29udGludWUgd2l0aCB0aGUgbmV4dCBjaHVuazpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHVwbG9hZCgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZGZkLnJlc29sdmVXaXRoKFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG8uY29udGV4dCxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBbcmVzdWx0LCB0ZXh0U3RhdHVzLCBqcVhIUl1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgICAgIC5mYWlsKGZ1bmN0aW9uIChqcVhIUiwgdGV4dFN0YXR1cywgZXJyb3JUaHJvd24pIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgby5qcVhIUiA9IGpxWEhSO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBvLnRleHRTdGF0dXMgPSB0ZXh0U3RhdHVzO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBvLmVycm9yVGhyb3duID0gZXJyb3JUaHJvd247XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQuX3RyaWdnZXIoJ2NodW5rZmFpbCcsIG51bGwsIG8pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGF0Ll90cmlnZ2VyKCdjaHVua2Fsd2F5cycsIG51bGwsIG8pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkZmQucmVqZWN0V2l0aChcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG8uY29udGV4dCxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIFtqcVhIUiwgdGV4dFN0YXR1cywgZXJyb3JUaHJvd25dXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIHRoaXMuX2VuaGFuY2VQcm9taXNlKHByb21pc2UpO1xyXG4gICAgICAgICAgICBwcm9taXNlLmFib3J0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGpxWEhSLmFib3J0KCk7XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIHVwbG9hZCgpO1xyXG4gICAgICAgICAgICByZXR1cm4gcHJvbWlzZTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfYmVmb3JlU2VuZDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMuX2FjdGl2ZSA9PT0gMCkge1xyXG4gICAgICAgICAgICAgICAgLy8gdGhlIHN0YXJ0IGNhbGxiYWNrIGlzIHRyaWdnZXJlZCB3aGVuIGFuIHVwbG9hZCBzdGFydHNcclxuICAgICAgICAgICAgICAgIC8vIGFuZCBubyBvdGhlciB1cGxvYWRzIGFyZSBjdXJyZW50bHkgcnVubmluZyxcclxuICAgICAgICAgICAgICAgIC8vIGVxdWl2YWxlbnQgdG8gdGhlIGdsb2JhbCBhamF4U3RhcnQgZXZlbnQ6XHJcbiAgICAgICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdzdGFydCcpO1xyXG4gICAgICAgICAgICAgICAgLy8gU2V0IHRpbWVyIGZvciBnbG9iYWwgYml0cmF0ZSBwcm9ncmVzcyBjYWxjdWxhdGlvbjpcclxuICAgICAgICAgICAgICAgIHRoaXMuX2JpdHJhdGVUaW1lciA9IG5ldyB0aGlzLl9CaXRyYXRlVGltZXIoKTtcclxuICAgICAgICAgICAgICAgIC8vIFJlc2V0IHRoZSBnbG9iYWwgcHJvZ3Jlc3MgdmFsdWVzOlxyXG4gICAgICAgICAgICAgICAgdGhpcy5fcHJvZ3Jlc3MubG9hZGVkID0gdGhpcy5fcHJvZ3Jlc3MudG90YWwgPSAwO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fcHJvZ3Jlc3MuYml0cmF0ZSA9IDA7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgLy8gTWFrZSBzdXJlIHRoZSBjb250YWluZXIgb2JqZWN0cyBmb3IgdGhlIC5yZXNwb25zZSgpIGFuZFxyXG4gICAgICAgICAgICAvLyAucHJvZ3Jlc3MoKSBtZXRob2RzIG9uIHRoZSBkYXRhIG9iamVjdCBhcmUgYXZhaWxhYmxlXHJcbiAgICAgICAgICAgIC8vIGFuZCByZXNldCB0byB0aGVpciBpbml0aWFsIHN0YXRlOlxyXG4gICAgICAgICAgICB0aGlzLl9pbml0UmVzcG9uc2VPYmplY3QoZGF0YSk7XHJcbiAgICAgICAgICAgIHRoaXMuX2luaXRQcm9ncmVzc09iamVjdChkYXRhKTtcclxuICAgICAgICAgICAgZGF0YS5fcHJvZ3Jlc3MubG9hZGVkID0gZGF0YS5sb2FkZWQgPSBkYXRhLnVwbG9hZGVkQnl0ZXMgfHwgMDtcclxuICAgICAgICAgICAgZGF0YS5fcHJvZ3Jlc3MudG90YWwgPSBkYXRhLnRvdGFsID0gdGhpcy5fZ2V0VG90YWwoZGF0YS5maWxlcykgfHwgMTtcclxuICAgICAgICAgICAgZGF0YS5fcHJvZ3Jlc3MuYml0cmF0ZSA9IGRhdGEuYml0cmF0ZSA9IDA7XHJcbiAgICAgICAgICAgIHRoaXMuX2FjdGl2ZSArPSAxO1xyXG4gICAgICAgICAgICAvLyBJbml0aWFsaXplIHRoZSBnbG9iYWwgcHJvZ3Jlc3MgdmFsdWVzOlxyXG4gICAgICAgICAgICB0aGlzLl9wcm9ncmVzcy5sb2FkZWQgKz0gZGF0YS5sb2FkZWQ7XHJcbiAgICAgICAgICAgIHRoaXMuX3Byb2dyZXNzLnRvdGFsICs9IGRhdGEudG90YWw7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX29uRG9uZTogZnVuY3Rpb24gKHJlc3VsdCwgdGV4dFN0YXR1cywganFYSFIsIG9wdGlvbnMpIHtcclxuICAgICAgICAgICAgdmFyIHRvdGFsID0gb3B0aW9ucy5fcHJvZ3Jlc3MudG90YWwsXHJcbiAgICAgICAgICAgICAgICByZXNwb25zZSA9IG9wdGlvbnMuX3Jlc3BvbnNlO1xyXG4gICAgICAgICAgICBpZiAob3B0aW9ucy5fcHJvZ3Jlc3MubG9hZGVkIDwgdG90YWwpIHtcclxuICAgICAgICAgICAgICAgIC8vIENyZWF0ZSBhIHByb2dyZXNzIGV2ZW50IGlmIG5vIGZpbmFsIHByb2dyZXNzIGV2ZW50XHJcbiAgICAgICAgICAgICAgICAvLyB3aXRoIGxvYWRlZCBlcXVhbGluZyB0b3RhbCBoYXMgYmVlbiB0cmlnZ2VyZWQ6XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9vblByb2dyZXNzKCQuRXZlbnQoJ3Byb2dyZXNzJywge1xyXG4gICAgICAgICAgICAgICAgICAgIGxlbmd0aENvbXB1dGFibGU6IHRydWUsXHJcbiAgICAgICAgICAgICAgICAgICAgbG9hZGVkOiB0b3RhbCxcclxuICAgICAgICAgICAgICAgICAgICB0b3RhbDogdG90YWxcclxuICAgICAgICAgICAgICAgIH0pLCBvcHRpb25zKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXNwb25zZS5yZXN1bHQgPSBvcHRpb25zLnJlc3VsdCA9IHJlc3VsdDtcclxuICAgICAgICAgICAgcmVzcG9uc2UudGV4dFN0YXR1cyA9IG9wdGlvbnMudGV4dFN0YXR1cyA9IHRleHRTdGF0dXM7XHJcbiAgICAgICAgICAgIHJlc3BvbnNlLmpxWEhSID0gb3B0aW9ucy5qcVhIUiA9IGpxWEhSO1xyXG4gICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdkb25lJywgbnVsbCwgb3B0aW9ucyk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX29uRmFpbDogZnVuY3Rpb24gKGpxWEhSLCB0ZXh0U3RhdHVzLCBlcnJvclRocm93biwgb3B0aW9ucykge1xyXG4gICAgICAgICAgICB2YXIgcmVzcG9uc2UgPSBvcHRpb25zLl9yZXNwb25zZTtcclxuICAgICAgICAgICAgaWYgKG9wdGlvbnMucmVjYWxjdWxhdGVQcm9ncmVzcykge1xyXG4gICAgICAgICAgICAgICAgLy8gUmVtb3ZlIHRoZSBmYWlsZWQgKGVycm9yIG9yIGFib3J0KSBmaWxlIHVwbG9hZCBmcm9tXHJcbiAgICAgICAgICAgICAgICAvLyB0aGUgZ2xvYmFsIHByb2dyZXNzIGNhbGN1bGF0aW9uOlxyXG4gICAgICAgICAgICAgICAgdGhpcy5fcHJvZ3Jlc3MubG9hZGVkIC09IG9wdGlvbnMuX3Byb2dyZXNzLmxvYWRlZDtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3Byb2dyZXNzLnRvdGFsIC09IG9wdGlvbnMuX3Byb2dyZXNzLnRvdGFsO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJlc3BvbnNlLmpxWEhSID0gb3B0aW9ucy5qcVhIUiA9IGpxWEhSO1xyXG4gICAgICAgICAgICByZXNwb25zZS50ZXh0U3RhdHVzID0gb3B0aW9ucy50ZXh0U3RhdHVzID0gdGV4dFN0YXR1cztcclxuICAgICAgICAgICAgcmVzcG9uc2UuZXJyb3JUaHJvd24gPSBvcHRpb25zLmVycm9yVGhyb3duID0gZXJyb3JUaHJvd247XHJcbiAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ2ZhaWwnLCBudWxsLCBvcHRpb25zKTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfb25BbHdheXM6IGZ1bmN0aW9uIChqcVhIUm9yUmVzdWx0LCB0ZXh0U3RhdHVzLCBqcVhIUm9yRXJyb3IsIG9wdGlvbnMpIHtcclxuICAgICAgICAgICAgLy8ganFYSFJvclJlc3VsdCwgdGV4dFN0YXR1cyBhbmQganFYSFJvckVycm9yIGFyZSBhZGRlZCB0byB0aGVcclxuICAgICAgICAgICAgLy8gb3B0aW9ucyBvYmplY3QgdmlhIGRvbmUgYW5kIGZhaWwgY2FsbGJhY2tzXHJcbiAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ2Fsd2F5cycsIG51bGwsIG9wdGlvbnMpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9vblNlbmQ6IGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcbiAgICAgICAgICAgIGlmICghZGF0YS5zdWJtaXQpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX2FkZENvbnZlbmllbmNlTWV0aG9kcyhlLCBkYXRhKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB2YXIgdGhhdCA9IHRoaXMsXHJcbiAgICAgICAgICAgICAgICBqcVhIUixcclxuICAgICAgICAgICAgICAgIGFib3J0ZWQsXHJcbiAgICAgICAgICAgICAgICBzbG90LFxyXG4gICAgICAgICAgICAgICAgcGlwZSxcclxuICAgICAgICAgICAgICAgIG9wdGlvbnMgPSB0aGF0Ll9nZXRBSkFYU2V0dGluZ3MoZGF0YSksXHJcbiAgICAgICAgICAgICAgICBzZW5kID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoYXQuX3NlbmRpbmcgKz0gMTtcclxuICAgICAgICAgICAgICAgICAgICAvLyBTZXQgdGltZXIgZm9yIGJpdHJhdGUgcHJvZ3Jlc3MgY2FsY3VsYXRpb246XHJcbiAgICAgICAgICAgICAgICAgICAgb3B0aW9ucy5fYml0cmF0ZVRpbWVyID0gbmV3IHRoYXQuX0JpdHJhdGVUaW1lcigpO1xyXG4gICAgICAgICAgICAgICAgICAgIGpxWEhSID0ganFYSFIgfHwgKFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAoKGFib3J0ZWQgfHwgdGhhdC5fdHJpZ2dlcihcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICdzZW5kJyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICQuRXZlbnQoJ3NlbmQnLCB7ZGVsZWdhdGVkRXZlbnQ6IGV9KSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG9wdGlvbnNcclxuICAgICAgICAgICAgICAgICAgICAgICAgKSA9PT0gZmFsc2UpICYmXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQuX2dldFhIUlByb21pc2UoZmFsc2UsIG9wdGlvbnMuY29udGV4dCwgYWJvcnRlZCkpIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQuX2NodW5rZWRVcGxvYWQob3B0aW9ucykgfHwgJC5hamF4KG9wdGlvbnMpXHJcbiAgICAgICAgICAgICAgICAgICAgKS5kb25lKGZ1bmN0aW9uIChyZXN1bHQsIHRleHRTdGF0dXMsIGpxWEhSKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoYXQuX29uRG9uZShyZXN1bHQsIHRleHRTdGF0dXMsIGpxWEhSLCBvcHRpb25zKTtcclxuICAgICAgICAgICAgICAgICAgICB9KS5mYWlsKGZ1bmN0aW9uIChqcVhIUiwgdGV4dFN0YXR1cywgZXJyb3JUaHJvd24pIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhhdC5fb25GYWlsKGpxWEhSLCB0ZXh0U3RhdHVzLCBlcnJvclRocm93biwgb3B0aW9ucyk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSkuYWx3YXlzKGZ1bmN0aW9uIChqcVhIUm9yUmVzdWx0LCB0ZXh0U3RhdHVzLCBqcVhIUm9yRXJyb3IpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhhdC5fb25BbHdheXMoXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBqcVhIUm9yUmVzdWx0LFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGV4dFN0YXR1cyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGpxWEhSb3JFcnJvcixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG9wdGlvbnNcclxuICAgICAgICAgICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhhdC5fc2VuZGluZyAtPSAxO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGF0Ll9hY3RpdmUgLT0gMTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKG9wdGlvbnMubGltaXRDb25jdXJyZW50VXBsb2FkcyAmJlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG9wdGlvbnMubGltaXRDb25jdXJyZW50VXBsb2FkcyA+IHRoYXQuX3NlbmRpbmcpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIFN0YXJ0IHRoZSBuZXh0IHF1ZXVlZCB1cGxvYWQsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyB0aGF0IGhhcyBub3QgYmVlbiBhYm9ydGVkOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIG5leHRTbG90ID0gdGhhdC5fc2xvdHMuc2hpZnQoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdoaWxlIChuZXh0U2xvdCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICh0aGF0Ll9nZXREZWZlcnJlZFN0YXRlKG5leHRTbG90KSA9PT0gJ3BlbmRpbmcnKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5leHRTbG90LnJlc29sdmUoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5leHRTbG90ID0gdGhhdC5fc2xvdHMuc2hpZnQoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAodGhhdC5fYWN0aXZlID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBUaGUgc3RvcCBjYWxsYmFjayBpcyB0cmlnZ2VyZWQgd2hlbiBhbGwgdXBsb2FkcyBoYXZlXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBiZWVuIGNvbXBsZXRlZCwgZXF1aXZhbGVudCB0byB0aGUgZ2xvYmFsIGFqYXhTdG9wIGV2ZW50OlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhhdC5fdHJpZ2dlcignc3RvcCcpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGpxWEhSO1xyXG4gICAgICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgdGhpcy5fYmVmb3JlU2VuZChlLCBvcHRpb25zKTtcclxuICAgICAgICAgICAgaWYgKHRoaXMub3B0aW9ucy5zZXF1ZW50aWFsVXBsb2FkcyB8fFxyXG4gICAgICAgICAgICAgICAgICAgICh0aGlzLm9wdGlvbnMubGltaXRDb25jdXJyZW50VXBsb2FkcyAmJlxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMub3B0aW9ucy5saW1pdENvbmN1cnJlbnRVcGxvYWRzIDw9IHRoaXMuX3NlbmRpbmcpKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5vcHRpb25zLmxpbWl0Q29uY3VycmVudFVwbG9hZHMgPiAxKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgc2xvdCA9ICQuRGVmZXJyZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLl9zbG90cy5wdXNoKHNsb3QpO1xyXG4gICAgICAgICAgICAgICAgICAgIHBpcGUgPSBzbG90LnBpcGUoc2VuZCk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuX3NlcXVlbmNlID0gdGhpcy5fc2VxdWVuY2UucGlwZShzZW5kLCBzZW5kKTtcclxuICAgICAgICAgICAgICAgICAgICBwaXBlID0gdGhpcy5fc2VxdWVuY2U7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAvLyBSZXR1cm4gdGhlIHBpcGVkIFByb21pc2Ugb2JqZWN0LCBlbmhhbmNlZCB3aXRoIGFuIGFib3J0IG1ldGhvZCxcclxuICAgICAgICAgICAgICAgIC8vIHdoaWNoIGlzIGRlbGVnYXRlZCB0byB0aGUganFYSFIgb2JqZWN0IG9mIHRoZSBjdXJyZW50IHVwbG9hZCxcclxuICAgICAgICAgICAgICAgIC8vIGFuZCBqcVhIUiBjYWxsYmFja3MgbWFwcGVkIHRvIHRoZSBlcXVpdmFsZW50IFByb21pc2UgbWV0aG9kczpcclxuICAgICAgICAgICAgICAgIHBpcGUuYWJvcnQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgYWJvcnRlZCA9IFt1bmRlZmluZWQsICdhYm9ydCcsICdhYm9ydCddO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghanFYSFIpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHNsb3QpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNsb3QucmVqZWN0V2l0aChvcHRpb25zLmNvbnRleHQsIGFib3J0ZWQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBzZW5kKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBqcVhIUi5hYm9ydCgpO1xyXG4gICAgICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLl9lbmhhbmNlUHJvbWlzZShwaXBlKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXR1cm4gc2VuZCgpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9vbkFkZDogZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuICAgICAgICAgICAgdmFyIHRoYXQgPSB0aGlzLFxyXG4gICAgICAgICAgICAgICAgcmVzdWx0ID0gdHJ1ZSxcclxuICAgICAgICAgICAgICAgIG9wdGlvbnMgPSAkLmV4dGVuZCh7fSwgdGhpcy5vcHRpb25zLCBkYXRhKSxcclxuICAgICAgICAgICAgICAgIGZpbGVzID0gZGF0YS5maWxlcyxcclxuICAgICAgICAgICAgICAgIGZpbGVzTGVuZ3RoID0gZmlsZXMubGVuZ3RoLFxyXG4gICAgICAgICAgICAgICAgbGltaXQgPSBvcHRpb25zLmxpbWl0TXVsdGlGaWxlVXBsb2FkcyxcclxuICAgICAgICAgICAgICAgIGxpbWl0U2l6ZSA9IG9wdGlvbnMubGltaXRNdWx0aUZpbGVVcGxvYWRTaXplLFxyXG4gICAgICAgICAgICAgICAgb3ZlcmhlYWQgPSBvcHRpb25zLmxpbWl0TXVsdGlGaWxlVXBsb2FkU2l6ZU92ZXJoZWFkLFxyXG4gICAgICAgICAgICAgICAgYmF0Y2hTaXplID0gMCxcclxuICAgICAgICAgICAgICAgIHBhcmFtTmFtZSA9IHRoaXMuX2dldFBhcmFtTmFtZShvcHRpb25zKSxcclxuICAgICAgICAgICAgICAgIHBhcmFtTmFtZVNldCxcclxuICAgICAgICAgICAgICAgIHBhcmFtTmFtZVNsaWNlLFxyXG4gICAgICAgICAgICAgICAgZmlsZVNldCxcclxuICAgICAgICAgICAgICAgIGksXHJcbiAgICAgICAgICAgICAgICBqID0gMDtcclxuICAgICAgICAgICAgaWYgKGxpbWl0U2l6ZSAmJiAoIWZpbGVzTGVuZ3RoIHx8IGZpbGVzWzBdLnNpemUgPT09IHVuZGVmaW5lZCkpIHtcclxuICAgICAgICAgICAgICAgIGxpbWl0U2l6ZSA9IHVuZGVmaW5lZDtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBpZiAoIShvcHRpb25zLnNpbmdsZUZpbGVVcGxvYWRzIHx8IGxpbWl0IHx8IGxpbWl0U2l6ZSkgfHxcclxuICAgICAgICAgICAgICAgICAgICAhdGhpcy5faXNYSFJVcGxvYWQob3B0aW9ucykpIHtcclxuICAgICAgICAgICAgICAgIGZpbGVTZXQgPSBbZmlsZXNdO1xyXG4gICAgICAgICAgICAgICAgcGFyYW1OYW1lU2V0ID0gW3BhcmFtTmFtZV07XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAoIShvcHRpb25zLnNpbmdsZUZpbGVVcGxvYWRzIHx8IGxpbWl0U2l6ZSkgJiYgbGltaXQpIHtcclxuICAgICAgICAgICAgICAgIGZpbGVTZXQgPSBbXTtcclxuICAgICAgICAgICAgICAgIHBhcmFtTmFtZVNldCA9IFtdO1xyXG4gICAgICAgICAgICAgICAgZm9yIChpID0gMDsgaSA8IGZpbGVzTGVuZ3RoOyBpICs9IGxpbWl0KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZVNldC5wdXNoKGZpbGVzLnNsaWNlKGksIGkgKyBsaW1pdCkpO1xyXG4gICAgICAgICAgICAgICAgICAgIHBhcmFtTmFtZVNsaWNlID0gcGFyYW1OYW1lLnNsaWNlKGksIGkgKyBsaW1pdCk7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCFwYXJhbU5hbWVTbGljZS5sZW5ndGgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcGFyYW1OYW1lU2xpY2UgPSBwYXJhbU5hbWU7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIHBhcmFtTmFtZVNldC5wdXNoKHBhcmFtTmFtZVNsaWNlKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSBlbHNlIGlmICghb3B0aW9ucy5zaW5nbGVGaWxlVXBsb2FkcyAmJiBsaW1pdFNpemUpIHtcclxuICAgICAgICAgICAgICAgIGZpbGVTZXQgPSBbXTtcclxuICAgICAgICAgICAgICAgIHBhcmFtTmFtZVNldCA9IFtdO1xyXG4gICAgICAgICAgICAgICAgZm9yIChpID0gMDsgaSA8IGZpbGVzTGVuZ3RoOyBpID0gaSArIDEpIHtcclxuICAgICAgICAgICAgICAgICAgICBiYXRjaFNpemUgKz0gZmlsZXNbaV0uc2l6ZSArIG92ZXJoZWFkO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChpICsgMSA9PT0gZmlsZXNMZW5ndGggfHxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICgoYmF0Y2hTaXplICsgZmlsZXNbaSArIDFdLnNpemUgKyBvdmVyaGVhZCkgPiBsaW1pdFNpemUpIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAobGltaXQgJiYgaSArIDEgLSBqID49IGxpbWl0KSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBmaWxlU2V0LnB1c2goZmlsZXMuc2xpY2UoaiwgaSArIDEpKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcGFyYW1OYW1lU2xpY2UgPSBwYXJhbU5hbWUuc2xpY2UoaiwgaSArIDEpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoIXBhcmFtTmFtZVNsaWNlLmxlbmd0aCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcGFyYW1OYW1lU2xpY2UgPSBwYXJhbU5hbWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgcGFyYW1OYW1lU2V0LnB1c2gocGFyYW1OYW1lU2xpY2UpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBqID0gaSArIDE7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGJhdGNoU2l6ZSA9IDA7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgcGFyYW1OYW1lU2V0ID0gcGFyYW1OYW1lO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGRhdGEub3JpZ2luYWxGaWxlcyA9IGZpbGVzO1xyXG4gICAgICAgICAgICAkLmVhY2goZmlsZVNldCB8fCBmaWxlcywgZnVuY3Rpb24gKGluZGV4LCBlbGVtZW50KSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgbmV3RGF0YSA9ICQuZXh0ZW5kKHt9LCBkYXRhKTtcclxuICAgICAgICAgICAgICAgIG5ld0RhdGEuZmlsZXMgPSBmaWxlU2V0ID8gZWxlbWVudCA6IFtlbGVtZW50XTtcclxuICAgICAgICAgICAgICAgIG5ld0RhdGEucGFyYW1OYW1lID0gcGFyYW1OYW1lU2V0W2luZGV4XTtcclxuICAgICAgICAgICAgICAgIHRoYXQuX2luaXRSZXNwb25zZU9iamVjdChuZXdEYXRhKTtcclxuICAgICAgICAgICAgICAgIHRoYXQuX2luaXRQcm9ncmVzc09iamVjdChuZXdEYXRhKTtcclxuICAgICAgICAgICAgICAgIHRoYXQuX2FkZENvbnZlbmllbmNlTWV0aG9kcyhlLCBuZXdEYXRhKTtcclxuICAgICAgICAgICAgICAgIHJlc3VsdCA9IHRoYXQuX3RyaWdnZXIoXHJcbiAgICAgICAgICAgICAgICAgICAgJ2FkZCcsXHJcbiAgICAgICAgICAgICAgICAgICAgJC5FdmVudCgnYWRkJywge2RlbGVnYXRlZEV2ZW50OiBlfSksXHJcbiAgICAgICAgICAgICAgICAgICAgbmV3RGF0YVxyXG4gICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9yZXBsYWNlRmlsZUlucHV0OiBmdW5jdGlvbiAoaW5wdXQpIHtcclxuICAgICAgICAgICAgdmFyIGlucHV0Q2xvbmUgPSBpbnB1dC5jbG9uZSh0cnVlKTtcclxuICAgICAgICAgICAgJCgnPGZvcm0+PC9mb3JtPicpLmFwcGVuZChpbnB1dENsb25lKVswXS5yZXNldCgpO1xyXG4gICAgICAgICAgICAvLyBEZXRhY2hpbmcgYWxsb3dzIHRvIGluc2VydCB0aGUgZmlsZUlucHV0IG9uIGFub3RoZXIgZm9ybVxyXG4gICAgICAgICAgICAvLyB3aXRob3V0IGxvb3NpbmcgdGhlIGZpbGUgaW5wdXQgdmFsdWU6XHJcbiAgICAgICAgICAgIGlucHV0LmFmdGVyKGlucHV0Q2xvbmUpLmRldGFjaCgpO1xyXG4gICAgICAgICAgICAvLyBBdm9pZCBtZW1vcnkgbGVha3Mgd2l0aCB0aGUgZGV0YWNoZWQgZmlsZSBpbnB1dDpcclxuICAgICAgICAgICAgJC5jbGVhbkRhdGEoaW5wdXQudW5iaW5kKCdyZW1vdmUnKSk7XHJcbiAgICAgICAgICAgIC8vIFJlcGxhY2UgdGhlIG9yaWdpbmFsIGZpbGUgaW5wdXQgZWxlbWVudCBpbiB0aGUgZmlsZUlucHV0XHJcbiAgICAgICAgICAgIC8vIGVsZW1lbnRzIHNldCB3aXRoIHRoZSBjbG9uZSwgd2hpY2ggaGFzIGJlZW4gY29waWVkIGluY2x1ZGluZ1xyXG4gICAgICAgICAgICAvLyBldmVudCBoYW5kbGVyczpcclxuICAgICAgICAgICAgdGhpcy5vcHRpb25zLmZpbGVJbnB1dCA9IHRoaXMub3B0aW9ucy5maWxlSW5wdXQubWFwKGZ1bmN0aW9uIChpLCBlbCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGVsID09PSBpbnB1dFswXSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBpbnB1dENsb25lWzBdO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGVsO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgLy8gSWYgdGhlIHdpZGdldCBoYXMgYmVlbiBpbml0aWFsaXplZCBvbiB0aGUgZmlsZSBpbnB1dCBpdHNlbGYsXHJcbiAgICAgICAgICAgIC8vIG92ZXJyaWRlIHRoaXMuZWxlbWVudCB3aXRoIHRoZSBmaWxlIGlucHV0IGNsb25lOlxyXG4gICAgICAgICAgICBpZiAoaW5wdXRbMF0gPT09IHRoaXMuZWxlbWVudFswXSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5lbGVtZW50ID0gaW5wdXRDbG9uZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9oYW5kbGVGaWxlVHJlZUVudHJ5OiBmdW5jdGlvbiAoZW50cnksIHBhdGgpIHtcclxuICAgICAgICAgICAgdmFyIHRoYXQgPSB0aGlzLFxyXG4gICAgICAgICAgICAgICAgZGZkID0gJC5EZWZlcnJlZCgpLFxyXG4gICAgICAgICAgICAgICAgZXJyb3JIYW5kbGVyID0gZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoZSAmJiAhZS5lbnRyeSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBlLmVudHJ5ID0gZW50cnk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIC8vIFNpbmNlICQud2hlbiByZXR1cm5zIGltbWVkaWF0ZWx5IGlmIG9uZVxyXG4gICAgICAgICAgICAgICAgICAgIC8vIERlZmVycmVkIGlzIHJlamVjdGVkLCB3ZSB1c2UgcmVzb2x2ZSBpbnN0ZWFkLlxyXG4gICAgICAgICAgICAgICAgICAgIC8vIFRoaXMgYWxsb3dzIHZhbGlkIGZpbGVzIGFuZCBpbnZhbGlkIGl0ZW1zXHJcbiAgICAgICAgICAgICAgICAgICAgLy8gdG8gYmUgcmV0dXJuZWQgdG9nZXRoZXIgaW4gb25lIHNldDpcclxuICAgICAgICAgICAgICAgICAgICBkZmQucmVzb2x2ZShbZV0pO1xyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIGRpclJlYWRlcjtcclxuICAgICAgICAgICAgcGF0aCA9IHBhdGggfHwgJyc7XHJcbiAgICAgICAgICAgIGlmIChlbnRyeS5pc0ZpbGUpIHtcclxuICAgICAgICAgICAgICAgIGlmIChlbnRyeS5fZmlsZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIC8vIFdvcmthcm91bmQgZm9yIENocm9tZSBidWcgIzE0OTczNVxyXG4gICAgICAgICAgICAgICAgICAgIGVudHJ5Ll9maWxlLnJlbGF0aXZlUGF0aCA9IHBhdGg7XHJcbiAgICAgICAgICAgICAgICAgICAgZGZkLnJlc29sdmUoZW50cnkuX2ZpbGUpO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICBlbnRyeS5maWxlKGZ1bmN0aW9uIChmaWxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGZpbGUucmVsYXRpdmVQYXRoID0gcGF0aDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGZkLnJlc29sdmUoZmlsZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSwgZXJyb3JIYW5kbGVyKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSBlbHNlIGlmIChlbnRyeS5pc0RpcmVjdG9yeSkge1xyXG4gICAgICAgICAgICAgICAgZGlyUmVhZGVyID0gZW50cnkuY3JlYXRlUmVhZGVyKCk7XHJcbiAgICAgICAgICAgICAgICBkaXJSZWFkZXIucmVhZEVudHJpZXMoZnVuY3Rpb24gKGVudHJpZXMpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGF0Ll9oYW5kbGVGaWxlVHJlZUVudHJpZXMoXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGVudHJpZXMsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHBhdGggKyBlbnRyeS5uYW1lICsgJy8nXHJcbiAgICAgICAgICAgICAgICAgICAgKS5kb25lKGZ1bmN0aW9uIChmaWxlcykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkZmQucmVzb2x2ZShmaWxlcyk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSkuZmFpbChlcnJvckhhbmRsZXIpO1xyXG4gICAgICAgICAgICAgICAgfSwgZXJyb3JIYW5kbGVyKTtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIC8vIFJldHVybiBhbiBlbXB5IGxpc3QgZm9yIGZpbGUgc3lzdGVtIGl0ZW1zXHJcbiAgICAgICAgICAgICAgICAvLyBvdGhlciB0aGFuIGZpbGVzIG9yIGRpcmVjdG9yaWVzOlxyXG4gICAgICAgICAgICAgICAgZGZkLnJlc29sdmUoW10pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJldHVybiBkZmQucHJvbWlzZSgpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9oYW5kbGVGaWxlVHJlZUVudHJpZXM6IGZ1bmN0aW9uIChlbnRyaWVzLCBwYXRoKSB7XHJcbiAgICAgICAgICAgIHZhciB0aGF0ID0gdGhpcztcclxuICAgICAgICAgICAgcmV0dXJuICQud2hlbi5hcHBseShcclxuICAgICAgICAgICAgICAgICQsXHJcbiAgICAgICAgICAgICAgICAkLm1hcChlbnRyaWVzLCBmdW5jdGlvbiAoZW50cnkpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhhdC5faGFuZGxlRmlsZVRyZWVFbnRyeShlbnRyeSwgcGF0aCk7XHJcbiAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICApLnBpcGUoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIEFycmF5LnByb3RvdHlwZS5jb25jYXQuYXBwbHkoXHJcbiAgICAgICAgICAgICAgICAgICAgW10sXHJcbiAgICAgICAgICAgICAgICAgICAgYXJndW1lbnRzXHJcbiAgICAgICAgICAgICAgICApO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfZ2V0RHJvcHBlZEZpbGVzOiBmdW5jdGlvbiAoZGF0YVRyYW5zZmVyKSB7XHJcbiAgICAgICAgICAgIGRhdGFUcmFuc2ZlciA9IGRhdGFUcmFuc2ZlciB8fCB7fTtcclxuICAgICAgICAgICAgdmFyIGl0ZW1zID0gZGF0YVRyYW5zZmVyLml0ZW1zO1xyXG4gICAgICAgICAgICBpZiAoaXRlbXMgJiYgaXRlbXMubGVuZ3RoICYmIChpdGVtc1swXS53ZWJraXRHZXRBc0VudHJ5IHx8XHJcbiAgICAgICAgICAgICAgICAgICAgaXRlbXNbMF0uZ2V0QXNFbnRyeSkpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLl9oYW5kbGVGaWxlVHJlZUVudHJpZXMoXHJcbiAgICAgICAgICAgICAgICAgICAgJC5tYXAoaXRlbXMsIGZ1bmN0aW9uIChpdGVtKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBlbnRyeTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGl0ZW0ud2Via2l0R2V0QXNFbnRyeSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZW50cnkgPSBpdGVtLndlYmtpdEdldEFzRW50cnkoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChlbnRyeSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIFdvcmthcm91bmQgZm9yIENocm9tZSBidWcgIzE0OTczNTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbnRyeS5fZmlsZSA9IGl0ZW0uZ2V0QXNGaWxlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gZW50cnk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGl0ZW0uZ2V0QXNFbnRyeSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICApO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJldHVybiAkLkRlZmVycmVkKCkucmVzb2x2ZShcclxuICAgICAgICAgICAgICAgICQubWFrZUFycmF5KGRhdGFUcmFuc2Zlci5maWxlcylcclxuICAgICAgICAgICAgKS5wcm9taXNlKCk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX2dldFNpbmdsZUZpbGVJbnB1dEZpbGVzOiBmdW5jdGlvbiAoZmlsZUlucHV0KSB7XHJcbiAgICAgICAgICAgIGZpbGVJbnB1dCA9ICQoZmlsZUlucHV0KTtcclxuICAgICAgICAgICAgdmFyIGVudHJpZXMgPSBmaWxlSW5wdXQucHJvcCgnd2Via2l0RW50cmllcycpIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZUlucHV0LnByb3AoJ2VudHJpZXMnKSxcclxuICAgICAgICAgICAgICAgIGZpbGVzLFxyXG4gICAgICAgICAgICAgICAgdmFsdWU7XHJcbiAgICAgICAgICAgIGlmIChlbnRyaWVzICYmIGVudHJpZXMubGVuZ3RoKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5faGFuZGxlRmlsZVRyZWVFbnRyaWVzKGVudHJpZXMpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGZpbGVzID0gJC5tYWtlQXJyYXkoZmlsZUlucHV0LnByb3AoJ2ZpbGVzJykpO1xyXG4gICAgICAgICAgICBpZiAoIWZpbGVzLmxlbmd0aCkge1xyXG4gICAgICAgICAgICAgICAgdmFsdWUgPSBmaWxlSW5wdXQucHJvcCgndmFsdWUnKTtcclxuICAgICAgICAgICAgICAgIGlmICghdmFsdWUpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gJC5EZWZlcnJlZCgpLnJlc29sdmUoW10pLnByb21pc2UoKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIC8vIElmIHRoZSBmaWxlcyBwcm9wZXJ0eSBpcyBub3QgYXZhaWxhYmxlLCB0aGUgYnJvd3NlciBkb2VzIG5vdFxyXG4gICAgICAgICAgICAgICAgLy8gc3VwcG9ydCB0aGUgRmlsZSBBUEkgYW5kIHdlIGFkZCBhIHBzZXVkbyBGaWxlIG9iamVjdCB3aXRoXHJcbiAgICAgICAgICAgICAgICAvLyB0aGUgaW5wdXQgdmFsdWUgYXMgbmFtZSB3aXRoIHBhdGggaW5mb3JtYXRpb24gcmVtb3ZlZDpcclxuICAgICAgICAgICAgICAgIGZpbGVzID0gW3tuYW1lOiB2YWx1ZS5yZXBsYWNlKC9eLipcXFxcLywgJycpfV07XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAoZmlsZXNbMF0ubmFtZSA9PT0gdW5kZWZpbmVkICYmIGZpbGVzWzBdLmZpbGVOYW1lKSB7XHJcbiAgICAgICAgICAgICAgICAvLyBGaWxlIG5vcm1hbGl6YXRpb24gZm9yIFNhZmFyaSA0IGFuZCBGaXJlZm94IDM6XHJcbiAgICAgICAgICAgICAgICAkLmVhY2goZmlsZXMsIGZ1bmN0aW9uIChpbmRleCwgZmlsZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGZpbGUubmFtZSA9IGZpbGUuZmlsZU5hbWU7XHJcbiAgICAgICAgICAgICAgICAgICAgZmlsZS5zaXplID0gZmlsZS5maWxlU2l6ZTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJldHVybiAkLkRlZmVycmVkKCkucmVzb2x2ZShmaWxlcykucHJvbWlzZSgpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9nZXRGaWxlSW5wdXRGaWxlczogZnVuY3Rpb24gKGZpbGVJbnB1dCkge1xyXG4gICAgICAgICAgICBpZiAoIShmaWxlSW5wdXQgaW5zdGFuY2VvZiAkKSB8fCBmaWxlSW5wdXQubGVuZ3RoID09PSAxKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5fZ2V0U2luZ2xlRmlsZUlucHV0RmlsZXMoZmlsZUlucHV0KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICByZXR1cm4gJC53aGVuLmFwcGx5KFxyXG4gICAgICAgICAgICAgICAgJCxcclxuICAgICAgICAgICAgICAgICQubWFwKGZpbGVJbnB1dCwgdGhpcy5fZ2V0U2luZ2xlRmlsZUlucHV0RmlsZXMpXHJcbiAgICAgICAgICAgICkucGlwZShmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gQXJyYXkucHJvdG90eXBlLmNvbmNhdC5hcHBseShcclxuICAgICAgICAgICAgICAgICAgICBbXSxcclxuICAgICAgICAgICAgICAgICAgICBhcmd1bWVudHNcclxuICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9vbkNoYW5nZTogZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgdmFyIHRoYXQgPSB0aGlzLFxyXG4gICAgICAgICAgICAgICAgZGF0YSA9IHtcclxuICAgICAgICAgICAgICAgICAgICBmaWxlSW5wdXQ6ICQoZS50YXJnZXQpLFxyXG4gICAgICAgICAgICAgICAgICAgIGZvcm06ICQoZS50YXJnZXQuZm9ybSlcclxuICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIHRoaXMuX2dldEZpbGVJbnB1dEZpbGVzKGRhdGEuZmlsZUlucHV0KS5hbHdheXMoZnVuY3Rpb24gKGZpbGVzKSB7XHJcbiAgICAgICAgICAgICAgICBkYXRhLmZpbGVzID0gZmlsZXM7XHJcbiAgICAgICAgICAgICAgICBpZiAodGhhdC5vcHRpb25zLnJlcGxhY2VGaWxlSW5wdXQpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGF0Ll9yZXBsYWNlRmlsZUlucHV0KGRhdGEuZmlsZUlucHV0KTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGlmICh0aGF0Ll90cmlnZ2VyKFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAnY2hhbmdlJyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgJC5FdmVudCgnY2hhbmdlJywge2RlbGVnYXRlZEV2ZW50OiBlfSksXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRhdGFcclxuICAgICAgICAgICAgICAgICAgICApICE9PSBmYWxzZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoYXQuX29uQWRkKGUsIGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfb25QYXN0ZTogZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgdmFyIGl0ZW1zID0gZS5vcmlnaW5hbEV2ZW50ICYmIGUub3JpZ2luYWxFdmVudC5jbGlwYm9hcmREYXRhICYmXHJcbiAgICAgICAgICAgICAgICAgICAgZS5vcmlnaW5hbEV2ZW50LmNsaXBib2FyZERhdGEuaXRlbXMsXHJcbiAgICAgICAgICAgICAgICBkYXRhID0ge2ZpbGVzOiBbXX07XHJcbiAgICAgICAgICAgIGlmIChpdGVtcyAmJiBpdGVtcy5sZW5ndGgpIHtcclxuICAgICAgICAgICAgICAgICQuZWFjaChpdGVtcywgZnVuY3Rpb24gKGluZGV4LCBpdGVtKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGZpbGUgPSBpdGVtLmdldEFzRmlsZSAmJiBpdGVtLmdldEFzRmlsZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChmaWxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRhdGEuZmlsZXMucHVzaChmaWxlKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLl90cmlnZ2VyKFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAncGFzdGUnLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkLkV2ZW50KCdwYXN0ZScsIHtkZWxlZ2F0ZWRFdmVudDogZX0pLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBkYXRhXHJcbiAgICAgICAgICAgICAgICAgICAgKSAhPT0gZmFsc2UpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLl9vbkFkZChlLCBkYXRhKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9vbkRyb3A6IGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgIGUuZGF0YVRyYW5zZmVyID0gZS5vcmlnaW5hbEV2ZW50ICYmIGUub3JpZ2luYWxFdmVudC5kYXRhVHJhbnNmZXI7XHJcbiAgICAgICAgICAgIHZhciB0aGF0ID0gdGhpcyxcclxuICAgICAgICAgICAgICAgIGRhdGFUcmFuc2ZlciA9IGUuZGF0YVRyYW5zZmVyLFxyXG4gICAgICAgICAgICAgICAgZGF0YSA9IHt9O1xyXG4gICAgICAgICAgICBpZiAoZGF0YVRyYW5zZmVyICYmIGRhdGFUcmFuc2Zlci5maWxlcyAmJiBkYXRhVHJhbnNmZXIuZmlsZXMubGVuZ3RoKSB7XHJcbiAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9nZXREcm9wcGVkRmlsZXMoZGF0YVRyYW5zZmVyKS5hbHdheXMoZnVuY3Rpb24gKGZpbGVzKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZGF0YS5maWxlcyA9IGZpbGVzO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGF0Ll90cmlnZ2VyKFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJ2Ryb3AnLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJC5FdmVudCgnZHJvcCcsIHtkZWxlZ2F0ZWRFdmVudDogZX0pLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZGF0YVxyXG4gICAgICAgICAgICAgICAgICAgICAgICApICE9PSBmYWxzZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGF0Ll9vbkFkZChlLCBkYXRhKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9vbkRyYWdPdmVyOiBmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICBlLmRhdGFUcmFuc2ZlciA9IGUub3JpZ2luYWxFdmVudCAmJiBlLm9yaWdpbmFsRXZlbnQuZGF0YVRyYW5zZmVyO1xyXG4gICAgICAgICAgICB2YXIgZGF0YVRyYW5zZmVyID0gZS5kYXRhVHJhbnNmZXI7XHJcbiAgICAgICAgICAgIGlmIChkYXRhVHJhbnNmZXIgJiYgJC5pbkFycmF5KCdGaWxlcycsIGRhdGFUcmFuc2Zlci50eXBlcykgIT09IC0xICYmXHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcihcclxuICAgICAgICAgICAgICAgICAgICAgICAgJ2RyYWdvdmVyJyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgJC5FdmVudCgnZHJhZ292ZXInLCB7ZGVsZWdhdGVkRXZlbnQ6IGV9KVxyXG4gICAgICAgICAgICAgICAgICAgICkgIT09IGZhbHNlKSB7XHJcbiAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICBkYXRhVHJhbnNmZXIuZHJvcEVmZmVjdCA9ICdjb3B5JztcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9pbml0RXZlbnRIYW5kbGVyczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5faXNYSFJVcGxvYWQodGhpcy5vcHRpb25zKSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fb24odGhpcy5vcHRpb25zLmRyb3Bab25lLCB7XHJcbiAgICAgICAgICAgICAgICAgICAgZHJhZ292ZXI6IHRoaXMuX29uRHJhZ092ZXIsXHJcbiAgICAgICAgICAgICAgICAgICAgZHJvcDogdGhpcy5fb25Ecm9wXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIHRoaXMuX29uKHRoaXMub3B0aW9ucy5wYXN0ZVpvbmUsIHtcclxuICAgICAgICAgICAgICAgICAgICBwYXN0ZTogdGhpcy5fb25QYXN0ZVxyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgaWYgKCQuc3VwcG9ydC5maWxlSW5wdXQpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX29uKHRoaXMub3B0aW9ucy5maWxlSW5wdXQsIHtcclxuICAgICAgICAgICAgICAgICAgICBjaGFuZ2U6IHRoaXMuX29uQ2hhbmdlXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9kZXN0cm95RXZlbnRIYW5kbGVyczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB0aGlzLl9vZmYodGhpcy5vcHRpb25zLmRyb3Bab25lLCAnZHJhZ292ZXIgZHJvcCcpO1xyXG4gICAgICAgICAgICB0aGlzLl9vZmYodGhpcy5vcHRpb25zLnBhc3RlWm9uZSwgJ3Bhc3RlJyk7XHJcbiAgICAgICAgICAgIHRoaXMuX29mZih0aGlzLm9wdGlvbnMuZmlsZUlucHV0LCAnY2hhbmdlJyk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgX3NldE9wdGlvbjogZnVuY3Rpb24gKGtleSwgdmFsdWUpIHtcclxuICAgICAgICAgICAgdmFyIHJlaW5pdCA9ICQuaW5BcnJheShrZXksIHRoaXMuX3NwZWNpYWxPcHRpb25zKSAhPT0gLTE7XHJcbiAgICAgICAgICAgIGlmIChyZWluaXQpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX2Rlc3Ryb3lFdmVudEhhbmRsZXJzKCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgdGhpcy5fc3VwZXIoa2V5LCB2YWx1ZSk7XHJcbiAgICAgICAgICAgIGlmIChyZWluaXQpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX2luaXRTcGVjaWFsT3B0aW9ucygpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5faW5pdEV2ZW50SGFuZGxlcnMoKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9pbml0U3BlY2lhbE9wdGlvbnM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIG9wdGlvbnMgPSB0aGlzLm9wdGlvbnM7XHJcbiAgICAgICAgICAgIGlmIChvcHRpb25zLmZpbGVJbnB1dCA9PT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgICAgICBvcHRpb25zLmZpbGVJbnB1dCA9IHRoaXMuZWxlbWVudC5pcygnaW5wdXRbdHlwZT1cImZpbGVcIl0nKSA/XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuZWxlbWVudCA6IHRoaXMuZWxlbWVudC5maW5kKCdpbnB1dFt0eXBlPVwiZmlsZVwiXScpO1xyXG4gICAgICAgICAgICB9IGVsc2UgaWYgKCEob3B0aW9ucy5maWxlSW5wdXQgaW5zdGFuY2VvZiAkKSkge1xyXG4gICAgICAgICAgICAgICAgb3B0aW9ucy5maWxlSW5wdXQgPSAkKG9wdGlvbnMuZmlsZUlucHV0KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBpZiAoIShvcHRpb25zLmRyb3Bab25lIGluc3RhbmNlb2YgJCkpIHtcclxuICAgICAgICAgICAgICAgIG9wdGlvbnMuZHJvcFpvbmUgPSAkKG9wdGlvbnMuZHJvcFpvbmUpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmICghKG9wdGlvbnMucGFzdGVab25lIGluc3RhbmNlb2YgJCkpIHtcclxuICAgICAgICAgICAgICAgIG9wdGlvbnMucGFzdGVab25lID0gJChvcHRpb25zLnBhc3RlWm9uZSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfZ2V0UmVnRXhwOiBmdW5jdGlvbiAoc3RyKSB7XHJcbiAgICAgICAgICAgIHZhciBwYXJ0cyA9IHN0ci5zcGxpdCgnLycpLFxyXG4gICAgICAgICAgICAgICAgbW9kaWZpZXJzID0gcGFydHMucG9wKCk7XHJcbiAgICAgICAgICAgIHBhcnRzLnNoaWZ0KCk7XHJcbiAgICAgICAgICAgIHJldHVybiBuZXcgUmVnRXhwKHBhcnRzLmpvaW4oJy8nKSwgbW9kaWZpZXJzKTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBfaXNSZWdFeHBPcHRpb246IGZ1bmN0aW9uIChrZXksIHZhbHVlKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBrZXkgIT09ICd1cmwnICYmICQudHlwZSh2YWx1ZSkgPT09ICdzdHJpbmcnICYmXHJcbiAgICAgICAgICAgICAgICAvXlxcLy4qXFwvW2lnbV17MCwzfSQvLnRlc3QodmFsdWUpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9pbml0RGF0YUF0dHJpYnV0ZXM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHRoYXQgPSB0aGlzLFxyXG4gICAgICAgICAgICAgICAgb3B0aW9ucyA9IHRoaXMub3B0aW9ucyxcclxuICAgICAgICAgICAgICAgIGNsb25lID0gJCh0aGlzLmVsZW1lbnRbMF0uY2xvbmVOb2RlKGZhbHNlKSk7XHJcbiAgICAgICAgICAgIC8vIEluaXRpYWxpemUgb3B0aW9ucyBzZXQgdmlhIEhUTUw1IGRhdGEtYXR0cmlidXRlczpcclxuICAgICAgICAgICAgJC5lYWNoKFxyXG4gICAgICAgICAgICAgICAgY2xvbmUuZGF0YSgpLFxyXG4gICAgICAgICAgICAgICAgZnVuY3Rpb24gKGtleSwgdmFsdWUpIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgZGF0YUF0dHJpYnV0ZU5hbWUgPSAnZGF0YS0nICtcclxuICAgICAgICAgICAgICAgICAgICAgICAgLy8gQ29udmVydCBjYW1lbENhc2UgdG8gaHlwaGVuLWF0ZWQga2V5OlxyXG4gICAgICAgICAgICAgICAgICAgICAgICBrZXkucmVwbGFjZSgvKFthLXpdKShbQS1aXSkvZywgJyQxLSQyJykudG9Mb3dlckNhc2UoKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoY2xvbmUuYXR0cihkYXRhQXR0cmlidXRlTmFtZSkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHRoYXQuX2lzUmVnRXhwT3B0aW9uKGtleSwgdmFsdWUpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YWx1ZSA9IHRoYXQuX2dldFJlZ0V4cCh2YWx1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgb3B0aW9uc1trZXldID0gdmFsdWU7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICApO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIF9jcmVhdGU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdGhpcy5faW5pdERhdGFBdHRyaWJ1dGVzKCk7XHJcbiAgICAgICAgICAgIHRoaXMuX2luaXRTcGVjaWFsT3B0aW9ucygpO1xyXG4gICAgICAgICAgICB0aGlzLl9zbG90cyA9IFtdO1xyXG4gICAgICAgICAgICB0aGlzLl9zZXF1ZW5jZSA9IHRoaXMuX2dldFhIUlByb21pc2UodHJ1ZSk7XHJcbiAgICAgICAgICAgIHRoaXMuX3NlbmRpbmcgPSB0aGlzLl9hY3RpdmUgPSAwO1xyXG4gICAgICAgICAgICB0aGlzLl9pbml0UHJvZ3Jlc3NPYmplY3QodGhpcyk7XHJcbiAgICAgICAgICAgIHRoaXMuX2luaXRFdmVudEhhbmRsZXJzKCk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgLy8gVGhpcyBtZXRob2QgaXMgZXhwb3NlZCB0byB0aGUgd2lkZ2V0IEFQSSBhbmQgYWxsb3dzIHRvIHF1ZXJ5XHJcbiAgICAgICAgLy8gdGhlIG51bWJlciBvZiBhY3RpdmUgdXBsb2FkczpcclxuICAgICAgICBhY3RpdmU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2FjdGl2ZTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICAvLyBUaGlzIG1ldGhvZCBpcyBleHBvc2VkIHRvIHRoZSB3aWRnZXQgQVBJIGFuZCBhbGxvd3MgdG8gcXVlcnlcclxuICAgICAgICAvLyB0aGUgd2lkZ2V0IHVwbG9hZCBwcm9ncmVzcy5cclxuICAgICAgICAvLyBJdCByZXR1cm5zIGFuIG9iamVjdCB3aXRoIGxvYWRlZCwgdG90YWwgYW5kIGJpdHJhdGUgcHJvcGVydGllc1xyXG4gICAgICAgIC8vIGZvciB0aGUgcnVubmluZyB1cGxvYWRzOlxyXG4gICAgICAgIHByb2dyZXNzOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9wcm9ncmVzcztcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICAvLyBUaGlzIG1ldGhvZCBpcyBleHBvc2VkIHRvIHRoZSB3aWRnZXQgQVBJIGFuZCBhbGxvd3MgYWRkaW5nIGZpbGVzXHJcbiAgICAgICAgLy8gdXNpbmcgdGhlIGZpbGV1cGxvYWQgQVBJLiBUaGUgZGF0YSBwYXJhbWV0ZXIgYWNjZXB0cyBhbiBvYmplY3Qgd2hpY2hcclxuICAgICAgICAvLyBtdXN0IGhhdmUgYSBmaWxlcyBwcm9wZXJ0eSBhbmQgY2FuIGNvbnRhaW4gYWRkaXRpb25hbCBvcHRpb25zOlxyXG4gICAgICAgIC8vIC5maWxldXBsb2FkKCdhZGQnLCB7ZmlsZXM6IGZpbGVzTGlzdH0pO1xyXG4gICAgICAgIGFkZDogZnVuY3Rpb24gKGRhdGEpIHtcclxuICAgICAgICAgICAgdmFyIHRoYXQgPSB0aGlzO1xyXG4gICAgICAgICAgICBpZiAoIWRhdGEgfHwgdGhpcy5vcHRpb25zLmRpc2FibGVkKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgaWYgKGRhdGEuZmlsZUlucHV0ICYmICFkYXRhLmZpbGVzKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9nZXRGaWxlSW5wdXRGaWxlcyhkYXRhLmZpbGVJbnB1dCkuYWx3YXlzKGZ1bmN0aW9uIChmaWxlcykge1xyXG4gICAgICAgICAgICAgICAgICAgIGRhdGEuZmlsZXMgPSBmaWxlcztcclxuICAgICAgICAgICAgICAgICAgICB0aGF0Ll9vbkFkZChudWxsLCBkYXRhKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgZGF0YS5maWxlcyA9ICQubWFrZUFycmF5KGRhdGEuZmlsZXMpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fb25BZGQobnVsbCwgZGF0YSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICAvLyBUaGlzIG1ldGhvZCBpcyBleHBvc2VkIHRvIHRoZSB3aWRnZXQgQVBJIGFuZCBhbGxvd3Mgc2VuZGluZyBmaWxlc1xyXG4gICAgICAgIC8vIHVzaW5nIHRoZSBmaWxldXBsb2FkIEFQSS4gVGhlIGRhdGEgcGFyYW1ldGVyIGFjY2VwdHMgYW4gb2JqZWN0IHdoaWNoXHJcbiAgICAgICAgLy8gbXVzdCBoYXZlIGEgZmlsZXMgb3IgZmlsZUlucHV0IHByb3BlcnR5IGFuZCBjYW4gY29udGFpbiBhZGRpdGlvbmFsIG9wdGlvbnM6XHJcbiAgICAgICAgLy8gLmZpbGV1cGxvYWQoJ3NlbmQnLCB7ZmlsZXM6IGZpbGVzTGlzdH0pO1xyXG4gICAgICAgIC8vIFRoZSBtZXRob2QgcmV0dXJucyBhIFByb21pc2Ugb2JqZWN0IGZvciB0aGUgZmlsZSB1cGxvYWQgY2FsbC5cclxuICAgICAgICBzZW5kOiBmdW5jdGlvbiAoZGF0YSkge1xyXG4gICAgICAgICAgICBpZiAoZGF0YSAmJiAhdGhpcy5vcHRpb25zLmRpc2FibGVkKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoZGF0YS5maWxlSW5wdXQgJiYgIWRhdGEuZmlsZXMpIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgdGhhdCA9IHRoaXMsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRmZCA9ICQuRGVmZXJyZWQoKSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgcHJvbWlzZSA9IGRmZC5wcm9taXNlKCksXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGpxWEhSLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBhYm9ydGVkO1xyXG4gICAgICAgICAgICAgICAgICAgIHByb21pc2UuYWJvcnQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGFib3J0ZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoanFYSFIpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBqcVhIUi5hYm9ydCgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRmZC5yZWplY3QobnVsbCwgJ2Fib3J0JywgJ2Fib3J0Jyk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBwcm9taXNlO1xyXG4gICAgICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5fZ2V0RmlsZUlucHV0RmlsZXMoZGF0YS5maWxlSW5wdXQpLmFsd2F5cyhcclxuICAgICAgICAgICAgICAgICAgICAgICAgZnVuY3Rpb24gKGZpbGVzKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoYWJvcnRlZCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghZmlsZXMubGVuZ3RoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZGZkLnJlamVjdCgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRhdGEuZmlsZXMgPSBmaWxlcztcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGpxWEhSID0gdGhhdC5fb25TZW5kKG51bGwsIGRhdGEpLnRoZW4oXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZnVuY3Rpb24gKHJlc3VsdCwgdGV4dFN0YXR1cywganFYSFIpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZGZkLnJlc29sdmUocmVzdWx0LCB0ZXh0U3RhdHVzLCBqcVhIUik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmdW5jdGlvbiAoanFYSFIsIHRleHRTdGF0dXMsIGVycm9yVGhyb3duKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRmZC5yZWplY3QoanFYSFIsIHRleHRTdGF0dXMsIGVycm9yVGhyb3duKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5fZW5oYW5jZVByb21pc2UocHJvbWlzZSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBkYXRhLmZpbGVzID0gJC5tYWtlQXJyYXkoZGF0YS5maWxlcyk7XHJcbiAgICAgICAgICAgICAgICBpZiAoZGF0YS5maWxlcy5sZW5ndGgpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5fb25TZW5kKG51bGwsIGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9nZXRYSFJQcm9taXNlKGZhbHNlLCBkYXRhICYmIGRhdGEuY29udGV4dCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgIH0pO1xyXG5cclxufSkpO1xyXG4iXSwiZmlsZSI6ImpxdWVyeS5maWxldXBsb2FkLmpzIiwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=